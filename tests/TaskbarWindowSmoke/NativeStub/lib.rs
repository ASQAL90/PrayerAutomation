//! Synthetic, owned test schedule/calendar. No OS-control APIs or real math.
use std::sync::atomic::{AtomicU32, Ordering};
static CALLS: AtomicU32 = AtomicU32::new(0);
#[repr(C)] pub struct Calendar { status:i32,year:i32,month:i32,day:i32,weekday:i32 }
#[repr(C)] pub struct Month { status:i32,year:i32,month:i32,days:i32,gy:i32,gm:i32,gd:i32,weekday:i32 }
#[repr(C)] pub struct Times { status:i32,fajr:i32,sunrise:i32,dhuhr:i32,asr:i32,maghrib:i32,isha:i32 }
#[no_mangle] pub extern "C" fn prayer_core_abi_version()->u32 { 1 }
#[no_mangle] pub extern "C" fn taskbar_fixture_calls()->u32 { CALLS.load(Ordering::SeqCst) }
#[no_mangle] pub unsafe extern "C" fn prayer_core_calendar_gregorian_to_hijri(_:i32, month:i32, day:i32, result:*mut Calendar)->i32 {
    CALLS.fetch_add(1,Ordering::SeqCst);
    if result.is_null(){return -1;}
    *result=Calendar{status:0,year:1448,month:month.clamp(1,12),day:day.clamp(1,28),weekday:0};0
}
#[no_mangle] pub unsafe extern "C" fn prayer_core_calendar_hijri_month(year:i32, month:i32, result:*mut Month)->i32 {
    CALLS.fetch_add(1,Ordering::SeqCst);
    if result.is_null(){return -1;}
    *result=Month{status:0,year,month:month.clamp(1,12),days:28,gy:2026,gm:month.clamp(1,12),gd:1,weekday:0};0
}
#[no_mangle] pub extern "C" fn prayer_core_calendar_holiday_mask(_:i32,_:i32,_:i32,_:i32)->u32 {CALLS.fetch_add(1,Ordering::SeqCst);0}
#[no_mangle] pub unsafe extern "C" fn prayer_core_calculate_prayer_times(_:i32,_:i32,_:i32,_:f64,_:f64,_:i32,_:i32,_:f64,_:f64,result:*mut Times)->i32 {
    CALLS.fetch_add(1,Ordering::SeqCst);
    if result.is_null(){return -1;}
    *result=Times{status:0,fajr:300,sunrise:360,dhuhr:720,asr:900,maghrib:1080,isha:1140};0
}
