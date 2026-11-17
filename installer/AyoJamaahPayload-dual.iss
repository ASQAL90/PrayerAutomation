; PLACEHOLDER — belum digenerate untuk payload 2.1.2.
; Installer 2.1.2 memakai file ini untuk mem-pin payload x86+x64.
;
; Setelah portable x86 dan x64 dibangun serta receipt-nya direview, jalankan:
;   python installer/generate_accepted_payload.py \
;     --receipt-x86 <receipt-x86> --source-x86 <portable-x86> \
;     --receipt-x64 <receipt-x64> --source-x64 <portable-x64> \
;     --output installer/AyoJamaahPayload-dual.iss
;
; Jangan compile installer dengan placeholder ini. File ini akan ditimpa oleh
; generator; file pin lain yang sudah ada tidak ditimpa tanpa review.
#error "AyoJamaahPayload-dual.iss belum digenerate untuk payload 2.1.2. Jalankan installer/generate_accepted_payload.py dengan receipt x86 dan x64."
