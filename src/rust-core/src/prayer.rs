//! Solar prayer-time calculations and their stable C ABI.

use std::f64::consts::PI;

#[repr(C)]
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
pub struct PrayerTimesResult {
    pub status: i32,
    pub fajr_minutes: i32,
    pub sunrise_minutes: i32,
    pub dhuhr_minutes: i32,
    pub asr_minutes: i32,
    pub maghrib_minutes: i32,
    pub isha_minutes: i32,
}

#[derive(Clone, Copy)]
struct MethodParameters {
    fajr_angle: f64,
    isha_angle: Option<f64>,
    isha_interval_after_maghrib: Option<i32>,
}

fn method_parameters(method: i32, custom_fajr: f64, custom_isha: f64) -> Option<MethodParameters> {
    let angles = |fajr, isha| MethodParameters {
        fajr_angle: fajr,
        isha_angle: Some(isha),
        isha_interval_after_maghrib: None,
    };
    match method {
        1 => Some(angles(18.0, 18.0)), // Karachi
        2 => Some(angles(15.0, 15.0)), // ISNA
        3 => Some(angles(18.0, 17.0)), // Muslim World League
        4 => Some(MethodParameters {
            // Umm Al-Qura, Makkah
            fajr_angle: 18.5,
            isha_angle: None,
            isha_interval_after_maghrib: Some(90),
        }),
        11 => Some(angles(20.0, 18.0)), // MUIS Singapore
        13 => Some(angles(18.0, 17.0)), // Diyanet
        17 => Some(angles(20.0, 18.0)), // JAKIM
        20 => Some(angles(20.0, 18.0)), // Kementerian Agama RI
        99 if custom_fajr.is_finite()
            && custom_isha.is_finite()
            && (0.0..60.0).contains(&custom_fajr)
            && (0.0..60.0).contains(&custom_isha) =>
        {
            Some(angles(custom_fajr, custom_isha))
        }
        _ => None,
    }
}

fn valid_gregorian_date(year: i32, month: i32, day: i32) -> bool {
    if !(1..=9999).contains(&year) {
        return false;
    }
    let leap = year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
    let days = match month {
        1 | 3 | 5 | 7 | 8 | 10 | 12 => 31,
        4 | 6 | 9 | 11 => 30,
        2 if leap => 29,
        2 => 28,
        _ => return false,
    };
    (1..=days).contains(&day)
}

/// Days from 1970-01-01, used only to determine day-of-year.
fn days_from_civil(year: i32, month: i32, day: i32) -> i64 {
    let mut y = i64::from(year);
    let m = i64::from(month);
    let d = i64::from(day);
    if m <= 2 {
        y -= 1;
    }
    let era = if y >= 0 { y } else { y - 399 }.div_euclid(400);
    let year_of_era = y - era * 400;
    let month_prime = m + if m > 2 { -3 } else { 9 };
    let day_of_year = (153 * month_prime + 2).div_euclid(5) + d - 1;
    let day_of_era = year_of_era * 365 + year_of_era / 4 - year_of_era / 100 + day_of_year;
    era * 146_097 + day_of_era - 719_468
}

fn solar_event_minutes(
    altitude_degrees: f64,
    latitude: f64,
    declination: f64,
    solar_noon_minutes: f64,
    morning: bool,
) -> Option<i32> {
    let altitude = altitude_degrees.to_radians();
    let numerator = altitude.sin() - latitude.sin() * declination.sin();
    let denominator = latitude.cos() * declination.cos();
    if denominator.abs() < 1.0e-12 {
        return None;
    }
    let cosine_hour_angle = numerator / denominator;
    if !(-1.0..=1.0).contains(&cosine_hour_angle) {
        return None;
    }
    let event_offset_minutes = cosine_hour_angle.acos().to_degrees() * 4.0;
    Some(
        (solar_noon_minutes
            + if morning {
                -event_offset_minutes
            } else {
                event_offset_minutes
            })
        .round() as i32,
    )
}

fn solar_declination_and_equation(year: i32, month: i32, day: i32) -> Option<(f64, f64)> {
    if !valid_gregorian_date(year, month, day) {
        return None;
    }
    let day_of_year = days_from_civil(year, month, day) - days_from_civil(year, 1, 1) + 1;
    let leap = year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
    let year_length = if leap { 366.0 } else { 365.0 };
    let gamma = 2.0 * PI / year_length * (day_of_year as f64 - 1.0);
    let equation_minutes = 229.18
        * (0.000_075 + 0.001_868 * gamma.cos()
            - 0.032_077 * gamma.sin()
            - 0.014_615 * (2.0 * gamma).cos()
            - 0.040_849 * (2.0 * gamma).sin());
    let declination = 0.006_918 - 0.399_912 * gamma.cos() + 0.070_257 * gamma.sin()
        - 0.006_758 * (2.0 * gamma).cos()
        + 0.000_907 * (2.0 * gamma).sin()
        - 0.002_697 * (3.0 * gamma).cos()
        + 0.001_480 * (3.0 * gamma).sin();
    Some((declination, equation_minutes))
}

// API permukaan C ABI: argumen numerik sengaja eksplisit agar pemanggil C# tidak
// perlu menyusun struct, sehingga clippy::too_many_arguments tidak berlaku di sini.
#[allow(clippy::too_many_arguments)]
fn calculate(
    year: i32,
    month: i32,
    day: i32,
    latitude: f64,
    longitude: f64,
    timezone_offset_minutes: i32,
    method: i32,
    custom_fajr_angle: f64,
    custom_isha_angle: f64,
) -> Result<PrayerTimesResult, i32> {
    if !valid_gregorian_date(year, month, day) {
        return Err(1);
    }
    if crate::prayer_core_validate_coordinates(latitude, longitude) != 0
        || !(-840..=840).contains(&timezone_offset_minutes)
    {
        return Err(2);
    }
    let parameters = method_parameters(method, custom_fajr_angle, custom_isha_angle).ok_or(3)?;
    let latitude_radians = latitude.clamp(-89.8, 89.8).to_radians();
    let (declination, equation_minutes) =
        solar_declination_and_equation(year, month, day).ok_or(1)?;
    let solar_noon =
        720.0 - 4.0 * longitude - equation_minutes + f64::from(timezone_offset_minutes);
    let sunrise =
        solar_event_minutes(-0.833, latitude_radians, declination, solar_noon, true).ok_or(4)?;
    let sunset =
        solar_event_minutes(-0.833, latitude_radians, declination, solar_noon, false).ok_or(4)?;
    let fajr = solar_event_minutes(
        -parameters.fajr_angle,
        latitude_radians,
        declination,
        solar_noon,
        true,
    )
    .ok_or(4)?;
    // Python's adhanpy default is the Shafi'i Asr shadow factor (1).
    let asr_altitude = (1.0 / (1.0 + (latitude_radians - declination).abs().tan()))
        .atan()
        .to_degrees();
    let asr = solar_event_minutes(
        asr_altitude,
        latitude_radians,
        declination,
        solar_noon,
        false,
    )
    .ok_or(4)?;
    let isha = if let Some(interval) = parameters.isha_interval_after_maghrib {
        sunset + interval
    } else {
        solar_event_minutes(
            -parameters.isha_angle.ok_or(3)?,
            latitude_radians,
            declination,
            solar_noon,
            false,
        )
        .ok_or(4)?
    };
    let dhuhr = solar_noon.round() as i32;
    if !(fajr < sunrise && sunrise < dhuhr && dhuhr < asr && asr < sunset && sunset <= isha) {
        return Err(4);
    }
    Ok(PrayerTimesResult {
        status: 0,
        fajr_minutes: fajr,
        sunrise_minutes: sunrise,
        dhuhr_minutes: dhuhr,
        asr_minutes: asr,
        maghrib_minutes: sunset,
        isha_minutes: isha,
    })
}

/// Calculate six prayer events in minutes after local midnight.
/// Status: 0 success; 1 invalid date; 2 invalid coordinates/offset;
/// 3 unsupported method/settings; 4 event unavailable; 5 null output; 6 panic.
/// The caller supplies writable storage for one PrayerTimesResult.
#[unsafe(no_mangle)]
pub extern "C" fn prayer_core_calculate_prayer_times(
    year: i32,
    month: i32,
    day: i32,
    latitude: f64,
    longitude: f64,
    timezone_offset_minutes: i32,
    method: i32,
    custom_fajr_angle: f64,
    custom_isha_angle: f64,
    out: *mut PrayerTimesResult,
) -> i32 {
    if out.is_null() {
        return 5;
    }
    let result = std::panic::catch_unwind(|| {
        calculate(
            year,
            month,
            day,
            latitude,
            longitude,
            timezone_offset_minutes,
            method,
            custom_fajr_angle,
            custom_isha_angle,
        )
    });
    match result {
        Ok(Ok(value)) => {
            unsafe { out.write(value) };
            0
        }
        Ok(Err(code)) => code,
        Err(_) => 6,
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn jakarta_events_are_chronological_and_known_methods_are_accepted() {
        let result = calculate(2026, 10, 19, -6.2088, 106.8456, 420, 20, 18.0, 18.0)
            .expect("astronomical events exist at Jakarta latitude");
        assert_eq!(result.status, 0);
        assert!(result.fajr_minutes < result.sunrise_minutes);
        assert!(result.sunrise_minutes < result.dhuhr_minutes);
        assert!(result.dhuhr_minutes < result.asr_minutes);
        assert!(result.asr_minutes < result.maghrib_minutes);
        assert!(result.maghrib_minutes < result.isha_minutes);
        for method in [1, 2, 3, 4, 11, 13, 17, 20, 99] {
            assert!(
                calculate(2026, 10, 19, -6.2088, 106.8456, 420, method, 18.0, 18.0).is_ok(),
                "method {method}"
            );
        }
        assert!(calculate(2026, 10, 19, -6.2, 106.8, 420, 987, 18.0, 18.0).is_err());
    }

    #[test]
    fn invalid_date_and_timezone_are_rejected() {
        assert_eq!(calculate(2026, 2, 29, 0.0, 0.0, 0, 20, 18.0, 18.0), Err(1));
        assert_eq!(calculate(2026, 1, 1, 0.0, 0.0, 900, 20, 18.0, 18.0), Err(2));
    }
}
