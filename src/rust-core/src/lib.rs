//! Pure domain logic and a narrow C ABI for the Prayer Automation UI.
//!
//! Kept independent of Windows APIs and GUI frameworks so the same behaviors
//! can be unit-tested on non-UI build agents.

mod calendar;
mod prayer;

/// ABI version for the C# host. Increment only alongside coordinated host changes.
#[unsafe(no_mangle)]
pub extern "C" fn prayer_core_abi_version() -> u32 {
    1
}

/// Return a stable status code for validating one manual correction value.
///
/// `0`: valid and within the currently supported -30..=30 minute range.
/// `1`: outside that range.
#[unsafe(no_mangle)]
pub extern "C" fn prayer_core_validate_correction_minutes(value: i32) -> i32 {
    if (-30..=30).contains(&value) { 0 } else { 1 }
}

/// Return stable status for validating geographic coordinates.
///
/// `0`: valid; `1`: invalid latitude; `2`: invalid longitude.
#[unsafe(no_mangle)]
pub extern "C" fn prayer_core_validate_coordinates(latitude: f64, longitude: f64) -> i32 {
    if !latitude.is_finite() || !(-90.0..=90.0).contains(&latitude) {
        return 1;
    }
    if !longitude.is_finite() || !(-180.0..=180.0).contains(&longitude) {
        return 2;
    }
    0
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn correction_limits_accept_endpoints_and_reject_outside() {
        assert_eq!(prayer_core_validate_correction_minutes(-30), 0);
        assert_eq!(prayer_core_validate_correction_minutes(30), 0);
        assert_eq!(prayer_core_validate_correction_minutes(-31), 1);
        assert_eq!(prayer_core_validate_correction_minutes(31), 1);
    }

    #[test]
    fn coordinate_validation_accepts_world_bounds_and_rejects_non_finite() {
        assert_eq!(prayer_core_validate_coordinates(-90.0, -180.0), 0);
        assert_eq!(prayer_core_validate_coordinates(90.0, 180.0), 0);
        assert_eq!(prayer_core_validate_coordinates(90.01, 0.0), 1);
        assert_eq!(prayer_core_validate_coordinates(0.0, -180.01), 2);
        assert_eq!(prayer_core_validate_coordinates(f64::NAN, 0.0), 1);
        assert_eq!(prayer_core_validate_coordinates(0.0, f64::INFINITY), 2);
    }
}
