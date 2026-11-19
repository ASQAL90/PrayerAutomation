//! Gregorian/Hijri calendar rules shared by the portable UI.

use std::sync::OnceLock;

/// Official KHGT month starts published by Muhammadiyah (api-khgt.muhammadiyah.or.id).
const KHGT_TABLE: &str = include_str!("khgt_table.txt");
/// First Hijri year covered by the embedded KHGT table.
#[allow(dead_code)] // Host mirrors this range; kept here next to the table.
pub const KHGT_FIRST_YEAR: i32 = 1440;
/// Last Hijri year covered by the embedded KHGT table.
#[allow(dead_code)]
pub const KHGT_LAST_YEAR: i32 = 1499;

const HIJRI_EPOCH_JDN: i64 = 1_948_440;
const EPOCH_1970_JDN: i64 = 2_440_588;

#[repr(C)]
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
pub struct PrayerCalendarDate {
    pub status: i32,
    pub year: i32,
    pub month: i32,
    pub day: i32,
    pub weekday_monday0: i32,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
pub struct PrayerHijriMonth {
    pub status: i32,
    pub hijri_year: i32,
    pub hijri_month: i32,
    pub days_in_month: i32,
    pub gregorian_year: i32,
    pub gregorian_month: i32,
    pub gregorian_day: i32,
    pub weekday_monday0: i32,
}

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
struct HijriDate {
    year: i32,
    month: i32,
    day: i32,
}

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
struct GregorianDate {
    year: i32,
    month: i32,
    day: i32,
}

fn is_leap_year(year: i32) -> bool {
    year % 4 == 0 && (year % 100 != 0 || year % 400 == 0)
}

fn days_in_month(year: i32, month: i32) -> Option<i32> {
    match month {
        1 | 3 | 5 | 7 | 8 | 10 | 12 => Some(31),
        4 | 6 | 9 | 11 => Some(30),
        2 if is_leap_year(year) => Some(29),
        2 => Some(28),
        _ => None,
    }
}

fn valid_gregorian_date(year: i32, month: i32, day: i32) -> bool {
    (1..=9999).contains(&year)
        && days_in_month(year, month).is_some_and(|days| (1..=days).contains(&day))
}

/// Days from 1970-01-01, using the proleptic Gregorian calendar.
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

fn civil_from_days(days_since_epoch: i64) -> GregorianDate {
    let z = days_since_epoch + 719_468;
    let era = if z >= 0 { z } else { z - 146_096 }.div_euclid(146_097);
    let day_of_era = z - era * 146_097;
    let year_of_era = (day_of_era - day_of_era / 1_460 + day_of_era / 36_524
        - day_of_era / 146_096)
        .div_euclid(365);
    let mut year = year_of_era + era * 400;
    let day_of_year = day_of_era - (365 * year_of_era + year_of_era / 4 - year_of_era / 100);
    let month_prime = (5 * day_of_year + 2).div_euclid(153);
    let day = day_of_year - (153 * month_prime + 2).div_euclid(5) + 1;
    let month = month_prime + if month_prime < 10 { 3 } else { -9 };
    if month <= 2 {
        year += 1;
    }
    GregorianDate {
        year: year as i32,
        month: month as i32,
        day: day as i32,
    }
}

fn weekday_monday0(days_since_epoch: i64) -> i32 {
    (days_since_epoch + 3).rem_euclid(7) as i32
}

/// Port of prayer_automation.py:gregorian_to_hijri. The same integer
/// boundaries are used to preserve its civil/tabular date results exactly.
fn hijri_from_gregorian(year: i32, month: i32, day: i32) -> Option<HijriDate> {
    if !valid_gregorian_date(year, month, day) {
        return None;
    }
    let z = days_from_civil(year, month, day) + EPOCH_1970_JDN;
    let mut l = z - HIJRI_EPOCH_JDN + 10_632;
    let n = (l - 1).div_euclid(10_631);
    l = l - 10_631 * n + 354;
    let j = ((10_985 - l).div_euclid(5_316)) * ((50 * l).div_euclid(17_719))
        + l.div_euclid(5_670) * ((43 * l).div_euclid(15_238));
    l = l
        - ((30 - j).div_euclid(15)) * ((17_719 * j).div_euclid(50))
        - j.div_euclid(16) * ((15_238 * j).div_euclid(43))
        + 29;
    let month = (24 * l).div_euclid(709);
    let day = l - (709 * month).div_euclid(24);
    let year = 30 * n + j - 30;
    if !(1..=12).contains(&month) || !(1..=9_666).contains(&year) {
        return None;
    }
    Some(HijriDate {
        year: year as i32,
        month: month as i32,
        day: day as i32,
    })
}

fn approximate_gregorian_year(hijri_year: i32) -> i32 {
    622 + (((hijri_year - 1) as f64 * 354.367) / 365.2425).floor() as i32
}

fn find_hijri_month_start(hijri_year: i32, hijri_month: i32) -> Option<i64> {
    if !(1..=9_666).contains(&hijri_year) || !(1..=12).contains(&hijri_month) {
        return None;
    }
    let approx_year = approximate_gregorian_year(hijri_year);
    if !(2..=9_998).contains(&approx_year) {
        return None;
    }
    let start = days_from_civil(approx_year - 2, 1, 1);
    let end = days_from_civil(approx_year + 3, 1, 1);
    (start..end).find(|serial| {
        let g = civil_from_days(*serial);
        hijri_from_gregorian(g.year, g.month, g.day)
            .is_some_and(|h| h.year == hijri_year && h.month == hijri_month && h.day == 1)
    })
}

/// Parsed KHGT month starts: (days since epoch, Hijri year, Hijri month), sorted by date.
fn khgt_starts() -> &'static [(i64, i32, i32)] {
    static STARTS: OnceLock<Vec<(i64, i32, i32)>> = OnceLock::new();
    STARTS.get_or_init(|| {
        let mut rows = Vec::new();
        for line in KHGT_TABLE.lines() {
            let line = line.trim();
            if line.is_empty() || line.starts_with('#') {
                continue;
            }
            let mut fields = line.split_whitespace();
            let (Some(year), Some(month), Some(date)) =
                (fields.next(), fields.next(), fields.next())
            else {
                continue;
            };
            let (Ok(year), Ok(month)) = (year.parse::<i32>(), month.parse::<i32>()) else {
                continue;
            };
            let mut parts = date.split('-');
            let (Some(gy), Some(gm), Some(gd)) = (parts.next(), parts.next(), parts.next()) else {
                continue;
            };
            let (Ok(gy), Ok(gm), Ok(gd)) = (gy.parse::<i32>(), gm.parse::<i32>(), gd.parse::<i32>())
            else {
                continue;
            };
            if !(1..=12).contains(&month) || !valid_gregorian_date(gy, gm, gd) {
                continue;
            }
            rows.push((days_from_civil(gy, gm, gd), year, month));
        }
        rows.sort_unstable_by_key(|row| row.0);
        rows
    })
}

fn khgt_month_index(hijri_year: i32, hijri_month: i32) -> Option<usize> {
    khgt_starts()
        .binary_search_by(|row| (row.1, row.2).cmp(&(hijri_year, hijri_month)))
        .ok()
}

/// Maps a Gregorian date through the official KHGT month table; `None` outside its range.
fn khgt_hijri_from_gregorian(year: i32, month: i32, day: i32) -> Option<HijriDate> {
    if !valid_gregorian_date(year, month, day) {
        return None;
    }
    let serial = days_from_civil(year, month, day);
    let starts = khgt_starts();
    let index = starts.partition_point(|row| row.0 <= serial).checked_sub(1)?;
    let day_in_month = serial - starts[index].0 + 1;
    let upper = if index + 1 < starts.len() {
        starts[index + 1].0 - starts[index].0
    } else {
        // The table stops after its final month start; accept a full 30-day month there.
        30
    };
    if !(1..=upper).contains(&day_in_month) {
        return None;
    }
    Some(HijriDate {
        year: starts[index].1,
        month: starts[index].2,
        day: day_in_month as i32,
    })
}

fn khgt_month_info(hijri_year: i32, hijri_month: i32) -> Option<(i64, i32)> {
    let starts = khgt_starts();
    let index = khgt_month_index(hijri_year, hijri_month)?;
    let days = if index + 1 < starts.len() {
        starts[index + 1].0 - starts[index].0
    } else {
        30
    };
    Some((starts[index].0, days as i32))
}

/// Convert Gregorian Y-M-D to the civil Hijri date used by the Python app.
/// `out` must point to writable storage for one PrayerCalendarDate.
#[unsafe(no_mangle)]
pub extern "C" fn prayer_core_calendar_gregorian_to_hijri(
    year: i32,
    month: i32,
    day: i32,
    out: *mut PrayerCalendarDate,
) -> i32 {
    if out.is_null() {
        return 2;
    }
    let result = std::panic::catch_unwind(|| {
        let h = hijri_from_gregorian(year, month, day).ok_or(1)?;
        Ok::<_, i32>(PrayerCalendarDate {
            status: 0,
            year: h.year,
            month: h.month,
            day: h.day,
            weekday_monday0: weekday_monday0(days_from_civil(year, month, day)),
        })
    });
    match result {
        Ok(Ok(value)) => {
            unsafe { out.write(value) };
            0
        }
        Ok(Err(code)) => code,
        Err(_) => 3,
    }
}

/// Load a full tabular Hijri month and its corresponding Gregorian start date.
/// `out` must point to writable storage for one PrayerHijriMonth.
#[unsafe(no_mangle)]
pub extern "C" fn prayer_core_calendar_hijri_month(
    hijri_year: i32,
    hijri_month: i32,
    out: *mut PrayerHijriMonth,
) -> i32 {
    if out.is_null() {
        return 2;
    }
    let result = std::panic::catch_unwind(|| {
        let start = find_hijri_month_start(hijri_year, hijri_month).ok_or(1)?;
        let (next_year, next_month) = if hijri_month == 12 {
            (hijri_year + 1, 1)
        } else {
            (hijri_year, hijri_month + 1)
        };
        let next = find_hijri_month_start(next_year, next_month).ok_or(1)?;
        let g = civil_from_days(start);
        let days = (next - start) as i32;
        if !(29..=30).contains(&days) {
            return Err(3);
        }
        Ok::<_, i32>(PrayerHijriMonth {
            status: 0,
            hijri_year,
            hijri_month,
            days_in_month: days,
            gregorian_year: g.year,
            gregorian_month: g.month,
            gregorian_day: g.day,
            weekday_monday0: weekday_monday0(start),
        })
    });
    match result {
        Ok(Ok(value)) => {
            unsafe { out.write(value) };
            0
        }
        Ok(Err(code)) => code,
        Err(_) => 4,
    }
}

/// Convert Gregorian Y-M-D through the official KHGT month table.
/// `out` must point to writable storage for one PrayerCalendarDate.
/// Returns 1 when the date falls outside the embedded table.
#[unsafe(no_mangle)]
pub extern "C" fn prayer_core_calendar_gregorian_to_hijri_khgt(
    year: i32,
    month: i32,
    day: i32,
    out: *mut PrayerCalendarDate,
) -> i32 {
    if out.is_null() {
        return 2;
    }
    let result = std::panic::catch_unwind(|| {
        let h = khgt_hijri_from_gregorian(year, month, day).ok_or(1)?;
        Ok::<_, i32>(PrayerCalendarDate {
            status: 0,
            year: h.year,
            month: h.month,
            day: h.day,
            weekday_monday0: weekday_monday0(days_from_civil(year, month, day)),
        })
    });
    match result {
        Ok(Ok(value)) => {
            unsafe { out.write(value) };
            0
        }
        Ok(Err(code)) => code,
        Err(_) => 3,
    }
}

/// Load a KHGT Hijri month from the official table.
/// `out` must point to writable storage for one PrayerHijriMonth.
#[unsafe(no_mangle)]
pub extern "C" fn prayer_core_calendar_hijri_month_khgt(
    hijri_year: i32,
    hijri_month: i32,
    out: *mut PrayerHijriMonth,
) -> i32 {
    if out.is_null() {
        return 2;
    }
    let result = std::panic::catch_unwind(|| {
        let (start, days) = khgt_month_info(hijri_year, hijri_month).ok_or(1)?;
        let g = civil_from_days(start);
        Ok::<_, i32>(PrayerHijriMonth {
            status: 0,
            hijri_year,
            hijri_month,
            days_in_month: days,
            gregorian_year: g.year,
            gregorian_month: g.month,
            gregorian_day: g.day,
            weekday_monday0: weekday_monday0(start),
        })
    });
    match result {
        Ok(Ok(value)) => {
            unsafe { out.write(value) };
            0
        }
        Ok(Err(code)) => code,
        Err(_) => 4,
    }
}

/// Return a bit mask for fixed/general and estimated Islamic calendar markers.
/// Bits 0-4: New Year, Labour Day, Pancasila, Independence Day, Christmas.
/// Bits 5-9: Islamic New Year, Mawlid, Isra Mi'raj, Eid al-Fitr, Eid al-Adha.
/// Bits 10-21: Ramadan events, Sunnah fasts, Tashriq and other observances.
/// Islamic dates are civil-calendar estimates, not official annual declarations.
fn islamic_observance_mask(h: HijriDate, weekday: i32) -> u32 {
    let mut mask = 0;
    match (h.month, h.day) {
        (9, 1) => mask |= 1 << 10,
        (9, 17) => mask |= 1 << 11,
        (1, 9) => mask |= 1 << 12,
        (1, 10) => mask |= 1 << 13,
        (12, 9) => mask |= 1 << 14,
        (12, 11..=13) => mask |= 1 << 15,
        (8, 15) => mask |= 1 << 20,
        (12, 1..=8) => mask |= 1 << 21,
        _ => {}
    }

    if h.month == 10 && (2..=30).contains(&h.day) {
        mask |= 1 << 17;
    }

    let is_ramadan = h.month == 9;
    let is_non_fasting_day =
        (h.month == 10 && h.day == 1) || (h.month == 12 && (10..=13).contains(&h.day));

    if !is_ramadan {
        if (13..=15).contains(&h.day) && !(h.month == 12 && h.day == 13) {
            mask |= 1 << 16;
        }
        if !is_non_fasting_day {
            match weekday {
                0 => mask |= 1 << 18,
                3 => mask |= 1 << 19,
                _ => {}
            }
        }
    }
    mask
}

/// Combines fixed Gregorian holidays with the Islamic markers for one Hijri date.
fn combined_observance_mask(year: i32, month: i32, day: i32, h: HijriDate) -> u32 {
    let mut mask = match (month, day) {
        (1, 1) => 1 << 0,
        (5, 1) => 1 << 1,
        (6, 1) => 1 << 2,
        (8, 17) => 1 << 3,
        (12, 25) => 1 << 4,
        _ => 0,
    };
    let bit = match (h.month, h.day) {
        (1, 1) => Some(5),
        (3, 12) => Some(6),
        (7, 27) => Some(7),
        (10, 1) => Some(8),
        (12, 10) => Some(9),
        _ => None,
    };
    if let Some(bit) = bit {
        mask |= 1 << bit;
    }
    mask | islamic_observance_mask(h, weekday_monday0(days_from_civil(year, month, day)))
}

#[unsafe(no_mangle)]
pub extern "C" fn prayer_core_calendar_holiday_mask(
    year: i32,
    month: i32,
    day: i32,
    hijri_correction_days: i32,
) -> u32 {
    if !valid_gregorian_date(year, month, day) {
        return 0;
    }
    let shifted = days_from_civil(year, month, day) + i64::from(hijri_correction_days.clamp(-7, 7));
    let g = civil_from_days(shifted);
    match hijri_from_gregorian(g.year, g.month, g.day) {
        Some(h) => combined_observance_mask(year, month, day, h),
        None => 0,
    }
}

/// Holiday mask using the official KHGT date. Returns 1 when the shifted date is
/// outside the embedded table, so the host can fall back to the civil calendar.
#[unsafe(no_mangle)]
pub extern "C" fn prayer_core_calendar_holiday_mask_khgt(
    year: i32,
    month: i32,
    day: i32,
    hijri_correction_days: i32,
    out_mask: *mut u32,
) -> i32 {
    if out_mask.is_null() {
        return 2;
    }
    if !valid_gregorian_date(year, month, day) {
        return 1;
    }
    let shifted = days_from_civil(year, month, day) + i64::from(hijri_correction_days.clamp(-7, 7));
    let g = civil_from_days(shifted);
    match khgt_hijri_from_gregorian(g.year, g.month, g.day) {
        Some(h) => {
            unsafe { out_mask.write(combined_observance_mask(year, month, day, h)) };
            0
        }
        None => 1,
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn gregorian_weekday_and_hijri_match_python_reference() {
        assert_eq!(weekday_monday0(days_from_civil(2026, 1, 1)), 3);
        assert_eq!(
            hijri_from_gregorian(2026, 1, 1),
            Some(HijriDate {
                year: 1447,
                month: 7,
                day: 12
            })
        );
        assert_eq!(
            hijri_from_gregorian(2026, 10, 19),
            Some(HijriDate {
                year: 1448,
                month: 5,
                day: 7
            })
        );
    }

    #[test]
    fn hijri_year_1500_loads_all_months_and_matches_python_month_lengths() {
        let expected = [30, 29, 30, 29, 30, 29, 30, 29, 30, 29, 30, 29];
        for month in 1..=12 {
            let start = find_hijri_month_start(1500, month).expect("month start exists");
            let (next_year, next_month) = if month == 12 {
                (1501, 1)
            } else {
                (1500, month + 1)
            };
            let next =
                find_hijri_month_start(next_year, next_month).expect("next month start exists");
            assert_eq!(
                (next - start) as i32,
                expected[(month - 1) as usize],
                "month {month}"
            );
        }
        let start = find_hijri_month_start(1500, 1).expect("Muharram start exists");
        assert_eq!(
            civil_from_days(start),
            GregorianDate {
                year: 2076,
                month: 11,
                day: 28
            }
        );
    }

    #[test]
    fn khgt_matches_official_1447_month_starts() {
        let start = |year: i32, month: i32| {
            let (serial, days) = khgt_month_info(year, month).expect("KHGT month exists");
            assert!((29..=30).contains(&days), "month {year}/{month} length");
            civil_from_days(serial)
        };
        assert_eq!(
            start(1447, 1),
            GregorianDate { year: 2025, month: 6, day: 26 }
        );
        assert_eq!(
            start(1447, 9),
            GregorianDate { year: 2026, month: 2, day: 18 }
        );
        assert_eq!(
            start(1447, 10),
            GregorianDate { year: 2026, month: 3, day: 20 }
        );
        assert_eq!(
            start(1447, 12),
            GregorianDate { year: 2026, month: 5, day: 18 }
        );
        assert_eq!(
            khgt_hijri_from_gregorian(2025, 6, 26),
            Some(HijriDate { year: 1447, month: 1, day: 1 })
        );
        assert_eq!(
            khgt_hijri_from_gregorian(2025, 6, 25),
            Some(HijriDate { year: 1446, month: 12, day: 29 })
        );
        assert_eq!(khgt_hijri_from_gregorian(2018, 9, 10), None);
        assert_eq!(khgt_hijri_from_gregorian(2076, 10, 30), None);
    }

    #[test]
    fn khgt_table_has_every_month_in_range_with_valid_lengths() {
        for year in KHGT_FIRST_YEAR..=KHGT_LAST_YEAR {
            for month in 1..=12 {
                if year == KHGT_LAST_YEAR && month > 11 {
                    continue;
                }
                let (serial, days) =
                    khgt_month_info(year, month).unwrap_or_else(|| panic!("{year}/{month}"));
                assert!((29..=30).contains(&days), "{year}/{month} length {days}");
                assert_eq!(khgt_month_index(year, month), khgt_starts().iter().position(|row| row.1 == year && row.2 == month));
                assert!(serial > 0);
            }
        }
    }

    #[test]
    fn khgt_holiday_mask_marks_eid_and_reports_out_of_range() {
        let mut mask = 0u32;
        assert_eq!(
            prayer_core_calendar_holiday_mask_khgt(2026, 3, 20, 0, &mut mask),
            0
        );
        assert_ne!(mask & (1 << 8), 0, "Idulfitri marker");
        let mut outside = 0u32;
        assert_eq!(
            prayer_core_calendar_holiday_mask_khgt(2018, 9, 10, 0, &mut outside),
            1
        );
    }

    #[test]
    fn holidays_mark_fixed_dates_and_follow_hijri_correction() {
        assert_eq!(prayer_core_calendar_holiday_mask(2026, 1, 1, 0) & 1, 1);
        assert_eq!(
            prayer_core_calendar_holiday_mask(2026, 1, 15, 0) & (1 << 7),
            0
        );
        assert_eq!(
            prayer_core_calendar_holiday_mask(2026, 1, 15, 1) & (1 << 7),
            1 << 7
        );
    }

    #[test]
    fn islamic_observances_mark_sunnah_fast_days_and_tashriq_separately() {
        let mask_for = |year: i32, month: i32, day: i32| {
            let serial = find_hijri_month_start(year, month).expect("Hijri month exists")
                + i64::from(day - 1);
            let gregorian = civil_from_days(serial);
            prayer_core_calendar_holiday_mask(gregorian.year, gregorian.month, gregorian.day, 0)
        };

        assert_ne!(mask_for(1448, 9, 1) & (1 << 10), 0, "Ramadan start");
        assert_ne!(
            mask_for(1448, 9, 17) & (1 << 11),
            0,
            "Nuzulul Qur'an marker"
        );
        assert_ne!(mask_for(1448, 1, 9) & (1 << 12), 0, "Tasu'a fast");
        assert_ne!(mask_for(1448, 1, 10) & (1 << 13), 0, "Ashura fast");
        assert_ne!(mask_for(1448, 12, 9) & (1 << 14), 0, "Arafah fast");
        assert_ne!(mask_for(1448, 12, 11) & (1 << 15), 0, "Tashriq day 1");
        assert_ne!(mask_for(1448, 12, 12) & (1 << 15), 0, "Tashriq day 2");
        assert_ne!(mask_for(1448, 12, 13) & (1 << 15), 0, "Tashriq day 3");
        assert_eq!(
            mask_for(1448, 12, 13) & (1 << 16),
            0,
            "13 Dhul Hijjah is not Ayyamul Bidh fasting"
        );
        assert_ne!(
            mask_for(1448, 12, 14) & (1 << 16),
            0,
            "14 Dhul Hijjah remains a white day"
        );
        assert_ne!(mask_for(1448, 10, 2) & (1 << 17), 0, "Shawwal fast window");
        assert_ne!(mask_for(1448, 8, 15) & (1 << 20), 0, "Nisfu Sya'ban marker");
        assert_ne!(
            mask_for(1448, 12, 8) & (1 << 21),
            0,
            "early Dhul Hijjah fast"
        );

        let monday_or_thursday = (1..=28)
            .flat_map(|day| [(1448, 2, day), (1448, 3, day), (1448, 4, day)])
            .find(|(year, month, day)| {
                let serial = find_hijri_month_start(*year, *month).unwrap() + i64::from(*day - 1);
                matches!(weekday_monday0(serial), 0 | 3)
            })
            .expect("a weekday fasting date exists");
        let weekday_serial = find_hijri_month_start(monday_or_thursday.0, monday_or_thursday.1)
            .unwrap()
            + i64::from(monday_or_thursday.2 - 1);
        let weekday_mask = mask_for(
            monday_or_thursday.0,
            monday_or_thursday.1,
            monday_or_thursday.2,
        );
        let weekday_bit = if weekday_monday0(weekday_serial) == 0 {
            18
        } else {
            19
        };
        assert_ne!(weekday_mask & (1 << weekday_bit), 0, "weekly Sunnah fast");

        let ramadan_weekday = (1..=29)
            .find(|day| {
                let serial = find_hijri_month_start(1448, 9).unwrap() + i64::from(*day - 1);
                matches!(weekday_monday0(serial), 0 | 3)
            })
            .expect("Ramadan contains a Monday or Thursday");
        let ramadan_serial =
            find_hijri_month_start(1448, 9).unwrap() + i64::from(ramadan_weekday - 1);
        assert_eq!(
            mask_for(1448, 9, ramadan_weekday) & ((1 << 18) | (1 << 19)),
            0,
            "obligatory Ramadan days must not be labeled Sunnah fasts"
        );
        assert!(weekday_monday0(ramadan_serial) == 0 || weekday_monday0(ramadan_serial) == 3);
    }
}
