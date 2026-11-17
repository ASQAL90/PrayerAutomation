; Generated from the technically reviewed Ayo Jama'ah! receipt. Normal UI/native acceptance PENDING. Do not substitute/rebuild runtime.
; GetSHA256OfFile is ISPP; mismatches stop compilation before packaging.
; Shared files are hash-checked against both source trees and installed once.
#if GetSHA256OfFile(MyAppSourceDirX64 + "\Accessibility.dll") != "88ed9af391301b919b254c321a7081c50e78485e57f1725878459322d3c3afdb"
  #error "Accepted payload mismatch (x64): Accessibility.dll"
#endif
Source: "{#MyAppSourceDirX64}\Accessibility.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\Accessibility.dll") != "86f2945485124f94affb2c840a03effc5009c12ae481685affc36ed76908035c"
  #error "Accepted payload mismatch (x86): Accessibility.dll"
#endif
Source: "{#MyAppSourceDirX86}\Accessibility.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\AyoJamaah.deps.json") != "f1adfca75bb86f8aa0a94cbb63aa62c04eeed11632d1b7e51d0e3e993a7e577d"
  #error "Accepted payload mismatch (x64): AyoJamaah.deps.json"
#endif
Source: "{#MyAppSourceDirX64}\AyoJamaah.deps.json"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\AyoJamaah.deps.json") != "5baf9da00e9d3af18e0ee4b42db3b36046010f9cc25c733e483b602f75b7b33a"
  #error "Accepted payload mismatch (x86): AyoJamaah.deps.json"
#endif
Source: "{#MyAppSourceDirX86}\AyoJamaah.deps.json"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\AyoJamaah.dll") != "c9ad7bcef8cfa233dde1de600be62d095d1624d79148c051260efc96b45f0078"
  #error "Accepted payload mismatch (x64): AyoJamaah.dll"
#endif
Source: "{#MyAppSourceDirX64}\AyoJamaah.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\AyoJamaah.dll") != "51bb84321107d47475ebbea4a2701297cb10fd720db45cbf795e8375cfc79f47"
  #error "Accepted payload mismatch (x86): AyoJamaah.dll"
#endif
Source: "{#MyAppSourceDirX86}\AyoJamaah.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\AyoJamaah.exe") != "c84feb92dff63a193b2caaa5d10fdaf66c03f2387941ef54c73a53c0c87f9035"
  #error "Accepted payload mismatch (x64): AyoJamaah.exe"
#endif
Source: "{#MyAppSourceDirX64}\AyoJamaah.exe"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\AyoJamaah.exe") != "412a490a885659e58037a8f28084f39b59dd7d1bca44069566fd79321774ad38"
  #error "Accepted payload mismatch (x86): AyoJamaah.exe"
#endif
Source: "{#MyAppSourceDirX86}\AyoJamaah.exe"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\AyoJamaah.ico") != "56027d17089ac6825494ebfe445f5c6ecde0e82a2ff39e7fd47c9ea593b2164c"
  #error "Accepted payload mismatch (x86): AyoJamaah.ico"
#endif
#if GetSHA256OfFile(MyAppSourceDirX64 + "\AyoJamaah.ico") != "56027d17089ac6825494ebfe445f5c6ecde0e82a2ff39e7fd47c9ea593b2164c"
  #error "Accepted payload mismatch (x64): AyoJamaah.ico"
#endif
Source: "{#MyAppSourceDirX64}\AyoJamaah.ico"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDirX64 + "\AyoJamaah.pdb") != "57455b1afecb366433edd33d8f22a0807fa033f7eecedfffe32f0bf381352d86"
  #error "Accepted payload mismatch (x64): AyoJamaah.pdb"
#endif
Source: "{#MyAppSourceDirX64}\AyoJamaah.pdb"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\AyoJamaah.pdb") != "fe99c326b0e3676292b6c5236576a25802aa4aab11ec68bd1d9a32b394a676c3"
  #error "Accepted payload mismatch (x86): AyoJamaah.pdb"
#endif
Source: "{#MyAppSourceDirX86}\AyoJamaah.pdb"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\AyoJamaah.runtimeconfig.json") != "67ca0e73a51958ed4b19f1b6536059f3646e760a1e9c795e8e481a4fd9413735"
  #error "Accepted payload mismatch (x86): AyoJamaah.runtimeconfig.json"
#endif
#if GetSHA256OfFile(MyAppSourceDirX64 + "\AyoJamaah.runtimeconfig.json") != "67ca0e73a51958ed4b19f1b6536059f3646e760a1e9c795e8e481a4fd9413735"
  #error "Accepted payload mismatch (x64): AyoJamaah.runtimeconfig.json"
#endif
Source: "{#MyAppSourceDirX64}\AyoJamaah.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDirX64 + "\D3DCompiler_47_cor3.dll") != "a05f99734f7c4822fefc12b367af21fd0976ed6608752fb1e1e80b6ece7ecbbb"
  #error "Accepted payload mismatch (x64): D3DCompiler_47_cor3.dll"
#endif
Source: "{#MyAppSourceDirX64}\D3DCompiler_47_cor3.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\D3DCompiler_47_cor3.dll") != "0ae6e52072857c3acfde53b98f56c3c6d9bd0e7b09f247ae7f40bfd451593c4c"
  #error "Accepted payload mismatch (x86): D3DCompiler_47_cor3.dll"
#endif
Source: "{#MyAppSourceDirX86}\D3DCompiler_47_cor3.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\DirectWriteForwarder.dll") != "43d9d2ac0ba4f67b5e0a1bf08f5e247ed44badf5227b1b3dd85b2bf962c7ca24"
  #error "Accepted payload mismatch (x64): DirectWriteForwarder.dll"
#endif
Source: "{#MyAppSourceDirX64}\DirectWriteForwarder.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\DirectWriteForwarder.dll") != "e7a8f12468b0f5d342618f0b81df1357a196ff24393d1f7d4ba0f14ba8e7e8d8"
  #error "Accepted payload mismatch (x86): DirectWriteForwarder.dll"
#endif
Source: "{#MyAppSourceDirX86}\DirectWriteForwarder.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\GeoTimeZone.dll") != "8f0ca2647579c95c35bb9894a484b230f01077e34608955be4015cc2057457b2"
  #error "Accepted payload mismatch (x86): GeoTimeZone.dll"
#endif
#if GetSHA256OfFile(MyAppSourceDirX64 + "\GeoTimeZone.dll") != "8f0ca2647579c95c35bb9894a484b230f01077e34608955be4015cc2057457b2"
  #error "Accepted payload mismatch (x64): GeoTimeZone.dll"
#endif
Source: "{#MyAppSourceDirX64}\GeoTimeZone.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDirX64 + "\Microsoft.CSharp.dll") != "b48d497a3ec9ef5cf7fd1eeaf01e6f77673d8852c9c04a25b3b513b09e1f1c0b"
  #error "Accepted payload mismatch (x64): Microsoft.CSharp.dll"
#endif
Source: "{#MyAppSourceDirX64}\Microsoft.CSharp.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\Microsoft.CSharp.dll") != "d0db83a0474f4f7c96e83ef17ac960113421ecd9f2aed028ef7fa5e2e473e11a"
  #error "Accepted payload mismatch (x86): Microsoft.CSharp.dll"
#endif
Source: "{#MyAppSourceDirX86}\Microsoft.CSharp.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\Microsoft.DiaSymReader.Native.amd64.dll") != "ad3e85687b8d2d2eb09d1f097fdf7095372dbb044db5b64179f2d4b8024fed2b"
  #error "Accepted payload mismatch (x64): Microsoft.DiaSymReader.Native.amd64.dll"
#endif
Source: "{#MyAppSourceDirX64}\Microsoft.DiaSymReader.Native.amd64.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\Microsoft.DiaSymReader.Native.x86.dll") != "34328fbc265b5ffc58147fdfb7b181bbec71071d56634ae5db50136c59d961e9"
  #error "Accepted payload mismatch (x86): Microsoft.DiaSymReader.Native.x86.dll"
#endif
Source: "{#MyAppSourceDirX86}\Microsoft.DiaSymReader.Native.x86.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\Microsoft.VisualBasic.Core.dll") != "c22b26a9b48eea35ea5411e97208d82b0d5b247701e2be676bf61a1db0952672"
  #error "Accepted payload mismatch (x64): Microsoft.VisualBasic.Core.dll"
#endif
Source: "{#MyAppSourceDirX64}\Microsoft.VisualBasic.Core.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\Microsoft.VisualBasic.Core.dll") != "ffda7dccf606282b0495fe0c48fb518db5f4b90d62b666265c4bcc700b5817fe"
  #error "Accepted payload mismatch (x86): Microsoft.VisualBasic.Core.dll"
#endif
Source: "{#MyAppSourceDirX86}\Microsoft.VisualBasic.Core.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\Microsoft.VisualBasic.Forms.dll") != "9ec2b355dc54744ad583b95301e7b98e16a7398fd73a91e09da141cd4338f235"
  #error "Accepted payload mismatch (x64): Microsoft.VisualBasic.Forms.dll"
#endif
Source: "{#MyAppSourceDirX64}\Microsoft.VisualBasic.Forms.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\Microsoft.VisualBasic.Forms.dll") != "d25163e9fbdc927e1474e15fee541db98796d18fd9fec6c17a037ae1764d2e5c"
  #error "Accepted payload mismatch (x86): Microsoft.VisualBasic.Forms.dll"
#endif
Source: "{#MyAppSourceDirX86}\Microsoft.VisualBasic.Forms.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\Microsoft.VisualBasic.dll") != "563423f7703eb2cebfe739eebe82a664f2e251cf4958db498e8f9f927858fd95"
  #error "Accepted payload mismatch (x64): Microsoft.VisualBasic.dll"
#endif
Source: "{#MyAppSourceDirX64}\Microsoft.VisualBasic.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\Microsoft.VisualBasic.dll") != "d747329c9fddf57535df8d03f4c05c8c578e46546c9338a684e6bf40c7179b86"
  #error "Accepted payload mismatch (x86): Microsoft.VisualBasic.dll"
#endif
Source: "{#MyAppSourceDirX86}\Microsoft.VisualBasic.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\Microsoft.Win32.Primitives.dll") != "01e04c9bae28f15661d0da411e959e305ef6d0d78a28eb938b3322e0473e0018"
  #error "Accepted payload mismatch (x64): Microsoft.Win32.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX64}\Microsoft.Win32.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\Microsoft.Win32.Primitives.dll") != "ce52c258c90d809956f3d60020208c0a6eb8ec58f8635c3d0695067d8e2898c4"
  #error "Accepted payload mismatch (x86): Microsoft.Win32.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX86}\Microsoft.Win32.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\Microsoft.Win32.Registry.AccessControl.dll") != "4b1a0ca7289b0fac3c3b2271c136e15c36d99d48a25dd8df838cf61bad4553c6"
  #error "Accepted payload mismatch (x64): Microsoft.Win32.Registry.AccessControl.dll"
#endif
Source: "{#MyAppSourceDirX64}\Microsoft.Win32.Registry.AccessControl.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\Microsoft.Win32.Registry.AccessControl.dll") != "9ebeb53f68eb4a203ab546917f006063c9c7cafa2a36e55fdc5ee1af3b95bb3a"
  #error "Accepted payload mismatch (x86): Microsoft.Win32.Registry.AccessControl.dll"
#endif
Source: "{#MyAppSourceDirX86}\Microsoft.Win32.Registry.AccessControl.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\Microsoft.Win32.Registry.dll") != "0d904208ba30d8808a4bbedd8e000bacf1a56417f6f725fc144f81a0c1d6097a"
  #error "Accepted payload mismatch (x64): Microsoft.Win32.Registry.dll"
#endif
Source: "{#MyAppSourceDirX64}\Microsoft.Win32.Registry.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\Microsoft.Win32.Registry.dll") != "da9bcd404454a47528cf7db0065c41631e9b7ce2fed188bf263ab37536481d6c"
  #error "Accepted payload mismatch (x86): Microsoft.Win32.Registry.dll"
#endif
Source: "{#MyAppSourceDirX86}\Microsoft.Win32.Registry.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\Microsoft.Win32.SystemEvents.dll") != "ca7c2be566d070f4e8d61058dbb662c5fa17164b56afef51b2a4b1046629358a"
  #error "Accepted payload mismatch (x64): Microsoft.Win32.SystemEvents.dll"
#endif
Source: "{#MyAppSourceDirX64}\Microsoft.Win32.SystemEvents.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\Microsoft.Win32.SystemEvents.dll") != "068b71c10eb45747a76a80bd9616242e1c66115a48725d92079817ce42d1aca9"
  #error "Accepted payload mismatch (x86): Microsoft.Win32.SystemEvents.dll"
#endif
Source: "{#MyAppSourceDirX86}\Microsoft.Win32.SystemEvents.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\Microsoft.Windows.SDK.NET.dll") != "c633ab241cd09846c2aa409f0bee2026962610404a7bf132c75cbd66b0e5a9f4"
  #error "Accepted payload mismatch (x86): Microsoft.Windows.SDK.NET.dll"
#endif
#if GetSHA256OfFile(MyAppSourceDirX64 + "\Microsoft.Windows.SDK.NET.dll") != "c633ab241cd09846c2aa409f0bee2026962610404a7bf132c75cbd66b0e5a9f4"
  #error "Accepted payload mismatch (x64): Microsoft.Windows.SDK.NET.dll"
#endif
Source: "{#MyAppSourceDirX64}\Microsoft.Windows.SDK.NET.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDirX64 + "\PenImc_cor3.dll") != "7abbbf1f9c64c08e69dee3340b56623ff4809297bcba42d7c92fb3c3decf11d0"
  #error "Accepted payload mismatch (x64): PenImc_cor3.dll"
#endif
Source: "{#MyAppSourceDirX64}\PenImc_cor3.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\PenImc_cor3.dll") != "b6adce49a237ca78af2eb5fbeb1c6ae1ac6fdbc87640a8a3c51e9138c42cd227"
  #error "Accepted payload mismatch (x86): PenImc_cor3.dll"
#endif
Source: "{#MyAppSourceDirX86}\PenImc_cor3.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\PresentationCore.dll") != "dd98b4b78def920fc60026ad6ff84237a729fe0a698237107aec625043a69ff7"
  #error "Accepted payload mismatch (x64): PresentationCore.dll"
#endif
Source: "{#MyAppSourceDirX64}\PresentationCore.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\PresentationCore.dll") != "e3ef6a0b3d4635078010902a26800b5f26e5ccafa102f39480956048411e5351"
  #error "Accepted payload mismatch (x86): PresentationCore.dll"
#endif
Source: "{#MyAppSourceDirX86}\PresentationCore.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\PresentationFramework-SystemCore.dll") != "8edb4073d02e3c3b488dfeed1467e4c16c350a2f51c3711e15cab212c9537381"
  #error "Accepted payload mismatch (x64): PresentationFramework-SystemCore.dll"
#endif
Source: "{#MyAppSourceDirX64}\PresentationFramework-SystemCore.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\PresentationFramework-SystemCore.dll") != "35c37e7441241679e993d11b7db85380adb31bd11178133d1c7354b0c5574c4b"
  #error "Accepted payload mismatch (x86): PresentationFramework-SystemCore.dll"
#endif
Source: "{#MyAppSourceDirX86}\PresentationFramework-SystemCore.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\PresentationFramework-SystemData.dll") != "a5a1138e8556f714c61320f76d336f985c363a72234d605bab8e66b36c99b0da"
  #error "Accepted payload mismatch (x64): PresentationFramework-SystemData.dll"
#endif
Source: "{#MyAppSourceDirX64}\PresentationFramework-SystemData.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\PresentationFramework-SystemData.dll") != "73d4523e6cde08032f14b670e538382614d9873c85b3d6aeab262367c680d70d"
  #error "Accepted payload mismatch (x86): PresentationFramework-SystemData.dll"
#endif
Source: "{#MyAppSourceDirX86}\PresentationFramework-SystemData.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\PresentationFramework-SystemDrawing.dll") != "8b8e3881b31b7b6042fb416dddb77f32c9e7bc39f79e22143b01c769e0595663"
  #error "Accepted payload mismatch (x64): PresentationFramework-SystemDrawing.dll"
#endif
Source: "{#MyAppSourceDirX64}\PresentationFramework-SystemDrawing.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\PresentationFramework-SystemDrawing.dll") != "25708048d67f75b80e76a78cf4d281880670903af58617b6c3b2d2fba5a8e756"
  #error "Accepted payload mismatch (x86): PresentationFramework-SystemDrawing.dll"
#endif
Source: "{#MyAppSourceDirX86}\PresentationFramework-SystemDrawing.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\PresentationFramework-SystemXml.dll") != "980c50cb01ae8a805b0396dccd1c858fb241a3e6fed1fdae0f1813a62b125bc4"
  #error "Accepted payload mismatch (x64): PresentationFramework-SystemXml.dll"
#endif
Source: "{#MyAppSourceDirX64}\PresentationFramework-SystemXml.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\PresentationFramework-SystemXml.dll") != "c6969d9815afe3c05b7b0bdfe2a9e01ed8e0718c058db90a6e06b9b5dfe49827"
  #error "Accepted payload mismatch (x86): PresentationFramework-SystemXml.dll"
#endif
Source: "{#MyAppSourceDirX86}\PresentationFramework-SystemXml.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\PresentationFramework-SystemXmlLinq.dll") != "1c16720792cf6221c6b659d7fe3ef536695d4064f2f18d99846a862a5d317315"
  #error "Accepted payload mismatch (x64): PresentationFramework-SystemXmlLinq.dll"
#endif
Source: "{#MyAppSourceDirX64}\PresentationFramework-SystemXmlLinq.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\PresentationFramework-SystemXmlLinq.dll") != "10c60e0d5564504cdaad87d446403cd86e96947a6cb00b83836f33646177e1d6"
  #error "Accepted payload mismatch (x86): PresentationFramework-SystemXmlLinq.dll"
#endif
Source: "{#MyAppSourceDirX86}\PresentationFramework-SystemXmlLinq.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\PresentationFramework.Aero.dll") != "3bd6ab48e3d628cc39c66de139e72fdcc981d504496403bcdd23ee29a696654b"
  #error "Accepted payload mismatch (x64): PresentationFramework.Aero.dll"
#endif
Source: "{#MyAppSourceDirX64}\PresentationFramework.Aero.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\PresentationFramework.Aero.dll") != "92b6b6e95fe9ce9067028abafa7cdc9dffaaa4e0e7fd51b69d4f6a8f30a6e3db"
  #error "Accepted payload mismatch (x86): PresentationFramework.Aero.dll"
#endif
Source: "{#MyAppSourceDirX86}\PresentationFramework.Aero.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\PresentationFramework.Aero2.dll") != "d09d44eafcbc49c647feeae25661d3052acb54ac7d07698ff714a76819a5d127"
  #error "Accepted payload mismatch (x64): PresentationFramework.Aero2.dll"
#endif
Source: "{#MyAppSourceDirX64}\PresentationFramework.Aero2.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\PresentationFramework.Aero2.dll") != "5ab2ddea5b6a18fa81fb527917e14a03ac502e2500f3554d86601531d26ba417"
  #error "Accepted payload mismatch (x86): PresentationFramework.Aero2.dll"
#endif
Source: "{#MyAppSourceDirX86}\PresentationFramework.Aero2.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\PresentationFramework.AeroLite.dll") != "d0ff5ad7b8610fe20b54264956c8b47825d1a463ef7aac04f7023004657c3406"
  #error "Accepted payload mismatch (x64): PresentationFramework.AeroLite.dll"
#endif
Source: "{#MyAppSourceDirX64}\PresentationFramework.AeroLite.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\PresentationFramework.AeroLite.dll") != "df29c8ebbc00a35b64aae3748c7cf7c7959fb13034f16efd920cca5b0f8bafdd"
  #error "Accepted payload mismatch (x86): PresentationFramework.AeroLite.dll"
#endif
Source: "{#MyAppSourceDirX86}\PresentationFramework.AeroLite.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\PresentationFramework.Classic.dll") != "4fdb51fa0bb95702baf7538a89e1338ecfa645b71283ea8e415f59409528f089"
  #error "Accepted payload mismatch (x64): PresentationFramework.Classic.dll"
#endif
Source: "{#MyAppSourceDirX64}\PresentationFramework.Classic.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\PresentationFramework.Classic.dll") != "0fde53fa67a4efb9f9179a2ee9dd2d8199ccd340fa1d66077176f053bb026cba"
  #error "Accepted payload mismatch (x86): PresentationFramework.Classic.dll"
#endif
Source: "{#MyAppSourceDirX86}\PresentationFramework.Classic.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\PresentationFramework.Fluent.dll") != "a7b4967a1dbb49ca618b432f7882e0463d24b2e141c26e11deae3589d17d5a13"
  #error "Accepted payload mismatch (x64): PresentationFramework.Fluent.dll"
#endif
Source: "{#MyAppSourceDirX64}\PresentationFramework.Fluent.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\PresentationFramework.Fluent.dll") != "c4523a619212fc184c3d8bce366ef890278d12de199342508e798de9128d0b60"
  #error "Accepted payload mismatch (x86): PresentationFramework.Fluent.dll"
#endif
Source: "{#MyAppSourceDirX86}\PresentationFramework.Fluent.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\PresentationFramework.Luna.dll") != "18b18e9ed3fb501d5bed1a1c0f733417e719c2898ba59721244cbcf981686bae"
  #error "Accepted payload mismatch (x64): PresentationFramework.Luna.dll"
#endif
Source: "{#MyAppSourceDirX64}\PresentationFramework.Luna.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\PresentationFramework.Luna.dll") != "e2ead5328150791b3846ec6fa3a313c6f97d3c3f44d5b544b9482f54de2e016a"
  #error "Accepted payload mismatch (x86): PresentationFramework.Luna.dll"
#endif
Source: "{#MyAppSourceDirX86}\PresentationFramework.Luna.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\PresentationFramework.Royale.dll") != "203e7094b7dafd3f367563206f8733976d486d9ca368d318eae423ffeb02b16b"
  #error "Accepted payload mismatch (x64): PresentationFramework.Royale.dll"
#endif
Source: "{#MyAppSourceDirX64}\PresentationFramework.Royale.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\PresentationFramework.Royale.dll") != "82587bdb31695d6fcd8c1d37bf8ebb1cc564a866d24615ab06995d14671a6aa8"
  #error "Accepted payload mismatch (x86): PresentationFramework.Royale.dll"
#endif
Source: "{#MyAppSourceDirX86}\PresentationFramework.Royale.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\PresentationFramework.dll") != "98fc9302e6d4c5116e6dfa27b5c88e373c13941ba3515f85c4520b86164d0883"
  #error "Accepted payload mismatch (x64): PresentationFramework.dll"
#endif
Source: "{#MyAppSourceDirX64}\PresentationFramework.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\PresentationFramework.dll") != "4e83f9f82f69fe1fc9af34968ae4d9c2308f054911dcb9aab0a1caf017ce3538"
  #error "Accepted payload mismatch (x86): PresentationFramework.dll"
#endif
Source: "{#MyAppSourceDirX86}\PresentationFramework.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\PresentationNative_cor3.dll") != "931ae412f521932b91db1fac7a10c1a5690721b08a5f6c86b767b5855f8b57db"
  #error "Accepted payload mismatch (x64): PresentationNative_cor3.dll"
#endif
Source: "{#MyAppSourceDirX64}\PresentationNative_cor3.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\PresentationNative_cor3.dll") != "033ce728870691845f04da40f1e6b2d760f0f0dc33d6b88abdea7d618926fccf"
  #error "Accepted payload mismatch (x86): PresentationNative_cor3.dll"
#endif
Source: "{#MyAppSourceDirX86}\PresentationNative_cor3.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\PresentationUI.dll") != "89f61ea3fe70dbf0e8b49891acf16e1df70ec5629f758d033b819d52fc07df98"
  #error "Accepted payload mismatch (x64): PresentationUI.dll"
#endif
Source: "{#MyAppSourceDirX64}\PresentationUI.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\PresentationUI.dll") != "4faecead83604378369143ec7adeb8e298166764a1c959d370ab0233bc65bb66"
  #error "Accepted payload mismatch (x86): PresentationUI.dll"
#endif
Source: "{#MyAppSourceDirX86}\PresentationUI.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ReachFramework.dll") != "9781bba218ac677414787441f71f83c98e1d6361f4fd2a7c627f674ebc7b8987"
  #error "Accepted payload mismatch (x64): ReachFramework.dll"
#endif
Source: "{#MyAppSourceDirX64}\ReachFramework.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ReachFramework.dll") != "03c2c778a94f00e99d722dfdd1f0389f48949981410793471c73533ba0e7baa1"
  #error "Accepted payload mismatch (x86): ReachFramework.dll"
#endif
Source: "{#MyAppSourceDirX86}\ReachFramework.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.AppContext.dll") != "5824233c5a6129f5741e1e0bd14f60a58a38600bac384dc8b3cd09f969d67543"
  #error "Accepted payload mismatch (x64): System.AppContext.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.AppContext.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.AppContext.dll") != "b68931ea2307824b66d3fa436c483601e2cd910da0aa88dea9846e4585f8a4da"
  #error "Accepted payload mismatch (x86): System.AppContext.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.AppContext.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Buffers.dll") != "b569db17c9a66433310a3b71a86157f025ae573f60d4bb4e20e5d1c3cac94896"
  #error "Accepted payload mismatch (x64): System.Buffers.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Buffers.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Buffers.dll") != "732954e0bad63d92f58232bc1f49325e6a0c2004bc49c8d6cb110c6918d34155"
  #error "Accepted payload mismatch (x86): System.Buffers.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Buffers.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.CodeDom.dll") != "011438b9d99d189ddecfaeb6f5439a447a186d6c8f90c1ae20ffea8e707ce01d"
  #error "Accepted payload mismatch (x64): System.CodeDom.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.CodeDom.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.CodeDom.dll") != "1397cb43fd5bf390c74734a12759b276a7735d4617ad8c405f603a0241351fdd"
  #error "Accepted payload mismatch (x86): System.CodeDom.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.CodeDom.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Collections.Concurrent.dll") != "00dcf6500d5a0796ec5878e5cff60f15bba20bae3c6d574e0a194e4c2089839c"
  #error "Accepted payload mismatch (x64): System.Collections.Concurrent.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Collections.Concurrent.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Collections.Concurrent.dll") != "37159fc4cbfb46af73101dfa9d633f63e7e811c2f16def867e27f4b760ea12a9"
  #error "Accepted payload mismatch (x86): System.Collections.Concurrent.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Collections.Concurrent.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Collections.Immutable.dll") != "9bf7f6c680bbb73d4412a3319368ddb8e7b58c6a6358802b738e3c2cdc5574d2"
  #error "Accepted payload mismatch (x64): System.Collections.Immutable.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Collections.Immutable.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Collections.Immutable.dll") != "cc0cf5712cf7734ef9255b7d48f1101ddca521f69b72320a4fb59b4f2bbc148f"
  #error "Accepted payload mismatch (x86): System.Collections.Immutable.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Collections.Immutable.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Collections.NonGeneric.dll") != "61bb3ddb69a1649ab3a77d5c9680532e41cf0596d347fdb9e43cb4729b2a6cdd"
  #error "Accepted payload mismatch (x64): System.Collections.NonGeneric.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Collections.NonGeneric.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Collections.NonGeneric.dll") != "20a25aef351a132bb6d20d5bb6c5fdd85f47f6d9aedfb375db8e768239cde71f"
  #error "Accepted payload mismatch (x86): System.Collections.NonGeneric.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Collections.NonGeneric.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Collections.Specialized.dll") != "1340913256353e85e0032196d5ba93507a190cd252686ffc19d5a5fc6afbb3d4"
  #error "Accepted payload mismatch (x64): System.Collections.Specialized.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Collections.Specialized.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Collections.Specialized.dll") != "8457b15d83478cacecfd16fa59eb5d9750beec3f7f4588d37cd40285ed164f2f"
  #error "Accepted payload mismatch (x86): System.Collections.Specialized.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Collections.Specialized.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Collections.dll") != "07dedb131e9d8707a8025669f2b394ce512be868817fd47bc107dc9e8e4645d3"
  #error "Accepted payload mismatch (x64): System.Collections.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Collections.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Collections.dll") != "c6a3bb6ae72ab9bc3e508239818af9afce963e4b3ec61ad480d5e5383170f344"
  #error "Accepted payload mismatch (x86): System.Collections.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Collections.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.ComponentModel.Annotations.dll") != "b3321a78a4d201096915ef125dd71579e88ae7e91dc8717245710f2dd2588476"
  #error "Accepted payload mismatch (x64): System.ComponentModel.Annotations.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.ComponentModel.Annotations.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.ComponentModel.Annotations.dll") != "eae1443abe75d4f538a8242aaacff7dd172e2fa0f931d676f87df494d69d6d65"
  #error "Accepted payload mismatch (x86): System.ComponentModel.Annotations.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.ComponentModel.Annotations.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.ComponentModel.DataAnnotations.dll") != "0b47479c27b050e90cc0c232561781f71a85df75686758eff459890e73045d81"
  #error "Accepted payload mismatch (x64): System.ComponentModel.DataAnnotations.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.ComponentModel.DataAnnotations.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.ComponentModel.DataAnnotations.dll") != "0d33d2eb4eae03da02460ad28a7885aa38c374e139cbb912b218d055566cd18f"
  #error "Accepted payload mismatch (x86): System.ComponentModel.DataAnnotations.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.ComponentModel.DataAnnotations.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.ComponentModel.EventBasedAsync.dll") != "0debe0eb6a38fb8f92ec7b58df01c742abb300ac71679e17a6bd42a9740499f2"
  #error "Accepted payload mismatch (x64): System.ComponentModel.EventBasedAsync.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.ComponentModel.EventBasedAsync.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.ComponentModel.EventBasedAsync.dll") != "c33b055190492a8e8819121d77e1f411afe9e030d1dd4702144c36db50415de8"
  #error "Accepted payload mismatch (x86): System.ComponentModel.EventBasedAsync.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.ComponentModel.EventBasedAsync.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.ComponentModel.Primitives.dll") != "088544116f6c6fc7c2d06ed33d580a0143fcea73c1f14aa9b83e54d441f6e2ec"
  #error "Accepted payload mismatch (x64): System.ComponentModel.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.ComponentModel.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.ComponentModel.Primitives.dll") != "4b888ecd7dcb371a64c59c3d42525f01fef1311332bee8bb8a1d8f4bf09b7a54"
  #error "Accepted payload mismatch (x86): System.ComponentModel.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.ComponentModel.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.ComponentModel.TypeConverter.dll") != "cb58d0133e1db4e85ff6502a55ac80160b7f06bef147b3970d01926075d0b588"
  #error "Accepted payload mismatch (x64): System.ComponentModel.TypeConverter.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.ComponentModel.TypeConverter.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.ComponentModel.TypeConverter.dll") != "3a98766f8fb65b5b69a537b2933a82a3ee13de7c7091363b99c8a7671233b766"
  #error "Accepted payload mismatch (x86): System.ComponentModel.TypeConverter.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.ComponentModel.TypeConverter.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.ComponentModel.dll") != "e6a85ff45d7ea39b809610331d1350275515ff840bc2cec5a4d352f76a000f8f"
  #error "Accepted payload mismatch (x64): System.ComponentModel.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.ComponentModel.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.ComponentModel.dll") != "ebe3de9484f30a680cf7874ca04d701c570f6dea67a6fbf5ee3568e6dcc3eb86"
  #error "Accepted payload mismatch (x86): System.ComponentModel.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.ComponentModel.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Configuration.ConfigurationManager.dll") != "9a1e33236d4a74c76241a91acba60522fc2e10e4f6dd8753c1c756e1a761fc92"
  #error "Accepted payload mismatch (x64): System.Configuration.ConfigurationManager.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Configuration.ConfigurationManager.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Configuration.ConfigurationManager.dll") != "3533c57f3bdcd459b5efde7d90f2075e958e196b709b45f598ef4413f29f59ce"
  #error "Accepted payload mismatch (x86): System.Configuration.ConfigurationManager.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Configuration.ConfigurationManager.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Configuration.dll") != "1f7c4f09bd5eebb988c7f6b16db293260a7ffdaf30e407ee44d1e492e83ae78f"
  #error "Accepted payload mismatch (x64): System.Configuration.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Configuration.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Configuration.dll") != "6c4c19dbd2aebc7f2f316dbbecffa7c540cac331ba9022776466d0bb6390b894"
  #error "Accepted payload mismatch (x86): System.Configuration.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Configuration.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Console.dll") != "e406f27a12fabb43798d5b32238ab1c34ee10739102c150eee50bc646a51e042"
  #error "Accepted payload mismatch (x64): System.Console.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Console.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Console.dll") != "264be9491d36cde5c5b4639fd7d568e7d3e44de2af8aa803c50a005e78c6e472"
  #error "Accepted payload mismatch (x86): System.Console.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Console.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Core.dll") != "7828fa13325f9b97ddd8d8e6dcb1cc54de1afc5fb2dc69a29f86ca686fb8e04c"
  #error "Accepted payload mismatch (x64): System.Core.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Core.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Core.dll") != "aed5cc211fd99f473bda6a0a925b888ac3d0bc0bb8a0415b8fc77fdd8355a216"
  #error "Accepted payload mismatch (x86): System.Core.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Core.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Data.Common.dll") != "92f99d39c9df497047c2a1a5ed126a5025e79d03908170be6dc6505453a27a1d"
  #error "Accepted payload mismatch (x64): System.Data.Common.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Data.Common.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Data.Common.dll") != "e48a2f06380ed3c4a668e19ee6cb98b40b5b5047bf5f2aa8b5fdedf6ad26c6f6"
  #error "Accepted payload mismatch (x86): System.Data.Common.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Data.Common.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Data.DataSetExtensions.dll") != "f7dec6629d252f03e1c90e0a2ead24e482a2a24914101710391aba6dc8a453c7"
  #error "Accepted payload mismatch (x64): System.Data.DataSetExtensions.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Data.DataSetExtensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Data.DataSetExtensions.dll") != "fcb8b3235c7dd9e33422721a0738733b32b64b3c3826b4b0c82477642744fba1"
  #error "Accepted payload mismatch (x86): System.Data.DataSetExtensions.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Data.DataSetExtensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Data.dll") != "5f1d2575f0a94826b2cc722be1548929a2756a5d9f8009c3e2235da400fddd9f"
  #error "Accepted payload mismatch (x64): System.Data.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Data.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Data.dll") != "3f591bc51b05888db3c4f75f79dd391f32faa514269cf7ff897fd4817fd8b2cb"
  #error "Accepted payload mismatch (x86): System.Data.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Data.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Design.dll") != "38e1692b3a318305e414e1a90ddfc50c31f7e94b2f359507e95c8ee2ff865f31"
  #error "Accepted payload mismatch (x64): System.Design.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Design.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Design.dll") != "339729b73e8ac741c6ff757d85ec1e9294242df21904e2e95aaaa3205d73a130"
  #error "Accepted payload mismatch (x86): System.Design.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Design.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Diagnostics.Contracts.dll") != "7d564319c9c214c9b0c023fa3152242a1cb57b1ba1859a4b04dadc78c332d51e"
  #error "Accepted payload mismatch (x64): System.Diagnostics.Contracts.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Diagnostics.Contracts.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Diagnostics.Contracts.dll") != "19bd70cbb52c85e4b5d97a76204cc55987a62870f203058968bfc037d0d333ec"
  #error "Accepted payload mismatch (x86): System.Diagnostics.Contracts.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Diagnostics.Contracts.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Diagnostics.Debug.dll") != "54340793a5d2f86aedc1573f0c438bb53ff810c13ad839362fe153a560acb9bc"
  #error "Accepted payload mismatch (x64): System.Diagnostics.Debug.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Diagnostics.Debug.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Diagnostics.Debug.dll") != "e88bc056355388edda600d579cdb0cad2b1a799de792c251443c922f2e5e98eb"
  #error "Accepted payload mismatch (x86): System.Diagnostics.Debug.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Diagnostics.Debug.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Diagnostics.DiagnosticSource.dll") != "8048c2af87406c3573d09c78696171ca6f3bb155fac3f7684e01406bba61e700"
  #error "Accepted payload mismatch (x64): System.Diagnostics.DiagnosticSource.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Diagnostics.DiagnosticSource.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Diagnostics.DiagnosticSource.dll") != "2acd6eb3030f0abce33deaa4a51dd2a9ac543111eedf1ff4f06f682073b7e5d4"
  #error "Accepted payload mismatch (x86): System.Diagnostics.DiagnosticSource.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Diagnostics.DiagnosticSource.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Diagnostics.EventLog.Messages.dll") != "c03238dc0843945462d9ec0abb80478eafbcb71e94e254dfddf15854d1dd1233"
  #error "Accepted payload mismatch (x64): System.Diagnostics.EventLog.Messages.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Diagnostics.EventLog.Messages.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Diagnostics.EventLog.Messages.dll") != "baf18ca8bd30595a82d543432583ca95f848c41e42d3df2367d1531f8105a584"
  #error "Accepted payload mismatch (x86): System.Diagnostics.EventLog.Messages.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Diagnostics.EventLog.Messages.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Diagnostics.EventLog.dll") != "963a6784919c68f7578a6d1763520bc80dce329143b2fd6e18d972845c87cc56"
  #error "Accepted payload mismatch (x64): System.Diagnostics.EventLog.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Diagnostics.EventLog.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Diagnostics.EventLog.dll") != "0066e405aa91510de6e9951c8a5ca9e65cbd3e6665180f27b31a2b64a48fb362"
  #error "Accepted payload mismatch (x86): System.Diagnostics.EventLog.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Diagnostics.EventLog.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Diagnostics.FileVersionInfo.dll") != "16257d9fd42996b0e12e7a0a2da8dcebaf05cc75ca6cf33e591790a10a69598a"
  #error "Accepted payload mismatch (x64): System.Diagnostics.FileVersionInfo.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Diagnostics.FileVersionInfo.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Diagnostics.FileVersionInfo.dll") != "e1c286b0eaaf8e022ecf703b5c4d42ce4fed535ce63cfcfa9636c574faa1db48"
  #error "Accepted payload mismatch (x86): System.Diagnostics.FileVersionInfo.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Diagnostics.FileVersionInfo.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Diagnostics.PerformanceCounter.dll") != "bd12a3657af54e66cd26d2dd7709ed3e040c9365d2798fe58bae9cc954558d6b"
  #error "Accepted payload mismatch (x64): System.Diagnostics.PerformanceCounter.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Diagnostics.PerformanceCounter.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Diagnostics.PerformanceCounter.dll") != "2f43c83c22801dddea7471f285a002f586d91b04a3da53fc7d8390f20851d56e"
  #error "Accepted payload mismatch (x86): System.Diagnostics.PerformanceCounter.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Diagnostics.PerformanceCounter.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Diagnostics.Process.dll") != "126bd07fb3a59d1880d1d5ac11ade6e161932535eb5104550c4eba1d0abbfef0"
  #error "Accepted payload mismatch (x64): System.Diagnostics.Process.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Diagnostics.Process.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Diagnostics.Process.dll") != "d000b0f5744f409c58dfd95ec805dce1921f0f75c8567d127650c2d95d0c5f2f"
  #error "Accepted payload mismatch (x86): System.Diagnostics.Process.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Diagnostics.Process.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Diagnostics.StackTrace.dll") != "d1e815e1fa1aebdb63bab5c1878ddba84dd4c0e837545ff83d593620e054cca4"
  #error "Accepted payload mismatch (x64): System.Diagnostics.StackTrace.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Diagnostics.StackTrace.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Diagnostics.StackTrace.dll") != "b53182189cac4af4dc74ef33974c84317035485d4029549c85d40cbbe3b8b98a"
  #error "Accepted payload mismatch (x86): System.Diagnostics.StackTrace.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Diagnostics.StackTrace.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Diagnostics.TextWriterTraceListener.dll") != "a2cc28d05d8b7a2f0d224b6c9a77d9a546d69029aad266a1a6ef33c4dde33f4f"
  #error "Accepted payload mismatch (x64): System.Diagnostics.TextWriterTraceListener.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Diagnostics.TextWriterTraceListener.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Diagnostics.TextWriterTraceListener.dll") != "17bdbe7cf3019714c81becef55081092a9a3cd472746a39dbd3b59fdb6434ef0"
  #error "Accepted payload mismatch (x86): System.Diagnostics.TextWriterTraceListener.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Diagnostics.TextWriterTraceListener.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Diagnostics.Tools.dll") != "a39e302a28f1f35cd9cd975c52358c7796330080920ddc4dcd3b336760b769fc"
  #error "Accepted payload mismatch (x64): System.Diagnostics.Tools.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Diagnostics.Tools.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Diagnostics.Tools.dll") != "91d1b7feece1697c21064945e934cc0b4a1e60e372cb455af229fa24b90003f5"
  #error "Accepted payload mismatch (x86): System.Diagnostics.Tools.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Diagnostics.Tools.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Diagnostics.TraceSource.dll") != "b2663087d8cf28ca42851a67213a44b6d608f5676e2e0dda0b1da14f371da723"
  #error "Accepted payload mismatch (x64): System.Diagnostics.TraceSource.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Diagnostics.TraceSource.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Diagnostics.TraceSource.dll") != "db3e7fdc7d5c52987a6dd22e2ddbad1baeda9b1dc391c23c151bebbed6f55a82"
  #error "Accepted payload mismatch (x86): System.Diagnostics.TraceSource.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Diagnostics.TraceSource.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Diagnostics.Tracing.dll") != "a6dccae8d2c8ecddbc6c53d70f58daec02b45431edb4affd74d9626cfe073d07"
  #error "Accepted payload mismatch (x64): System.Diagnostics.Tracing.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Diagnostics.Tracing.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Diagnostics.Tracing.dll") != "1d3e3f8c01bb2c07037193694508cdd330e5651fddd8c0cbcafff295fecc6e81"
  #error "Accepted payload mismatch (x86): System.Diagnostics.Tracing.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Diagnostics.Tracing.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.DirectoryServices.dll") != "3516b5024c20c9bd91c0dd0031b71e20dad648757e38cceaf3e3896c0916b74c"
  #error "Accepted payload mismatch (x64): System.DirectoryServices.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.DirectoryServices.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.DirectoryServices.dll") != "8826f0159c25db7642bee6fb85819d7966fba6354680a60c86f95907b2559c0a"
  #error "Accepted payload mismatch (x86): System.DirectoryServices.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.DirectoryServices.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Drawing.Common.dll") != "ac603599f35d7d8389cf91fd407f72cbbc5896274b4f8383cb47dc67f671f03d"
  #error "Accepted payload mismatch (x64): System.Drawing.Common.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Drawing.Common.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Drawing.Common.dll") != "f0558f7ec0a904793a62430b6fb98ebc22f75f9d8a79165b44e5019b721c3f42"
  #error "Accepted payload mismatch (x86): System.Drawing.Common.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Drawing.Common.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Drawing.Design.dll") != "2ea6eef09fef23d0ba0704c21b5a76c342006e4fb9cea9bdc948cc7927eb61af"
  #error "Accepted payload mismatch (x64): System.Drawing.Design.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Drawing.Design.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Drawing.Design.dll") != "31d653fee2d1fcb2493cbc5f2f94a85a4f586643cd6a395fe053466e3bb26dba"
  #error "Accepted payload mismatch (x86): System.Drawing.Design.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Drawing.Design.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Drawing.Primitives.dll") != "f86fecb7b6d545d57f4584f1c6dd499a747c760afc408b5d4f33a0a22813257f"
  #error "Accepted payload mismatch (x64): System.Drawing.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Drawing.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Drawing.Primitives.dll") != "f1c4683c6993a5d082fe4712eb10563a71a0e5fd6a951fb1a18f213aff991921"
  #error "Accepted payload mismatch (x86): System.Drawing.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Drawing.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Drawing.dll") != "790a6025caafc9d21cd9cbbe438b70c4b0ad781b4fb9e210e1f50868a4dff42f"
  #error "Accepted payload mismatch (x64): System.Drawing.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Drawing.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Drawing.dll") != "490cbf0e58f1b288bd09698df6dd21d94cde1ce6e14cac409404a36ed3fc821a"
  #error "Accepted payload mismatch (x86): System.Drawing.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Drawing.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Dynamic.Runtime.dll") != "02b2c3fd58f793718fd4a0707993b3cfac272541166ada7b0373902fe5fc6384"
  #error "Accepted payload mismatch (x64): System.Dynamic.Runtime.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Dynamic.Runtime.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Dynamic.Runtime.dll") != "4154d8ca7ad615f87fa587960d64a58e727563c8994a04d5f96e1b90e11a1310"
  #error "Accepted payload mismatch (x86): System.Dynamic.Runtime.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Dynamic.Runtime.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Formats.Asn1.dll") != "dddde5e0942e6ed249f905d5d58bba797756477fe3424c94bd60abb73e95bc8f"
  #error "Accepted payload mismatch (x64): System.Formats.Asn1.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Formats.Asn1.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Formats.Asn1.dll") != "b6a01ac36babb2c97a0b44e7e3ed1457b41a14e9db841741f860b37b18eea9fe"
  #error "Accepted payload mismatch (x86): System.Formats.Asn1.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Formats.Asn1.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Formats.Nrbf.dll") != "7adf49f2d9d995a8b5df5c5345df23d78b33666579eea906329d18d8852f59ba"
  #error "Accepted payload mismatch (x64): System.Formats.Nrbf.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Formats.Nrbf.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Formats.Nrbf.dll") != "61d237057b5e158f82c511c58ae0c77e46f7ed6c09f3b611d4d66ce45a11f8fa"
  #error "Accepted payload mismatch (x86): System.Formats.Nrbf.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Formats.Nrbf.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Formats.Tar.dll") != "0a49e083ed13e4dde812dc870e26f87e1e86a1b0592a661ed498ee49a274c2f3"
  #error "Accepted payload mismatch (x64): System.Formats.Tar.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Formats.Tar.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Formats.Tar.dll") != "d0603f5575db5fb8d394b633a71ce531cbebe68563a39fe8909f8d094d991983"
  #error "Accepted payload mismatch (x86): System.Formats.Tar.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Formats.Tar.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Globalization.Calendars.dll") != "d169ed333b4bab2d6f9ba6a0fc9e4f1a8301e97e9c265a46d949ea5651080c0d"
  #error "Accepted payload mismatch (x64): System.Globalization.Calendars.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Globalization.Calendars.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Globalization.Calendars.dll") != "8bdd309e5b9374680856ac293701ce98886daf33345ae94acc36fa2fb0e4fd12"
  #error "Accepted payload mismatch (x86): System.Globalization.Calendars.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Globalization.Calendars.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Globalization.Extensions.dll") != "4a5ff15607bb3af1016a471eb2e3e93b94ab60da4b862de5de9aea27fb1ddf8a"
  #error "Accepted payload mismatch (x64): System.Globalization.Extensions.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Globalization.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Globalization.Extensions.dll") != "88b66e6bde36c2ce5be92c7a6f8b6546f9a17395d5125cba87b0bd53aae7afbc"
  #error "Accepted payload mismatch (x86): System.Globalization.Extensions.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Globalization.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Globalization.dll") != "bdeb8d26d917976915190c6213907967b5a2ce83b7c575b67a71d121993dfd24"
  #error "Accepted payload mismatch (x64): System.Globalization.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Globalization.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Globalization.dll") != "c3d309443cf4ebbd5b28f903d54f436580dd5e90eeb6b9fec39e42b5f29510ef"
  #error "Accepted payload mismatch (x86): System.Globalization.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Globalization.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.IO.Compression.Brotli.dll") != "4453592a0674b86b0cd41bf3e021b22b3e118b7598926d5df4676da1a8ce24e6"
  #error "Accepted payload mismatch (x64): System.IO.Compression.Brotli.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.IO.Compression.Brotli.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.IO.Compression.Brotli.dll") != "f4ae1287b2bb8caf9de9104e6923d20dd9b34b0db7a53faecfaf908b068933cc"
  #error "Accepted payload mismatch (x86): System.IO.Compression.Brotli.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.IO.Compression.Brotli.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.IO.Compression.FileSystem.dll") != "889ecc7a7c5b44071aab2826fd6d6b8dad8b88ea04119870a3e4f2caaae95009"
  #error "Accepted payload mismatch (x64): System.IO.Compression.FileSystem.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.IO.Compression.FileSystem.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.IO.Compression.FileSystem.dll") != "ebbaf7de263bed9fc389f08a7e8bf411dc13b55fc2161756e9916977937ad2ac"
  #error "Accepted payload mismatch (x86): System.IO.Compression.FileSystem.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.IO.Compression.FileSystem.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.IO.Compression.Native.dll") != "68d68e5aad37470310f2b723afeda6447cf53746ad8e3ffabf44cda8cdcda6bd"
  #error "Accepted payload mismatch (x64): System.IO.Compression.Native.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.IO.Compression.Native.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.IO.Compression.Native.dll") != "c8c0996766cf92756b34c918e18b4d98a4ba237e3f60b6fae08ed136e3e3917b"
  #error "Accepted payload mismatch (x86): System.IO.Compression.Native.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.IO.Compression.Native.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.IO.Compression.ZipFile.dll") != "5534bfe9bccfb28357b85bb4b9f8692a25aeff40c03951813f55621f333bc5b4"
  #error "Accepted payload mismatch (x64): System.IO.Compression.ZipFile.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.IO.Compression.ZipFile.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.IO.Compression.ZipFile.dll") != "6b93c3ceb71abb4bb57dbc96d49a2a00b3a5592f3304f15f08ba4db8e09c4a50"
  #error "Accepted payload mismatch (x86): System.IO.Compression.ZipFile.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.IO.Compression.ZipFile.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.IO.Compression.dll") != "8637cdee8cb539b4dc492b775805c6611b3a21226f13faa91babbb427e931fcf"
  #error "Accepted payload mismatch (x64): System.IO.Compression.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.IO.Compression.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.IO.Compression.dll") != "69ae6457717682ce8c8c41dd78600e7d99ab680b9666d16406302c1514d2bc34"
  #error "Accepted payload mismatch (x86): System.IO.Compression.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.IO.Compression.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.IO.FileSystem.AccessControl.dll") != "c692a48ab5d7ef5389af22fbeafaf44a7a495eac4d0e9d18f5cedd8a691818b8"
  #error "Accepted payload mismatch (x64): System.IO.FileSystem.AccessControl.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.IO.FileSystem.AccessControl.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.IO.FileSystem.AccessControl.dll") != "f747ee2f5d46bc40addebca3a4a70e1e2f818c2845be1ba0451742e4dac852ab"
  #error "Accepted payload mismatch (x86): System.IO.FileSystem.AccessControl.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.IO.FileSystem.AccessControl.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.IO.FileSystem.DriveInfo.dll") != "b590790f7378243adeb02003911506491a9b76dc3d2b01acd5b7dc485a0c76bb"
  #error "Accepted payload mismatch (x64): System.IO.FileSystem.DriveInfo.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.IO.FileSystem.DriveInfo.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.IO.FileSystem.DriveInfo.dll") != "31ef30a4ac2fbd14c70586cb9934cf8b9d7cf1b490bd5e859e5f83cec8621275"
  #error "Accepted payload mismatch (x86): System.IO.FileSystem.DriveInfo.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.IO.FileSystem.DriveInfo.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.IO.FileSystem.Primitives.dll") != "0602e2f6227d8a0842bba942fb84c9a68331266802c449af9e11aeb45f9193fe"
  #error "Accepted payload mismatch (x64): System.IO.FileSystem.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.IO.FileSystem.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.IO.FileSystem.Primitives.dll") != "03474554078fb7430e6a3b34d66083a92cd3f307a2369c8a972c2455cf4b8a42"
  #error "Accepted payload mismatch (x86): System.IO.FileSystem.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.IO.FileSystem.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.IO.FileSystem.Watcher.dll") != "0ffc38c087054451077a66e4c7f3c5b35869ef7911b5cc1598e7939ef3879a07"
  #error "Accepted payload mismatch (x64): System.IO.FileSystem.Watcher.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.IO.FileSystem.Watcher.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.IO.FileSystem.Watcher.dll") != "0b118365323b2a6f1e069dae073b35ce48f817ca25221af0933b47f723848e98"
  #error "Accepted payload mismatch (x86): System.IO.FileSystem.Watcher.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.IO.FileSystem.Watcher.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.IO.FileSystem.dll") != "b2b09f39e7cb61dddb3658d95a4feb62498d25a9a477a1e11f5faf36d829a19b"
  #error "Accepted payload mismatch (x64): System.IO.FileSystem.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.IO.FileSystem.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.IO.FileSystem.dll") != "34489408d980d73c69ea5891a779c875e815d2d4ceaf34b7c305d264d12d30a8"
  #error "Accepted payload mismatch (x86): System.IO.FileSystem.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.IO.FileSystem.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.IO.IsolatedStorage.dll") != "aac0d8b2736ec46166bef6f2796b6f621af686552326f3e7e73921b7792989dd"
  #error "Accepted payload mismatch (x64): System.IO.IsolatedStorage.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.IO.IsolatedStorage.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.IO.IsolatedStorage.dll") != "717316afe3374de4c938c6b948ce6f522cb2c8421d0bad6a1102787d7f8e38eb"
  #error "Accepted payload mismatch (x86): System.IO.IsolatedStorage.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.IO.IsolatedStorage.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.IO.MemoryMappedFiles.dll") != "eb75d6a39e2babc14f70c12b1dbcdb57fb8763bd79d038c80b21182689b70f50"
  #error "Accepted payload mismatch (x64): System.IO.MemoryMappedFiles.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.IO.MemoryMappedFiles.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.IO.MemoryMappedFiles.dll") != "327ecf34d5896494711cff7ddbae07635941f33afca9badaf9687404912d1231"
  #error "Accepted payload mismatch (x86): System.IO.MemoryMappedFiles.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.IO.MemoryMappedFiles.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.IO.Packaging.dll") != "677d1647c54627e092a898f3ce63740f3ca7335834312411835be950a9aee698"
  #error "Accepted payload mismatch (x64): System.IO.Packaging.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.IO.Packaging.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.IO.Packaging.dll") != "2daf3a82d61ee1d512efa7d50ed584a7a9d987e369fd40ae1e51b1d401ddda24"
  #error "Accepted payload mismatch (x86): System.IO.Packaging.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.IO.Packaging.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.IO.Pipelines.dll") != "876f3ff1e05892226c1f0363468cdfda8f023b21400ee6177a56a1405914e1aa"
  #error "Accepted payload mismatch (x64): System.IO.Pipelines.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.IO.Pipelines.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.IO.Pipelines.dll") != "aa533ea8114d1a768011f260acd03f43157b4ebf4f5a88ba71465254056078cd"
  #error "Accepted payload mismatch (x86): System.IO.Pipelines.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.IO.Pipelines.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.IO.Pipes.AccessControl.dll") != "e34322b26ea5aca715c6f92c3177556a02083e82d561ea90f37a12caaebe90d3"
  #error "Accepted payload mismatch (x64): System.IO.Pipes.AccessControl.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.IO.Pipes.AccessControl.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.IO.Pipes.AccessControl.dll") != "34ef6bc823de45355e3dba83680aef8b954eb80c83e0593530d093a0fca4a257"
  #error "Accepted payload mismatch (x86): System.IO.Pipes.AccessControl.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.IO.Pipes.AccessControl.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.IO.Pipes.dll") != "ad25ec1fd00def55631c6ec891b3bd270b80ce1754f90d4190f2caea28b8b842"
  #error "Accepted payload mismatch (x64): System.IO.Pipes.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.IO.Pipes.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.IO.Pipes.dll") != "7840b469244a396d854225f9ed2659d2826b88de06d78467fe95e84d48797a3c"
  #error "Accepted payload mismatch (x86): System.IO.Pipes.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.IO.Pipes.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.IO.UnmanagedMemoryStream.dll") != "e3b1ba5a4ff28ee788f1e88dfbb50762135a7c29f0adb93dbac9ac5284dec769"
  #error "Accepted payload mismatch (x64): System.IO.UnmanagedMemoryStream.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.IO.UnmanagedMemoryStream.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.IO.UnmanagedMemoryStream.dll") != "597148400cdb4e0c761206a6bf7cc4dfcc47a3da602515f6e09c2697deb1cf80"
  #error "Accepted payload mismatch (x86): System.IO.UnmanagedMemoryStream.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.IO.UnmanagedMemoryStream.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.IO.dll") != "2699ce957e080e2fa5f87cfedfc0d469475fd8d1ada1f5db09c48a085c2282e3"
  #error "Accepted payload mismatch (x64): System.IO.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.IO.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.IO.dll") != "be7c941e25209ee357567650afb2870ee53c4b1de7a1a4fef66440a2a709840e"
  #error "Accepted payload mismatch (x86): System.IO.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.IO.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Linq.AsyncEnumerable.dll") != "e4a6fcc5bfc7d9b9038e3a2d26e17aa9244b7b6a25b10c4ce3cf205689661040"
  #error "Accepted payload mismatch (x64): System.Linq.AsyncEnumerable.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Linq.AsyncEnumerable.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Linq.AsyncEnumerable.dll") != "d8d8093972c882112b2718a426e8bd396ad4bb4b67d3f5974b4afbe9d69993ad"
  #error "Accepted payload mismatch (x86): System.Linq.AsyncEnumerable.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Linq.AsyncEnumerable.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Linq.Expressions.dll") != "a4fcbd909bb6fdd8a058c06f23bd149a9dfb0b9462c2f757dbbc1da9dfd583e4"
  #error "Accepted payload mismatch (x64): System.Linq.Expressions.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Linq.Expressions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Linq.Expressions.dll") != "decaaea334c1c238f0fa32615dc21e073cb9a8743475780b4e46083b606ed04a"
  #error "Accepted payload mismatch (x86): System.Linq.Expressions.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Linq.Expressions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Linq.Parallel.dll") != "adea4cf143f5da17a6d34bbd786deb47ec299dd6010f844dc9d53c386f4740d4"
  #error "Accepted payload mismatch (x64): System.Linq.Parallel.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Linq.Parallel.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Linq.Parallel.dll") != "13467327c879371838336cf872139de280d0ef6b8cabcc0ca8d021fc94d57631"
  #error "Accepted payload mismatch (x86): System.Linq.Parallel.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Linq.Parallel.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Linq.Queryable.dll") != "600e77c705bf8550dbf0d1c3965020046424537362c3f76234feaeb12cc69844"
  #error "Accepted payload mismatch (x64): System.Linq.Queryable.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Linq.Queryable.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Linq.Queryable.dll") != "e69f91b07f74fe26ffb416c3be48d0e43aaacf6c3d7822a3148017f85c33545a"
  #error "Accepted payload mismatch (x86): System.Linq.Queryable.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Linq.Queryable.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Linq.dll") != "dc992ba1c1bac514d14cc923c154a6466be5a250c2b02c220f8f5d63074718e9"
  #error "Accepted payload mismatch (x64): System.Linq.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Linq.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Linq.dll") != "a082127296288a50a9b22a3ea4ef463a674f201bf9db2238d49d52a6cf168a7d"
  #error "Accepted payload mismatch (x86): System.Linq.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Linq.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Memory.dll") != "ad367e1b7b733aa840cd6595ead967acb9fd9969cf644e06689bf2e841b515a0"
  #error "Accepted payload mismatch (x64): System.Memory.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Memory.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Memory.dll") != "a43f880e6e91d2f047467e9847e36806d285e2b91fa267ba2efd3e22088a59aa"
  #error "Accepted payload mismatch (x86): System.Memory.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Memory.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.Http.Json.dll") != "354c0900786b9614bd89130888542b2098b1968ebb1a3bf838c82bbc0fb0211f"
  #error "Accepted payload mismatch (x64): System.Net.Http.Json.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.Http.Json.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.Http.Json.dll") != "278a1b0180f7549de44d7fe79ef93535e94597a596e1c773b356f86ff6112015"
  #error "Accepted payload mismatch (x86): System.Net.Http.Json.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.Http.Json.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.Http.dll") != "2db574037fe8cb2465043d080677d64c2b0ac0e4bb3944e6b6e84218c65d691c"
  #error "Accepted payload mismatch (x64): System.Net.Http.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.Http.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.Http.dll") != "f39d23662adbd3f0bcb396cb61cc8e0a7f71c2995c45fbfa2b3ca9d7e61ceab8"
  #error "Accepted payload mismatch (x86): System.Net.Http.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.Http.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.HttpListener.dll") != "3ff234307783f345d9b11946bf1b8cdcac1f761e58526563d96392acb2612ff7"
  #error "Accepted payload mismatch (x64): System.Net.HttpListener.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.HttpListener.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.HttpListener.dll") != "b76ff818fb98e2324063c41b0253004def2323bd45ded5ad18671c5b2680e381"
  #error "Accepted payload mismatch (x86): System.Net.HttpListener.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.HttpListener.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.Mail.dll") != "be2372646d6bf42afef16f9fe5c3514577ca8aa99b3d667fe7c808a5999cab9a"
  #error "Accepted payload mismatch (x64): System.Net.Mail.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.Mail.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.Mail.dll") != "1e7a918b7e48bf6c9118c47aa5a47b573142b1d5a606ca848a2d22a50c4d48b5"
  #error "Accepted payload mismatch (x86): System.Net.Mail.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.Mail.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.NameResolution.dll") != "7bbeba9120a3ece73bd39fa2539750695f37815c8c6411c190cd20d3493bfdca"
  #error "Accepted payload mismatch (x64): System.Net.NameResolution.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.NameResolution.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.NameResolution.dll") != "8539a04b0bf45b44de45ac1e7501cc1356846301afee95eeae4f59e74989328a"
  #error "Accepted payload mismatch (x86): System.Net.NameResolution.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.NameResolution.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.NetworkInformation.dll") != "499f0d2788382e9e628c3c82f57bf0dd8e86d7cc00b70765bca5153315eaa10f"
  #error "Accepted payload mismatch (x64): System.Net.NetworkInformation.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.NetworkInformation.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.NetworkInformation.dll") != "ddf4041281a8315e9ce0fe9068a27ce2562001f8a4c5ccc06fdf3fba3c1c3a15"
  #error "Accepted payload mismatch (x86): System.Net.NetworkInformation.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.NetworkInformation.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.Ping.dll") != "ce93405cf2004efc69da60f2eba371bf6461631e8d37365209f74516dd917113"
  #error "Accepted payload mismatch (x64): System.Net.Ping.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.Ping.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.Ping.dll") != "455f515d02114aca4c8a8c3b486c3fa16116adfc4e818d0bf6b030c2dad1eb9e"
  #error "Accepted payload mismatch (x86): System.Net.Ping.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.Ping.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.Primitives.dll") != "607bcbf14bebe5643257d26d67968e16cbe6b02180e7ff8d337b428aaa80d52b"
  #error "Accepted payload mismatch (x64): System.Net.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.Primitives.dll") != "fc6f3e2f4edcbbadd537f75b00f0b83ee955e8d8257ef80e5565da4bedc08575"
  #error "Accepted payload mismatch (x86): System.Net.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.Quic.dll") != "01f534df790ac9e11cf79e0e69c42cfc2dfab44c3da8c87d7d4af69188ff9307"
  #error "Accepted payload mismatch (x64): System.Net.Quic.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.Quic.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.Quic.dll") != "367aa7d96a4cc824988a6554f5a8eafb60ea2cb895d05a5cbc0a002f838af462"
  #error "Accepted payload mismatch (x86): System.Net.Quic.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.Quic.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.Requests.dll") != "83586745515c0fd0e5d48248871fd9f7225657de5595c8b48e4d6a88bf4efeec"
  #error "Accepted payload mismatch (x64): System.Net.Requests.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.Requests.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.Requests.dll") != "a24ccc8cc244817aecc5c51b35d804ae151743d156e58b2f65abeaaad646c7e1"
  #error "Accepted payload mismatch (x86): System.Net.Requests.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.Requests.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.Security.dll") != "5ca4924950689ade61055daec5fd99d9f617acafabdd5b21c09e4329195909a7"
  #error "Accepted payload mismatch (x64): System.Net.Security.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.Security.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.Security.dll") != "7a6264a665c995a96c586e8218b9a1752ed4908197c1385b3ad03271047379dd"
  #error "Accepted payload mismatch (x86): System.Net.Security.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.Security.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.ServerSentEvents.dll") != "324b0f8d305c967adccce3f126bf470e8aaa0a62d84695f71feb700f741d0abf"
  #error "Accepted payload mismatch (x64): System.Net.ServerSentEvents.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.ServerSentEvents.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.ServerSentEvents.dll") != "4246b763145c769488e3c17acdfc61788bc7aa9f754c1b023b5a4f5cc404fa8b"
  #error "Accepted payload mismatch (x86): System.Net.ServerSentEvents.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.ServerSentEvents.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.ServicePoint.dll") != "678104fe7e696b9b182734ed4bbde61f23e8466c942a68baa9f9e504781aa978"
  #error "Accepted payload mismatch (x64): System.Net.ServicePoint.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.ServicePoint.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.ServicePoint.dll") != "dc738361c1ee5a44c4439efefd68ddb7e39dc3da697edb5b01ee2b5043909236"
  #error "Accepted payload mismatch (x86): System.Net.ServicePoint.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.ServicePoint.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.Sockets.dll") != "6ee2e8c007d307d94dcdb9b1864c0894ca1621c51f43d2683981cac4f7823b7e"
  #error "Accepted payload mismatch (x64): System.Net.Sockets.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.Sockets.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.Sockets.dll") != "dfba16936971fa65ed35fedb5cddad2f305d0a0b921645b7cd46e6229d424d33"
  #error "Accepted payload mismatch (x86): System.Net.Sockets.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.Sockets.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.WebClient.dll") != "6d34f1715cbbb24445b88d3fa62bde425835471b72b04a6a20e2f96dc356a96f"
  #error "Accepted payload mismatch (x64): System.Net.WebClient.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.WebClient.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.WebClient.dll") != "a06bf7cd673e83999e181f0d69e63e70ee913bfc619580ecc6545ab5e60b666f"
  #error "Accepted payload mismatch (x86): System.Net.WebClient.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.WebClient.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.WebHeaderCollection.dll") != "31e1ee3ad85465b84e818a903e0e87ab86b7b9c8943fea7136e13884f555cf8f"
  #error "Accepted payload mismatch (x64): System.Net.WebHeaderCollection.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.WebHeaderCollection.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.WebHeaderCollection.dll") != "0b013b61f176d8e09ca0d945fb1da30663d79c794f4bbb8f82581d2b61dce7d2"
  #error "Accepted payload mismatch (x86): System.Net.WebHeaderCollection.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.WebHeaderCollection.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.WebProxy.dll") != "3e0fff6d5a0f0fcd6fac449ec6f81ab4bbb65bf4d1844097264de03c6a9e12b5"
  #error "Accepted payload mismatch (x64): System.Net.WebProxy.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.WebProxy.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.WebProxy.dll") != "aad077c2686f45a502693dd3b9505288ccbe859dbb664464ffae90c73a8858a6"
  #error "Accepted payload mismatch (x86): System.Net.WebProxy.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.WebProxy.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.WebSockets.Client.dll") != "3929ecbf7fe9730aa09087ce1b0b95f7ad488700ed297a7b1db9bcdffd379eb7"
  #error "Accepted payload mismatch (x64): System.Net.WebSockets.Client.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.WebSockets.Client.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.WebSockets.Client.dll") != "7b76a61294371c02678e89590b8b12a94550ae7527677dac98d3f9df9fd0b62d"
  #error "Accepted payload mismatch (x86): System.Net.WebSockets.Client.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.WebSockets.Client.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.WebSockets.dll") != "e53874d48acec015893a570bbf02502905985fa66ca75b4f5f864eff219f5f87"
  #error "Accepted payload mismatch (x64): System.Net.WebSockets.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.WebSockets.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.WebSockets.dll") != "e4df19e65515bc001d85de22cc6f4eb78926ef75115c264afc032712d4fdeb26"
  #error "Accepted payload mismatch (x86): System.Net.WebSockets.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.WebSockets.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Net.dll") != "9966e1605fda50d083beaa307bcdb9daf0d8b7bae84771265d8c44534748ffd8"
  #error "Accepted payload mismatch (x64): System.Net.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Net.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Net.dll") != "3d4022f06e4c80863ea18d8af81bace16938f76b8c14475d094103b076e1c078"
  #error "Accepted payload mismatch (x86): System.Net.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Net.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Numerics.Vectors.dll") != "1a2e6f7979950300c1bd28a8ea82d37bbf361d83fb2265b1577513b0d6df9633"
  #error "Accepted payload mismatch (x64): System.Numerics.Vectors.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Numerics.Vectors.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Numerics.Vectors.dll") != "a3f5e4bc6bc43210567365f9bd7255b90309d5d523d27c3d0631a786d4d50cbb"
  #error "Accepted payload mismatch (x86): System.Numerics.Vectors.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Numerics.Vectors.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Numerics.dll") != "da30ddc2fbfed6382c9a029f4ab19eca96a60ce36bda432f2feb5e209ccd2457"
  #error "Accepted payload mismatch (x64): System.Numerics.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Numerics.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Numerics.dll") != "6fcbed709cb6c8e16d75800d89886e00e09dd305037badaf07a75d82b05a4342"
  #error "Accepted payload mismatch (x86): System.Numerics.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Numerics.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.ObjectModel.dll") != "e10ee7ce6c29fb120f6609c74ab7d15732bc9eecca99e2341d955799f6a33216"
  #error "Accepted payload mismatch (x64): System.ObjectModel.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.ObjectModel.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.ObjectModel.dll") != "858b4eb73c0f5272b97c2a4ae814be57e92d623cf12bfdf6d9f1a683d137b3ea"
  #error "Accepted payload mismatch (x86): System.ObjectModel.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.ObjectModel.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Printing.dll") != "88273c8779147bb044be4ad0f7447b14b4088c3a42349de7f516d119a96c39b1"
  #error "Accepted payload mismatch (x64): System.Printing.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Printing.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Printing.dll") != "edcf4631ad612aa6b8a3af354307fbb9a38939c3a6f0b905218f68def7dfe5ed"
  #error "Accepted payload mismatch (x86): System.Printing.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Printing.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Private.CoreLib.dll") != "983f5c8957437aa38fcd239606e60dc2a4a6f551605311591a72e5b285eae957"
  #error "Accepted payload mismatch (x64): System.Private.CoreLib.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Private.CoreLib.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Private.CoreLib.dll") != "33f24b294d0c6d8523ea3dd5caef1bbc6769177aecdb85b0fa8ee5a11811c123"
  #error "Accepted payload mismatch (x86): System.Private.CoreLib.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Private.CoreLib.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Private.DataContractSerialization.dll") != "708714c803ec517c49f88a367e973e4c8a9b818d1a040f67d0a99ca1863e50fb"
  #error "Accepted payload mismatch (x64): System.Private.DataContractSerialization.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Private.DataContractSerialization.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Private.DataContractSerialization.dll") != "a46db0c9f76b9d951b37b8c59b78d23787c9ff66d53b6c5df04d021f1474f380"
  #error "Accepted payload mismatch (x86): System.Private.DataContractSerialization.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Private.DataContractSerialization.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Private.Uri.dll") != "e13e63bee843aab6b4d21a4913001e9233397fa0c499b9131a63b75feacf3d5e"
  #error "Accepted payload mismatch (x64): System.Private.Uri.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Private.Uri.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Private.Uri.dll") != "24616b6f65fcb725b756da4ff9ccdf3360ca567eaf60b5284c8c0f3095e782a0"
  #error "Accepted payload mismatch (x86): System.Private.Uri.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Private.Uri.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Private.Windows.Core.dll") != "dc096f18e2b6af06160d3772cd2d04281f9fd63f3bc2ce5620541601806bec89"
  #error "Accepted payload mismatch (x64): System.Private.Windows.Core.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Private.Windows.Core.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Private.Windows.Core.dll") != "513e80902c830e3c0c2e9d08bde2fdf510b9f0c49e232b385d1348fc3f7e8eae"
  #error "Accepted payload mismatch (x86): System.Private.Windows.Core.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Private.Windows.Core.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Private.Windows.GdiPlus.dll") != "2c758b2425726d0c10f62e09c14ca7adf1a62fbfb8212129af8ebaf0e32079be"
  #error "Accepted payload mismatch (x64): System.Private.Windows.GdiPlus.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Private.Windows.GdiPlus.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Private.Windows.GdiPlus.dll") != "107de4739804dbe4b285f25f4c061def6f9f3042ef17aa1b3dd88294e22bd661"
  #error "Accepted payload mismatch (x86): System.Private.Windows.GdiPlus.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Private.Windows.GdiPlus.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Private.Xml.Linq.dll") != "18c4e66a11ddc87ad9783439fe8097658acbc363e637b64ce65ec3574e84b057"
  #error "Accepted payload mismatch (x64): System.Private.Xml.Linq.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Private.Xml.Linq.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Private.Xml.Linq.dll") != "040ab093ed32615a7ae6a60aefb669aaf5d981d022bce584d4d88f2d917f9146"
  #error "Accepted payload mismatch (x86): System.Private.Xml.Linq.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Private.Xml.Linq.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Private.Xml.dll") != "e821f3686bc217ca44fe3a41c1b1c87ed4074651c8a95640b7b7bc567ab029fc"
  #error "Accepted payload mismatch (x64): System.Private.Xml.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Private.Xml.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Private.Xml.dll") != "b236b1076199c96a253b9c74fe4a4e5fd26b94ec9a896f1e1e018c721b245f40"
  #error "Accepted payload mismatch (x86): System.Private.Xml.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Private.Xml.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Reflection.DispatchProxy.dll") != "aa7216f50f0729958763cb49931e74369c689c22618a0ebac4f92c7df5b1ed52"
  #error "Accepted payload mismatch (x64): System.Reflection.DispatchProxy.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Reflection.DispatchProxy.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Reflection.DispatchProxy.dll") != "922614534e38d648aa9e220b66cf5be23c7769ee3e67c0efb7b10d17488da1af"
  #error "Accepted payload mismatch (x86): System.Reflection.DispatchProxy.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Reflection.DispatchProxy.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Reflection.Emit.ILGeneration.dll") != "04b1bf369e833d65c35ea84ecea7f69d125df2c5a224962b95ac43372790bd1a"
  #error "Accepted payload mismatch (x64): System.Reflection.Emit.ILGeneration.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Reflection.Emit.ILGeneration.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Reflection.Emit.ILGeneration.dll") != "f7b72469e966c2e7261bd6b8d41283aa1e5f98ec320137f99fc8e494c9daf1c6"
  #error "Accepted payload mismatch (x86): System.Reflection.Emit.ILGeneration.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Reflection.Emit.ILGeneration.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Reflection.Emit.Lightweight.dll") != "70d3e152e9c235b5d2945ebc8ad1df3c06a815677097a9df9cb192c33e4b1c38"
  #error "Accepted payload mismatch (x64): System.Reflection.Emit.Lightweight.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Reflection.Emit.Lightweight.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Reflection.Emit.Lightweight.dll") != "369a1522e5bc9b0e0547b8e3b64de3051fcc9e27af3d05f8a1aaea140e3a6ca0"
  #error "Accepted payload mismatch (x86): System.Reflection.Emit.Lightweight.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Reflection.Emit.Lightweight.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Reflection.Emit.dll") != "98ecc074b2e42194daabc81ddafb55a7b3552ea18e8218442301beee0d5556e1"
  #error "Accepted payload mismatch (x64): System.Reflection.Emit.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Reflection.Emit.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Reflection.Emit.dll") != "684de8766ef108aa295b25838f637cc049739adc02920fe62c9cc2c6dbe52bcc"
  #error "Accepted payload mismatch (x86): System.Reflection.Emit.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Reflection.Emit.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Reflection.Extensions.dll") != "1acbc49deb1a9410cfa6b357c1f2e185e8183584cf4ab237961ddd7a561a0efb"
  #error "Accepted payload mismatch (x64): System.Reflection.Extensions.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Reflection.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Reflection.Extensions.dll") != "6927254a7706cba888f9e1fbc149195a56d95bbd5a96d7a0be5de98f2d1959c0"
  #error "Accepted payload mismatch (x86): System.Reflection.Extensions.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Reflection.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Reflection.Metadata.dll") != "4130c090aa1ed9a026ea6473be258b9e07dcb05097836ac569853272679a5c97"
  #error "Accepted payload mismatch (x64): System.Reflection.Metadata.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Reflection.Metadata.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Reflection.Metadata.dll") != "f50ffd2b3477bd8fbf7cc44242b1aeb1574b0fc65be98922726ed895f3ee6653"
  #error "Accepted payload mismatch (x86): System.Reflection.Metadata.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Reflection.Metadata.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Reflection.Primitives.dll") != "8b2327ca2dffb76764610d6dcde98416df67f18891aea728e1259cfc001e73a9"
  #error "Accepted payload mismatch (x64): System.Reflection.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Reflection.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Reflection.Primitives.dll") != "9a4e764737ea53a46028b7ef50abfdc987a25fb46e1dc0533fa085866edbe04b"
  #error "Accepted payload mismatch (x86): System.Reflection.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Reflection.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Reflection.TypeExtensions.dll") != "380e0b37748c77a120f42ef91453099520bc45af1b0c65b0d3fa6b88f0fa4738"
  #error "Accepted payload mismatch (x64): System.Reflection.TypeExtensions.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Reflection.TypeExtensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Reflection.TypeExtensions.dll") != "72322e43120ea50a21da14c3682a4d6afb9787e6eaa128d5424bb87b6cbde003"
  #error "Accepted payload mismatch (x86): System.Reflection.TypeExtensions.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Reflection.TypeExtensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Reflection.dll") != "9aa8acb636611ba2324bedd4dd9650bc9e7a1f016cf7acc4c2a69051c0cbed4b"
  #error "Accepted payload mismatch (x64): System.Reflection.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Reflection.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Reflection.dll") != "54296a9d3e660b19bdf89e24e50f8dddeb5ab99d7ec192664a58168101e805b4"
  #error "Accepted payload mismatch (x86): System.Reflection.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Reflection.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Resources.Extensions.dll") != "0313434ea8678f32a42c8f96f84ac32af44ae73b6a5e393724d59df32861b6f0"
  #error "Accepted payload mismatch (x64): System.Resources.Extensions.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Resources.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Resources.Extensions.dll") != "e806cb9bb0bc03bf3ed8411fd163684dfb6de3b43498890e390b8cb0b26d5a90"
  #error "Accepted payload mismatch (x86): System.Resources.Extensions.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Resources.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Resources.Reader.dll") != "c72a47e82365921b09a61f1ca72b722e4d28683d085035937dbfce32ba4503e2"
  #error "Accepted payload mismatch (x64): System.Resources.Reader.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Resources.Reader.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Resources.Reader.dll") != "b0c9f0ee290823120faf01253710730f7565595608a11dc01bd34071b2970fae"
  #error "Accepted payload mismatch (x86): System.Resources.Reader.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Resources.Reader.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Resources.ResourceManager.dll") != "41b34aa003a48c68fba186cd7b5ea4f68e8f6ce4418bd5a68a158a79ad58293c"
  #error "Accepted payload mismatch (x64): System.Resources.ResourceManager.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Resources.ResourceManager.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Resources.ResourceManager.dll") != "9f84d409ce1da79707f589000ed9000308ac18ad2a118db61c290c39642b0478"
  #error "Accepted payload mismatch (x86): System.Resources.ResourceManager.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Resources.ResourceManager.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Resources.Writer.dll") != "f4dd1d6fc7f69848f36ff753727331b10d49a5f58d089e18e5d0c10eaf5a0afa"
  #error "Accepted payload mismatch (x64): System.Resources.Writer.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Resources.Writer.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Resources.Writer.dll") != "0372d906f3944fef1d97ff4e8c2d45a9cf4e53d0863b1c54e725c81002eb0be2"
  #error "Accepted payload mismatch (x86): System.Resources.Writer.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Resources.Writer.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Runtime.CompilerServices.Unsafe.dll") != "0c2a4e77f62bc47e73ee4868364d7de3013cc41b42f71989f601c0cfb2ef8c8d"
  #error "Accepted payload mismatch (x64): System.Runtime.CompilerServices.Unsafe.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Runtime.CompilerServices.Unsafe.dll") != "1c3833a1f9ada5e18a5226b7fe327d741705c90524a8da0ffbccb133b4db0b3c"
  #error "Accepted payload mismatch (x86): System.Runtime.CompilerServices.Unsafe.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Runtime.CompilerServices.VisualC.dll") != "3ed2279decba500727a9cb1d810c08148bda497975efdc5db21d636b4b78f9ef"
  #error "Accepted payload mismatch (x64): System.Runtime.CompilerServices.VisualC.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Runtime.CompilerServices.VisualC.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Runtime.CompilerServices.VisualC.dll") != "f5164bc21d809cfef89d2d33937acbb03fab4e943d9ec86d70ab7b6c3c3aca92"
  #error "Accepted payload mismatch (x86): System.Runtime.CompilerServices.VisualC.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Runtime.CompilerServices.VisualC.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Runtime.Extensions.dll") != "95b093657b594177ef20472048c926bb9ae39feae5783d6c630f56183937c433"
  #error "Accepted payload mismatch (x64): System.Runtime.Extensions.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Runtime.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Runtime.Extensions.dll") != "32503aa71237e4389a48f872a9340dcc073b7dfea6419e38d522b0213217c74b"
  #error "Accepted payload mismatch (x86): System.Runtime.Extensions.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Runtime.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Runtime.Handles.dll") != "ddedbbb02981acde482bd310b3b48fe9a7003dec68d0cc4b9bcc3663f187a2a8"
  #error "Accepted payload mismatch (x64): System.Runtime.Handles.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Runtime.Handles.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Runtime.Handles.dll") != "6f07471b0f53dc79867271640fd4acc90ede8516616649d8145b40afd1111ff4"
  #error "Accepted payload mismatch (x86): System.Runtime.Handles.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Runtime.Handles.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Runtime.InteropServices.JavaScript.dll") != "ac07e7df59a35bf4ee5ef78502ce187a1880b83612a7bac427f78255c172d35c"
  #error "Accepted payload mismatch (x64): System.Runtime.InteropServices.JavaScript.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Runtime.InteropServices.JavaScript.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Runtime.InteropServices.JavaScript.dll") != "1c101513c6c0ce285ba81744c39eea50166396b88b1b4849791a52d24f9917bf"
  #error "Accepted payload mismatch (x86): System.Runtime.InteropServices.JavaScript.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Runtime.InteropServices.JavaScript.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Runtime.InteropServices.RuntimeInformation.dll") != "499c566f61b78a68d0f48ebb3a6d431c4f3875a1067943d903eab1e7d43fedc0"
  #error "Accepted payload mismatch (x64): System.Runtime.InteropServices.RuntimeInformation.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Runtime.InteropServices.RuntimeInformation.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Runtime.InteropServices.RuntimeInformation.dll") != "c9da3125521f1a1e925a6814675038a43794023bf3dad996c06ac917e55e50ff"
  #error "Accepted payload mismatch (x86): System.Runtime.InteropServices.RuntimeInformation.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Runtime.InteropServices.RuntimeInformation.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Runtime.InteropServices.dll") != "5ab8804e4fbd1a5a54afd9e674af544642b73916e25ee672e7b99adc3161adfc"
  #error "Accepted payload mismatch (x64): System.Runtime.InteropServices.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Runtime.InteropServices.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Runtime.InteropServices.dll") != "98deafa7a27b50803c6438d4326c5fb6e08e40e32ce37a7ec6b143565874cc41"
  #error "Accepted payload mismatch (x86): System.Runtime.InteropServices.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Runtime.InteropServices.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Runtime.Intrinsics.dll") != "422f93053057449c08daa91b49f2eb7503fa4ccc1462ef5556cfe0e1fa5b0003"
  #error "Accepted payload mismatch (x64): System.Runtime.Intrinsics.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Runtime.Intrinsics.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Runtime.Intrinsics.dll") != "622865a448219a96a848e7a1ee0955d11ac1758fc9d17fc95cf24f386abdfa1d"
  #error "Accepted payload mismatch (x86): System.Runtime.Intrinsics.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Runtime.Intrinsics.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Runtime.Loader.dll") != "20660ce31358fe64b0a44aa0bb08022e1081f7d8e1c24a27673e74123000908e"
  #error "Accepted payload mismatch (x64): System.Runtime.Loader.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Runtime.Loader.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Runtime.Loader.dll") != "e516c05dc8219745d040efeedf822b3b245d3756a2aaeb5c079cd853ceaaa358"
  #error "Accepted payload mismatch (x86): System.Runtime.Loader.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Runtime.Loader.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Runtime.Numerics.dll") != "53b5bf64fd79c665adc58607116d33c4b2be3ae12e22360267e263e695886e57"
  #error "Accepted payload mismatch (x64): System.Runtime.Numerics.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Runtime.Numerics.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Runtime.Numerics.dll") != "a531e50795738a7ffaf8f88574194bffe6375aba98c9e0a919b48f42fa29ac55"
  #error "Accepted payload mismatch (x86): System.Runtime.Numerics.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Runtime.Numerics.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Runtime.Serialization.Formatters.dll") != "771bc4a61a546cd4a24332fa9161a94d2e55fa08600f8bf9250f6eb08a9afb77"
  #error "Accepted payload mismatch (x64): System.Runtime.Serialization.Formatters.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Runtime.Serialization.Formatters.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Runtime.Serialization.Formatters.dll") != "e938706af48dd17caf8484541a0b4297dad7e55ae78e466c97d80e13a9793341"
  #error "Accepted payload mismatch (x86): System.Runtime.Serialization.Formatters.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Runtime.Serialization.Formatters.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Runtime.Serialization.Json.dll") != "025a34f5dbff2489bd101924aca35a01bfa1f9d842598e2e041b6edb8ff6845d"
  #error "Accepted payload mismatch (x64): System.Runtime.Serialization.Json.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Runtime.Serialization.Json.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Runtime.Serialization.Json.dll") != "96e29a0bd510b2876a95dba98623e06885b719f2610edca89f93ccd87dd427e9"
  #error "Accepted payload mismatch (x86): System.Runtime.Serialization.Json.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Runtime.Serialization.Json.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Runtime.Serialization.Primitives.dll") != "acf3de0f2562fc05d70627944453443b1a9396a1d0a6ed598beeac35ca266057"
  #error "Accepted payload mismatch (x64): System.Runtime.Serialization.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Runtime.Serialization.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Runtime.Serialization.Primitives.dll") != "2207df3a0bf2968829a600dee7130c05cf97af87d5bdb449b2efd267241db4ee"
  #error "Accepted payload mismatch (x86): System.Runtime.Serialization.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Runtime.Serialization.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Runtime.Serialization.Xml.dll") != "347900ebbba53a24797599363bf10070580fc970a9a6ce8d84dc95f416febfea"
  #error "Accepted payload mismatch (x64): System.Runtime.Serialization.Xml.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Runtime.Serialization.Xml.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Runtime.Serialization.Xml.dll") != "1b80c396b370a87366b5f65d664a63a6dae825643ac9ba940e921481a5c11c72"
  #error "Accepted payload mismatch (x86): System.Runtime.Serialization.Xml.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Runtime.Serialization.Xml.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Runtime.Serialization.dll") != "20c4a3e000268704d5a9fa72e10ddec5a462f2ed5ecd18e24d53c0b6929d15ed"
  #error "Accepted payload mismatch (x64): System.Runtime.Serialization.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Runtime.Serialization.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Runtime.Serialization.dll") != "574ed4585a7c42314ba3f2d4c4f8e278d8947b688232ca0edbd85711820b3742"
  #error "Accepted payload mismatch (x86): System.Runtime.Serialization.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Runtime.Serialization.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Runtime.dll") != "d610a5690ab8f3ab8329c06517694c51aec0e31cd4d7e375584ddcff7c844751"
  #error "Accepted payload mismatch (x64): System.Runtime.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Runtime.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Runtime.dll") != "ee1605985716d20b70c9c4fcfc5b3b449465240924f22c3f407ced8f744ba5e8"
  #error "Accepted payload mismatch (x86): System.Runtime.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Runtime.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Security.AccessControl.dll") != "2081ed669eec7431786a8529506f60d2c2ed3b83523eea0d4fd627dac29e2f4a"
  #error "Accepted payload mismatch (x64): System.Security.AccessControl.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Security.AccessControl.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Security.AccessControl.dll") != "f7157e98f0aebe2e825ce701ee73eaffc9e7d242a1aea2fc11333d4487dd0cdd"
  #error "Accepted payload mismatch (x86): System.Security.AccessControl.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Security.AccessControl.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Security.Claims.dll") != "bc239918decbbd1e90f16c30f0182507342a5b0a82dc2d095e9b365ae0a391ab"
  #error "Accepted payload mismatch (x64): System.Security.Claims.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Security.Claims.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Security.Claims.dll") != "f3695106a3d3a128b1ccc6f5a1fbc54367552f7139f4fca8f4cef338055453d2"
  #error "Accepted payload mismatch (x86): System.Security.Claims.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Security.Claims.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Security.Cryptography.Algorithms.dll") != "bfebcc7308e202b1971d621883574a203ecab74ae992a152dd4071ed08fa00d7"
  #error "Accepted payload mismatch (x64): System.Security.Cryptography.Algorithms.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Security.Cryptography.Algorithms.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Security.Cryptography.Algorithms.dll") != "bd7b2c8ae5ca1b5917a90624ea03961c5f5555171f0680af972bb3c8feacd769"
  #error "Accepted payload mismatch (x86): System.Security.Cryptography.Algorithms.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Security.Cryptography.Algorithms.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Security.Cryptography.Cng.dll") != "a238bc58dabf109aa46685cd4aa3d18a8cc35f4b1e3788710465f2afe5813c9c"
  #error "Accepted payload mismatch (x64): System.Security.Cryptography.Cng.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Security.Cryptography.Cng.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Security.Cryptography.Cng.dll") != "d5198cce54224d753a8f88c0e5f94a31728d465f06ea3e9acb6ce99d0b5db72d"
  #error "Accepted payload mismatch (x86): System.Security.Cryptography.Cng.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Security.Cryptography.Cng.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Security.Cryptography.Csp.dll") != "3737f8ea4d0a41d83e3e2d860e74b3ca2aab7349f9a265577b0f66faaa6ef109"
  #error "Accepted payload mismatch (x64): System.Security.Cryptography.Csp.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Security.Cryptography.Csp.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Security.Cryptography.Csp.dll") != "3b0516c9daee0504d17e1db96ac67959ed97cbc6f1057ee8d562a304e9b1d72c"
  #error "Accepted payload mismatch (x86): System.Security.Cryptography.Csp.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Security.Cryptography.Csp.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Security.Cryptography.Encoding.dll") != "75b36584506dbd64b68e0d60a2189f70f03262d620cdf26b394b0abc97fe52fd"
  #error "Accepted payload mismatch (x64): System.Security.Cryptography.Encoding.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Security.Cryptography.Encoding.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Security.Cryptography.Encoding.dll") != "252d5080cf3ddd7ed9e49718f4011574aa521c900a0cbc6a840d0a048bf49bb7"
  #error "Accepted payload mismatch (x86): System.Security.Cryptography.Encoding.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Security.Cryptography.Encoding.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Security.Cryptography.OpenSsl.dll") != "dc856022ebdec7f030817c7a6a5fcda06aefaa7e2c7b4cd83d8525db476d9823"
  #error "Accepted payload mismatch (x64): System.Security.Cryptography.OpenSsl.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Security.Cryptography.OpenSsl.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Security.Cryptography.OpenSsl.dll") != "85b82c75f9ce4579755312789d364af79b97fcc6ac362efe5e857bc2a8b18568"
  #error "Accepted payload mismatch (x86): System.Security.Cryptography.OpenSsl.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Security.Cryptography.OpenSsl.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Security.Cryptography.Pkcs.dll") != "5f636c5b1ac88857c417edd4eea592c87d24f1c57b2f8bf1a588d36b1005fab6"
  #error "Accepted payload mismatch (x64): System.Security.Cryptography.Pkcs.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Security.Cryptography.Pkcs.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Security.Cryptography.Pkcs.dll") != "5422f688d6bb1a059f8776c2ccd6e52a8a99079be5a564ed7e01ee0f2f3ddac3"
  #error "Accepted payload mismatch (x86): System.Security.Cryptography.Pkcs.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Security.Cryptography.Pkcs.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Security.Cryptography.Primitives.dll") != "2d4f209f4b2f7bc0af1e7fb39c99349f6ad081689e89028a8dce1d580e1f39d9"
  #error "Accepted payload mismatch (x64): System.Security.Cryptography.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Security.Cryptography.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Security.Cryptography.Primitives.dll") != "93e380ef8b205a929020c3649d06a8c459b98381a94c8322de25ed73f4bab833"
  #error "Accepted payload mismatch (x86): System.Security.Cryptography.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Security.Cryptography.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Security.Cryptography.ProtectedData.dll") != "4538db41590e506e3bedc9d3f0a55230dd8abd0ecfbc0dbcc49dc904bb14e6c5"
  #error "Accepted payload mismatch (x64): System.Security.Cryptography.ProtectedData.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Security.Cryptography.ProtectedData.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Security.Cryptography.ProtectedData.dll") != "51e4bde65e383efdff2cecd83782ece954cf2443ea688688670ea3a5eb901449"
  #error "Accepted payload mismatch (x86): System.Security.Cryptography.ProtectedData.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Security.Cryptography.ProtectedData.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Security.Cryptography.X509Certificates.dll") != "6fe5848bafd502672fd206e4208b64e1aca84627427e131e33167b0795309207"
  #error "Accepted payload mismatch (x64): System.Security.Cryptography.X509Certificates.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Security.Cryptography.X509Certificates.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Security.Cryptography.X509Certificates.dll") != "a7cec64152b62d8eb9bba9c7db06a973ab895e9c8cc1c2c8086e67679e37708e"
  #error "Accepted payload mismatch (x86): System.Security.Cryptography.X509Certificates.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Security.Cryptography.X509Certificates.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Security.Cryptography.Xml.dll") != "a6881fb32d9318edef94694958dc06c15bcf162ac6bcf5d1792325f73e7bf23a"
  #error "Accepted payload mismatch (x64): System.Security.Cryptography.Xml.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Security.Cryptography.Xml.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Security.Cryptography.Xml.dll") != "3de8c602f2eebbb1e46ae6e4976f5a7c8bdae899c0d38c91bb86077dbbdcc4f5"
  #error "Accepted payload mismatch (x86): System.Security.Cryptography.Xml.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Security.Cryptography.Xml.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Security.Cryptography.dll") != "bea35c3eaf37895b32ff2b90d4b077d61732a7bcc7c9178aa441719d72503be0"
  #error "Accepted payload mismatch (x64): System.Security.Cryptography.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Security.Cryptography.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Security.Cryptography.dll") != "35dabe92c377fddc40e11150edc2626b362d740cb8e1a5a0bf9367f6ce464e8e"
  #error "Accepted payload mismatch (x86): System.Security.Cryptography.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Security.Cryptography.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Security.Permissions.dll") != "e56984c7c415f3d06503a8bc30b434269facd7d172af6a0287cc677ec1b55583"
  #error "Accepted payload mismatch (x64): System.Security.Permissions.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Security.Permissions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Security.Permissions.dll") != "e6897c2929d3a4c1f1ed3eb81595f83e8aa27d219c2e67346c6f2f4bca93a561"
  #error "Accepted payload mismatch (x86): System.Security.Permissions.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Security.Permissions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Security.Principal.Windows.dll") != "6d8813fb7478e09d1411e48143d05306a11c88e2d7b48ced68b527c3e3bc97db"
  #error "Accepted payload mismatch (x64): System.Security.Principal.Windows.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Security.Principal.Windows.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Security.Principal.Windows.dll") != "bc68fdad5f0a545b455e7132c24ed3020d8208e3520521471d00914de478222b"
  #error "Accepted payload mismatch (x86): System.Security.Principal.Windows.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Security.Principal.Windows.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Security.Principal.dll") != "089428925c94b2cd5a5faede9bf7799ffa144e899a1e56f1c2c05159660da580"
  #error "Accepted payload mismatch (x64): System.Security.Principal.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Security.Principal.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Security.Principal.dll") != "c6fc1db99e9e6d7b9545087f191ec5f0a4d2064e337984c6777716ed2526dd4a"
  #error "Accepted payload mismatch (x86): System.Security.Principal.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Security.Principal.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Security.SecureString.dll") != "f51d458bde0a367ebec626cc1c2247a374f5145170fd5df9fd501426f2cc2c4b"
  #error "Accepted payload mismatch (x64): System.Security.SecureString.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Security.SecureString.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Security.SecureString.dll") != "6708b01a065c569497780aaaaea90d06a6ebd08392b1b397804243f8e30afba3"
  #error "Accepted payload mismatch (x86): System.Security.SecureString.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Security.SecureString.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Security.dll") != "4887e93d7fbf6c6f0909579400ef892d42896a2123cc1a0c702f7c555e9e9d98"
  #error "Accepted payload mismatch (x64): System.Security.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Security.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Security.dll") != "c6adb3c2424670c0b06c169fe61fbb52305ae93bbf3ca1ae15414db25b1078ce"
  #error "Accepted payload mismatch (x86): System.Security.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Security.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.ServiceModel.Web.dll") != "9af5e984adf9989a462618c55de79429563ee20f583e40ad9d001f5022d0b4bd"
  #error "Accepted payload mismatch (x64): System.ServiceModel.Web.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.ServiceModel.Web.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.ServiceModel.Web.dll") != "1bc27d97dba619c97ee6131c8e5b7ceab65fae8b9a161715f16c498dfd5acf29"
  #error "Accepted payload mismatch (x86): System.ServiceModel.Web.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.ServiceModel.Web.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.ServiceProcess.dll") != "e40fc253a50bee22e6e20e1af1140bdeb5e92f4459f0427bc8a05a0e81e74821"
  #error "Accepted payload mismatch (x64): System.ServiceProcess.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.ServiceProcess.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.ServiceProcess.dll") != "bcb6a679671f66efa7a66573de0a54aeb4b1122033308550ec5895a9c58c22be"
  #error "Accepted payload mismatch (x86): System.ServiceProcess.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.ServiceProcess.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Text.Encoding.CodePages.dll") != "372a3490aa5e4f6a56625a34615597590475e75588dbf27a20202ce5c12ae998"
  #error "Accepted payload mismatch (x64): System.Text.Encoding.CodePages.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Text.Encoding.CodePages.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Text.Encoding.CodePages.dll") != "fbbe42ff7bfed21946c17d3e3141d42685558f0e88cd4dfe264eee54559be12a"
  #error "Accepted payload mismatch (x86): System.Text.Encoding.CodePages.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Text.Encoding.CodePages.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Text.Encoding.Extensions.dll") != "3fc0097ce6e2d9f30f180ec390213593b778c04668bb882f860157ba7f6e956d"
  #error "Accepted payload mismatch (x64): System.Text.Encoding.Extensions.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Text.Encoding.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Text.Encoding.Extensions.dll") != "3e08067de1e462b93df195e2fd187384f30ed704e874d4eaf110572e6d8e40aa"
  #error "Accepted payload mismatch (x86): System.Text.Encoding.Extensions.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Text.Encoding.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Text.Encoding.dll") != "677fef60be9acb153be1ccaac221ecc93c358231d21cbfd69c98ae98fa71929b"
  #error "Accepted payload mismatch (x64): System.Text.Encoding.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Text.Encoding.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Text.Encoding.dll") != "fac01a67049428420d52c5a8ca43bbc14d3e2fe1acdb6536fe51c66232079a80"
  #error "Accepted payload mismatch (x86): System.Text.Encoding.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Text.Encoding.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Text.Encodings.Web.dll") != "34236c5ddbadbd3f96d0aaeb20e4525e88534fc28f1ca0b016d83848529f1167"
  #error "Accepted payload mismatch (x64): System.Text.Encodings.Web.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Text.Encodings.Web.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Text.Encodings.Web.dll") != "91517f35300ffac41b368fec8603fcbf6514997071450de9eb3490ec136972aa"
  #error "Accepted payload mismatch (x86): System.Text.Encodings.Web.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Text.Encodings.Web.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Text.Json.dll") != "24556d153c5c00af2f5722a36aaf76939849676bca64fce21ad9b30ecc1a7f64"
  #error "Accepted payload mismatch (x64): System.Text.Json.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Text.Json.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Text.Json.dll") != "c0ddcd60b12775bc23fd39512a648dfe3a2ab5fae64208cb204d1b9ad3e9f849"
  #error "Accepted payload mismatch (x86): System.Text.Json.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Text.Json.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Text.RegularExpressions.dll") != "19cc43f776292f988fe33abb15fe5cdd1f0540e97a2ec631161c9207e30da6ef"
  #error "Accepted payload mismatch (x64): System.Text.RegularExpressions.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Text.RegularExpressions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Text.RegularExpressions.dll") != "41d34d338083acf82af81d673eb8486b578879f8c04c121a9308be79443c3043"
  #error "Accepted payload mismatch (x86): System.Text.RegularExpressions.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Text.RegularExpressions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Threading.AccessControl.dll") != "cf3cd1099d2bf0d14d2663413efa99ef35ed934d3515be1c7264df58ff79c1e2"
  #error "Accepted payload mismatch (x64): System.Threading.AccessControl.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Threading.AccessControl.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Threading.AccessControl.dll") != "f9a6ea488cbdea0697ceec546927d0531a0574124cce4eaf5acd7df3d9515228"
  #error "Accepted payload mismatch (x86): System.Threading.AccessControl.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Threading.AccessControl.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Threading.Channels.dll") != "91748936a96442fad345cceda65225708729950e3e19ab1259522f24f70bdc5a"
  #error "Accepted payload mismatch (x64): System.Threading.Channels.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Threading.Channels.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Threading.Channels.dll") != "05362c0f3e838045a895c9808f48f182bd64dc91bda57cf15ecd8aba49c34c1e"
  #error "Accepted payload mismatch (x86): System.Threading.Channels.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Threading.Channels.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Threading.Overlapped.dll") != "3042c32bb00bc3f4c182b29b39043b8bfe519dbc15ae31f1b7a57b8231958a58"
  #error "Accepted payload mismatch (x64): System.Threading.Overlapped.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Threading.Overlapped.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Threading.Overlapped.dll") != "5576c0047fcc8009558081d396bdaa9f0b63832b5019f05cb4b231dd12e3704a"
  #error "Accepted payload mismatch (x86): System.Threading.Overlapped.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Threading.Overlapped.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Threading.Tasks.Dataflow.dll") != "e17e81e9395f0620f8f19653a4e10cabdc968bfc2b4f8f0bfbfa729a1042537a"
  #error "Accepted payload mismatch (x64): System.Threading.Tasks.Dataflow.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Threading.Tasks.Dataflow.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Threading.Tasks.Dataflow.dll") != "47657207900df4823644890f703897521b36643525d4254b227f3aef4b9fe02d"
  #error "Accepted payload mismatch (x86): System.Threading.Tasks.Dataflow.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Threading.Tasks.Dataflow.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Threading.Tasks.Extensions.dll") != "c5273c4e16be5914a4975a7fe8c63d5db054931fe9bbb8fcef249e83ed246eb0"
  #error "Accepted payload mismatch (x64): System.Threading.Tasks.Extensions.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Threading.Tasks.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Threading.Tasks.Extensions.dll") != "11733586f902f2dc5c0e3aae3fe3d43f4fad5b3f760d0eb82a0b9075b431f283"
  #error "Accepted payload mismatch (x86): System.Threading.Tasks.Extensions.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Threading.Tasks.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Threading.Tasks.Parallel.dll") != "52aaf2bb3024068f062e92db12b6913b5c9ad3b92122b1ac0370fe0f40f06753"
  #error "Accepted payload mismatch (x64): System.Threading.Tasks.Parallel.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Threading.Tasks.Parallel.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Threading.Tasks.Parallel.dll") != "a0903bd8d8deca4ab4319b9c2833877a36baf28af5d92e7ff979d9fbde0b2340"
  #error "Accepted payload mismatch (x86): System.Threading.Tasks.Parallel.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Threading.Tasks.Parallel.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Threading.Tasks.dll") != "f0bb5a82e4f78a07499688319ba0bf5078758dddd5c6007028d65a18baeff45e"
  #error "Accepted payload mismatch (x64): System.Threading.Tasks.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Threading.Tasks.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Threading.Tasks.dll") != "d86a4d82792a4f1c270c0115006471627f5b1f3c53365f0ad0822a293d7a257d"
  #error "Accepted payload mismatch (x86): System.Threading.Tasks.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Threading.Tasks.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Threading.Thread.dll") != "cd8833667e8f837040194ee9cca2e3e047c50a897e1514f59049c9d95e354a42"
  #error "Accepted payload mismatch (x64): System.Threading.Thread.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Threading.Thread.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Threading.Thread.dll") != "bdbed99955f4a2ab1827b9cc32b41fd691141340ceb3e98cf5fe5eb36fa8b2e6"
  #error "Accepted payload mismatch (x86): System.Threading.Thread.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Threading.Thread.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Threading.ThreadPool.dll") != "5d6b7a66161bca76c646c3ef954386bddef60070317d7381bd1f3cae3b6ed4cb"
  #error "Accepted payload mismatch (x64): System.Threading.ThreadPool.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Threading.ThreadPool.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Threading.ThreadPool.dll") != "7f0cc6063a83537415a5c8134843668feb58b07d62ce30231913cef88f36b2c0"
  #error "Accepted payload mismatch (x86): System.Threading.ThreadPool.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Threading.ThreadPool.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Threading.Timer.dll") != "af0370e66f201115b9cc2bc774ee6e54858647381ed55bb72e44095478d03a44"
  #error "Accepted payload mismatch (x64): System.Threading.Timer.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Threading.Timer.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Threading.Timer.dll") != "80398e2390c4cb0b84aed7a386ac7492859716d0944decc26da4d838eb4c2d3c"
  #error "Accepted payload mismatch (x86): System.Threading.Timer.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Threading.Timer.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Threading.dll") != "ebaa9bd6123c18b401b0a6606d5be5275a4bb3fe73ff623698c07df256dd5e3e"
  #error "Accepted payload mismatch (x64): System.Threading.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Threading.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Threading.dll") != "a04d5273fbf7a42b49e9d26b3423110ee4fbeab0955ccc5d043ad2ad5fe28042"
  #error "Accepted payload mismatch (x86): System.Threading.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Threading.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Transactions.Local.dll") != "2d43ff58649a8b680a95731321a172473381a54fb18bd78063702ecd1804df6f"
  #error "Accepted payload mismatch (x64): System.Transactions.Local.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Transactions.Local.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Transactions.Local.dll") != "3b57a0f3ca078659eb76c55de4f674b86c6d2e8fef74404368547e204ae02592"
  #error "Accepted payload mismatch (x86): System.Transactions.Local.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Transactions.Local.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Transactions.dll") != "32c9035d0d0425ac81823e0dcf390da9873af970d0c2856f584ac2470d806550"
  #error "Accepted payload mismatch (x64): System.Transactions.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Transactions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Transactions.dll") != "6590e820a4abc496a5ef70ad1edadc8f8aa498d7c005316a5aa923eb4c6a1c53"
  #error "Accepted payload mismatch (x86): System.Transactions.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Transactions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.ValueTuple.dll") != "e6ec3e8c2013a3387978258aa60e947b46ef1d0bdb97d26f860609e696e1365e"
  #error "Accepted payload mismatch (x64): System.ValueTuple.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.ValueTuple.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.ValueTuple.dll") != "e1b9760dcf0483c111a1cbab44c4d6578fea38d94f7b74fc505085a77b395127"
  #error "Accepted payload mismatch (x86): System.ValueTuple.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.ValueTuple.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Web.HttpUtility.dll") != "a8228332990bae9ea580dc61cab15ac7a9d3ba02fc0ca66d88658fc9a60a1299"
  #error "Accepted payload mismatch (x64): System.Web.HttpUtility.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Web.HttpUtility.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Web.HttpUtility.dll") != "9357e6dcb94113cdb4ee01285055c7fa09b08ce61f1a549a7a9c81603d05bf6a"
  #error "Accepted payload mismatch (x86): System.Web.HttpUtility.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Web.HttpUtility.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Web.dll") != "86c80fb864eecf77cd27c40bba62c5385e44bb689c4fc3c123769ecf4d77efc2"
  #error "Accepted payload mismatch (x64): System.Web.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Web.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Web.dll") != "fa5c0d6d67eb46adcfb0075812c3c3098ca69cf381457cb09afc2079fd617005"
  #error "Accepted payload mismatch (x86): System.Web.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Web.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Windows.Controls.Ribbon.dll") != "02bdb12db731e0b882ad4004e06f18b418f8072e4bfe14c8ec0f4cf1a10f45b5"
  #error "Accepted payload mismatch (x64): System.Windows.Controls.Ribbon.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Windows.Controls.Ribbon.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Windows.Controls.Ribbon.dll") != "d487c3a533685b1f2d3cc202ff94cc88ec8e5522e0e6bf9b98e30ec967610b53"
  #error "Accepted payload mismatch (x86): System.Windows.Controls.Ribbon.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Windows.Controls.Ribbon.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Windows.Extensions.dll") != "d1fd61d926de0ceee305bce6ad639829cbafacf9e91c4999b999cfd88955cad6"
  #error "Accepted payload mismatch (x64): System.Windows.Extensions.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Windows.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Windows.Extensions.dll") != "18066d5e3d2556610945cbe4558064a7a82c7961c366324f1a844901f6cda09b"
  #error "Accepted payload mismatch (x86): System.Windows.Extensions.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Windows.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Windows.Forms.Design.Editors.dll") != "c6ce67b9a2e53c1abdd6791209e83be2ed72e34d52643ec55f96b23f6abb756f"
  #error "Accepted payload mismatch (x64): System.Windows.Forms.Design.Editors.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Windows.Forms.Design.Editors.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Windows.Forms.Design.Editors.dll") != "2769ddea85ace9bbd79b132310a885777db77cd533dcb3528a5c0ffd03403082"
  #error "Accepted payload mismatch (x86): System.Windows.Forms.Design.Editors.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Windows.Forms.Design.Editors.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Windows.Forms.Design.dll") != "f15d79f9c205a8189a38c753caa5a2c556bf601099d889d96df4b6c9bc86d1ae"
  #error "Accepted payload mismatch (x64): System.Windows.Forms.Design.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Windows.Forms.Design.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Windows.Forms.Design.dll") != "bee79194f10a47b8a4d8e04f02bb5f55ee363af8df320ebab1b7a533ca12f84e"
  #error "Accepted payload mismatch (x86): System.Windows.Forms.Design.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Windows.Forms.Design.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Windows.Forms.Primitives.dll") != "dac7659c0440b15657f3500f17b7c54d97e7e40e43dd8563904090d43a93e947"
  #error "Accepted payload mismatch (x64): System.Windows.Forms.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Windows.Forms.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Windows.Forms.Primitives.dll") != "83d609366e35e8d903b8b8fa5edb19f187155dfaa37eda18b98ba7f108296840"
  #error "Accepted payload mismatch (x86): System.Windows.Forms.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Windows.Forms.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Windows.Forms.dll") != "e64a7d4cc4988dc78f8231c8b2a2a501d3464f70e461fc72253207b194b3901d"
  #error "Accepted payload mismatch (x64): System.Windows.Forms.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Windows.Forms.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Windows.Forms.dll") != "8b000daf4f926de2360a8e4d1ed1a2869abb665a7ef3ea9402682f65e9d269b7"
  #error "Accepted payload mismatch (x86): System.Windows.Forms.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Windows.Forms.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Windows.Input.Manipulations.dll") != "06ab63add1f3c0e1e355a44e568d615b5bb5db5f7965fa880fe686d3fd79fcdd"
  #error "Accepted payload mismatch (x64): System.Windows.Input.Manipulations.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Windows.Input.Manipulations.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Windows.Input.Manipulations.dll") != "ad2ca9bf1e1ccdd74d31605075a69144c2beb0e90b73b00ff001adcb054b4e9d"
  #error "Accepted payload mismatch (x86): System.Windows.Input.Manipulations.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Windows.Input.Manipulations.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Windows.Presentation.dll") != "500549d35b85d7d1860d1a55475dc63e2187563806a34a4d627a0072fc8c0717"
  #error "Accepted payload mismatch (x64): System.Windows.Presentation.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Windows.Presentation.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Windows.Presentation.dll") != "bf49f5fe84ba3b91e200d0eb40836dfacae1ab4ad41aa49675b6a87372362766"
  #error "Accepted payload mismatch (x86): System.Windows.Presentation.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Windows.Presentation.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Windows.Primitives.dll") != "cb33b5f0d10eb3e1f03cc74b6f68c40c828c7c9a0240a216dfa3599bacc6736a"
  #error "Accepted payload mismatch (x64): System.Windows.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Windows.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Windows.Primitives.dll") != "5cc3960d1f188182e59f65fc6c880876fec525150057fe992e286b0ba22bb6df"
  #error "Accepted payload mismatch (x86): System.Windows.Primitives.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Windows.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Windows.dll") != "32dcb5c5f2dbd85b744ff708f39e3a703f5a75e96bb715569452601de11f1b4f"
  #error "Accepted payload mismatch (x64): System.Windows.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Windows.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Windows.dll") != "4ab0095216ec0c1d969c1f5f8a83b2734d5ee27b117ac5b5443d826fe6195dc3"
  #error "Accepted payload mismatch (x86): System.Windows.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Windows.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Xaml.dll") != "75b4d598e3ddcff1c78994f694fbf9a6aa6c505f762d11be828bd19011ed2918"
  #error "Accepted payload mismatch (x64): System.Xaml.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Xaml.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Xaml.dll") != "9cb84c0f9b41052aa8e7f59963425eec554184e52a7376359dc914fa1cde3606"
  #error "Accepted payload mismatch (x86): System.Xaml.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Xaml.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Xml.Linq.dll") != "dac823176f31b117dbac2ad9576f59607425eca50c73d02331ac3bc4852bb32e"
  #error "Accepted payload mismatch (x64): System.Xml.Linq.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Xml.Linq.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Xml.Linq.dll") != "060a4d35cdc9de8d57fb6de4887bbc634e82d18f2e0e3e14bb6c490f9f7cb8b0"
  #error "Accepted payload mismatch (x86): System.Xml.Linq.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Xml.Linq.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Xml.ReaderWriter.dll") != "2c0d73ceb380f0bf0e2620dd3c270d926c903dc563a5e8879333eaae5f57bd49"
  #error "Accepted payload mismatch (x64): System.Xml.ReaderWriter.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Xml.ReaderWriter.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Xml.ReaderWriter.dll") != "3a976b9817c62db1956e9687bd4b62c1b29add2e6fa51b48771c0b7566338b38"
  #error "Accepted payload mismatch (x86): System.Xml.ReaderWriter.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Xml.ReaderWriter.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Xml.Serialization.dll") != "3db5344fc841410d44647dd5a1fef3b1d543501d59ded9ebbc96be2372913191"
  #error "Accepted payload mismatch (x64): System.Xml.Serialization.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Xml.Serialization.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Xml.Serialization.dll") != "ace14b1695dc2196ed6e878fb1e4ef7d5ff28fb4b1f336d48500dd8f23d9ad62"
  #error "Accepted payload mismatch (x86): System.Xml.Serialization.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Xml.Serialization.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Xml.XDocument.dll") != "30651001959f0b6dfccd4432cbcfd0d07da0c5948d8914a109f108a9517d3b8b"
  #error "Accepted payload mismatch (x64): System.Xml.XDocument.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Xml.XDocument.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Xml.XDocument.dll") != "b0cdb095da9f2cea2ed5b1bad27900d9b72f8b7f5c1f2a4380705c9885c57137"
  #error "Accepted payload mismatch (x86): System.Xml.XDocument.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Xml.XDocument.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Xml.XPath.XDocument.dll") != "2dffedf3b83ab6c74f09eb71e0af6abc577d9df70545a09297546d2ad9c75eff"
  #error "Accepted payload mismatch (x64): System.Xml.XPath.XDocument.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Xml.XPath.XDocument.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Xml.XPath.XDocument.dll") != "160b52eea13f298b4f305fa27afd743f3329e2eee05513e1255ae9181eba69d0"
  #error "Accepted payload mismatch (x86): System.Xml.XPath.XDocument.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Xml.XPath.XDocument.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Xml.XPath.dll") != "eadca97de5fb0750e725c7e1f02ba20636fce6e0f8417e67d72f0a66e709a86f"
  #error "Accepted payload mismatch (x64): System.Xml.XPath.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Xml.XPath.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Xml.XPath.dll") != "eae05c7b5ea58013468cb432b99fd863b4f279105f53109befb0cca35579fe30"
  #error "Accepted payload mismatch (x86): System.Xml.XPath.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Xml.XPath.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Xml.XmlDocument.dll") != "dd70d2a5487da1fd127582dce7f9981dcba248d50683052055643dc9e321126b"
  #error "Accepted payload mismatch (x64): System.Xml.XmlDocument.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Xml.XmlDocument.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Xml.XmlDocument.dll") != "1de54d42e9731da1cac76d034943086a3a1d069c3288742338dac2a0be1214a4"
  #error "Accepted payload mismatch (x86): System.Xml.XmlDocument.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Xml.XmlDocument.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Xml.XmlSerializer.dll") != "d7a56b018cd7419e83b60aecbc5ab01a1bbcc2b17f6c29e31e1cec8d3b1c08f8"
  #error "Accepted payload mismatch (x64): System.Xml.XmlSerializer.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Xml.XmlSerializer.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Xml.XmlSerializer.dll") != "124ba1443e34dada7b1052c2460a6530aeef8bb47e2d29e3889e544a7b7815ab"
  #error "Accepted payload mismatch (x86): System.Xml.XmlSerializer.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Xml.XmlSerializer.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.Xml.dll") != "28f10f4ffad07b173af6114e74c5f57e8e02accb59850ef22abc4c6351ef349e"
  #error "Accepted payload mismatch (x64): System.Xml.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.Xml.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.Xml.dll") != "dd0ba42e98eb440823ef3f9ba60bff880f04819c70958ea9243d579fe0d2c288"
  #error "Accepted payload mismatch (x86): System.Xml.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.Xml.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\System.dll") != "22226debb0d6601d54f69ca122fd65af07bb869a6f8cb3141374bce0705e859d"
  #error "Accepted payload mismatch (x64): System.dll"
#endif
Source: "{#MyAppSourceDirX64}\System.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\System.dll") != "e41761b9bd31e895fbc9765dfb5bb2af276dd5f7516cdc0bdab790ce09e6a2fb"
  #error "Accepted payload mismatch (x86): System.dll"
#endif
Source: "{#MyAppSourceDirX86}\System.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\TimeZoneConverter.dll") != "d344e45cfce87c651340205b1ed8d5cb85b9d784a5eb219f43d7076d62a491ea"
  #error "Accepted payload mismatch (x86): TimeZoneConverter.dll"
#endif
#if GetSHA256OfFile(MyAppSourceDirX64 + "\TimeZoneConverter.dll") != "d344e45cfce87c651340205b1ed8d5cb85b9d784a5eb219f43d7076d62a491ea"
  #error "Accepted payload mismatch (x64): TimeZoneConverter.dll"
#endif
Source: "{#MyAppSourceDirX64}\TimeZoneConverter.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDirX64 + "\UIAutomationClient.dll") != "8ce3ffcf3d0761326e58481281868d59540d4057188f59e12fa27d662d1bbc44"
  #error "Accepted payload mismatch (x64): UIAutomationClient.dll"
#endif
Source: "{#MyAppSourceDirX64}\UIAutomationClient.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\UIAutomationClient.dll") != "c8bb3630b5956fec2a4319a1d8ef584339221cc9eb63b0d610d3a0eb941cc920"
  #error "Accepted payload mismatch (x86): UIAutomationClient.dll"
#endif
Source: "{#MyAppSourceDirX86}\UIAutomationClient.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\UIAutomationClientSideProviders.dll") != "55534151555a2dd8052b7d4d3d97bcb6f5cb726ccbb7dcd833a8b0550f900418"
  #error "Accepted payload mismatch (x64): UIAutomationClientSideProviders.dll"
#endif
Source: "{#MyAppSourceDirX64}\UIAutomationClientSideProviders.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\UIAutomationClientSideProviders.dll") != "c659bd63041c6e0a7928db4b0aa99551c76c663352b509571e0dc59a1e4bc436"
  #error "Accepted payload mismatch (x86): UIAutomationClientSideProviders.dll"
#endif
Source: "{#MyAppSourceDirX86}\UIAutomationClientSideProviders.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\UIAutomationProvider.dll") != "024a82a1f44cddc9762607d61a55ad73beaf09811f2686493cefe02378260907"
  #error "Accepted payload mismatch (x64): UIAutomationProvider.dll"
#endif
Source: "{#MyAppSourceDirX64}\UIAutomationProvider.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\UIAutomationProvider.dll") != "38b40fea6dd4d2ad1783fe55513a1288771f544f2490344ef124235d9f9483e1"
  #error "Accepted payload mismatch (x86): UIAutomationProvider.dll"
#endif
Source: "{#MyAppSourceDirX86}\UIAutomationProvider.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\UIAutomationTypes.dll") != "c336708aeef1a9254cf685d76600891142c252b7cc2019e00cc764fe2b37ba44"
  #error "Accepted payload mismatch (x64): UIAutomationTypes.dll"
#endif
Source: "{#MyAppSourceDirX64}\UIAutomationTypes.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\UIAutomationTypes.dll") != "1eec5996606db69f9f7b82cb34223d19bb44d68e0b8e647052df4c00abb324b5"
  #error "Accepted payload mismatch (x86): UIAutomationTypes.dll"
#endif
Source: "{#MyAppSourceDirX86}\UIAutomationTypes.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\WinRT.Runtime.dll") != "bcf3a14e8712a90837fc5c0d8c8a24696af2bd7f74e9767597ec81ededfb23db"
  #error "Accepted payload mismatch (x86): WinRT.Runtime.dll"
#endif
#if GetSHA256OfFile(MyAppSourceDirX64 + "\WinRT.Runtime.dll") != "bcf3a14e8712a90837fc5c0d8c8a24696af2bd7f74e9767597ec81ededfb23db"
  #error "Accepted payload mismatch (x64): WinRT.Runtime.dll"
#endif
Source: "{#MyAppSourceDirX64}\WinRT.Runtime.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDirX64 + "\WindowsBase.dll") != "d0d5387034e8d0625a2fa9830dcd52cf1f591d9aff5f8689fe2cc00db7a49ec8"
  #error "Accepted payload mismatch (x64): WindowsBase.dll"
#endif
Source: "{#MyAppSourceDirX64}\WindowsBase.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\WindowsBase.dll") != "efb16b4a55eee661826b8ae0dabd693e5b045c03159ce1bca9c0d0c0ad59032f"
  #error "Accepted payload mismatch (x86): WindowsBase.dll"
#endif
Source: "{#MyAppSourceDirX86}\WindowsBase.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\WindowsFormsIntegration.dll") != "d2d133c35fe183f6924d8502d73ab3ee4d341a99d0b44dbfbb6af71af7b132d1"
  #error "Accepted payload mismatch (x64): WindowsFormsIntegration.dll"
#endif
Source: "{#MyAppSourceDirX64}\WindowsFormsIntegration.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\WindowsFormsIntegration.dll") != "4c4207a93cd38267e6445b2b6bea2a1e3e34f3d8ff5c688e5721a0533cadead5"
  #error "Accepted payload mismatch (x86): WindowsFormsIntegration.dll"
#endif
Source: "{#MyAppSourceDirX86}\WindowsFormsIntegration.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\clretwrc.dll") != "f25a11d20b069b3bab1279cfef14d46c058bc2dfe8d2fe8fe8d74fefe52448a0"
  #error "Accepted payload mismatch (x64): clretwrc.dll"
#endif
Source: "{#MyAppSourceDirX64}\clretwrc.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\clretwrc.dll") != "e2c3bf6e488418227f0a651b16322bc3c0476295a7f58b61469c431d33f4234c"
  #error "Accepted payload mismatch (x86): clretwrc.dll"
#endif
Source: "{#MyAppSourceDirX86}\clretwrc.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\clrgc.dll") != "20a907670cdf3162d9b300914991d10ba28fc3c6b704377b58322c2886d9b8b3"
  #error "Accepted payload mismatch (x64): clrgc.dll"
#endif
Source: "{#MyAppSourceDirX64}\clrgc.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\clrgc.dll") != "76ac1c2057fdb21d5caf187af9c85c70feed76a6c3bf413d1f082dc177547153"
  #error "Accepted payload mismatch (x86): clrgc.dll"
#endif
Source: "{#MyAppSourceDirX86}\clrgc.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\clrgcexp.dll") != "b574745f1cebab1a7479b184a0299a5e284508f4095c7c0af72651de80461c53"
  #error "Accepted payload mismatch (x64): clrgcexp.dll"
#endif
Source: "{#MyAppSourceDirX64}\clrgcexp.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\clrjit.dll") != "22231ec740134103c575c7869b060b4ba2250b4ab9b5f964b93e94298b53a1e1"
  #error "Accepted payload mismatch (x64): clrjit.dll"
#endif
Source: "{#MyAppSourceDirX64}\clrjit.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\clrjit.dll") != "6da32dcd16debc8615b802cb28d453fe6acb7aec9b0ffd028978541867db0693"
  #error "Accepted payload mismatch (x86): clrjit.dll"
#endif
Source: "{#MyAppSourceDirX86}\clrjit.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\coreclr.dll") != "a6988fc77326c245873986446de4f15218df929664750d333cb2d42486c071da"
  #error "Accepted payload mismatch (x64): coreclr.dll"
#endif
Source: "{#MyAppSourceDirX64}\coreclr.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\coreclr.dll") != "825e62eb5412565b1f3239d4828cf4fbd0dd68509820e26a0948fbbab22da41c"
  #error "Accepted payload mismatch (x86): coreclr.dll"
#endif
Source: "{#MyAppSourceDirX86}\coreclr.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\createdump.exe") != "5161f24ccbe3af9ff224cab8d667f87920a58136dd045b054999e19ac29004f3"
  #error "Accepted payload mismatch (x64): createdump.exe"
#endif
Source: "{#MyAppSourceDirX64}\createdump.exe"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\createdump.exe") != "a1ecf1ef1698fc9511bb5f9569bc95805982b95da2a21ab07b7b620b7f0c0730"
  #error "Accepted payload mismatch (x86): createdump.exe"
#endif
Source: "{#MyAppSourceDirX86}\createdump.exe"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\cs\Microsoft.VisualBasic.Forms.resources.dll") != "d31c75788fc92d092c02e54d55fabc672f4fe7d2eebc402e75a6255b42a13073"
  #error "Accepted payload mismatch (x64): cs\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\cs\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\cs\Microsoft.VisualBasic.Forms.resources.dll") != "5bcf98a03409fe31ce23c0439fde8c37fca651e49deeb35c73515d970aa90dbf"
  #error "Accepted payload mismatch (x86): cs\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\cs\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\cs\PresentationCore.resources.dll") != "ac74e5248d7ae1db1502f4c5697c62b53873447ca06b4dbe1411b30e4748e559"
  #error "Accepted payload mismatch (x64): cs\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\cs\PresentationCore.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\cs\PresentationCore.resources.dll") != "af61a7e92e926b25359cc33a7cb7fa94a958ef2e359f6bc843911fafb8bc4076"
  #error "Accepted payload mismatch (x86): cs\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\cs\PresentationCore.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\cs\PresentationFramework.resources.dll") != "5b057e077bdcbd09fafdeff6891d43e091acddbecbb3576cbac5b7dfd7f15558"
  #error "Accepted payload mismatch (x64): cs\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\cs\PresentationFramework.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\cs\PresentationFramework.resources.dll") != "8b2012dca0272ca32328ee6bd886b8040739a2ab26d7a872d54dbef874d6c6d7"
  #error "Accepted payload mismatch (x86): cs\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\cs\PresentationFramework.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\cs\PresentationUI.resources.dll") != "850904e4f0ee0517d06f1f636f45f0bc158dfc070667c0ba6eca06bd5cf2c122"
  #error "Accepted payload mismatch (x64): cs\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\cs\PresentationUI.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\cs\PresentationUI.resources.dll") != "d91ca6cd20d39cfae2542db9a5fb3ff0344aaab2f7fd8a868b86d04b4876beb4"
  #error "Accepted payload mismatch (x86): cs\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\cs\PresentationUI.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\cs\ReachFramework.resources.dll") != "741fcfd77e9b548a9d12c9cd9336ef92673b9e1772cb738445f5eab01c9efbae"
  #error "Accepted payload mismatch (x64): cs\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\cs\ReachFramework.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\cs\ReachFramework.resources.dll") != "22ff144dc78cdaf8aa503c0ce3d1a83357ded96d8eb1dbab54d8f80d503d766e"
  #error "Accepted payload mismatch (x86): cs\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\cs\ReachFramework.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\cs\System.Windows.Controls.Ribbon.resources.dll") != "a39f400a3cea74558cf0c3503e0f743794e0c3d1c6295278b9045f59b4e079bc"
  #error "Accepted payload mismatch (x64): cs\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\cs\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\cs\System.Windows.Controls.Ribbon.resources.dll") != "18a0a6c2fb0134db7f75682e76302362eacfda6fcc2a2b3dfc5fcde00e96111f"
  #error "Accepted payload mismatch (x86): cs\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\cs\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\cs\System.Windows.Forms.Design.resources.dll") != "9aa9dce15d37834ebc654f0aeafff89aa652ac2fbe8ccfbff44a91b0d489f872"
  #error "Accepted payload mismatch (x64): cs\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\cs\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\cs\System.Windows.Forms.Design.resources.dll") != "351512b90f57397b5bcce05a3da06c05b461a238662b83d901ff865064584e2a"
  #error "Accepted payload mismatch (x86): cs\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\cs\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\cs\System.Windows.Forms.Primitives.resources.dll") != "87febce16fdd1ae3e4b09e5b239bc40205277187d5975ca98a7dadbb700df743"
  #error "Accepted payload mismatch (x64): cs\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\cs\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\cs\System.Windows.Forms.Primitives.resources.dll") != "e4bdd6b72815f629df41bf476eb11cfa0799257bf873c2969fec5cd02105f12f"
  #error "Accepted payload mismatch (x86): cs\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\cs\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\cs\System.Windows.Forms.resources.dll") != "9856da735778baca258e8fe4d1aaa2d0620fdaa69aab57060737c6d14e3061c1"
  #error "Accepted payload mismatch (x64): cs\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\cs\System.Windows.Forms.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\cs\System.Windows.Forms.resources.dll") != "e20db498181ba9cdb632b98a541fba296d2f7bf0cb594d42ea0cf8425209b142"
  #error "Accepted payload mismatch (x86): cs\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\cs\System.Windows.Forms.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\cs\System.Windows.Input.Manipulations.resources.dll") != "7271ec25d2b33ddcaa37d7cef863f2d5c87c11f6f5e4a1d12b9f1364ce385954"
  #error "Accepted payload mismatch (x64): cs\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\cs\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\cs\System.Windows.Input.Manipulations.resources.dll") != "b6629d187ee768e3a28e740b21ad303c0eca6d3db818ce24f45b32b819b78178"
  #error "Accepted payload mismatch (x86): cs\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\cs\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\cs\System.Xaml.resources.dll") != "5cbac22be0790dbcb9a3d34802937bb81931f43676e5a743ff5587068fe16b8e"
  #error "Accepted payload mismatch (x64): cs\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\cs\System.Xaml.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\cs\System.Xaml.resources.dll") != "ef776347a7334ca6119eca058a5df42819072a4f5eb2a9ad690afa4133c43704"
  #error "Accepted payload mismatch (x86): cs\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\cs\System.Xaml.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\cs\UIAutomationClient.resources.dll") != "e114297e542027437fe98f20d40a6072d4b771904e7fd4ecdd6e2db85b60a079"
  #error "Accepted payload mismatch (x64): cs\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\cs\UIAutomationClient.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\cs\UIAutomationClient.resources.dll") != "931376c3d2045e5c7aeab027f2b27293ba4913b1f8cc5779fd1d894ba778e515"
  #error "Accepted payload mismatch (x86): cs\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\cs\UIAutomationClient.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\cs\UIAutomationClientSideProviders.resources.dll") != "a2ebed7f8cea042654fe324120e5995eb3134a919da18e2b770e4362075bb8ab"
  #error "Accepted payload mismatch (x64): cs\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\cs\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\cs\UIAutomationClientSideProviders.resources.dll") != "a8b1e3e4dc5f0bcd009918f312f64f555f1ca2d650145cbd773a26297cf1f5f0"
  #error "Accepted payload mismatch (x86): cs\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\cs\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\cs\UIAutomationProvider.resources.dll") != "41f8ad73fbe546b6aeb1f6d89676655eb5c579ec67bb115aa4aab92a06fba478"
  #error "Accepted payload mismatch (x64): cs\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\cs\UIAutomationProvider.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\cs\UIAutomationProvider.resources.dll") != "9f9f1996833b7939a27925e9145eaf8f927a742cc8a711a0bb54334262c66d4c"
  #error "Accepted payload mismatch (x86): cs\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\cs\UIAutomationProvider.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\cs\UIAutomationTypes.resources.dll") != "ed21aeef21f2cb2d04fae1129da1289de83092dbd5d8acc64365a19078193ca4"
  #error "Accepted payload mismatch (x64): cs\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\cs\UIAutomationTypes.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\cs\UIAutomationTypes.resources.dll") != "b5ee2efd62ab9f2b3648af901cb467e06af533d31a1ae2f089d9c08ded33e29e"
  #error "Accepted payload mismatch (x86): cs\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\cs\UIAutomationTypes.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\cs\WindowsBase.resources.dll") != "91f7a22f183b7ebd839ef3f61c9573ff46a627a916b8381444cfb090caa6567f"
  #error "Accepted payload mismatch (x64): cs\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\cs\WindowsBase.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\cs\WindowsBase.resources.dll") != "5f7d90534d55fe0e3c722556a4ff7a7ba47b37c6cce91b841f75859f0e2a8b82"
  #error "Accepted payload mismatch (x86): cs\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\cs\WindowsBase.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\cs\WindowsFormsIntegration.resources.dll") != "4e1b4ff8c733345997ef7e688efb43a90dfb48fd92cf78d418354a7a78800b11"
  #error "Accepted payload mismatch (x64): cs\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\cs\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\cs\WindowsFormsIntegration.resources.dll") != "1e1d5d7414baadb3a787b3a2847efb961b30a54f7ce1c7e2279d134fd965810f"
  #error "Accepted payload mismatch (x86): cs\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\cs\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\de\Microsoft.VisualBasic.Forms.resources.dll") != "10d567773a33067adc1aca3bf8bc06bc2a00e75e3fadd160d6711225f9912ad4"
  #error "Accepted payload mismatch (x64): de\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\de\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\de\Microsoft.VisualBasic.Forms.resources.dll") != "329d72d21185cfc025603b9e9e9f66797288fa1ed29a1db551a31a2c1ddc3ecc"
  #error "Accepted payload mismatch (x86): de\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\de\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\de\PresentationCore.resources.dll") != "7cd072627ed82196b6923d078ab2c77dba4d4dbdafa242c0d5af5dfe42f46e1b"
  #error "Accepted payload mismatch (x64): de\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\de\PresentationCore.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\de\PresentationCore.resources.dll") != "fd3f641a5079fb63e0a7b614713363d7755a28d3240c3a7092ff23414d52ffc5"
  #error "Accepted payload mismatch (x86): de\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\de\PresentationCore.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\de\PresentationFramework.resources.dll") != "3b041c9ce01f776a128cd380ca99b3d3235bf9fe2c5cd43bc5c15d602a7c12d7"
  #error "Accepted payload mismatch (x64): de\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\de\PresentationFramework.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\de\PresentationFramework.resources.dll") != "3803a71252c00645f087ff231fa904779a0e70b8929ce5b54110cff71a2b4229"
  #error "Accepted payload mismatch (x86): de\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\de\PresentationFramework.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\de\PresentationUI.resources.dll") != "6fb053c24f261cbf3d1eb4a485f0ac9ee55c2ddada42d1b1745c63091a946881"
  #error "Accepted payload mismatch (x64): de\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\de\PresentationUI.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\de\PresentationUI.resources.dll") != "a87d000cda256b6c0228cbb9a73776e4c9b762bf8e08636b7e556cd9168be075"
  #error "Accepted payload mismatch (x86): de\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\de\PresentationUI.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\de\ReachFramework.resources.dll") != "45d7f790fb02d0451216a31e459f226a0f2dea5ce5c1c924cdc302d5646bfbb3"
  #error "Accepted payload mismatch (x64): de\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\de\ReachFramework.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\de\ReachFramework.resources.dll") != "60b843bf4b66fb0727dfb8232f385b5ed821b9233d7a1228a5bdbc3482a5fa37"
  #error "Accepted payload mismatch (x86): de\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\de\ReachFramework.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\de\System.Windows.Controls.Ribbon.resources.dll") != "c1df1a043e1de549c39ff2347b73ce7c024e7c4640919ed6ae005586084162a5"
  #error "Accepted payload mismatch (x64): de\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\de\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\de\System.Windows.Controls.Ribbon.resources.dll") != "26f65f91ad25e62ce7784da5a073450599be5a4853073be4d5a1ca7ad0283b36"
  #error "Accepted payload mismatch (x86): de\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\de\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\de\System.Windows.Forms.Design.resources.dll") != "17782abef02e8411e347532f65bfd1a4cb358e981ffb6d080f68ed81ccc3721f"
  #error "Accepted payload mismatch (x64): de\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\de\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\de\System.Windows.Forms.Design.resources.dll") != "21a721041b6031177908194631fd0358339869778cd359f5ed15cde34cca8243"
  #error "Accepted payload mismatch (x86): de\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\de\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\de\System.Windows.Forms.Primitives.resources.dll") != "a868b161bdc28cfae4d7546015a01504d6f9579839f27211e26c171fd0cae945"
  #error "Accepted payload mismatch (x64): de\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\de\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\de\System.Windows.Forms.Primitives.resources.dll") != "d02d3c8348e4bf71a45501981058f5092be835d6806cabb9587124fbfaa6216a"
  #error "Accepted payload mismatch (x86): de\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\de\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\de\System.Windows.Forms.resources.dll") != "8f7c0bfdc30baeaa71425b4d41d98bb6e4cfe7fd0d5c4a72ca1f17fa0b802db2"
  #error "Accepted payload mismatch (x64): de\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\de\System.Windows.Forms.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\de\System.Windows.Forms.resources.dll") != "e91ed269f5115949f520effb2d4a0529b0a2b0bc77ba15f7fda7f01b2b5894f6"
  #error "Accepted payload mismatch (x86): de\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\de\System.Windows.Forms.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\de\System.Windows.Input.Manipulations.resources.dll") != "588112c789c33cf90cea6c15aca0d5b9d507ce7b85754a508d25f866cf9630be"
  #error "Accepted payload mismatch (x64): de\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\de\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\de\System.Windows.Input.Manipulations.resources.dll") != "65e245a97fd68c2c8f72401dd3305677b60dfc74aed416ba0dee6576e640d5d8"
  #error "Accepted payload mismatch (x86): de\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\de\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\de\System.Xaml.resources.dll") != "a11b329afb26b5e56f6a0acb5768840c642784a731bfd1547bfc2443a9050f00"
  #error "Accepted payload mismatch (x64): de\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\de\System.Xaml.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\de\System.Xaml.resources.dll") != "345af18f2d99a9df2e02c6ed742b02d979cadf7f1248fa8a610d68e7a047d5f1"
  #error "Accepted payload mismatch (x86): de\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\de\System.Xaml.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\de\UIAutomationClient.resources.dll") != "f8b8532cc8140df250320f177c69d8a47187ef6234efbc041e7ab53a651df8bc"
  #error "Accepted payload mismatch (x64): de\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\de\UIAutomationClient.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\de\UIAutomationClient.resources.dll") != "c92f286be706d49d042fe38b35134b7365a94c18bb3dab7ae3d32eb7070129d3"
  #error "Accepted payload mismatch (x86): de\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\de\UIAutomationClient.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\de\UIAutomationClientSideProviders.resources.dll") != "fa46d5db0299750598b5a47cc43f4a52008c8d933b4245d19ff2909e686801d5"
  #error "Accepted payload mismatch (x64): de\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\de\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\de\UIAutomationClientSideProviders.resources.dll") != "9bd7af17b031e2b3098d93aa06f1b1c6df27ed74a22cad70e84dc080ef19b297"
  #error "Accepted payload mismatch (x86): de\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\de\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\de\UIAutomationProvider.resources.dll") != "7a8fed2cc4dc418b22c9a977cd470ca3d7c7cb2c68d738497a5ec842bffe768c"
  #error "Accepted payload mismatch (x64): de\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\de\UIAutomationProvider.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\de\UIAutomationProvider.resources.dll") != "3d1a7046309cef705dbeb9a5d9800e8b24143562030acf3f51f4436353de4de3"
  #error "Accepted payload mismatch (x86): de\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\de\UIAutomationProvider.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\de\UIAutomationTypes.resources.dll") != "aa0e2f165379418563fec9acf7f202237eb6d9235d81cde2c368b1ad109623d7"
  #error "Accepted payload mismatch (x64): de\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\de\UIAutomationTypes.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\de\UIAutomationTypes.resources.dll") != "a01c3983db79e4222ca3b37aea3f7d1f3012b8b0340172586c6a309f56857bf6"
  #error "Accepted payload mismatch (x86): de\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\de\UIAutomationTypes.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\de\WindowsBase.resources.dll") != "21c9d6cd68bea7c385a8980fe7843fc67f196ec618da08df00d5e8249171f105"
  #error "Accepted payload mismatch (x64): de\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\de\WindowsBase.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\de\WindowsBase.resources.dll") != "4e5f324e8119a1ec29d8cd9129d3af52c9f179f87fe80bab46888d3c68b10a96"
  #error "Accepted payload mismatch (x86): de\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\de\WindowsBase.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\de\WindowsFormsIntegration.resources.dll") != "1eb126bfe3e9e467df237ac7920104b8c18bf1e975dbead4c3391b11e6416739"
  #error "Accepted payload mismatch (x64): de\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\de\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\de\WindowsFormsIntegration.resources.dll") != "10155166f82d3a0e3c92b501608714409056a945605acb60c20c82f123ca0c90"
  #error "Accepted payload mismatch (x86): de\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\de\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\es\Microsoft.VisualBasic.Forms.resources.dll") != "ac05982841e43b17e230e14cca7bf16345fe2ea696d7b6ce12abb2e8ea59504c"
  #error "Accepted payload mismatch (x64): es\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\es\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\es\Microsoft.VisualBasic.Forms.resources.dll") != "068a13f5c5da66f89107fadbf8d50d014c4216de8fb258faf2de235f6092b5a1"
  #error "Accepted payload mismatch (x86): es\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\es\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\es\PresentationCore.resources.dll") != "d2d2fc44e5cf2798e42e87cb69f0a4e1a916404b1f008bba7390fc0cfd3a2088"
  #error "Accepted payload mismatch (x64): es\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\es\PresentationCore.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\es\PresentationCore.resources.dll") != "37e51060bbdd1953a9b8895e479e2930346021e471f442aa48058b2f2415b222"
  #error "Accepted payload mismatch (x86): es\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\es\PresentationCore.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\es\PresentationFramework.resources.dll") != "71e8f43017313ecc9ece724d7046e94c6752f35b74997560ee62d253a808e1dd"
  #error "Accepted payload mismatch (x64): es\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\es\PresentationFramework.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\es\PresentationFramework.resources.dll") != "b3fb8b1eac24b528d824860195cba4bb6680358033b17f1e9aa20a0ecbba6c9e"
  #error "Accepted payload mismatch (x86): es\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\es\PresentationFramework.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\es\PresentationUI.resources.dll") != "eeb870215521bd6a7c94952eb0182c4423393eb815d264a803dc1f4548fc3af1"
  #error "Accepted payload mismatch (x64): es\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\es\PresentationUI.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\es\PresentationUI.resources.dll") != "b6e0fb15a825d678daf021fc23b9ba57d1e28568bd51e2c015f802ead20c0afc"
  #error "Accepted payload mismatch (x86): es\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\es\PresentationUI.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\es\ReachFramework.resources.dll") != "71721c7f89940d8731fbf3a1c7653b6389d6db111b058fcd5be0949ae1eea9bb"
  #error "Accepted payload mismatch (x64): es\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\es\ReachFramework.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\es\ReachFramework.resources.dll") != "80c23f11bf0776c5260767e520fd51505dc5598d1cd702844c5be7765b45febe"
  #error "Accepted payload mismatch (x86): es\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\es\ReachFramework.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\es\System.Windows.Controls.Ribbon.resources.dll") != "c7fffea74cb421f8bf7aaa718ba13c4eb1f825a93de6a863ad49fb75c93c9e40"
  #error "Accepted payload mismatch (x64): es\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\es\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\es\System.Windows.Controls.Ribbon.resources.dll") != "612b2e8db61e1764aba4caf03d550f3f7cb526cb614a3b0c6ac6058909088ff4"
  #error "Accepted payload mismatch (x86): es\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\es\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\es\System.Windows.Forms.Design.resources.dll") != "635d3e2932b3473a99c6e7e5b6350e315c07f84059e513a9c800a144a536d5f5"
  #error "Accepted payload mismatch (x64): es\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\es\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\es\System.Windows.Forms.Design.resources.dll") != "7b24c752aa0273a36ab3faac55022d9126937cb3aa0520677e92aeba0b245653"
  #error "Accepted payload mismatch (x86): es\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\es\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\es\System.Windows.Forms.Primitives.resources.dll") != "d745b599f97647ca95a5fa2251627d9dffb8ab7ce5782009bcc607334b7008a3"
  #error "Accepted payload mismatch (x64): es\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\es\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\es\System.Windows.Forms.Primitives.resources.dll") != "3258b0279e6bdbbde8913fb37d7542d7e2b2a9efdac8ba4078bfffcd9f4d2861"
  #error "Accepted payload mismatch (x86): es\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\es\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\es\System.Windows.Forms.resources.dll") != "38f55975bfd3a5321b134d5de6615edc6e01c887ae5361d2e90ee99e53edaf3c"
  #error "Accepted payload mismatch (x64): es\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\es\System.Windows.Forms.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\es\System.Windows.Forms.resources.dll") != "e33098d93aff2718edf6478dc3afd583370db526ad7f5c49d526ce1132414329"
  #error "Accepted payload mismatch (x86): es\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\es\System.Windows.Forms.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\es\System.Windows.Input.Manipulations.resources.dll") != "d05da7880fffdcfbe569603bcad98ce397b06ff46f1d1a68373b4ae6bf690fac"
  #error "Accepted payload mismatch (x64): es\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\es\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\es\System.Windows.Input.Manipulations.resources.dll") != "4faea574775a44c4195fa9aa6a0ecbf08ca396c3ba89d7303cce04a4dc19e68f"
  #error "Accepted payload mismatch (x86): es\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\es\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\es\System.Xaml.resources.dll") != "a2639b69aae31737349fd1289652987878ea1ff989fa289927752c6bbcdb568e"
  #error "Accepted payload mismatch (x64): es\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\es\System.Xaml.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\es\System.Xaml.resources.dll") != "08f40fa9b283d219c5df27f17c49b3c2030b7e1f784fd23cd3c0906b7aa47107"
  #error "Accepted payload mismatch (x86): es\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\es\System.Xaml.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\es\UIAutomationClient.resources.dll") != "850cb2c46c1011608ba5eceea5b08c7db5b36c2b4a907da50ed9e6413b267647"
  #error "Accepted payload mismatch (x64): es\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\es\UIAutomationClient.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\es\UIAutomationClient.resources.dll") != "8215bc3973a674af886db5dceb5cc838aac070ac49be760a1e562b203c170318"
  #error "Accepted payload mismatch (x86): es\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\es\UIAutomationClient.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\es\UIAutomationClientSideProviders.resources.dll") != "b5a1936e357318fa1aafdbb1fd69369ef30e91b71f9b166e02f8264d1c20433d"
  #error "Accepted payload mismatch (x64): es\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\es\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\es\UIAutomationClientSideProviders.resources.dll") != "4fc9b1f34c136ec57a12d4654dd7d06c4b16fbd88a43fe7cc6f8af0f0fe11db5"
  #error "Accepted payload mismatch (x86): es\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\es\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\es\UIAutomationProvider.resources.dll") != "7fa37a71dc6622c6c77a6c1c17f744810943991727cbc3790ab6c0d52cfbed5b"
  #error "Accepted payload mismatch (x64): es\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\es\UIAutomationProvider.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\es\UIAutomationProvider.resources.dll") != "3b23c2d8f35e557311dfa613b8cd2533609b20279087d469055b5cc2ec1be03d"
  #error "Accepted payload mismatch (x86): es\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\es\UIAutomationProvider.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\es\UIAutomationTypes.resources.dll") != "803db4e00255009bfd4efd8e7902f67f09e68101668fed5e76dcf17684f68953"
  #error "Accepted payload mismatch (x64): es\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\es\UIAutomationTypes.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\es\UIAutomationTypes.resources.dll") != "e5ee2a38a27707f35bb7333b0caf015b10346ec6b5646dcb6a01190aedae1489"
  #error "Accepted payload mismatch (x86): es\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\es\UIAutomationTypes.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\es\WindowsBase.resources.dll") != "4bf0bcccf3e6b22365897618520918959f30b436a3c9b22b6e6c91dd9e6ab4a4"
  #error "Accepted payload mismatch (x64): es\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\es\WindowsBase.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\es\WindowsBase.resources.dll") != "4439af999337ca458c25d32317d70cf1f7a0225cfb4767facec5663fd128c716"
  #error "Accepted payload mismatch (x86): es\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\es\WindowsBase.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\es\WindowsFormsIntegration.resources.dll") != "fbf2587acfb89eb09f2115f73837fe5158b6be5c9eab0442ad3704e55dbd255f"
  #error "Accepted payload mismatch (x64): es\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\es\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\es\WindowsFormsIntegration.resources.dll") != "c118144e08baa615f2a447718ef38d004aff1294a7aad158699f3b16df31b5f2"
  #error "Accepted payload mismatch (x86): es\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\es\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\fr\Microsoft.VisualBasic.Forms.resources.dll") != "97fe8f6aa81a9acbbef39b8a83cebdf0be4c674e5d2dc2dd89e3cd4b0d207779"
  #error "Accepted payload mismatch (x64): fr\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\fr\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\fr\Microsoft.VisualBasic.Forms.resources.dll") != "8ea08e4a94f7da5f2e6a72e00379166cb45722bf951958d109c214c01625b071"
  #error "Accepted payload mismatch (x86): fr\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\fr\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\fr\PresentationCore.resources.dll") != "0cd31db6aa7336fcccdaabe281178ff2049631fc15d65a495f0c4055c5ebab13"
  #error "Accepted payload mismatch (x64): fr\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\fr\PresentationCore.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\fr\PresentationCore.resources.dll") != "44441f00f35a449ea94f1989c19ff8b33e764faa3af4e07a89e3acc1e6530881"
  #error "Accepted payload mismatch (x86): fr\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\fr\PresentationCore.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\fr\PresentationFramework.resources.dll") != "6fc498cc8f54bec8075188b8bae83d0d8efc131f04b06c923411303a23a879dc"
  #error "Accepted payload mismatch (x64): fr\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\fr\PresentationFramework.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\fr\PresentationFramework.resources.dll") != "d7900fee0b47e50d65827b5e79de31e6172e208ff218396a077b843c9af049d4"
  #error "Accepted payload mismatch (x86): fr\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\fr\PresentationFramework.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\fr\PresentationUI.resources.dll") != "7aac5b8784402f5c218bd0be38ddbdbc6a824bca261c59aa70b7b7b68b6afcb3"
  #error "Accepted payload mismatch (x64): fr\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\fr\PresentationUI.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\fr\PresentationUI.resources.dll") != "fe732b4b89166dd343c17eb377ef1d41a02a18974665380c6b8bab7545ce3dcb"
  #error "Accepted payload mismatch (x86): fr\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\fr\PresentationUI.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\fr\ReachFramework.resources.dll") != "10383e1b7448f785638ac7a308c497f90fb0ceaef034d96563fbb9c4beeb96fb"
  #error "Accepted payload mismatch (x64): fr\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\fr\ReachFramework.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\fr\ReachFramework.resources.dll") != "f84d61fe0dfc1c2d288da4ee5b3a398713ebf9b671a0ac723581ffa0568437d2"
  #error "Accepted payload mismatch (x86): fr\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\fr\ReachFramework.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\fr\System.Windows.Controls.Ribbon.resources.dll") != "4054a26acd891dffbc2532413444bef53241dd32a41a29efdfaee8646aa4fc9f"
  #error "Accepted payload mismatch (x64): fr\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\fr\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\fr\System.Windows.Controls.Ribbon.resources.dll") != "5b0e0272510deb0722c9c17fc70a38d6190cf69ab6bee0f2a647e101c01a0b1b"
  #error "Accepted payload mismatch (x86): fr\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\fr\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\fr\System.Windows.Forms.Design.resources.dll") != "4b41b6e03a0419ef03a997b38f0ff40d9f43938b36ee6803db4f8917ea6803c6"
  #error "Accepted payload mismatch (x64): fr\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\fr\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\fr\System.Windows.Forms.Design.resources.dll") != "702d2ce2ae56950e370cf28b00a1bdde98a366dd1614b7dc085a22dab8d43578"
  #error "Accepted payload mismatch (x86): fr\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\fr\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\fr\System.Windows.Forms.Primitives.resources.dll") != "97bc774e3b7ac795e78a88173813acff70cf01912281cbb9b6aeec59093c1254"
  #error "Accepted payload mismatch (x64): fr\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\fr\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\fr\System.Windows.Forms.Primitives.resources.dll") != "30107f647acf0c89b249e16c680b5701118ba05dbf64bf153ddb6a59bf3235c2"
  #error "Accepted payload mismatch (x86): fr\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\fr\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\fr\System.Windows.Forms.resources.dll") != "4775f95dfa273f7bbac51f05cd67ce9efc9aae79c8e41adc83021f36fdadcc83"
  #error "Accepted payload mismatch (x64): fr\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\fr\System.Windows.Forms.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\fr\System.Windows.Forms.resources.dll") != "593091db2b18475d5f0166f6196c112505982d95349f0b5c20d4eb5e2400bbee"
  #error "Accepted payload mismatch (x86): fr\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\fr\System.Windows.Forms.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\fr\System.Windows.Input.Manipulations.resources.dll") != "43097dc22fe65f6c4e84fb63dfa98a02c9a200a54fdd06a1d41d40204632badb"
  #error "Accepted payload mismatch (x64): fr\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\fr\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\fr\System.Windows.Input.Manipulations.resources.dll") != "3c5092f869a779c64c615002c825265c1fa060b64e917ae76d7fec2b44d54dd5"
  #error "Accepted payload mismatch (x86): fr\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\fr\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\fr\System.Xaml.resources.dll") != "883af5abf8ca1d3a843adc12a03fc3702cdf205bf527dc8bb1024fd4bcef361b"
  #error "Accepted payload mismatch (x64): fr\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\fr\System.Xaml.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\fr\System.Xaml.resources.dll") != "af7fe75e9ac67c23ad176322dbf11a4a59e36e77033bbb2cf148a9409b3b4f1a"
  #error "Accepted payload mismatch (x86): fr\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\fr\System.Xaml.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\fr\UIAutomationClient.resources.dll") != "47075f17317cdfe75e280e1d7d272e421b25adc420c010a4ad09aa6fd7cc9771"
  #error "Accepted payload mismatch (x64): fr\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\fr\UIAutomationClient.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\fr\UIAutomationClient.resources.dll") != "2f5971ebd7ffedffb881942f683e63b02735878f19e9b6118684d34052dd73d2"
  #error "Accepted payload mismatch (x86): fr\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\fr\UIAutomationClient.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\fr\UIAutomationClientSideProviders.resources.dll") != "1554819052c3feec851e11e1cb662b79d011a1f2211950cc3d7add115eeb15fe"
  #error "Accepted payload mismatch (x64): fr\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\fr\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\fr\UIAutomationClientSideProviders.resources.dll") != "ee99c6ff8abd597feb2155b0a68ac65b200625c30895da7696c86c2dab88eb4c"
  #error "Accepted payload mismatch (x86): fr\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\fr\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\fr\UIAutomationProvider.resources.dll") != "6e3da91553c899e7cd22fe414253bc877da114dbae12bfe0b49a536427d2d593"
  #error "Accepted payload mismatch (x64): fr\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\fr\UIAutomationProvider.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\fr\UIAutomationProvider.resources.dll") != "a2000b997ce1dde299711d7c4b7eba0f929383e6a5d7d14b4d89355624fff560"
  #error "Accepted payload mismatch (x86): fr\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\fr\UIAutomationProvider.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\fr\UIAutomationTypes.resources.dll") != "319ceca2fbaa75c9d9130a21df24d0f843bbf2f529bac9f30bbeaa3cc201e845"
  #error "Accepted payload mismatch (x64): fr\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\fr\UIAutomationTypes.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\fr\UIAutomationTypes.resources.dll") != "44fda47f3eae6941b9d8ad37f76c8b018c865940c39f857e041a10149c35d04d"
  #error "Accepted payload mismatch (x86): fr\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\fr\UIAutomationTypes.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\fr\WindowsBase.resources.dll") != "e9fd047669b1cdc505e3ce1cdea94419c3e385f15788792c046832446c224b24"
  #error "Accepted payload mismatch (x64): fr\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\fr\WindowsBase.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\fr\WindowsBase.resources.dll") != "7f8731ba5af5ffa295f424bd1cea17bcea6f30db34578aaf907ff397e4ab9e1d"
  #error "Accepted payload mismatch (x86): fr\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\fr\WindowsBase.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\fr\WindowsFormsIntegration.resources.dll") != "fdc210f7b54b1221baad6b30055938adfff2e467081907ad245490db1c75e235"
  #error "Accepted payload mismatch (x64): fr\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\fr\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\fr\WindowsFormsIntegration.resources.dll") != "67071d89409ae7579e0f310368680f8f567bef5bde2d5823d2c4c75db0a713d5"
  #error "Accepted payload mismatch (x86): fr\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\fr\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\geonames\NOTICE.txt") != "b116bb5f613ad8b2476eb8dbdee7af5310cc5b396798d2bf9e6e0b56f0a116f2"
  #error "Accepted payload mismatch (x86): geonames\NOTICE.txt"
#endif
#if GetSHA256OfFile(MyAppSourceDirX64 + "\geonames\NOTICE.txt") != "b116bb5f613ad8b2476eb8dbdee7af5310cc5b396798d2bf9e6e0b56f0a116f2"
  #error "Accepted payload mismatch (x64): geonames\NOTICE.txt"
#endif
Source: "{#MyAppSourceDirX64}\geonames\NOTICE.txt"; DestDir: "{app}\geonames"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDirX86 + "\geonames\cities1000.tsv.gz") != "e377a5ffddd22ad4e47e84452d394bc2227f35eb67dfdcacf186ee9b8462e2f1"
  #error "Accepted payload mismatch (x86): geonames\cities1000.tsv.gz"
#endif
#if GetSHA256OfFile(MyAppSourceDirX64 + "\geonames\cities1000.tsv.gz") != "e377a5ffddd22ad4e47e84452d394bc2227f35eb67dfdcacf186ee9b8462e2f1"
  #error "Accepted payload mismatch (x64): geonames\cities1000.tsv.gz"
#endif
Source: "{#MyAppSourceDirX64}\geonames\cities1000.tsv.gz"; DestDir: "{app}\geonames"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDirX64 + "\hostfxr.dll") != "2c14e9b4a540d98971b60cbaffcdde4f22328bd24bf75b68aa9e6bee13c2ca25"
  #error "Accepted payload mismatch (x64): hostfxr.dll"
#endif
Source: "{#MyAppSourceDirX64}\hostfxr.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\hostfxr.dll") != "94a372de4af5dd3291d38819dc9bd0981f9b696be2c2d0f90a28d143ad13470c"
  #error "Accepted payload mismatch (x86): hostfxr.dll"
#endif
Source: "{#MyAppSourceDirX86}\hostfxr.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\hostpolicy.dll") != "20405152b719ccb8d4e0361f3259db8875d8d4f3c97d1fbea4e1ca6cc6d21902"
  #error "Accepted payload mismatch (x64): hostpolicy.dll"
#endif
Source: "{#MyAppSourceDirX64}\hostpolicy.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\hostpolicy.dll") != "7c93fab93397fa4cc381935157ace1e41508a6efbb200df64de30cee850f366d"
  #error "Accepted payload mismatch (x86): hostpolicy.dll"
#endif
Source: "{#MyAppSourceDirX86}\hostpolicy.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\it\Microsoft.VisualBasic.Forms.resources.dll") != "d3fb4eb5b5d4a08961b24e3a55c27f0697b854d185853bdb69987fd290d4e63a"
  #error "Accepted payload mismatch (x64): it\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\it\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\it\Microsoft.VisualBasic.Forms.resources.dll") != "42f73484ab6024108caa222ece8c770cd578a809e0cf3e4b17374aaa7e354ffa"
  #error "Accepted payload mismatch (x86): it\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\it\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\it\PresentationCore.resources.dll") != "399338513fa3b95aa4aac44f8f29877ba1893f53ca5932f05b2a39f1561108de"
  #error "Accepted payload mismatch (x64): it\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\it\PresentationCore.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\it\PresentationCore.resources.dll") != "3aa12fbf64cf29021e74c27137685b6b7b8438ca49db7c8f780b9f528a834535"
  #error "Accepted payload mismatch (x86): it\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\it\PresentationCore.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\it\PresentationFramework.resources.dll") != "59741e1fc4a7fabdd4f7dbc98a49ae59ed7a394722883589a451063ca26662bc"
  #error "Accepted payload mismatch (x64): it\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\it\PresentationFramework.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\it\PresentationFramework.resources.dll") != "7285306dd937e225b4eaff5eda58056a9ef4389dba8cdcefbf673cd739faca8c"
  #error "Accepted payload mismatch (x86): it\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\it\PresentationFramework.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\it\PresentationUI.resources.dll") != "e0f338e4247c6f7650ccd4575d4d18abd7ef2303aba0702225de3ed3ab3f4399"
  #error "Accepted payload mismatch (x64): it\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\it\PresentationUI.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\it\PresentationUI.resources.dll") != "bb47383565938c96d9276efae9028f88f90cecba811bd3bbd980559061401daf"
  #error "Accepted payload mismatch (x86): it\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\it\PresentationUI.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\it\ReachFramework.resources.dll") != "a50fc5705ad57ae81caa90a2a81d9b53249cb7d9a698d0362a2b5ba1db9313a1"
  #error "Accepted payload mismatch (x64): it\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\it\ReachFramework.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\it\ReachFramework.resources.dll") != "176a8573258a427620b0660ab9d1f83d9bfbd844b5db1e31b1a0cb4a55992aa6"
  #error "Accepted payload mismatch (x86): it\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\it\ReachFramework.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\it\System.Windows.Controls.Ribbon.resources.dll") != "5e24aafb0dfa1477a476e3870a1eeef0ef43b455b7aaa71cb44619338b71f15e"
  #error "Accepted payload mismatch (x64): it\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\it\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\it\System.Windows.Controls.Ribbon.resources.dll") != "3665070967b8037fbfd352ce1aed599cdc9d46f7d0a7fd764f9e6ae00276f0ba"
  #error "Accepted payload mismatch (x86): it\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\it\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\it\System.Windows.Forms.Design.resources.dll") != "913a6d138e5d86aebffc171b8772710aace6c941d90dc9c82e894b67016b004e"
  #error "Accepted payload mismatch (x64): it\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\it\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\it\System.Windows.Forms.Design.resources.dll") != "0704318b8f049d6c7004d20f531e748f34fc029434969c2939a1c68bb99c82b0"
  #error "Accepted payload mismatch (x86): it\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\it\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\it\System.Windows.Forms.Primitives.resources.dll") != "718a82efabda1bb7a0241bd5d4f39539b028d6c09d502bd876185289c4491c20"
  #error "Accepted payload mismatch (x64): it\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\it\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\it\System.Windows.Forms.Primitives.resources.dll") != "967cd40af658faba36534fa348f217939a4f36e8b5380237d58953803238929c"
  #error "Accepted payload mismatch (x86): it\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\it\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\it\System.Windows.Forms.resources.dll") != "3399fff5938c4ab5cf3307ecca4d0b7e0c6c630dffd5c2eeab6470485bf91d63"
  #error "Accepted payload mismatch (x64): it\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\it\System.Windows.Forms.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\it\System.Windows.Forms.resources.dll") != "ff26a456113ad8b0970949fe13032bc244365edf13e0739eed34b18fcde0d9c7"
  #error "Accepted payload mismatch (x86): it\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\it\System.Windows.Forms.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\it\System.Windows.Input.Manipulations.resources.dll") != "cfc7490fd67a09c2609eaa971c9519fe26c1ef5411bc9f48ad64903a3f9ba1df"
  #error "Accepted payload mismatch (x64): it\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\it\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\it\System.Windows.Input.Manipulations.resources.dll") != "07b5d1925bdb06c9d5ca7930339f24f937187f248596ec9d8b13c2669a32b661"
  #error "Accepted payload mismatch (x86): it\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\it\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\it\System.Xaml.resources.dll") != "2515d305f7b8087f07cecad399dd9bd824eb65d9375586126b768e777fcaa4ce"
  #error "Accepted payload mismatch (x64): it\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\it\System.Xaml.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\it\System.Xaml.resources.dll") != "477e184da2167ea0aa1f1be88ed50037c6f7275942b1fef9cfb0bae7d16a6300"
  #error "Accepted payload mismatch (x86): it\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\it\System.Xaml.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\it\UIAutomationClient.resources.dll") != "0812af693ff39bb7b8c3fc786f7ad27899f60a1ac7d885f223698582a62b986e"
  #error "Accepted payload mismatch (x64): it\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\it\UIAutomationClient.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\it\UIAutomationClient.resources.dll") != "cc277f25936e99204a2d5c63bf51a96983de73fa9aaef9b3b511d8d9cf878f4f"
  #error "Accepted payload mismatch (x86): it\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\it\UIAutomationClient.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\it\UIAutomationClientSideProviders.resources.dll") != "4c6163246253fdced329a0af4d76924d90949e06d7500cd6e4f64c583dad21a8"
  #error "Accepted payload mismatch (x64): it\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\it\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\it\UIAutomationClientSideProviders.resources.dll") != "060c70cad625180ac5552371c77e160babb2ded7a8a40b0acb5b8dcf5ba08a24"
  #error "Accepted payload mismatch (x86): it\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\it\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\it\UIAutomationProvider.resources.dll") != "a13ee917e64997cdca92947045911d0d2b6096c85f560f924a891ed6c95fc632"
  #error "Accepted payload mismatch (x64): it\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\it\UIAutomationProvider.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\it\UIAutomationProvider.resources.dll") != "fd71ec8ad72a196666ca44616ee6fd85e6f7e3d9a8f89e5faa435adf2546d061"
  #error "Accepted payload mismatch (x86): it\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\it\UIAutomationProvider.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\it\UIAutomationTypes.resources.dll") != "dc080fdbb058b1f6b47c69fc64d9bf038ac8350c137491d386e2a88ce35f7f13"
  #error "Accepted payload mismatch (x64): it\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\it\UIAutomationTypes.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\it\UIAutomationTypes.resources.dll") != "82e0188b213c41f9610e7f42ec60b43b78006622f2e1f628c3afb9bf08cc832b"
  #error "Accepted payload mismatch (x86): it\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\it\UIAutomationTypes.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\it\WindowsBase.resources.dll") != "6eb6e19f880c725a10f0c0b21f44ca37a75c9bfebf8c3035812fe9885567c61d"
  #error "Accepted payload mismatch (x64): it\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\it\WindowsBase.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\it\WindowsBase.resources.dll") != "e28ec4f4b6e44360c822402ea6005c0fd34f3be68dff1be1eac3b4d66973a55d"
  #error "Accepted payload mismatch (x86): it\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\it\WindowsBase.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\it\WindowsFormsIntegration.resources.dll") != "f9ec83c023a87a23739abef6b4a9a4134efcf13740bcc64218739f82a8a98899"
  #error "Accepted payload mismatch (x64): it\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\it\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\it\WindowsFormsIntegration.resources.dll") != "7c36c683954b1b775d95914f1b4826558e8ef6aa5a2d00612c97cf88d27f2d61"
  #error "Accepted payload mismatch (x86): it\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\it\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ja\Microsoft.VisualBasic.Forms.resources.dll") != "4991cc59d0e62ffd6c6db8bf4cdfccdf67deb64138692fd70f310963a7402e52"
  #error "Accepted payload mismatch (x64): ja\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ja\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ja\Microsoft.VisualBasic.Forms.resources.dll") != "685d90701586a7c1acaf00a77882d4c8724dcab595c33520f5ce99737b978194"
  #error "Accepted payload mismatch (x86): ja\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ja\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ja\PresentationCore.resources.dll") != "bd7550df913f8c426e737b4021010921cf1709415352ca3f50f2329ff7922e3d"
  #error "Accepted payload mismatch (x64): ja\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ja\PresentationCore.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ja\PresentationCore.resources.dll") != "4eda8557600f155d8985623ece2465c88d370dfce2141905e4923634de249e13"
  #error "Accepted payload mismatch (x86): ja\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ja\PresentationCore.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ja\PresentationFramework.resources.dll") != "33dde91e962afcc8e8dca4a7a71713a9f66930478460d9d21bb645025fe13e5d"
  #error "Accepted payload mismatch (x64): ja\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ja\PresentationFramework.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ja\PresentationFramework.resources.dll") != "e1b94ea53715f8a58dc3385148f8069e4852c8fda53388f46d1bf68941c77d27"
  #error "Accepted payload mismatch (x86): ja\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ja\PresentationFramework.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ja\PresentationUI.resources.dll") != "795332c02426f638961a29dd7b9c404f8236ef4aff839f1a5790ae5b6e0d6841"
  #error "Accepted payload mismatch (x64): ja\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ja\PresentationUI.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ja\PresentationUI.resources.dll") != "18175fab3d769f0f2870462db97880b7c2bda6e1c97361faaf11f3b27996ac0e"
  #error "Accepted payload mismatch (x86): ja\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ja\PresentationUI.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ja\ReachFramework.resources.dll") != "69d58b3accfde039a63d8e75a84ab25ccff3e3ac28c80e482423df02e81fc0de"
  #error "Accepted payload mismatch (x64): ja\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ja\ReachFramework.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ja\ReachFramework.resources.dll") != "de87bcf84c068ef1f788ba409c8f76b132f346634db47b4180f1d43078c12ebb"
  #error "Accepted payload mismatch (x86): ja\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ja\ReachFramework.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ja\System.Windows.Controls.Ribbon.resources.dll") != "9890e00bf093d6d3be5c64e6cb03e7a0ab001903d796954cdf96e88bdeb80cd2"
  #error "Accepted payload mismatch (x64): ja\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ja\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ja\System.Windows.Controls.Ribbon.resources.dll") != "30071d563c2a40a2189703417de17863b3fcd136d06a25d1bdf1f9419069b4a3"
  #error "Accepted payload mismatch (x86): ja\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ja\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ja\System.Windows.Forms.Design.resources.dll") != "78baa78d00aaf2d9cea87b878339f78f4a9ec5e9736d9b5bfd8aecd2267ea8b4"
  #error "Accepted payload mismatch (x64): ja\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ja\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ja\System.Windows.Forms.Design.resources.dll") != "7ef256ff67460387456509dd97de1c59944d14cabfb7af9a6789cff57603fc9d"
  #error "Accepted payload mismatch (x86): ja\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ja\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ja\System.Windows.Forms.Primitives.resources.dll") != "9ae1695f8915d59d41d2375efc9c2a2e9893fb1a2987b549ef9e5ff9d78a9569"
  #error "Accepted payload mismatch (x64): ja\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ja\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ja\System.Windows.Forms.Primitives.resources.dll") != "559cc19378f310dbb98835ebbfeb953c54d0e1102f87e7ca7f9749894ea70598"
  #error "Accepted payload mismatch (x86): ja\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ja\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ja\System.Windows.Forms.resources.dll") != "31711553613ead7dae9542d9983161ae8faf0affd37809a18255c8499b575311"
  #error "Accepted payload mismatch (x64): ja\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ja\System.Windows.Forms.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ja\System.Windows.Forms.resources.dll") != "be19fa125eee683441b987085c78ce9734247fa452a72eb53dc42a7dadca8443"
  #error "Accepted payload mismatch (x86): ja\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ja\System.Windows.Forms.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ja\System.Windows.Input.Manipulations.resources.dll") != "7a5d1cf8868946d1bf9eb2e99ddef188bf8bebd0e74ee95b54882ab5f6537ac3"
  #error "Accepted payload mismatch (x64): ja\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ja\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ja\System.Windows.Input.Manipulations.resources.dll") != "39b67a52ff88b72399dde239605e6c150c6ac4175437ad16e32aabb81cbf76c3"
  #error "Accepted payload mismatch (x86): ja\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ja\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ja\System.Xaml.resources.dll") != "fdbcf3ae79613129f878a9d9fbc10ff3445d4ebce225d4d29b3211e0a60e4a5b"
  #error "Accepted payload mismatch (x64): ja\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ja\System.Xaml.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ja\System.Xaml.resources.dll") != "ad1fd52b6cc124ba7521fe7e62095b3c0ed21a4624f0850514cc97c2e68a8ab1"
  #error "Accepted payload mismatch (x86): ja\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ja\System.Xaml.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ja\UIAutomationClient.resources.dll") != "4b33da4411919fcc42faa67a906d697d08b0dfb7118dde7a89fd1d59e9463363"
  #error "Accepted payload mismatch (x64): ja\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ja\UIAutomationClient.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ja\UIAutomationClient.resources.dll") != "a6833f3de32f6b9e1237e9d21668156895bfed5ed690d001d7bfb95bb900d833"
  #error "Accepted payload mismatch (x86): ja\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ja\UIAutomationClient.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ja\UIAutomationClientSideProviders.resources.dll") != "37603165fd377763ca61ead4e289f274dc74ac387b031b7956e2789c9be363df"
  #error "Accepted payload mismatch (x64): ja\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ja\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ja\UIAutomationClientSideProviders.resources.dll") != "538a06dc90d5268830c89e4b27bd6a5e1df353acd0c21cdc864e739e55bcdb44"
  #error "Accepted payload mismatch (x86): ja\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ja\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ja\UIAutomationProvider.resources.dll") != "4c050e517d3d7081c08f1b45f316f2e0cf4838a46d54917629b05f41ae8d0cc4"
  #error "Accepted payload mismatch (x64): ja\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ja\UIAutomationProvider.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ja\UIAutomationProvider.resources.dll") != "82e103fe266509b104a968c1436df7f38a817982922d8a2603abf4db83a7fdd8"
  #error "Accepted payload mismatch (x86): ja\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ja\UIAutomationProvider.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ja\UIAutomationTypes.resources.dll") != "c06b9f0f108a4c4ef902e9168b8d1a05216c029a0a7fedfdf8c25fca755aff3e"
  #error "Accepted payload mismatch (x64): ja\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ja\UIAutomationTypes.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ja\UIAutomationTypes.resources.dll") != "1f4184ffc08a818b8970967d8d46b65b2d2dd0d359afa991ec58c65668c9d061"
  #error "Accepted payload mismatch (x86): ja\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ja\UIAutomationTypes.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ja\WindowsBase.resources.dll") != "8483bf6f2482cfdc269a5cc8d64e5bb01d361da1342c7591faadc7a468c8ef19"
  #error "Accepted payload mismatch (x64): ja\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ja\WindowsBase.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ja\WindowsBase.resources.dll") != "84dda4dc664adc83c1e40c0e85aa2a0ed6772f27d8c12f55ce4e793ff83e9766"
  #error "Accepted payload mismatch (x86): ja\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ja\WindowsBase.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ja\WindowsFormsIntegration.resources.dll") != "e8306416f5ed9b86dddb085142cc6bbba7266309909bfaaa5905f3f012129bae"
  #error "Accepted payload mismatch (x64): ja\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ja\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ja\WindowsFormsIntegration.resources.dll") != "7b2cab412cdb3ca7148cf5fa9fa00153368c54be50db8fd2ab68c08ed609f3dd"
  #error "Accepted payload mismatch (x86): ja\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ja\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ko\Microsoft.VisualBasic.Forms.resources.dll") != "ce4055a363d683226785b8169ece21c09d039f4852b1522f01e79e366c37a657"
  #error "Accepted payload mismatch (x64): ko\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ko\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ko\Microsoft.VisualBasic.Forms.resources.dll") != "ced7f84285ec09297d91eee94a9e15c9c1f17830b4ecb238806bab7eaa61d027"
  #error "Accepted payload mismatch (x86): ko\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ko\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ko\PresentationCore.resources.dll") != "816d8aa14ac7959c8b306509677aedbf2cfc764614671fd477611cbd58ff17f0"
  #error "Accepted payload mismatch (x64): ko\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ko\PresentationCore.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ko\PresentationCore.resources.dll") != "90517beeea12e53fb7c3275fa0c4c9675e59feb975678613a0e6a3048878d9a4"
  #error "Accepted payload mismatch (x86): ko\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ko\PresentationCore.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ko\PresentationFramework.resources.dll") != "e22d08f1467753f29b17c66d1eb429595a55fefe3121c090299616dc23715289"
  #error "Accepted payload mismatch (x64): ko\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ko\PresentationFramework.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ko\PresentationFramework.resources.dll") != "738340d0bde556ac497ba43e358f65db73761c774a547e00d97b85354eb1195a"
  #error "Accepted payload mismatch (x86): ko\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ko\PresentationFramework.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ko\PresentationUI.resources.dll") != "b55fceb319f7dfb52170354977767d9ad35be48087e8b32856e14a74e10742a0"
  #error "Accepted payload mismatch (x64): ko\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ko\PresentationUI.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ko\PresentationUI.resources.dll") != "6f2d29094a42268e42602b18ace4418b15ab6bf5397495b8e01d2c3d3e6248cb"
  #error "Accepted payload mismatch (x86): ko\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ko\PresentationUI.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ko\ReachFramework.resources.dll") != "1c77c79450837996b8dfee8863f1a177d5add9c498fb995a16412b3be12e77fa"
  #error "Accepted payload mismatch (x64): ko\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ko\ReachFramework.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ko\ReachFramework.resources.dll") != "f42001fc3685e393632ac46cfc7e9b3f35cf675430b8e817954728eb631f08bb"
  #error "Accepted payload mismatch (x86): ko\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ko\ReachFramework.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ko\System.Windows.Controls.Ribbon.resources.dll") != "d969dc68ca736164cac77b96d07d4a751b8de6ae57b07ec380ed8562c1488102"
  #error "Accepted payload mismatch (x64): ko\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ko\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ko\System.Windows.Controls.Ribbon.resources.dll") != "b87421911c86e24cbccb89af4657d1758d9b70580b361c8949436aa891732053"
  #error "Accepted payload mismatch (x86): ko\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ko\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ko\System.Windows.Forms.Design.resources.dll") != "7481befc6d47f84ad6d41bb3092c64772c9f7a03b0132e517f79e438dc12814c"
  #error "Accepted payload mismatch (x64): ko\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ko\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ko\System.Windows.Forms.Design.resources.dll") != "3c8f7bb8bf03e0548bdd274409b6b9a60a61b6bdb4d95380aedc238c786a5b8f"
  #error "Accepted payload mismatch (x86): ko\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ko\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ko\System.Windows.Forms.Primitives.resources.dll") != "ae22707435a96c26f49bd4453380a14033913e17198df52b176ed04d26c8a381"
  #error "Accepted payload mismatch (x64): ko\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ko\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ko\System.Windows.Forms.Primitives.resources.dll") != "8f97349985673629b84c2e9b1323475752e09a32cd621779d1863a906c0a3908"
  #error "Accepted payload mismatch (x86): ko\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ko\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ko\System.Windows.Forms.resources.dll") != "367d3522b043e53d9e6d050c86c0f5f96ab1f37cb525c31ad8d46cb28a14b5af"
  #error "Accepted payload mismatch (x64): ko\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ko\System.Windows.Forms.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ko\System.Windows.Forms.resources.dll") != "27278d4dda006d59c6dc71cbf0c0e4131b51585145d266edfe4cb8ea4b0ba4ea"
  #error "Accepted payload mismatch (x86): ko\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ko\System.Windows.Forms.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ko\System.Windows.Input.Manipulations.resources.dll") != "48ba4cc61750a28d3338c886428a3116ea204bb769b8522a268eb562778b7ce0"
  #error "Accepted payload mismatch (x64): ko\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ko\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ko\System.Windows.Input.Manipulations.resources.dll") != "9cc7ce7b531e6e5ba4214ac42a4a1e54cd33253c032a8d207f9e1b2ea9644b6e"
  #error "Accepted payload mismatch (x86): ko\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ko\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ko\System.Xaml.resources.dll") != "6e34d2ce45ffccb23f801b93b9cdda07ee3d051c3d5aa7fac3a2712ee769fe99"
  #error "Accepted payload mismatch (x64): ko\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ko\System.Xaml.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ko\System.Xaml.resources.dll") != "1afa0dbac4b421821e90a3ea326291ed92975cfc340cd053a3f6f3a957230054"
  #error "Accepted payload mismatch (x86): ko\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ko\System.Xaml.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ko\UIAutomationClient.resources.dll") != "a361eff1e23238251fbf96555694775b2a6617b7722e7ad01493cd2722e3f0bb"
  #error "Accepted payload mismatch (x64): ko\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ko\UIAutomationClient.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ko\UIAutomationClient.resources.dll") != "e9541b86c46928787c5cc81efd4053c442db571a9ddd3950dc53cf7904dde7fc"
  #error "Accepted payload mismatch (x86): ko\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ko\UIAutomationClient.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ko\UIAutomationClientSideProviders.resources.dll") != "126751e0463eef8e56f8d861c601e646b281d18122c00bdc2c3a6c7d86ebd7dc"
  #error "Accepted payload mismatch (x64): ko\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ko\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ko\UIAutomationClientSideProviders.resources.dll") != "4643e38f9982a9f37caec47509b0805faf7a861fce6b078ecabb2f8beea51fd5"
  #error "Accepted payload mismatch (x86): ko\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ko\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ko\UIAutomationProvider.resources.dll") != "64fb21260fc246cc376f5f80edaf120be9793e740530ade1f4f2f217d3976391"
  #error "Accepted payload mismatch (x64): ko\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ko\UIAutomationProvider.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ko\UIAutomationProvider.resources.dll") != "ac7cc7d4b31671bbc72bc402ccf29cf7e637bc413d4775a1819a3c716e96229d"
  #error "Accepted payload mismatch (x86): ko\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ko\UIAutomationProvider.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ko\UIAutomationTypes.resources.dll") != "cd51eff76bfe027cece528e95e26b614294954145076fb8af1e95822b8a9b901"
  #error "Accepted payload mismatch (x64): ko\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ko\UIAutomationTypes.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ko\UIAutomationTypes.resources.dll") != "85749b9ef7bdc5b6345f5b35872a7af0a5d8e854b30326b4bbbd03bc8f6f2aff"
  #error "Accepted payload mismatch (x86): ko\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ko\UIAutomationTypes.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ko\WindowsBase.resources.dll") != "e7171939db91653eb11be360afda65d0bab9a121de837ec0f135dfae830db73f"
  #error "Accepted payload mismatch (x64): ko\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ko\WindowsBase.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ko\WindowsBase.resources.dll") != "44e552f55b489be8bd6f776b703870ecf838e1c845c68e09c2c6ee68dd278650"
  #error "Accepted payload mismatch (x86): ko\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ko\WindowsBase.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ko\WindowsFormsIntegration.resources.dll") != "c4e055e04241db4b87f7f99dd0d4d6a44789b8367d5d3c721ae57cf213a888fd"
  #error "Accepted payload mismatch (x64): ko\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ko\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ko\WindowsFormsIntegration.resources.dll") != "a4a2d1adf19316fc251814cb8cf9f475da22dd1c8f4d32778e9608562f0945d9"
  #error "Accepted payload mismatch (x86): ko\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ko\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\mscordaccore.dll") != "3eb536b8bd054c0b4a19332d25ffc90787f307b0367bae4dc222417d8107d247"
  #error "Accepted payload mismatch (x64): mscordaccore.dll"
#endif
Source: "{#MyAppSourceDirX64}\mscordaccore.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\mscordaccore.dll") != "744de33c4d4d889cd7312bf1331abde9439f04653b2cc3b7959206b2f0b0f594"
  #error "Accepted payload mismatch (x86): mscordaccore.dll"
#endif
Source: "{#MyAppSourceDirX86}\mscordaccore.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\mscordaccore_amd64_amd64_10.0.1526.53008.dll") != "3eb536b8bd054c0b4a19332d25ffc90787f307b0367bae4dc222417d8107d247"
  #error "Accepted payload mismatch (x64): mscordaccore_amd64_amd64_10.0.1526.53008.dll"
#endif
Source: "{#MyAppSourceDirX64}\mscordaccore_amd64_amd64_10.0.1526.53008.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\mscordaccore_x86_x86_10.0.1526.53008.dll") != "744de33c4d4d889cd7312bf1331abde9439f04653b2cc3b7959206b2f0b0f594"
  #error "Accepted payload mismatch (x86): mscordaccore_x86_x86_10.0.1526.53008.dll"
#endif
Source: "{#MyAppSourceDirX86}\mscordaccore_x86_x86_10.0.1526.53008.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\mscordbi.dll") != "d3c09b90b6599b15c37c6172cdc6f2e61d63c7c72922a49e4703a0668f8f9af9"
  #error "Accepted payload mismatch (x64): mscordbi.dll"
#endif
Source: "{#MyAppSourceDirX64}\mscordbi.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\mscordbi.dll") != "87fdfd8e2d037b10ebd18ff374f6d1f29bc6012e5da8da36d30252a5ed287198"
  #error "Accepted payload mismatch (x86): mscordbi.dll"
#endif
Source: "{#MyAppSourceDirX86}\mscordbi.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\mscorlib.dll") != "3b72dd3231822316d3f4d649a7c6db115c7829e40978d564b543c4d34f5c5c16"
  #error "Accepted payload mismatch (x64): mscorlib.dll"
#endif
Source: "{#MyAppSourceDirX64}\mscorlib.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\mscorlib.dll") != "96782d5789b10a3a8b4ffbc2f519584d5f273985805cb6082c0311d2e3715017"
  #error "Accepted payload mismatch (x86): mscorlib.dll"
#endif
Source: "{#MyAppSourceDirX86}\mscorlib.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\mscorrc.dll") != "41ee8d8da7be90058ac9cf3be848871a9b78f60fe6caf82777ea9776ac85f75c"
  #error "Accepted payload mismatch (x64): mscorrc.dll"
#endif
Source: "{#MyAppSourceDirX64}\mscorrc.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\mscorrc.dll") != "6c6aba852b24d36dd604f3be4b1496ef86a402bc71bab58d8319a99acd39aa51"
  #error "Accepted payload mismatch (x86): mscorrc.dll"
#endif
Source: "{#MyAppSourceDirX86}\mscorrc.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\msquic.dll") != "d363de7b6b08b3c68f0bc87ae74843cb152067a9ea589a265631895f41a0ca45"
  #error "Accepted payload mismatch (x64): msquic.dll"
#endif
Source: "{#MyAppSourceDirX64}\msquic.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\msquic.dll") != "69c4d45b0c1e96d109561210e26d8d31a84e9b150a35b0a3ac88f27cefa11189"
  #error "Accepted payload mismatch (x86): msquic.dll"
#endif
Source: "{#MyAppSourceDirX86}\msquic.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\netstandard.dll") != "6b7a22cce7004dea2d1747db1f150e2809afa7d41508b1b288f967ec1332f2f5"
  #error "Accepted payload mismatch (x64): netstandard.dll"
#endif
Source: "{#MyAppSourceDirX64}\netstandard.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\netstandard.dll") != "91323fb27553fbc8fe7a6774276d4e79fa3ebbca35f3f55f6f45aa312bd7e3ec"
  #error "Accepted payload mismatch (x86): netstandard.dll"
#endif
Source: "{#MyAppSourceDirX86}\netstandard.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pl\Microsoft.VisualBasic.Forms.resources.dll") != "fc688c164590a1b9f1ea8df9ab574d74045f90cc19ad0e02ee0b5bb4a139e8ef"
  #error "Accepted payload mismatch (x64): pl\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pl\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pl\Microsoft.VisualBasic.Forms.resources.dll") != "dfdd4b577f11b2becfd27ca4efddef0452906bba65f5b695ae2e49188386e4fe"
  #error "Accepted payload mismatch (x86): pl\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pl\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pl\PresentationCore.resources.dll") != "e325ced891fbd357fea2ec3754e01c6b023686160583382eaa4231060aece11b"
  #error "Accepted payload mismatch (x64): pl\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pl\PresentationCore.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pl\PresentationCore.resources.dll") != "ca5f6980ef9d34cd219cfc46b2623b2dd7dfb9ee347ab57af86d251849b59be2"
  #error "Accepted payload mismatch (x86): pl\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pl\PresentationCore.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pl\PresentationFramework.resources.dll") != "b3135b3657eeb2fac9c5a7f256d3a33bcc3fe013f7e4cdc3172e0ac4ba8c930e"
  #error "Accepted payload mismatch (x64): pl\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pl\PresentationFramework.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pl\PresentationFramework.resources.dll") != "6be65a7546e4b057c3a7c8551492ee486c0b39e5e1e52bde2aa2bd33472e1841"
  #error "Accepted payload mismatch (x86): pl\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pl\PresentationFramework.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pl\PresentationUI.resources.dll") != "3af4edbcbb5c5d540e690e81ea1050ee17b15ded81f4ef945dc9f4e5ae44c96d"
  #error "Accepted payload mismatch (x64): pl\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pl\PresentationUI.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pl\PresentationUI.resources.dll") != "677e449df1651a47dfd0f329a62f959b0c2ad3f43703324c7193b0cab06f1680"
  #error "Accepted payload mismatch (x86): pl\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pl\PresentationUI.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pl\ReachFramework.resources.dll") != "686d619d8bcf16c6a01657c75ebbac62de277e523bc4ea1bcc26572e324c3b6b"
  #error "Accepted payload mismatch (x64): pl\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pl\ReachFramework.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pl\ReachFramework.resources.dll") != "f0ebe123a283d688888375c6bb8fa02a225ee43a7877fde568fd8977f5318c61"
  #error "Accepted payload mismatch (x86): pl\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pl\ReachFramework.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pl\System.Windows.Controls.Ribbon.resources.dll") != "82238b3ccffff7b483a3ec1cdd75b564e6227c062979d60db50a2450f02fd8c4"
  #error "Accepted payload mismatch (x64): pl\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pl\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pl\System.Windows.Controls.Ribbon.resources.dll") != "f83f66cdcf667c45707cda16e0dadf80407248a9b275357c7d3bbc3cfec3cfd2"
  #error "Accepted payload mismatch (x86): pl\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pl\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pl\System.Windows.Forms.Design.resources.dll") != "4529e0f25b2bd0b5fc66ed2ce4339051554e85350e67217432cdd41ccb3b3bd8"
  #error "Accepted payload mismatch (x64): pl\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pl\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pl\System.Windows.Forms.Design.resources.dll") != "956e942cc4d8bce31708db1c1cf703bba191e5862c6ca85ad2655cd33a7b9875"
  #error "Accepted payload mismatch (x86): pl\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pl\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pl\System.Windows.Forms.Primitives.resources.dll") != "2a2a99394552217bc8697c435fc5e2db2585c66e2f339e75d6cde6879772f8f5"
  #error "Accepted payload mismatch (x64): pl\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pl\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pl\System.Windows.Forms.Primitives.resources.dll") != "80d424c1b61539dc59fcf50ff1aabe27f85d63b2ae3f7a5b054790cfcaa26c4b"
  #error "Accepted payload mismatch (x86): pl\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pl\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pl\System.Windows.Forms.resources.dll") != "853f1f76e8a593f6ab43afd9d966be5c366713132f052e64195dcccd05c706a6"
  #error "Accepted payload mismatch (x64): pl\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pl\System.Windows.Forms.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pl\System.Windows.Forms.resources.dll") != "761dd6a2e8d90ea5b77dda2bbf1832745c6e3b95b2c65139a7d603e315f0fbef"
  #error "Accepted payload mismatch (x86): pl\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pl\System.Windows.Forms.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pl\System.Windows.Input.Manipulations.resources.dll") != "93897609f969c41e3b4ac7219e708697122b45eeef950e16e275375996104370"
  #error "Accepted payload mismatch (x64): pl\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pl\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pl\System.Windows.Input.Manipulations.resources.dll") != "b1995c7640c6bbeacaa7e4e03312848b214b16d4c8a0a4bb1a42caaebcb8da11"
  #error "Accepted payload mismatch (x86): pl\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pl\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pl\System.Xaml.resources.dll") != "b78fafbc5312fe149638a7ec9d824ea5e0c5ec9b59a5613742b28be4bf2c70d2"
  #error "Accepted payload mismatch (x64): pl\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pl\System.Xaml.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pl\System.Xaml.resources.dll") != "41f7d8081b520259fc0daa1d6963dd97d1cb5f5c3f2067258af4ca75317e6be4"
  #error "Accepted payload mismatch (x86): pl\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pl\System.Xaml.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pl\UIAutomationClient.resources.dll") != "36714ccc463d8b618ad59e679932dceedbfa99deb0b44276fda356035a6d3d00"
  #error "Accepted payload mismatch (x64): pl\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pl\UIAutomationClient.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pl\UIAutomationClient.resources.dll") != "90d3372ea9006fb88b9cc43eaef55e3fbf02be50c92d66f1e33efee2b2b22127"
  #error "Accepted payload mismatch (x86): pl\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pl\UIAutomationClient.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pl\UIAutomationClientSideProviders.resources.dll") != "068b290ef6d171ade9f9d29cf66f75fd529088fafc170ac18790c50ca00a9995"
  #error "Accepted payload mismatch (x64): pl\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pl\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pl\UIAutomationClientSideProviders.resources.dll") != "c3cf7e9a604763eecc73a458cc61bc435a71599a817fdc3846d22a0da7e50030"
  #error "Accepted payload mismatch (x86): pl\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pl\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pl\UIAutomationProvider.resources.dll") != "d1b8e0624d3efe8a12e156465c2fc3e57228d9547e7cd143485f0e0359eeddd4"
  #error "Accepted payload mismatch (x64): pl\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pl\UIAutomationProvider.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pl\UIAutomationProvider.resources.dll") != "131f6e87ad2562ffff17a61f8a72b6192d406700a282f5a0a16e42a86bbef7d1"
  #error "Accepted payload mismatch (x86): pl\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pl\UIAutomationProvider.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pl\UIAutomationTypes.resources.dll") != "f9fa731f422bf97235605e4007ecba8cf740577234113f88a54e5708e1104efa"
  #error "Accepted payload mismatch (x64): pl\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pl\UIAutomationTypes.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pl\UIAutomationTypes.resources.dll") != "a7b6bdcf63c04fe4c6929862894098c34122e21f988cf1e87e5a0c49b07b4f27"
  #error "Accepted payload mismatch (x86): pl\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pl\UIAutomationTypes.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pl\WindowsBase.resources.dll") != "a7f9440e41cc868f77c258de1de149bf69f67c067d40dd51d05af9fd42c613ad"
  #error "Accepted payload mismatch (x64): pl\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pl\WindowsBase.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pl\WindowsBase.resources.dll") != "db608cb7cb73723b12db224c2de394b72801da8517abf343478462e4a5dcd0ea"
  #error "Accepted payload mismatch (x86): pl\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pl\WindowsBase.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pl\WindowsFormsIntegration.resources.dll") != "a20b5780a5a0fb454c97e8e81dcf38ec2c33a5d36ceb50e463cc434aa8e98ac7"
  #error "Accepted payload mismatch (x64): pl\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pl\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pl\WindowsFormsIntegration.resources.dll") != "eb57f79cfacee0a3b1742b36f4470c00a0a7ffd50ff17494990c2179b77aa957"
  #error "Accepted payload mismatch (x86): pl\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pl\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\prayer_core.dll") != "af5dfae10f3ddb16fc4719137396554817089a6abd3309fe154707ba727d53ba"
  #error "Accepted payload mismatch (x64): prayer_core.dll"
#endif
Source: "{#MyAppSourceDirX64}\prayer_core.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\prayer_core.dll") != "845687aafb2f90f3e39eedd79e7d84b198a715d5c448267c6c278f63061d7ca1"
  #error "Accepted payload mismatch (x86): prayer_core.dll"
#endif
Source: "{#MyAppSourceDirX86}\prayer_core.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pt-BR\Microsoft.VisualBasic.Forms.resources.dll") != "22265057bb2a74571e492ca1927072b08ccfa8093be91bc12b1fc9d70218651b"
  #error "Accepted payload mismatch (x64): pt-BR\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pt-BR\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pt-BR\Microsoft.VisualBasic.Forms.resources.dll") != "ba87650c71e2f34f15e252546945cf8ad503cae690b1d0e62ce36398983817e9"
  #error "Accepted payload mismatch (x86): pt-BR\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pt-BR\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pt-BR\PresentationCore.resources.dll") != "0d1531e40b6e50ecbe999faeb757968cef31a50de433b8fbc3728091116ca158"
  #error "Accepted payload mismatch (x64): pt-BR\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pt-BR\PresentationCore.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pt-BR\PresentationCore.resources.dll") != "6d6f27884be693f44966eab27bf9da545b6de8229a5b576a075284fcb8ad8b4a"
  #error "Accepted payload mismatch (x86): pt-BR\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pt-BR\PresentationCore.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pt-BR\PresentationFramework.resources.dll") != "43cd8cb54849af15be0938e8ce50fde76d7edd834f837c24ce1e975365856be2"
  #error "Accepted payload mismatch (x64): pt-BR\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pt-BR\PresentationFramework.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pt-BR\PresentationFramework.resources.dll") != "66a578d3ce3b69df8541fb463d3735a0cb283f9668bde57033aa10bdd187b177"
  #error "Accepted payload mismatch (x86): pt-BR\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pt-BR\PresentationFramework.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pt-BR\PresentationUI.resources.dll") != "3c89bd0aabe33b4b1c37446d6409674a6012ee4126618199fc69f7920f1512f0"
  #error "Accepted payload mismatch (x64): pt-BR\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pt-BR\PresentationUI.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pt-BR\PresentationUI.resources.dll") != "5913c3ba879fe169f7b29f728826340f47e7de8692a3fabec31971ecae763c26"
  #error "Accepted payload mismatch (x86): pt-BR\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pt-BR\PresentationUI.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pt-BR\ReachFramework.resources.dll") != "b4f74cb89e0fefb30c854e7d5a4bba4432a626fbc79b412705b651c88c2535e0"
  #error "Accepted payload mismatch (x64): pt-BR\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pt-BR\ReachFramework.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pt-BR\ReachFramework.resources.dll") != "edd76661a34da926cda33e11e053bd8059dce34449b73b19752c178da9e1cf49"
  #error "Accepted payload mismatch (x86): pt-BR\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pt-BR\ReachFramework.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pt-BR\System.Windows.Controls.Ribbon.resources.dll") != "a77c25f5385b84d776a7731063ce271181873c4e8131c68955ef4e5b9eb83d79"
  #error "Accepted payload mismatch (x64): pt-BR\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pt-BR\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pt-BR\System.Windows.Controls.Ribbon.resources.dll") != "f9d43cea8a561126b22b4dac8ee0052ced627d409f2dc6e20ef13e8ec012c327"
  #error "Accepted payload mismatch (x86): pt-BR\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pt-BR\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pt-BR\System.Windows.Forms.Design.resources.dll") != "aff7b041dd63da65dfcd4445b6bc13f6fa7a9995e5e507b534bb7d6c161a31db"
  #error "Accepted payload mismatch (x64): pt-BR\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pt-BR\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pt-BR\System.Windows.Forms.Design.resources.dll") != "668162e5030f8e021f7a43a1ad8fb17e683ac24a2fbffd366dd90165e7197e44"
  #error "Accepted payload mismatch (x86): pt-BR\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pt-BR\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pt-BR\System.Windows.Forms.Primitives.resources.dll") != "58653d5b8632686927d2ca80fd261016be05cfc44aeb18356b62e9396e81dddd"
  #error "Accepted payload mismatch (x64): pt-BR\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pt-BR\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pt-BR\System.Windows.Forms.Primitives.resources.dll") != "578d2c481eda22feb7159eb1be393c6eb517e67641832c47398ddb66e62f853d"
  #error "Accepted payload mismatch (x86): pt-BR\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pt-BR\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pt-BR\System.Windows.Forms.resources.dll") != "854044ce3f8f1e969a8ddcda5fd0134a1445c964ce9c17e7fd6b9c9cf6aae215"
  #error "Accepted payload mismatch (x64): pt-BR\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pt-BR\System.Windows.Forms.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pt-BR\System.Windows.Forms.resources.dll") != "0dd6be5dd976abeb3aedcc104d87648e78c1a61c44d850acb43971944f929cda"
  #error "Accepted payload mismatch (x86): pt-BR\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pt-BR\System.Windows.Forms.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pt-BR\System.Windows.Input.Manipulations.resources.dll") != "0a861491b4030f19a34c574cb7252adf9ff3390e32277dffdaf74fa53e41c129"
  #error "Accepted payload mismatch (x64): pt-BR\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pt-BR\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pt-BR\System.Windows.Input.Manipulations.resources.dll") != "4a443dc3d9bbd708762a782f0355c8cfa19f18f18e297651b61217b891e34fb9"
  #error "Accepted payload mismatch (x86): pt-BR\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pt-BR\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pt-BR\System.Xaml.resources.dll") != "9a85e88964339e7bd11823ddfdf6b5723b581a81bc9f5aa734fc5a7588b6b2dc"
  #error "Accepted payload mismatch (x64): pt-BR\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pt-BR\System.Xaml.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pt-BR\System.Xaml.resources.dll") != "3cf6e97d1d01ab389b78edef07ec62de7718d77d53bd0f38750c3705546df7d9"
  #error "Accepted payload mismatch (x86): pt-BR\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pt-BR\System.Xaml.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pt-BR\UIAutomationClient.resources.dll") != "36b5081d679e8cb02c339ae8f925024540865616aa81362d8e3061143dc46013"
  #error "Accepted payload mismatch (x64): pt-BR\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pt-BR\UIAutomationClient.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pt-BR\UIAutomationClient.resources.dll") != "1dd330f97cd78a36a11b11a0eed4263308a0c2656bd837898495d69a40b8bfb2"
  #error "Accepted payload mismatch (x86): pt-BR\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pt-BR\UIAutomationClient.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pt-BR\UIAutomationClientSideProviders.resources.dll") != "045fdecf635ddb6ea4da9562869278f034744fc1ae1e462521c6c46d6ae8d5d7"
  #error "Accepted payload mismatch (x64): pt-BR\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pt-BR\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pt-BR\UIAutomationClientSideProviders.resources.dll") != "9d4bd119404749034b2296e437796a806e437fd020cc226e583572e215b48bd4"
  #error "Accepted payload mismatch (x86): pt-BR\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pt-BR\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pt-BR\UIAutomationProvider.resources.dll") != "672111ff2bb6cfb543a4c410eebbba8d9dab68731de7dcc2a12b4c6c7b228625"
  #error "Accepted payload mismatch (x64): pt-BR\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pt-BR\UIAutomationProvider.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pt-BR\UIAutomationProvider.resources.dll") != "1f3bf2d66b67d1e0385d41e41d87c0625721bfbfc7daca9798b24a8d79630482"
  #error "Accepted payload mismatch (x86): pt-BR\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pt-BR\UIAutomationProvider.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pt-BR\UIAutomationTypes.resources.dll") != "1e215878bff7de64ff0e7fa9333b37799594ea7e136263904863e8d872c80875"
  #error "Accepted payload mismatch (x64): pt-BR\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pt-BR\UIAutomationTypes.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pt-BR\UIAutomationTypes.resources.dll") != "91549ace1bf5c9a19be2befab7105e72305aca54728a8536278a59bda7b1b114"
  #error "Accepted payload mismatch (x86): pt-BR\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pt-BR\UIAutomationTypes.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pt-BR\WindowsBase.resources.dll") != "8afcb765fc0cb1874edee7bc139364b789b3f336587464a68a7a8b6c36860934"
  #error "Accepted payload mismatch (x64): pt-BR\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pt-BR\WindowsBase.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pt-BR\WindowsBase.resources.dll") != "47239d9ff21cc24636b0f59b658c323e67972a7e22bb6f261caa0937aa105eb7"
  #error "Accepted payload mismatch (x86): pt-BR\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pt-BR\WindowsBase.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\pt-BR\WindowsFormsIntegration.resources.dll") != "03522d1f855865adb481b5c19e5929e44a31f0fade0a2af79f67e7b10f835172"
  #error "Accepted payload mismatch (x64): pt-BR\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\pt-BR\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\pt-BR\WindowsFormsIntegration.resources.dll") != "7e7b1bdc12cb4cef7bb49aabb50baaaf33c24183e4dcd0cca807b6cca14c9155"
  #error "Accepted payload mismatch (x86): pt-BR\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\pt-BR\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ru\Microsoft.VisualBasic.Forms.resources.dll") != "9e864f31dc8b37bbb6880c8861e0bf4a848fcd3d1d1825865aa0c6d882f354f5"
  #error "Accepted payload mismatch (x64): ru\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ru\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ru\Microsoft.VisualBasic.Forms.resources.dll") != "d575e4bf9dd19d8fa02e5eb855583d5420cf537b480665fc6c176532299935a9"
  #error "Accepted payload mismatch (x86): ru\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ru\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ru\PresentationCore.resources.dll") != "2dce077185679161482aaacdc3f1aeb054a90f69c82343819f3e79f00f0abb3c"
  #error "Accepted payload mismatch (x64): ru\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ru\PresentationCore.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ru\PresentationCore.resources.dll") != "3273c050d0f0d6160ab8ea6924da0c5de7b8424578fd150f71249ac080f9aaa1"
  #error "Accepted payload mismatch (x86): ru\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ru\PresentationCore.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ru\PresentationFramework.resources.dll") != "05b6767a66b16e16e3b3f05cc5e9ebca2efd9d5324d3e815bc3043316cca5fbc"
  #error "Accepted payload mismatch (x64): ru\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ru\PresentationFramework.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ru\PresentationFramework.resources.dll") != "39d33bbaaf72184471bf2a855c4d913a1841d96bad5950ee54baa59d4f267d48"
  #error "Accepted payload mismatch (x86): ru\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ru\PresentationFramework.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ru\PresentationUI.resources.dll") != "5fc5e3277ef9095d0976d6175248c8652e722d0f13867ec8d42e098ae620376c"
  #error "Accepted payload mismatch (x64): ru\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ru\PresentationUI.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ru\PresentationUI.resources.dll") != "a6cab7321a5034cbded78c686f6a9ae9e5bbaec51f30ad397ab4c7399e21cc40"
  #error "Accepted payload mismatch (x86): ru\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ru\PresentationUI.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ru\ReachFramework.resources.dll") != "66d6bd56b16b74542fa1e0ac6a66208b155382c06511dd92c665abf2ecb56756"
  #error "Accepted payload mismatch (x64): ru\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ru\ReachFramework.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ru\ReachFramework.resources.dll") != "c3abb42f885ee5f0fc7c152fa2dc4b3ad7d1175b61fde1c307bbfe997e171895"
  #error "Accepted payload mismatch (x86): ru\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ru\ReachFramework.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ru\System.Windows.Controls.Ribbon.resources.dll") != "2503d0660c1b97a7afb6a762cfe118ae66b5cb796ae6c663730d7f3e74fdf16c"
  #error "Accepted payload mismatch (x64): ru\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ru\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ru\System.Windows.Controls.Ribbon.resources.dll") != "f2bc7a92caab7c78ab61a823b77c6ff4021538a11b9b416c21602d935b8c8728"
  #error "Accepted payload mismatch (x86): ru\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ru\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ru\System.Windows.Forms.Design.resources.dll") != "1be97e129e64652902befc35ee8dd8b4fc90a5e8dc1e2b3918537ad4eb54c4aa"
  #error "Accepted payload mismatch (x64): ru\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ru\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ru\System.Windows.Forms.Design.resources.dll") != "ed5f3a95918d5d74db8d44eb5c183a6ad6a153dee4e4eabe8dfc5cb7b5e45545"
  #error "Accepted payload mismatch (x86): ru\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ru\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ru\System.Windows.Forms.Primitives.resources.dll") != "671831a29ffdd4323a11baa127dff2965a0b15501eefdae5ee760c9c076b833d"
  #error "Accepted payload mismatch (x64): ru\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ru\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ru\System.Windows.Forms.Primitives.resources.dll") != "701edb411aa4f93b586130af1fc04ce0d3fd642ee7aa387511f05fae5fe71432"
  #error "Accepted payload mismatch (x86): ru\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ru\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ru\System.Windows.Forms.resources.dll") != "7160c03cf2d0c7f5b4b83f7edf029c8e99d05303e063a69f3b980715b47cdb30"
  #error "Accepted payload mismatch (x64): ru\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ru\System.Windows.Forms.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ru\System.Windows.Forms.resources.dll") != "bd4a3c1633586e3f44def871dfe5241cea265b37759a5cd55d45d752bd6c128e"
  #error "Accepted payload mismatch (x86): ru\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ru\System.Windows.Forms.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ru\System.Windows.Input.Manipulations.resources.dll") != "b721c108821a3993ebf04a5b73f0f65791cc3745fb7dcbf9641eeb3cadbf1a92"
  #error "Accepted payload mismatch (x64): ru\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ru\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ru\System.Windows.Input.Manipulations.resources.dll") != "ae5cf7829b66f97772202c7311a91d8ad07d885bdf7b6102aef062d5f8591d93"
  #error "Accepted payload mismatch (x86): ru\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ru\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ru\System.Xaml.resources.dll") != "efb03a6255f46834ae3d8ca5da1f194cdbf63823c7d96fa369c636b2df16361a"
  #error "Accepted payload mismatch (x64): ru\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ru\System.Xaml.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ru\System.Xaml.resources.dll") != "118a76fd4f68296c7d6bfd0abe263c318a65ac51a32dbd32d3c9f81c13b877f5"
  #error "Accepted payload mismatch (x86): ru\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ru\System.Xaml.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ru\UIAutomationClient.resources.dll") != "be64e226c0cb615c96474f314b108265ce1239d8ff0595c03093fb0c4a490604"
  #error "Accepted payload mismatch (x64): ru\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ru\UIAutomationClient.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ru\UIAutomationClient.resources.dll") != "cdf461180314c95bfb731a3ef396ed3716b00d21f7231ab3b176ba83ae69bd04"
  #error "Accepted payload mismatch (x86): ru\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ru\UIAutomationClient.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ru\UIAutomationClientSideProviders.resources.dll") != "6fe40bd42f4181520ab76e6c63384ad2c0f0cf049e53c30ad6578ffda91bfe5a"
  #error "Accepted payload mismatch (x64): ru\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ru\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ru\UIAutomationClientSideProviders.resources.dll") != "6692c10c6236ff0e664022c84a72f06a053190dada6253c29f7786e38fafcb30"
  #error "Accepted payload mismatch (x86): ru\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ru\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ru\UIAutomationProvider.resources.dll") != "95820df1027a1cb2046e9cc2abcd5f7fc66409b64e27cacf472ca38e2ae2e9fc"
  #error "Accepted payload mismatch (x64): ru\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ru\UIAutomationProvider.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ru\UIAutomationProvider.resources.dll") != "ac6570a07ddea24a030023aa811eb53447d3f69d8d1e67e47a6d34f16d190511"
  #error "Accepted payload mismatch (x86): ru\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ru\UIAutomationProvider.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ru\UIAutomationTypes.resources.dll") != "de30ed051674a2cca77a2c9b7c7be4a08e7622e9678db243d0848745c3486596"
  #error "Accepted payload mismatch (x64): ru\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ru\UIAutomationTypes.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ru\UIAutomationTypes.resources.dll") != "e51ebc920ca4c3c4b2484ab210854172816e4d5c5273f15c2b450e397f264552"
  #error "Accepted payload mismatch (x86): ru\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ru\UIAutomationTypes.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ru\WindowsBase.resources.dll") != "78bc67f919c1302a55d0acbf3e0ed06b5340e921d1d4628283412d49b75239b0"
  #error "Accepted payload mismatch (x64): ru\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ru\WindowsBase.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ru\WindowsBase.resources.dll") != "0793b250974a41bb3271212b762468a2b08ffb9e0ab323921bf0bc680d8192e6"
  #error "Accepted payload mismatch (x86): ru\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ru\WindowsBase.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\ru\WindowsFormsIntegration.resources.dll") != "625104b3365227aff162368e140ff4e523aef39c045fcdbe6a60506bbe34a43e"
  #error "Accepted payload mismatch (x64): ru\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\ru\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\ru\WindowsFormsIntegration.resources.dll") != "2da96cf9f4f28b65267703917b4610206aa9b6cc04ba3f56395d3256da1e955a"
  #error "Accepted payload mismatch (x86): ru\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\ru\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\sedang_sholat.png") != "e64b8515280119b4e4e7f0e59c74244da5a44a815f1dd4cb5c4e2dbc7868a5df"
  #error "Accepted payload mismatch (x86): sedang_sholat.png"
#endif
#if GetSHA256OfFile(MyAppSourceDirX64 + "\sedang_sholat.png") != "e64b8515280119b4e4e7f0e59c74244da5a44a815f1dd4cb5c4e2dbc7868a5df"
  #error "Accepted payload mismatch (x64): sedang_sholat.png"
#endif
Source: "{#MyAppSourceDirX64}\sedang_sholat.png"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDirX64 + "\tr\Microsoft.VisualBasic.Forms.resources.dll") != "e7d8f53b05a6942e92c54a86364af161e7523acd1dd416c62b244dedb1e27659"
  #error "Accepted payload mismatch (x64): tr\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\tr\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\tr\Microsoft.VisualBasic.Forms.resources.dll") != "d4a282fcdf686b6ec8913f1421d4df42dea8642ac70c85cfc4578f98c3f42e27"
  #error "Accepted payload mismatch (x86): tr\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\tr\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\tr\PresentationCore.resources.dll") != "d507b188db1c6cf3a2c1d5305a10dbf2a6ea87f5fefac75c3ab65e3dcbb044d2"
  #error "Accepted payload mismatch (x64): tr\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\tr\PresentationCore.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\tr\PresentationCore.resources.dll") != "fa92a11bc85697559197eff8b183ca4ec07ef430c0fa77ea007d7ed5af047f19"
  #error "Accepted payload mismatch (x86): tr\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\tr\PresentationCore.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\tr\PresentationFramework.resources.dll") != "99df896f2b0e7f382843efe8de57255cf1e0ab785ed4c999c155960234b0611c"
  #error "Accepted payload mismatch (x64): tr\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\tr\PresentationFramework.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\tr\PresentationFramework.resources.dll") != "428298e1de2d49b151316e2ab9a8357418f8a22599a9d95c1691c76f32e987f9"
  #error "Accepted payload mismatch (x86): tr\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\tr\PresentationFramework.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\tr\PresentationUI.resources.dll") != "fb4ad8f69c2c628727fd7bb50bff35eb29ee54a8fec5b0d1373d056c72d7e0ca"
  #error "Accepted payload mismatch (x64): tr\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\tr\PresentationUI.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\tr\PresentationUI.resources.dll") != "10cd3fafa8276a721588ce110d2752ac1de9e39bc46ed169b7fdba5780bdf16b"
  #error "Accepted payload mismatch (x86): tr\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\tr\PresentationUI.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\tr\ReachFramework.resources.dll") != "e69eceeddaadd2b43ebd64c8861b48edd1780968e471c5dc1036d0a8411bcd2d"
  #error "Accepted payload mismatch (x64): tr\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\tr\ReachFramework.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\tr\ReachFramework.resources.dll") != "8ab1abf60b16f20882030e8132bb100c6d59731d885e4efe1cd46e6490382f3f"
  #error "Accepted payload mismatch (x86): tr\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\tr\ReachFramework.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\tr\System.Windows.Controls.Ribbon.resources.dll") != "f6b9c384b21dcc421bbbe411719f6cb62c6cf493608bb603afa3450ab2534007"
  #error "Accepted payload mismatch (x64): tr\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\tr\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\tr\System.Windows.Controls.Ribbon.resources.dll") != "28434dfd26bb0b336b853ca900ccb24556bd98f2ffd165e6839394918d38aaa9"
  #error "Accepted payload mismatch (x86): tr\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\tr\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\tr\System.Windows.Forms.Design.resources.dll") != "693c5bb1fa9d2b77e34eddd54264c7b75c603bcf06fab10f123e094a9646485a"
  #error "Accepted payload mismatch (x64): tr\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\tr\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\tr\System.Windows.Forms.Design.resources.dll") != "529396c14dd14506defa5f22c5afa500c16b71bc90cf4df1a1f44e3638b2d317"
  #error "Accepted payload mismatch (x86): tr\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\tr\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\tr\System.Windows.Forms.Primitives.resources.dll") != "091b79e409ba557a143d7c9ce0f8d93fbd08d6bfdf5fa0f67849e5a9fda96a1b"
  #error "Accepted payload mismatch (x64): tr\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\tr\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\tr\System.Windows.Forms.Primitives.resources.dll") != "a6af44b857b85c60c18e6df34bc77f0f4482c902b2f903252d181412b44cc2ac"
  #error "Accepted payload mismatch (x86): tr\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\tr\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\tr\System.Windows.Forms.resources.dll") != "b9f4b8216c53087044441f31671e67dc14dff8aea6626295a38220fcc5d8fe22"
  #error "Accepted payload mismatch (x64): tr\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\tr\System.Windows.Forms.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\tr\System.Windows.Forms.resources.dll") != "8f82983ce6d8537e79428994ff4313d4d91d7a455eb420687a2360f086442d9a"
  #error "Accepted payload mismatch (x86): tr\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\tr\System.Windows.Forms.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\tr\System.Windows.Input.Manipulations.resources.dll") != "ee82ba1eaedef8cf267be1b21b1a338158ec22f1d255c279cbf6e8ef19042322"
  #error "Accepted payload mismatch (x64): tr\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\tr\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\tr\System.Windows.Input.Manipulations.resources.dll") != "19dbcc5c99a93ce1f75669e069999b4a4c393f6b0fecf6dab4608c60d4b90da0"
  #error "Accepted payload mismatch (x86): tr\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\tr\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\tr\System.Xaml.resources.dll") != "4335ea915a43676b5446a3f925b7175711f6caef9f13d19de7a1026d2c4c9684"
  #error "Accepted payload mismatch (x64): tr\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\tr\System.Xaml.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\tr\System.Xaml.resources.dll") != "47ee328f699a55cd986cf15eb764219623aaf9eed67620cb60f1d49446cc27d9"
  #error "Accepted payload mismatch (x86): tr\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\tr\System.Xaml.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\tr\UIAutomationClient.resources.dll") != "2c231dc6c3f57698eba849f8a9e57c0da16fe1ca6d60de45bff8045b77e04fd8"
  #error "Accepted payload mismatch (x64): tr\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\tr\UIAutomationClient.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\tr\UIAutomationClient.resources.dll") != "f120856a0443c6ae0d0bb3a6d5aad8c4b885b47bd4ad921334574848e3f1c2ca"
  #error "Accepted payload mismatch (x86): tr\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\tr\UIAutomationClient.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\tr\UIAutomationClientSideProviders.resources.dll") != "c1544021f60a88ed8c2554511c855b1db0370e784222035b184a1d064e7d5ac2"
  #error "Accepted payload mismatch (x64): tr\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\tr\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\tr\UIAutomationClientSideProviders.resources.dll") != "53107d4d920e049c097de465ad803cf022b0307ca5c627d4ba2c6f6778ede9cc"
  #error "Accepted payload mismatch (x86): tr\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\tr\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\tr\UIAutomationProvider.resources.dll") != "c3a05778e1203680ec621d989830418a72bad402f1da78e767de441a169a6a65"
  #error "Accepted payload mismatch (x64): tr\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\tr\UIAutomationProvider.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\tr\UIAutomationProvider.resources.dll") != "5bf40181ec87abda2e3d962c6ac1c0c4ad8e5f1564369d8931224ea1bf7ed80f"
  #error "Accepted payload mismatch (x86): tr\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\tr\UIAutomationProvider.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\tr\UIAutomationTypes.resources.dll") != "ef9f5cbeeb319cc6f0bcd59223e10898a19993f0375e81dc7bc425c6098a7cb5"
  #error "Accepted payload mismatch (x64): tr\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\tr\UIAutomationTypes.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\tr\UIAutomationTypes.resources.dll") != "611dafa69c993dcb6484c9fd50f397d29f3aa4d330408997fd9538fdcd77e58a"
  #error "Accepted payload mismatch (x86): tr\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\tr\UIAutomationTypes.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\tr\WindowsBase.resources.dll") != "5315c4a11f199cd4eb149f90d6e6dc9e5632afaac832aaf2564f1b94b7682da9"
  #error "Accepted payload mismatch (x64): tr\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\tr\WindowsBase.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\tr\WindowsBase.resources.dll") != "8f28564349aff93d86caaf0fd391661c96e859c0173c8ac7a620da890db1198b"
  #error "Accepted payload mismatch (x86): tr\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\tr\WindowsBase.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\tr\WindowsFormsIntegration.resources.dll") != "bdbdd0c6437018eea10867698c67fb1718f038f69b50920e65ca30501cab2b94"
  #error "Accepted payload mismatch (x64): tr\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\tr\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\tr\WindowsFormsIntegration.resources.dll") != "6f9f0e8732232d5251418eb428dc32bc9b216a7229969af747f78b9ae15a4cfa"
  #error "Accepted payload mismatch (x86): tr\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\tr\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\vcruntime140_cor3.dll") != "d5e4d9a3e835fa679450145d6a7d94e36573a509317111904d9b3712c30d9066"
  #error "Accepted payload mismatch (x64): vcruntime140_cor3.dll"
#endif
Source: "{#MyAppSourceDirX64}\vcruntime140_cor3.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\vcruntime140_cor3.dll") != "a9d7732a3c3de09f0601a7fa2555093dcb49f08e2fc62a86091461f0f38393ed"
  #error "Accepted payload mismatch (x86): vcruntime140_cor3.dll"
#endif
Source: "{#MyAppSourceDirX86}\vcruntime140_cor3.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\wpfgfx_cor3.dll") != "4032bb15f41399123c77a7f02bb19037f213cb32a66159cf1b536bf5f9f1ff92"
  #error "Accepted payload mismatch (x64): wpfgfx_cor3.dll"
#endif
Source: "{#MyAppSourceDirX64}\wpfgfx_cor3.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\wpfgfx_cor3.dll") != "c30263f3598942c9dd55839446fba6cdf49fd0845d7f89ece180c431c0d91204"
  #error "Accepted payload mismatch (x86): wpfgfx_cor3.dll"
#endif
Source: "{#MyAppSourceDirX86}\wpfgfx_cor3.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hans\Microsoft.VisualBasic.Forms.resources.dll") != "aa904e05e141d33f966f7ecbe450368921aeb33b0a4de84806f3c30c57683529"
  #error "Accepted payload mismatch (x64): zh-Hans\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hans\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hans\Microsoft.VisualBasic.Forms.resources.dll") != "6489a27f72d7c0bbefe91d1544b24bee49750825ac5645463663808ca0c71799"
  #error "Accepted payload mismatch (x86): zh-Hans\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hans\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hans\PresentationCore.resources.dll") != "c38ff390718f1713068659be729c99530ec37d01fef738df1f492bf7022ccde3"
  #error "Accepted payload mismatch (x64): zh-Hans\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hans\PresentationCore.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hans\PresentationCore.resources.dll") != "9f3c8f26178da8fd48ca96de13abb90463d819fa699bd2ad30314d1c7bcf32dc"
  #error "Accepted payload mismatch (x86): zh-Hans\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hans\PresentationCore.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hans\PresentationFramework.resources.dll") != "91a357574d2467d375aef931e21356548000dea7b56a12de549f20705946bacc"
  #error "Accepted payload mismatch (x64): zh-Hans\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hans\PresentationFramework.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hans\PresentationFramework.resources.dll") != "75bce0fce3c366ad395cddd3c23c2cd2c62fc711c224eaa1631159af573187c9"
  #error "Accepted payload mismatch (x86): zh-Hans\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hans\PresentationFramework.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hans\PresentationUI.resources.dll") != "0b06c7a1e1294913e104ee06f4db72249953c1b398496c5e3f25f26ed49f5be4"
  #error "Accepted payload mismatch (x64): zh-Hans\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hans\PresentationUI.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hans\PresentationUI.resources.dll") != "a92623ae5cefdcfb45a402de22a63da88725663594e77a78231a733f8be455ed"
  #error "Accepted payload mismatch (x86): zh-Hans\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hans\PresentationUI.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hans\ReachFramework.resources.dll") != "2f2c3228b3e9f11bcca1983d25a55abc1d207fdc055fcb14acc3654b15669fbb"
  #error "Accepted payload mismatch (x64): zh-Hans\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hans\ReachFramework.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hans\ReachFramework.resources.dll") != "27c71401c2a5cd7ec03dfa91d3445e573a9a9279813b5edce817b63a41ee0e84"
  #error "Accepted payload mismatch (x86): zh-Hans\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hans\ReachFramework.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hans\System.Windows.Controls.Ribbon.resources.dll") != "b58b4cea2dd6ee126b554b9b2dbdf722b2c267e4a8f28d90cc4c9b3ffe505276"
  #error "Accepted payload mismatch (x64): zh-Hans\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hans\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hans\System.Windows.Controls.Ribbon.resources.dll") != "cc1f186ec73337660cd4f9dc7cafe43bf243a34f6ab637dcff93f00e6566cc3c"
  #error "Accepted payload mismatch (x86): zh-Hans\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hans\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hans\System.Windows.Forms.Design.resources.dll") != "0b6281d6ebb670931f25df6532554056507a3075fd90001ecc8d07ff45ba553f"
  #error "Accepted payload mismatch (x64): zh-Hans\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hans\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hans\System.Windows.Forms.Design.resources.dll") != "96f504d5c393746033760e45b1f90eac9cafdd501b9721a00cb19b7abe089640"
  #error "Accepted payload mismatch (x86): zh-Hans\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hans\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hans\System.Windows.Forms.Primitives.resources.dll") != "40461222cc03c81935e8049f3f30a5100a58516fddf262a60cdc58105ac1ac93"
  #error "Accepted payload mismatch (x64): zh-Hans\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hans\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hans\System.Windows.Forms.Primitives.resources.dll") != "656f68521191e70727897ae694d8e0c5bcb16b7c2984efca6cf0fdf2490dbb29"
  #error "Accepted payload mismatch (x86): zh-Hans\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hans\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hans\System.Windows.Forms.resources.dll") != "c7135ff6e1c12fa2536d4ab54929b4738ff32df2a9f7f96a901f286ea6ae1dbb"
  #error "Accepted payload mismatch (x64): zh-Hans\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hans\System.Windows.Forms.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hans\System.Windows.Forms.resources.dll") != "5c7ac551a767f362249f76630af82c52e9e0678107b6956e2ea6d03bd23be394"
  #error "Accepted payload mismatch (x86): zh-Hans\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hans\System.Windows.Forms.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hans\System.Windows.Input.Manipulations.resources.dll") != "d5572b01b96ed911785f2e0a5fd540516a65d4486db1b4d71044c8a127195df6"
  #error "Accepted payload mismatch (x64): zh-Hans\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hans\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hans\System.Windows.Input.Manipulations.resources.dll") != "507e95ae0858b2c2e27a18a0832d1362174c3a3d76d51ead14f2b58c7a127ab1"
  #error "Accepted payload mismatch (x86): zh-Hans\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hans\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hans\System.Xaml.resources.dll") != "3757e3776d6861a87b42a8cdd0949eed6a92b8cc509efb9fb91b9f3c235ad0fe"
  #error "Accepted payload mismatch (x64): zh-Hans\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hans\System.Xaml.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hans\System.Xaml.resources.dll") != "0f8665b20bf53ba07bfcdfbf71673d79fda408d5cc0edc154938a4ffe6929715"
  #error "Accepted payload mismatch (x86): zh-Hans\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hans\System.Xaml.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hans\UIAutomationClient.resources.dll") != "9a73cbd13d86f6eaf2ec29dd0a07a044b79ab7ec0a92ee642fba27d84c6cc4e9"
  #error "Accepted payload mismatch (x64): zh-Hans\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hans\UIAutomationClient.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hans\UIAutomationClient.resources.dll") != "7f08b950840f37cd210ce036f624ea69d5f52dfba763eb77738168d091bb6771"
  #error "Accepted payload mismatch (x86): zh-Hans\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hans\UIAutomationClient.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hans\UIAutomationClientSideProviders.resources.dll") != "b7c9db78f3cb8ea4f34adde1f884043ab8566c10b51bbd7f3e048e92682fa16f"
  #error "Accepted payload mismatch (x64): zh-Hans\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hans\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hans\UIAutomationClientSideProviders.resources.dll") != "85fc83d070b19bc28046ef85d797949c84aea63c4e0a00d112dbcd0a3faa2709"
  #error "Accepted payload mismatch (x86): zh-Hans\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hans\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hans\UIAutomationProvider.resources.dll") != "4f776959ca842ce49bdc565f233b742cc320dfb32931b0f1e58fcd1ef2ae9b6a"
  #error "Accepted payload mismatch (x64): zh-Hans\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hans\UIAutomationProvider.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hans\UIAutomationProvider.resources.dll") != "3586bdd6ba044479d2cfe815419b638f0f0c5d18068f31b4222233cfc08cadc2"
  #error "Accepted payload mismatch (x86): zh-Hans\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hans\UIAutomationProvider.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hans\UIAutomationTypes.resources.dll") != "c1347f5fc3efbfb6f8c850a8b6667f56380bff0b9eab5d06bb24cbd70e86aa35"
  #error "Accepted payload mismatch (x64): zh-Hans\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hans\UIAutomationTypes.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hans\UIAutomationTypes.resources.dll") != "daa76be302953d05bc48abed4a6e30cd3acfe38808ad1c4a5266be63f1a1a716"
  #error "Accepted payload mismatch (x86): zh-Hans\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hans\UIAutomationTypes.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hans\WindowsBase.resources.dll") != "7ac0ae9f46cb4aaae580892f2783cb58286f52d5824c5d6552ceee4f9ed9b199"
  #error "Accepted payload mismatch (x64): zh-Hans\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hans\WindowsBase.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hans\WindowsBase.resources.dll") != "b41c6aaea6c2469dcf45a9daaec2ea446d002c25bd8b892ab3eac8d0d3c35f4e"
  #error "Accepted payload mismatch (x86): zh-Hans\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hans\WindowsBase.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hans\WindowsFormsIntegration.resources.dll") != "bdbba66f74d4875237530ecbcb7c033b24a596c33943e5e7644d55cb109c904b"
  #error "Accepted payload mismatch (x64): zh-Hans\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hans\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hans\WindowsFormsIntegration.resources.dll") != "7cb1a3196fa4fb1cda5f7eb708637d226fa26a8e3fbfb6208c6541df50318b41"
  #error "Accepted payload mismatch (x86): zh-Hans\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hans\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hant\Microsoft.VisualBasic.Forms.resources.dll") != "d077acaf53db36b7081532467c2a6c993bde6cf09314d86e76844773d4e649b1"
  #error "Accepted payload mismatch (x64): zh-Hant\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hant\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hant\Microsoft.VisualBasic.Forms.resources.dll") != "665d2da3cbcad7a798e0e147c63ffb7385fbde62e9a58b736ac5a28fd1fac533"
  #error "Accepted payload mismatch (x86): zh-Hant\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hant\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hant\PresentationCore.resources.dll") != "a57249cf8d505b31d46ee75df92d7e60b19db49a3ac3942b4e8f81458c616c2a"
  #error "Accepted payload mismatch (x64): zh-Hant\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hant\PresentationCore.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hant\PresentationCore.resources.dll") != "d7d6c2665e99adbcba3aee2e8de1a4268f7f65c0d567d7eb82d897aed6d99d1e"
  #error "Accepted payload mismatch (x86): zh-Hant\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hant\PresentationCore.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hant\PresentationFramework.resources.dll") != "79abdf4e2a3e0093f8145881ecd4432b85df711612ea1050c527145a011849aa"
  #error "Accepted payload mismatch (x64): zh-Hant\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hant\PresentationFramework.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hant\PresentationFramework.resources.dll") != "35710e010fafca5f0a44107ba43a909176a9f2cbf0590a885e4cbcf60332a480"
  #error "Accepted payload mismatch (x86): zh-Hant\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hant\PresentationFramework.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hant\PresentationUI.resources.dll") != "98de31f40b7e5a34ea9ebfa74fc7cbbb1307709551542748843a346a9247aaaf"
  #error "Accepted payload mismatch (x64): zh-Hant\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hant\PresentationUI.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hant\PresentationUI.resources.dll") != "2534a49bd51ce3de315d5356c6dada28d97a9226bd2c971198df92aac81382c2"
  #error "Accepted payload mismatch (x86): zh-Hant\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hant\PresentationUI.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hant\ReachFramework.resources.dll") != "e92ffed87f7ba8000bd5e322d4362b878e23fa1628d4506049641e5270f4ffbc"
  #error "Accepted payload mismatch (x64): zh-Hant\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hant\ReachFramework.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hant\ReachFramework.resources.dll") != "cdc9c85ef5e3fe0d7cfc796296602181fbb83e3a9bf7ef14793880484f875e8c"
  #error "Accepted payload mismatch (x86): zh-Hant\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hant\ReachFramework.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hant\System.Windows.Controls.Ribbon.resources.dll") != "bd05fb9c2a4674c91099ba275fd905d2107d1dabc1c6f19f073de908322f83c8"
  #error "Accepted payload mismatch (x64): zh-Hant\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hant\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hant\System.Windows.Controls.Ribbon.resources.dll") != "af4237bfad4a397f30d7fddb45ecd14f3c9a708cde8615ce85b5858a0cecc263"
  #error "Accepted payload mismatch (x86): zh-Hant\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hant\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hant\System.Windows.Forms.Design.resources.dll") != "f2c73a92cea6b0e15b4c86b0942ad03c2076aa11c9dac324f19bc6b561ed201c"
  #error "Accepted payload mismatch (x64): zh-Hant\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hant\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hant\System.Windows.Forms.Design.resources.dll") != "34e1f72aed9a63385074eb144bc2861634757f7abf73326e102b5524179c7a40"
  #error "Accepted payload mismatch (x86): zh-Hant\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hant\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hant\System.Windows.Forms.Primitives.resources.dll") != "0d8fa3aa1a36839991a6d56fcc4872f8750e4405ed1a84ab5dea3777d1fef3cf"
  #error "Accepted payload mismatch (x64): zh-Hant\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hant\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hant\System.Windows.Forms.Primitives.resources.dll") != "82b9b08b28b9a5a4057e6cf04babbc3ff5fb60fa7bfce1e839fca4c94e3edfd7"
  #error "Accepted payload mismatch (x86): zh-Hant\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hant\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hant\System.Windows.Forms.resources.dll") != "5856d81267e2f23ddf9627519562618c4688aed0b1625f441afd8cb24fd9e83b"
  #error "Accepted payload mismatch (x64): zh-Hant\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hant\System.Windows.Forms.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hant\System.Windows.Forms.resources.dll") != "e28a8591b3fa8df8d609a2ca8345cb62ec0e8ab8280de92f5cfa4951342f3f5e"
  #error "Accepted payload mismatch (x86): zh-Hant\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hant\System.Windows.Forms.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hant\System.Windows.Input.Manipulations.resources.dll") != "117a1fb21d97d95dde5f6fa295532670443ac325ebaeb16a1b25f184e4f64ddc"
  #error "Accepted payload mismatch (x64): zh-Hant\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hant\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hant\System.Windows.Input.Manipulations.resources.dll") != "9f744733af77fa95c619f7a3a17236168daa5e323de8b4e1e2f25fd261ce8fb5"
  #error "Accepted payload mismatch (x86): zh-Hant\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hant\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hant\System.Xaml.resources.dll") != "b2f97bb31674036d8a06c06950281497ed8498c374f9be5b4d26c7fe4d2750a0"
  #error "Accepted payload mismatch (x64): zh-Hant\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hant\System.Xaml.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hant\System.Xaml.resources.dll") != "e98a861c3e85cf407fd948351b8d94485993b503320dedc53c20a9c4bd26ac9b"
  #error "Accepted payload mismatch (x86): zh-Hant\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hant\System.Xaml.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hant\UIAutomationClient.resources.dll") != "9a1dc8bdd0b782981d57037093be3288b487f96e7b91519fd665c84bf43a2285"
  #error "Accepted payload mismatch (x64): zh-Hant\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hant\UIAutomationClient.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hant\UIAutomationClient.resources.dll") != "7b80ffeabf6dc8a8b18e637e9d62824fe1c2dff772aad656f1830078887f66c2"
  #error "Accepted payload mismatch (x86): zh-Hant\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hant\UIAutomationClient.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hant\UIAutomationClientSideProviders.resources.dll") != "495f8bcb66130e269f9ad24ef5f68657aa56b9fb8f7d816861ce0eb21e9cea34"
  #error "Accepted payload mismatch (x64): zh-Hant\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hant\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hant\UIAutomationClientSideProviders.resources.dll") != "028323d6d6ee1694d2a0c6ff07f8f0cae9187411d2357335d81bb821a2785279"
  #error "Accepted payload mismatch (x86): zh-Hant\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hant\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hant\UIAutomationProvider.resources.dll") != "97b87a33d84f0b3dda9aa0565e5c677b6d04c372a028780f839cd2ed7aca5b78"
  #error "Accepted payload mismatch (x64): zh-Hant\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hant\UIAutomationProvider.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hant\UIAutomationProvider.resources.dll") != "10526ce32a18fbf0798c0aab9f17be165a26dd4fc6894e029b868ff8543173f1"
  #error "Accepted payload mismatch (x86): zh-Hant\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hant\UIAutomationProvider.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hant\UIAutomationTypes.resources.dll") != "1a3c6b9df0ef1787f8bb51b4c5eafebb41f49f0b6e5bc80aecfc82fdff2b8428"
  #error "Accepted payload mismatch (x64): zh-Hant\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hant\UIAutomationTypes.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hant\UIAutomationTypes.resources.dll") != "51409ce46c2a3adbd1bd2048329ca4ef72f73c5db4d78e3a6a85b63ccbff1797"
  #error "Accepted payload mismatch (x86): zh-Hant\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hant\UIAutomationTypes.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hant\WindowsBase.resources.dll") != "0889cc76ec99f87eeeeaa6ebb6640664e11807ce69f6a4a6cf90d4283eca8817"
  #error "Accepted payload mismatch (x64): zh-Hant\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hant\WindowsBase.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hant\WindowsBase.resources.dll") != "158f21acfa03cf6405ace17cb91a166fb21e7b707456428d023971a5d45173fb"
  #error "Accepted payload mismatch (x86): zh-Hant\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hant\WindowsBase.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
#if GetSHA256OfFile(MyAppSourceDirX64 + "\zh-Hant\WindowsFormsIntegration.resources.dll") != "26f1099810375f61752a493e68ed45e392739878f892f251f13bf5083e51fa13"
  #error "Accepted payload mismatch (x64): zh-Hant\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX64}\zh-Hant\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX64Arch
#if GetSHA256OfFile(MyAppSourceDirX86 + "\zh-Hant\WindowsFormsIntegration.resources.dll") != "d1832cc36fbb03c586d08db7700f2eee6c76c1eabd16ffef7812fe9a6f9917ed"
  #error "Accepted payload mismatch (x86): zh-Hant\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDirX86}\zh-Hant\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite and IsX86Arch
