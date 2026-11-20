; ---------------------------------------------------------------------------
; AyoVersionCompare - numeric comparison of dotted version strings in NSIS.
;
; Adapted from the NSIS Wiki "VersionCompare" by Instructor, which is based on
; Afrow UK's function "VersionCheckNew2":
;   https://nsis.sourceforge.io/VersionCompare
;
; Syntax:
;   ${AyoVersionCompare} "1.2.3" "1.2.4" $result
;
;   $result = 0 : versions are equal
;             1 : version 1 is newer
;             2 : version 2 is newer
;
; Supports 1-4 numeric, dot-separated segments. Missing segments count as zero
; ("2.4.1" equals "2.4.1.0") and leading zeroes are compared numerically
; ("1.2.01" is older than "1.2.2"). No external plug-ins are used.
;
; Preserves all registers, including $0/$1 used by the caller.
; ---------------------------------------------------------------------------

!macro AyoVersionCompareCall _VER1 _VER2 _RESULT
  Push `${_VER1}`
  Push `${_VER2}`
  Call AyoVersionCompareImpl
  Pop `${_RESULT}`
!macroend
!define AyoVersionCompare `!insertmacro AyoVersionCompareCall`

Function AyoVersionCompareImpl
  ; The caller pushed version 1 first and version 2 last, so version 2 is on
  ; top. Save the caller's registers before consuming both arguments.
  Push $0
  Push $1
  Pop $1 ; version 2
  Pop $0 ; version 1
  Push $2
  Push $3
  Push $4
  Push $5
  Push $6
  Push $7
  Push $8
  Push $9

  ayo_vcmp_begin:
    ; Split the next dot-separated segment off version 1 into $4.
    StrCpy $2 -1
    IntOp $2 $2 + 1
    StrCpy $3 $0 1 $2
    StrCmp $3 '' +2
    StrCmp $3 '.' 0 -3
    StrCpy $4 $0 $2
    IntOp $2 $2 + 1
    StrCpy $0 $0 '' $2

    ; Split the next dot-separated segment off version 2 into $5.
    StrCpy $2 -1
    IntOp $2 $2 + 1
    StrCpy $3 $1 1 $2
    StrCmp $3 '' +2
    StrCmp $3 '.' 0 -3
    StrCpy $5 $1 $2
    IntOp $2 $2 + 1
    StrCpy $1 $1 '' $2

    ; Both versions exhausted: equal.
    StrCmp $4$5 '' ayo_vcmp_equal

    ; Strip leading zeroes; $6/$7 count them (fewer zeroes = longer number).
    StrCpy $6 -1
    IntOp $6 $6 + 1
    StrCpy $3 $4 1 $6
    StrCmp $3 '0' -2
    StrCmp $3 '' 0 +3
    StrCpy $4 0
    StrCpy $6 0

    StrCpy $7 -1
    IntOp $7 $7 + 1
    StrCpy $3 $5 1 $7
    StrCmp $3 '0' -2
    StrCmp $3 '' 0 +3
    StrCpy $5 0
    StrCpy $7 0

    ; Zero segments first; otherwise compare digit counts, then the numbers.
    StrCmp $4 0 0 +2
    StrCmp $5 0 ayo_vcmp_begin ayo_vcmp_newer2
    StrCmp $5 0 ayo_vcmp_newer1
    IntCmp $6 $7 0 ayo_vcmp_newer1 ayo_vcmp_newer2

    StrCpy $4 '1$4'
    StrCpy $5 '1$5'
    IntCmp $4 $5 ayo_vcmp_begin ayo_vcmp_newer2 ayo_vcmp_newer1

  ayo_vcmp_equal:
    StrCpy $0 0
    Goto ayo_vcmp_end
  ayo_vcmp_newer1:
    StrCpy $0 1
    Goto ayo_vcmp_end
  ayo_vcmp_newer2:
    StrCpy $0 2

  ayo_vcmp_end:
    ; Put the result on top, then restore each saved register from underneath
    ; it. Exch + Pop leaves the result as the only value for the caller's Pop.
    Push $0
    Exch
    Pop $9
    Exch
    Pop $8
    Exch
    Pop $7
    Exch
    Pop $6
    Exch
    Pop $5
    Exch
    Pop $4
    Exch
    Pop $3
    Exch
    Pop $2
    Exch
    Pop $1
    Exch
    Pop $0
FunctionEnd
