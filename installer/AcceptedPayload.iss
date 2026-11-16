; Generated from technically reviewed 2026-11-02 Taskbar GdiFix receipt. Runtime acceptance PENDING. Do not substitute/rebuild runtime.
; GetSHA256OfFile is ISPP; mismatches stop compilation before packaging.
#if GetSHA256OfFile(MyAppSourceDir + "\Accessibility.dll") != "79c8466d82324e8f599efbc039e9cbe833b2a21e6246dd4c2290a4fe84445412"
  #error "Accepted payload mismatch: Accessibility.dll"
#endif
Source: "{#MyAppSourceDir}\Accessibility.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\BACA_SAYA.txt") != "fa73a7c46fada91a8ecec283e8aa5af1dd6bf049a2843db115290f8ba0043fe8"
  #error "Accepted payload mismatch: BACA_SAYA.txt"
#endif
Source: "{#MyAppSourceDir}\BACA_SAYA.txt"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\D3DCompiler_47_cor3.dll") != "a05f99734f7c4822fefc12b367af21fd0976ed6608752fb1e1e80b6ece7ecbbb"
  #error "Accepted payload mismatch: D3DCompiler_47_cor3.dll"
#endif
Source: "{#MyAppSourceDir}\D3DCompiler_47_cor3.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\DirectWriteForwarder.dll") != "614a3c33df38aba9136eae6aa6bb57aa487d93643c1476f7f5f10edac67ed4ec"
  #error "Accepted payload mismatch: DirectWriteForwarder.dll"
#endif
Source: "{#MyAppSourceDir}\DirectWriteForwarder.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\GeoTimeZone.dll") != "8f0ca2647579c95c35bb9894a484b230f01077e34608955be4015cc2057457b2"
  #error "Accepted payload mismatch: GeoTimeZone.dll"
#endif
Source: "{#MyAppSourceDir}\GeoTimeZone.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\Microsoft.CSharp.dll") != "c43485d9e96cb000ae0e21d7db370955c4ccb77e9142be76477d6481aa2e72e1"
  #error "Accepted payload mismatch: Microsoft.CSharp.dll"
#endif
Source: "{#MyAppSourceDir}\Microsoft.CSharp.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\Microsoft.DiaSymReader.Native.amd64.dll") != "315a61c47c41b8d4c815f4274bb6584c1d24b4640c7eedc354ed5b03a9a1361e"
  #error "Accepted payload mismatch: Microsoft.DiaSymReader.Native.amd64.dll"
#endif
Source: "{#MyAppSourceDir}\Microsoft.DiaSymReader.Native.amd64.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\Microsoft.VisualBasic.Core.dll") != "28a5995189906c15f76e6738cb2c53fa8ff23f0e60d15a412bb06dc298d03aa2"
  #error "Accepted payload mismatch: Microsoft.VisualBasic.Core.dll"
#endif
Source: "{#MyAppSourceDir}\Microsoft.VisualBasic.Core.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\Microsoft.VisualBasic.Forms.dll") != "83a7a9eb1fecdc393d9981bd70420b074bc47063fbfa2ebe70facd116d1d856d"
  #error "Accepted payload mismatch: Microsoft.VisualBasic.Forms.dll"
#endif
Source: "{#MyAppSourceDir}\Microsoft.VisualBasic.Forms.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\Microsoft.VisualBasic.dll") != "c6e84fc1c8cea3c51f123fecb5ed930fe3d0c3303e39adc34ad237923e7997b5"
  #error "Accepted payload mismatch: Microsoft.VisualBasic.dll"
#endif
Source: "{#MyAppSourceDir}\Microsoft.VisualBasic.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\Microsoft.Win32.Primitives.dll") != "948107ac6afb355344751a1454bfb9ff8bee5a90909be71e5a4a9856d40e963f"
  #error "Accepted payload mismatch: Microsoft.Win32.Primitives.dll"
#endif
Source: "{#MyAppSourceDir}\Microsoft.Win32.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\Microsoft.Win32.Registry.AccessControl.dll") != "726da374e1b9e25d18ff40624ae925ca708eb66601b6d180819795ce569f135a"
  #error "Accepted payload mismatch: Microsoft.Win32.Registry.AccessControl.dll"
#endif
Source: "{#MyAppSourceDir}\Microsoft.Win32.Registry.AccessControl.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\Microsoft.Win32.Registry.dll") != "c3bc5123f63651c7b1faa11c313a03826895721b0934e1f2b3118ba7a70df5d5"
  #error "Accepted payload mismatch: Microsoft.Win32.Registry.dll"
#endif
Source: "{#MyAppSourceDir}\Microsoft.Win32.Registry.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\Microsoft.Win32.SystemEvents.dll") != "4f128a65a31239ec084cc39d02a1a0e8cfca92a19032d282c071c09a9a41e4c9"
  #error "Accepted payload mismatch: Microsoft.Win32.SystemEvents.dll"
#endif
Source: "{#MyAppSourceDir}\Microsoft.Win32.SystemEvents.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\Microsoft.Windows.SDK.NET.dll") != "c633ab241cd09846c2aa409f0bee2026962610404a7bf132c75cbd66b0e5a9f4"
  #error "Accepted payload mismatch: Microsoft.Windows.SDK.NET.dll"
#endif
Source: "{#MyAppSourceDir}\Microsoft.Windows.SDK.NET.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PenImc_cor3.dll") != "bc54196507efb35021b4d08e29aa518a9b501fcab9009345c521bd30de6d93e9"
  #error "Accepted payload mismatch: PenImc_cor3.dll"
#endif
Source: "{#MyAppSourceDir}\PenImc_cor3.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PrayerAutomation.UI.deps.json") != "b873e64643a35a7b95a59220864fe569e4fd3aa12383c6d6020446613efe974c"
  #error "Accepted payload mismatch: PrayerAutomation.UI.deps.json"
#endif
Source: "{#MyAppSourceDir}\PrayerAutomation.UI.deps.json"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PrayerAutomation.UI.dll") != "442333a50f9606ca280ca35daf89b1ca01c1493a3e2c83c6f1e4b2f2e64f654a"
  #error "Accepted payload mismatch: PrayerAutomation.UI.dll"
#endif
Source: "{#MyAppSourceDir}\PrayerAutomation.UI.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PrayerAutomation.UI.exe") != "3bb90110e0d85febf38d00b5bba7b9640f4c091a90e8b708d663237f3a410f49"
  #error "Accepted payload mismatch: PrayerAutomation.UI.exe"
#endif
Source: "{#MyAppSourceDir}\PrayerAutomation.UI.exe"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PrayerAutomation.UI.pdb") != "3cde3449e545c4012dee478df00373147a35208076e260116994d411ef99c30a"
  #error "Accepted payload mismatch: PrayerAutomation.UI.pdb"
#endif
Source: "{#MyAppSourceDir}\PrayerAutomation.UI.pdb"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PrayerAutomation.UI.runtimeconfig.json") != "9a69d58d5a2eb42e3ebc8042a8e7a714876f568dd62da97c3937e2bd49167a1f"
  #error "Accepted payload mismatch: PrayerAutomation.UI.runtimeconfig.json"
#endif
Source: "{#MyAppSourceDir}\PrayerAutomation.UI.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PresentationCore.dll") != "2264156caa886da031791f6459d5bc78cec0c52a117b2333393546c82c3642de"
  #error "Accepted payload mismatch: PresentationCore.dll"
#endif
Source: "{#MyAppSourceDir}\PresentationCore.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PresentationFramework-SystemCore.dll") != "7dcf47ab89d8db8d213ff0625edc76132519212e01f50a6133b8ffb5df890736"
  #error "Accepted payload mismatch: PresentationFramework-SystemCore.dll"
#endif
Source: "{#MyAppSourceDir}\PresentationFramework-SystemCore.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PresentationFramework-SystemData.dll") != "cb82feb3d5f3ddd01f84ae5708f3199a27c75e71e535f9ba939cd37f47844fb1"
  #error "Accepted payload mismatch: PresentationFramework-SystemData.dll"
#endif
Source: "{#MyAppSourceDir}\PresentationFramework-SystemData.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PresentationFramework-SystemDrawing.dll") != "d33cb8d248226287900602778e8195faca7359197dde90e7a2e8427346085294"
  #error "Accepted payload mismatch: PresentationFramework-SystemDrawing.dll"
#endif
Source: "{#MyAppSourceDir}\PresentationFramework-SystemDrawing.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PresentationFramework-SystemXml.dll") != "4e87b2096d8c8f3b4d7022d90ac90ae7f3070966853c6ecf59c467bbb542910d"
  #error "Accepted payload mismatch: PresentationFramework-SystemXml.dll"
#endif
Source: "{#MyAppSourceDir}\PresentationFramework-SystemXml.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PresentationFramework-SystemXmlLinq.dll") != "0f911860ad8c16b97cdb772c6e3a86d8067fd4397c81d79e683be9013ae1f4ee"
  #error "Accepted payload mismatch: PresentationFramework-SystemXmlLinq.dll"
#endif
Source: "{#MyAppSourceDir}\PresentationFramework-SystemXmlLinq.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PresentationFramework.Aero.dll") != "626fb45b67c368247fc58bef1bd09059625c9a4a9fac8e47e2112a1a1cf27e39"
  #error "Accepted payload mismatch: PresentationFramework.Aero.dll"
#endif
Source: "{#MyAppSourceDir}\PresentationFramework.Aero.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PresentationFramework.Aero2.dll") != "9a41c4d4ce8c149e76db2648092d23b9a97d4720af2c8ec6a01060f394e7f724"
  #error "Accepted payload mismatch: PresentationFramework.Aero2.dll"
#endif
Source: "{#MyAppSourceDir}\PresentationFramework.Aero2.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PresentationFramework.AeroLite.dll") != "338f1ed777eaa42ad5a09567471f4db62dac9353cbe8d0fb4fbb3b0dbdac107c"
  #error "Accepted payload mismatch: PresentationFramework.AeroLite.dll"
#endif
Source: "{#MyAppSourceDir}\PresentationFramework.AeroLite.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PresentationFramework.Classic.dll") != "0ff07d331c89b8043405cbf66350dcb7cbf725740ae4ea0736ce59202236f1c2"
  #error "Accepted payload mismatch: PresentationFramework.Classic.dll"
#endif
Source: "{#MyAppSourceDir}\PresentationFramework.Classic.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PresentationFramework.Fluent.dll") != "9add6c86b2c8c3bc172e2da895473b6ccc3516c261ab9cb506de184302753cca"
  #error "Accepted payload mismatch: PresentationFramework.Fluent.dll"
#endif
Source: "{#MyAppSourceDir}\PresentationFramework.Fluent.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PresentationFramework.Luna.dll") != "b7dc964038db525881cc39699e7706bae5f5474f0f04b5f7925fe3f2656c14f2"
  #error "Accepted payload mismatch: PresentationFramework.Luna.dll"
#endif
Source: "{#MyAppSourceDir}\PresentationFramework.Luna.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PresentationFramework.Royale.dll") != "c7c5760d45bb23abbcb5ca423f4ecff2b80706a210046a650cc69c625e647bd1"
  #error "Accepted payload mismatch: PresentationFramework.Royale.dll"
#endif
Source: "{#MyAppSourceDir}\PresentationFramework.Royale.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PresentationFramework.dll") != "f13135c4cdf7d1b40469c1c0b6396ea59782f267e8cdd7c37b480c229c9175ff"
  #error "Accepted payload mismatch: PresentationFramework.dll"
#endif
Source: "{#MyAppSourceDir}\PresentationFramework.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PresentationNative_cor3.dll") != "58c4e1174515ec7d07dc6b1a5c258aae88572576e1bc9f09ea92e555752b2554"
  #error "Accepted payload mismatch: PresentationNative_cor3.dll"
#endif
Source: "{#MyAppSourceDir}\PresentationNative_cor3.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\PresentationUI.dll") != "ad0e1a34dc5ee8f8d0cc95cb14e957a25df4e1132491a415770f82de442baadd"
  #error "Accepted payload mismatch: PresentationUI.dll"
#endif
Source: "{#MyAppSourceDir}\PresentationUI.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ReachFramework.dll") != "805710d3d27131fd380fc52e8bcf3cf47f472998bfaf38f8351f99130f8d5889"
  #error "Accepted payload mismatch: ReachFramework.dll"
#endif
Source: "{#MyAppSourceDir}\ReachFramework.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.AppContext.dll") != "d5a9be99ff048122f11f39b8c5b4394c3ab5753596c2eff7b7ee1abeaea9bc1b"
  #error "Accepted payload mismatch: System.AppContext.dll"
#endif
Source: "{#MyAppSourceDir}\System.AppContext.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Buffers.dll") != "e1ba509ef2ae77acafbe60907fd0fea4ee1c32e8afa0399ed1e23d4d93fc2aa3"
  #error "Accepted payload mismatch: System.Buffers.dll"
#endif
Source: "{#MyAppSourceDir}\System.Buffers.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.CodeDom.dll") != "8ef3a94544ea03907c8121fac3c0126c103223d171cb166a1619e002bc0f4a20"
  #error "Accepted payload mismatch: System.CodeDom.dll"
#endif
Source: "{#MyAppSourceDir}\System.CodeDom.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Collections.Concurrent.dll") != "0d1a713952d044f3f707a6d0b7b03bd64d410d4866d55cd0c172aea5c315387d"
  #error "Accepted payload mismatch: System.Collections.Concurrent.dll"
#endif
Source: "{#MyAppSourceDir}\System.Collections.Concurrent.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Collections.Immutable.dll") != "f1bf4d036baf22580ced0afcd1512d1582e9990edad9ad50e83a0a5870330a29"
  #error "Accepted payload mismatch: System.Collections.Immutable.dll"
#endif
Source: "{#MyAppSourceDir}\System.Collections.Immutable.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Collections.NonGeneric.dll") != "79e04d080b8c3eb92bd75d3358939b2e0adf2cad2639654ccc3b9722dde2ba14"
  #error "Accepted payload mismatch: System.Collections.NonGeneric.dll"
#endif
Source: "{#MyAppSourceDir}\System.Collections.NonGeneric.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Collections.Specialized.dll") != "5393620f914b78e5b40caa4d04bc5340fe5af5d23e288f1400d531cc9708fd75"
  #error "Accepted payload mismatch: System.Collections.Specialized.dll"
#endif
Source: "{#MyAppSourceDir}\System.Collections.Specialized.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Collections.dll") != "e8a4b43005fff65121433b298659322410a69bc02bcf44b223f1ed282a3a0335"
  #error "Accepted payload mismatch: System.Collections.dll"
#endif
Source: "{#MyAppSourceDir}\System.Collections.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.ComponentModel.Annotations.dll") != "a1017091b7811152d6781a5d40aa4c9e20df80e7cfdaa5b851e8f7943720b04c"
  #error "Accepted payload mismatch: System.ComponentModel.Annotations.dll"
#endif
Source: "{#MyAppSourceDir}\System.ComponentModel.Annotations.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.ComponentModel.DataAnnotations.dll") != "937368f90c008d6f264187d2c63d1f9ef293e93840e989b12c05e0f915c3fc1d"
  #error "Accepted payload mismatch: System.ComponentModel.DataAnnotations.dll"
#endif
Source: "{#MyAppSourceDir}\System.ComponentModel.DataAnnotations.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.ComponentModel.EventBasedAsync.dll") != "cb3d56e9f47d93ef5d71d9e4c5fbaf66d36d360d652772c82461154785329247"
  #error "Accepted payload mismatch: System.ComponentModel.EventBasedAsync.dll"
#endif
Source: "{#MyAppSourceDir}\System.ComponentModel.EventBasedAsync.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.ComponentModel.Primitives.dll") != "cd2ba2cd76736c694e681b16b637bd6438d8bbda85017097efe4f3434ceaea3e"
  #error "Accepted payload mismatch: System.ComponentModel.Primitives.dll"
#endif
Source: "{#MyAppSourceDir}\System.ComponentModel.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.ComponentModel.TypeConverter.dll") != "2028ce8bddd7438ae07851921dba921c4bec2f2554038658aca4f9a58a24a16d"
  #error "Accepted payload mismatch: System.ComponentModel.TypeConverter.dll"
#endif
Source: "{#MyAppSourceDir}\System.ComponentModel.TypeConverter.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.ComponentModel.dll") != "aa146801ec84f20bdd0ba2f9114d868b8dc5723b75c7052d73dc16482eef7d4a"
  #error "Accepted payload mismatch: System.ComponentModel.dll"
#endif
Source: "{#MyAppSourceDir}\System.ComponentModel.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Configuration.ConfigurationManager.dll") != "e99979abcd01edf1496b806250c3c83f9c2262dbf6d46e15db3f5a4a18fc4233"
  #error "Accepted payload mismatch: System.Configuration.ConfigurationManager.dll"
#endif
Source: "{#MyAppSourceDir}\System.Configuration.ConfigurationManager.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Configuration.dll") != "b4eb54b12477324bd48a7b31553ac5b407e5f3016170ca7a1a6987e568cabc2e"
  #error "Accepted payload mismatch: System.Configuration.dll"
#endif
Source: "{#MyAppSourceDir}\System.Configuration.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Console.dll") != "07313d866c7e63c8e197b49389bf6c8fc7f14a23d21c9f8851743a45c6d6b305"
  #error "Accepted payload mismatch: System.Console.dll"
#endif
Source: "{#MyAppSourceDir}\System.Console.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Core.dll") != "0028df97a8bc980612c9307e955bbb484d8b896942ccf11f3fb83be8f095ecd0"
  #error "Accepted payload mismatch: System.Core.dll"
#endif
Source: "{#MyAppSourceDir}\System.Core.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Data.Common.dll") != "edff10951101269bfd3ea05c722f724c3e4cb337801209c3b4cbb32b7844f298"
  #error "Accepted payload mismatch: System.Data.Common.dll"
#endif
Source: "{#MyAppSourceDir}\System.Data.Common.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Data.DataSetExtensions.dll") != "cdf705bbec712be45beab5fef030c35839f86249044ef97776b6233c7d742ad3"
  #error "Accepted payload mismatch: System.Data.DataSetExtensions.dll"
#endif
Source: "{#MyAppSourceDir}\System.Data.DataSetExtensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Data.dll") != "80124b86561d529f986b4f539b09b957555d82a1dc4cd42a184096bbaa9258f6"
  #error "Accepted payload mismatch: System.Data.dll"
#endif
Source: "{#MyAppSourceDir}\System.Data.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Design.dll") != "76f733db9788318eab3d8c9b8a369ee1ebfa15a61d09d46aaf857ed9e671b905"
  #error "Accepted payload mismatch: System.Design.dll"
#endif
Source: "{#MyAppSourceDir}\System.Design.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Diagnostics.Contracts.dll") != "00c65d5b567ca6eb3d9688e5e43aa526efec14c02b87cd1dcb52e70087fc28bf"
  #error "Accepted payload mismatch: System.Diagnostics.Contracts.dll"
#endif
Source: "{#MyAppSourceDir}\System.Diagnostics.Contracts.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Diagnostics.Debug.dll") != "313f40e64ad892cb72f7892969fcb833a751c9e8b5ddaa498c5597bc04ae7ea5"
  #error "Accepted payload mismatch: System.Diagnostics.Debug.dll"
#endif
Source: "{#MyAppSourceDir}\System.Diagnostics.Debug.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Diagnostics.DiagnosticSource.dll") != "659102f1bd66b38a5416ecc387f90881d9afb99ee3a32df66f21711424af16d1"
  #error "Accepted payload mismatch: System.Diagnostics.DiagnosticSource.dll"
#endif
Source: "{#MyAppSourceDir}\System.Diagnostics.DiagnosticSource.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Diagnostics.EventLog.Messages.dll") != "5902ae820e3e42de683999e65383b111110174337811a40573a3f7e39ebb42e8"
  #error "Accepted payload mismatch: System.Diagnostics.EventLog.Messages.dll"
#endif
Source: "{#MyAppSourceDir}\System.Diagnostics.EventLog.Messages.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Diagnostics.EventLog.dll") != "dc55370acee2b234981f2ecd39ce74ab15ee2907a6d69f30fc908fab848a9027"
  #error "Accepted payload mismatch: System.Diagnostics.EventLog.dll"
#endif
Source: "{#MyAppSourceDir}\System.Diagnostics.EventLog.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Diagnostics.FileVersionInfo.dll") != "8fdb8e65845e0eebc8d633811230a2a09209f003caefbc1512ef8190c1013e20"
  #error "Accepted payload mismatch: System.Diagnostics.FileVersionInfo.dll"
#endif
Source: "{#MyAppSourceDir}\System.Diagnostics.FileVersionInfo.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Diagnostics.PerformanceCounter.dll") != "04bbd7b6d217b808bd00b5c76eccda7b36e26429df762743373da4a6a7ea587c"
  #error "Accepted payload mismatch: System.Diagnostics.PerformanceCounter.dll"
#endif
Source: "{#MyAppSourceDir}\System.Diagnostics.PerformanceCounter.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Diagnostics.Process.dll") != "3e1e85eb09b1e7f23c5a26e40f725dfe95810e27b7d038fdfbf24664948de5d3"
  #error "Accepted payload mismatch: System.Diagnostics.Process.dll"
#endif
Source: "{#MyAppSourceDir}\System.Diagnostics.Process.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Diagnostics.StackTrace.dll") != "12fc06aa3e24fd2d68792d67fd0de4f33eea209851d626a3e6598ff71ef93e77"
  #error "Accepted payload mismatch: System.Diagnostics.StackTrace.dll"
#endif
Source: "{#MyAppSourceDir}\System.Diagnostics.StackTrace.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Diagnostics.TextWriterTraceListener.dll") != "1e86c620203b06637500c2f9f09e0259b2696721f5dbc13f9cf7e97b4e798eba"
  #error "Accepted payload mismatch: System.Diagnostics.TextWriterTraceListener.dll"
#endif
Source: "{#MyAppSourceDir}\System.Diagnostics.TextWriterTraceListener.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Diagnostics.Tools.dll") != "bb16f727574d9bd88ac301e30f652bbcf510466492057995edf9e647f0c4e984"
  #error "Accepted payload mismatch: System.Diagnostics.Tools.dll"
#endif
Source: "{#MyAppSourceDir}\System.Diagnostics.Tools.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Diagnostics.TraceSource.dll") != "1c6f990375348ad3230f025d1b7243ef1f945e19150d7a807262ecf5766945bd"
  #error "Accepted payload mismatch: System.Diagnostics.TraceSource.dll"
#endif
Source: "{#MyAppSourceDir}\System.Diagnostics.TraceSource.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Diagnostics.Tracing.dll") != "6da7623be2f8f1bae1876bf9b8f3ad459e26bdb90a24d29d4d7b46fd6df618aa"
  #error "Accepted payload mismatch: System.Diagnostics.Tracing.dll"
#endif
Source: "{#MyAppSourceDir}\System.Diagnostics.Tracing.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.DirectoryServices.dll") != "0cd4cbc6c1ef10d48b7d1f4e9278f3496f4df37700dec21d17f6b9dc840e3d3e"
  #error "Accepted payload mismatch: System.DirectoryServices.dll"
#endif
Source: "{#MyAppSourceDir}\System.DirectoryServices.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Drawing.Common.dll") != "565c21208548ec4d366b9b4c74d142f20acf5ea1c99b8aacdb5e07d6078908c7"
  #error "Accepted payload mismatch: System.Drawing.Common.dll"
#endif
Source: "{#MyAppSourceDir}\System.Drawing.Common.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Drawing.Design.dll") != "8bde63ba8f3051dfbf61a0632124ba2c0f61889bbf8d4c70c7104e28d8ddba62"
  #error "Accepted payload mismatch: System.Drawing.Design.dll"
#endif
Source: "{#MyAppSourceDir}\System.Drawing.Design.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Drawing.Primitives.dll") != "f3433ee65bd917f3a7053db0c82b279e497b20d21e7315c30ae0ea7db7988679"
  #error "Accepted payload mismatch: System.Drawing.Primitives.dll"
#endif
Source: "{#MyAppSourceDir}\System.Drawing.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Drawing.dll") != "6bde9fdfce1f902a2002833697aa8b4b87908eb8f766c69a8330e5fd7323acb3"
  #error "Accepted payload mismatch: System.Drawing.dll"
#endif
Source: "{#MyAppSourceDir}\System.Drawing.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Dynamic.Runtime.dll") != "11944c0ad34cb0c5d223f5f28867782f76b724304dd82a3062f367a1d0cdb6fd"
  #error "Accepted payload mismatch: System.Dynamic.Runtime.dll"
#endif
Source: "{#MyAppSourceDir}\System.Dynamic.Runtime.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Formats.Asn1.dll") != "b469339fc34280c568934a40f2aacaa57e6a39596895e6872f5820b85e0c8e2e"
  #error "Accepted payload mismatch: System.Formats.Asn1.dll"
#endif
Source: "{#MyAppSourceDir}\System.Formats.Asn1.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Formats.Nrbf.dll") != "eb366c5d1580e206ae6984c6709c76ddf60cb726a1be5e995f1d61198473044f"
  #error "Accepted payload mismatch: System.Formats.Nrbf.dll"
#endif
Source: "{#MyAppSourceDir}\System.Formats.Nrbf.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Formats.Tar.dll") != "fe6ee82b2812733676603a8b0a1519ffb77637d806de1fafcc7de0bc9369c136"
  #error "Accepted payload mismatch: System.Formats.Tar.dll"
#endif
Source: "{#MyAppSourceDir}\System.Formats.Tar.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Globalization.Calendars.dll") != "c627092c73fb4edf920688b3cabfa501f5afe3b6d7f60444206035dea61c5d5b"
  #error "Accepted payload mismatch: System.Globalization.Calendars.dll"
#endif
Source: "{#MyAppSourceDir}\System.Globalization.Calendars.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Globalization.Extensions.dll") != "6b3fb2d2e07ce6c9ea283f644ce80085ba8e968f64ed44b73bef90b8802340c0"
  #error "Accepted payload mismatch: System.Globalization.Extensions.dll"
#endif
Source: "{#MyAppSourceDir}\System.Globalization.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Globalization.dll") != "d3cb08ee8e2733ea4b285a0af9ea7e33c2cbdbd27ccf6cb3d102980958cca50b"
  #error "Accepted payload mismatch: System.Globalization.dll"
#endif
Source: "{#MyAppSourceDir}\System.Globalization.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.IO.Compression.Brotli.dll") != "ef253b34086d2819f0c4764d15c13a862cb9a320ae65e801a8b51de225f80d14"
  #error "Accepted payload mismatch: System.IO.Compression.Brotli.dll"
#endif
Source: "{#MyAppSourceDir}\System.IO.Compression.Brotli.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.IO.Compression.FileSystem.dll") != "300adfa8e1b24efc98fe6efe174022119b7a7c3bc6aeea4da080844c48f86845"
  #error "Accepted payload mismatch: System.IO.Compression.FileSystem.dll"
#endif
Source: "{#MyAppSourceDir}\System.IO.Compression.FileSystem.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.IO.Compression.Native.dll") != "3345a88bec37b8cd6af68060a3d714e521184864249d1d136ebb6486ed0a51e9"
  #error "Accepted payload mismatch: System.IO.Compression.Native.dll"
#endif
Source: "{#MyAppSourceDir}\System.IO.Compression.Native.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.IO.Compression.ZipFile.dll") != "bf8e10ebf3dbf781030af6003dd2552f17a61e84a289922ef29b28bdb04397d6"
  #error "Accepted payload mismatch: System.IO.Compression.ZipFile.dll"
#endif
Source: "{#MyAppSourceDir}\System.IO.Compression.ZipFile.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.IO.Compression.dll") != "fe9f9a5629d365bbc366b009218e79875d130bfc9e479b8bab681d0307d16ea4"
  #error "Accepted payload mismatch: System.IO.Compression.dll"
#endif
Source: "{#MyAppSourceDir}\System.IO.Compression.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.IO.FileSystem.AccessControl.dll") != "df43290dde6312e9e217c0a2eb223f2891f5629f5d2215aae413abc71bf0b4e8"
  #error "Accepted payload mismatch: System.IO.FileSystem.AccessControl.dll"
#endif
Source: "{#MyAppSourceDir}\System.IO.FileSystem.AccessControl.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.IO.FileSystem.DriveInfo.dll") != "f1e5141478ef400c2ca193675e44f46137cf02ddc29a931698df1cd2080931c3"
  #error "Accepted payload mismatch: System.IO.FileSystem.DriveInfo.dll"
#endif
Source: "{#MyAppSourceDir}\System.IO.FileSystem.DriveInfo.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.IO.FileSystem.Primitives.dll") != "2fa73a05d19a1f7fdac471f8969016a188eab7e604072c05f2e1bcf6e337512c"
  #error "Accepted payload mismatch: System.IO.FileSystem.Primitives.dll"
#endif
Source: "{#MyAppSourceDir}\System.IO.FileSystem.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.IO.FileSystem.Watcher.dll") != "ef6b54491ec9be9491ed4b4dfacde183edc0f5a17a35cfb5a2da09e623d25299"
  #error "Accepted payload mismatch: System.IO.FileSystem.Watcher.dll"
#endif
Source: "{#MyAppSourceDir}\System.IO.FileSystem.Watcher.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.IO.FileSystem.dll") != "4a22e9a2d690bab42aa8131850f86356b87f6a48348b3044c34f862b0fc34f57"
  #error "Accepted payload mismatch: System.IO.FileSystem.dll"
#endif
Source: "{#MyAppSourceDir}\System.IO.FileSystem.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.IO.IsolatedStorage.dll") != "34283a7e1620021dedc07bb0a61a26809dafab36698289a6b0774fec5475473e"
  #error "Accepted payload mismatch: System.IO.IsolatedStorage.dll"
#endif
Source: "{#MyAppSourceDir}\System.IO.IsolatedStorage.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.IO.MemoryMappedFiles.dll") != "c1cc1836b805f2544f49bbfa0fb3bc920143a1d6f8c437214cfdef655037e2d5"
  #error "Accepted payload mismatch: System.IO.MemoryMappedFiles.dll"
#endif
Source: "{#MyAppSourceDir}\System.IO.MemoryMappedFiles.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.IO.Packaging.dll") != "f3381b93404bfc515dd9461d7b9088852ff30ef86bc125ad3cab7fab3183c733"
  #error "Accepted payload mismatch: System.IO.Packaging.dll"
#endif
Source: "{#MyAppSourceDir}\System.IO.Packaging.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.IO.Pipelines.dll") != "337f64b658e93192560bfb6f1a0debf152f02263c39aed68636181df48921086"
  #error "Accepted payload mismatch: System.IO.Pipelines.dll"
#endif
Source: "{#MyAppSourceDir}\System.IO.Pipelines.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.IO.Pipes.AccessControl.dll") != "8ce9a1db25eb64e032ff190b3a3511c3c7069a97751651f8cc2ae7a888dd7a55"
  #error "Accepted payload mismatch: System.IO.Pipes.AccessControl.dll"
#endif
Source: "{#MyAppSourceDir}\System.IO.Pipes.AccessControl.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.IO.Pipes.dll") != "476d4e967080c0f41b4f71d3593f4c6cbd6f492e14a923f21e5d5c272c0eb6ce"
  #error "Accepted payload mismatch: System.IO.Pipes.dll"
#endif
Source: "{#MyAppSourceDir}\System.IO.Pipes.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.IO.UnmanagedMemoryStream.dll") != "8014405c32e07f9f4f8d024d374855aabf09a6490e6f5a7c2758a46b8d0c727e"
  #error "Accepted payload mismatch: System.IO.UnmanagedMemoryStream.dll"
#endif
Source: "{#MyAppSourceDir}\System.IO.UnmanagedMemoryStream.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.IO.dll") != "f5f0aaa66f552c7c0867c3ca00c3df1b4108a8539817b24a33f002da19e59ac1"
  #error "Accepted payload mismatch: System.IO.dll"
#endif
Source: "{#MyAppSourceDir}\System.IO.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Linq.AsyncEnumerable.dll") != "bad2460349585a7bd8a661c0f71c00005bc4e7261e00853fdd1e16cb5d5b1e0b"
  #error "Accepted payload mismatch: System.Linq.AsyncEnumerable.dll"
#endif
Source: "{#MyAppSourceDir}\System.Linq.AsyncEnumerable.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Linq.Expressions.dll") != "359b7d6512e2cce0520548a919bb9a2bfc1948b523c8c276b6ea4bfa1d550090"
  #error "Accepted payload mismatch: System.Linq.Expressions.dll"
#endif
Source: "{#MyAppSourceDir}\System.Linq.Expressions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Linq.Parallel.dll") != "9f3559634e8f812d6571b1e1cfe8199e7356b92c211d333f62d604395466c3b9"
  #error "Accepted payload mismatch: System.Linq.Parallel.dll"
#endif
Source: "{#MyAppSourceDir}\System.Linq.Parallel.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Linq.Queryable.dll") != "4471914fc73ffc60739d53e3c0ec0e24059f70c62ba66043089c94e3c63f3ad1"
  #error "Accepted payload mismatch: System.Linq.Queryable.dll"
#endif
Source: "{#MyAppSourceDir}\System.Linq.Queryable.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Linq.dll") != "2142f1e852b8edbf8dceddb575177295dde02c8a368e716f0ca0c902d42bfe03"
  #error "Accepted payload mismatch: System.Linq.dll"
#endif
Source: "{#MyAppSourceDir}\System.Linq.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Memory.dll") != "8a9eb705fe5a69e936e49f86b507cbcb8dd1d2889f925485b5cd412deaa8dc50"
  #error "Accepted payload mismatch: System.Memory.dll"
#endif
Source: "{#MyAppSourceDir}\System.Memory.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.Http.Json.dll") != "aecb94668a6a5cd744978e4551bcbcbb88b93e9a4a028015cea7d499e03987f9"
  #error "Accepted payload mismatch: System.Net.Http.Json.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.Http.Json.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.Http.dll") != "9b6b1e1f5e8b6ac535241cd514e1b1c1e15b2fac9df30fc0f1369c1b8a9d1433"
  #error "Accepted payload mismatch: System.Net.Http.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.Http.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.HttpListener.dll") != "5d4770ade352b4d33018025e8d3195d62847e6bfba7e7fdb38e504a7015b1858"
  #error "Accepted payload mismatch: System.Net.HttpListener.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.HttpListener.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.Mail.dll") != "fd6280fdb57aeaec0d753509ecf88b5a8a4a6ca79d3e617f83ae345ccc382a49"
  #error "Accepted payload mismatch: System.Net.Mail.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.Mail.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.NameResolution.dll") != "7c25f1ccb5adca2e5303d12e9ea7c1d0fb1c89a03eaa54dd23355533274384b5"
  #error "Accepted payload mismatch: System.Net.NameResolution.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.NameResolution.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.NetworkInformation.dll") != "9cf015327c5587233d3663ed26ee6d23096ea26b388199afbe397fa18d4a4f52"
  #error "Accepted payload mismatch: System.Net.NetworkInformation.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.NetworkInformation.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.Ping.dll") != "2aaa666a5ead9a907d71ccd6751841f7e9787b747733124ac193595ead5b5c45"
  #error "Accepted payload mismatch: System.Net.Ping.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.Ping.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.Primitives.dll") != "e18855a267b2c1ee06b45d35f5497fe12762850eec5b15595aeb2d2f0520e107"
  #error "Accepted payload mismatch: System.Net.Primitives.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.Quic.dll") != "7443294ca7ccea1b8732989e6f94e65cd1e3049fbc8ebdbf5f98e515b7e74ea2"
  #error "Accepted payload mismatch: System.Net.Quic.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.Quic.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.Requests.dll") != "ac796cba2e96c0720f151c204a6a296f7f7b443b8ffc288d9d6afe4f05105c3a"
  #error "Accepted payload mismatch: System.Net.Requests.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.Requests.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.Security.dll") != "34349b8333c371434f9bf516d6d41c50c96556ab0fe65605d452fbe5157e1a39"
  #error "Accepted payload mismatch: System.Net.Security.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.Security.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.ServerSentEvents.dll") != "6d30c7a67699f727bb3a64f28d4cf3d322673a6ea2220c3e341e40a1145838f2"
  #error "Accepted payload mismatch: System.Net.ServerSentEvents.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.ServerSentEvents.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.ServicePoint.dll") != "b967a71748365c892ebb74add5d14c4cb2db2f8a20e28ceba2c3794a2bfe9dab"
  #error "Accepted payload mismatch: System.Net.ServicePoint.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.ServicePoint.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.Sockets.dll") != "59e0541248eec218e55cfb8512d659bbfee679475e9e90cf3e481400e85e52e2"
  #error "Accepted payload mismatch: System.Net.Sockets.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.Sockets.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.WebClient.dll") != "1528a8add57ac0f3694a495e251bcc0ae5002d344df874c3611e8cdd940af185"
  #error "Accepted payload mismatch: System.Net.WebClient.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.WebClient.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.WebHeaderCollection.dll") != "b2f04b6bbdd761ae7f92bff371325cc3a7bf92bb07eb04025ea6f6a24bb29532"
  #error "Accepted payload mismatch: System.Net.WebHeaderCollection.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.WebHeaderCollection.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.WebProxy.dll") != "d695d9832cc46434e88ada89551c59d481e19f95f8dca8ec87f7c34223fb84cf"
  #error "Accepted payload mismatch: System.Net.WebProxy.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.WebProxy.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.WebSockets.Client.dll") != "c40513e37e56feb92e80890a945192d48234946031d751072d078bb4493a1743"
  #error "Accepted payload mismatch: System.Net.WebSockets.Client.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.WebSockets.Client.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.WebSockets.dll") != "e263f60be5a675465d98380924f6d08fbc9e008ec9ffb0f231df1ecbf93c618b"
  #error "Accepted payload mismatch: System.Net.WebSockets.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.WebSockets.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Net.dll") != "913d4c5541e2e6a06068d248fa287dc0dc1a680172346dcd11c620c60463959a"
  #error "Accepted payload mismatch: System.Net.dll"
#endif
Source: "{#MyAppSourceDir}\System.Net.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Numerics.Vectors.dll") != "7dfa028fb9fa83782a0f07123639179345bb68d14a9303d33b200270c66cee13"
  #error "Accepted payload mismatch: System.Numerics.Vectors.dll"
#endif
Source: "{#MyAppSourceDir}\System.Numerics.Vectors.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Numerics.dll") != "5e8f8df8f1a9e4a8d6c6f0294b74621d83feabb3e7b6682433ef33a4db6b12f0"
  #error "Accepted payload mismatch: System.Numerics.dll"
#endif
Source: "{#MyAppSourceDir}\System.Numerics.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.ObjectModel.dll") != "967398fe2576406a60b4b0e9a1a6c81f9440799cb3923b2787a74fe3a02044da"
  #error "Accepted payload mismatch: System.ObjectModel.dll"
#endif
Source: "{#MyAppSourceDir}\System.ObjectModel.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Printing.dll") != "78c7888e9fc2959769f54bd894fddf4831d0df2a07c4621584265fd7367e3e8a"
  #error "Accepted payload mismatch: System.Printing.dll"
#endif
Source: "{#MyAppSourceDir}\System.Printing.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Private.CoreLib.dll") != "dc1945de746f94987ec705a1f27d512abf72a41a1da37e414d1951e4e823037a"
  #error "Accepted payload mismatch: System.Private.CoreLib.dll"
#endif
Source: "{#MyAppSourceDir}\System.Private.CoreLib.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Private.DataContractSerialization.dll") != "33be9654ddcd570100ac6882500684ed95d3715415071ea3a1b35e8da7366896"
  #error "Accepted payload mismatch: System.Private.DataContractSerialization.dll"
#endif
Source: "{#MyAppSourceDir}\System.Private.DataContractSerialization.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Private.Uri.dll") != "5eba7dfb1d8a639c918ea182a15bd35256a11f9853003d39e7ad757ae08d4fca"
  #error "Accepted payload mismatch: System.Private.Uri.dll"
#endif
Source: "{#MyAppSourceDir}\System.Private.Uri.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Private.Windows.Core.dll") != "901887bf2ef710a98611271877e9e81c6f393906e3360d1386e3f512c538d585"
  #error "Accepted payload mismatch: System.Private.Windows.Core.dll"
#endif
Source: "{#MyAppSourceDir}\System.Private.Windows.Core.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Private.Windows.GdiPlus.dll") != "5ff73fb9161809b831f7bdcf30526e3f88e248ad16d06e48c50c76b507e8b295"
  #error "Accepted payload mismatch: System.Private.Windows.GdiPlus.dll"
#endif
Source: "{#MyAppSourceDir}\System.Private.Windows.GdiPlus.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Private.Xml.Linq.dll") != "ed6e043dca4b801ae45167cd0e392485c6742e07a4598eb5eb0df8e72ee24cc6"
  #error "Accepted payload mismatch: System.Private.Xml.Linq.dll"
#endif
Source: "{#MyAppSourceDir}\System.Private.Xml.Linq.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Private.Xml.dll") != "2a77ab83449ec192b49dcff0abec753de95f8dc693fcfa07ed2f7024c3c169e6"
  #error "Accepted payload mismatch: System.Private.Xml.dll"
#endif
Source: "{#MyAppSourceDir}\System.Private.Xml.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Reflection.DispatchProxy.dll") != "ac12d3522a371eb0a86204fdb4471c65c4a850d88551aff12ef571d4c13be06f"
  #error "Accepted payload mismatch: System.Reflection.DispatchProxy.dll"
#endif
Source: "{#MyAppSourceDir}\System.Reflection.DispatchProxy.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Reflection.Emit.ILGeneration.dll") != "d54aa2d2647145b759feb6ed8531d1bddd5ed159ee1049d127d4204da38c708d"
  #error "Accepted payload mismatch: System.Reflection.Emit.ILGeneration.dll"
#endif
Source: "{#MyAppSourceDir}\System.Reflection.Emit.ILGeneration.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Reflection.Emit.Lightweight.dll") != "4e9733b036d0b4464b81a1ff049e85856fd9248dc170a78841733837aa0ff2d6"
  #error "Accepted payload mismatch: System.Reflection.Emit.Lightweight.dll"
#endif
Source: "{#MyAppSourceDir}\System.Reflection.Emit.Lightweight.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Reflection.Emit.dll") != "e9dda395b1f6d686c8304f0e962b46ef25b3f52b5c384a19e0c97c117f9f92e0"
  #error "Accepted payload mismatch: System.Reflection.Emit.dll"
#endif
Source: "{#MyAppSourceDir}\System.Reflection.Emit.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Reflection.Extensions.dll") != "b36f7ef141d5b1e43e5961bd6f0caffb243a48a90c7d5e7567f5c608b53ed668"
  #error "Accepted payload mismatch: System.Reflection.Extensions.dll"
#endif
Source: "{#MyAppSourceDir}\System.Reflection.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Reflection.Metadata.dll") != "03bcaa6abdec4bb0194689e41fb9c8bd07e4eb098421bba60d19d2134691a265"
  #error "Accepted payload mismatch: System.Reflection.Metadata.dll"
#endif
Source: "{#MyAppSourceDir}\System.Reflection.Metadata.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Reflection.Primitives.dll") != "996481e88cf68e50c182bd89da1e490a19e8587a87c2e7f4f92a110b40416ef9"
  #error "Accepted payload mismatch: System.Reflection.Primitives.dll"
#endif
Source: "{#MyAppSourceDir}\System.Reflection.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Reflection.TypeExtensions.dll") != "0918b2aef8c6d740d3f92772d2ce7584a1202963e8301913543c0e1e0b06fdf1"
  #error "Accepted payload mismatch: System.Reflection.TypeExtensions.dll"
#endif
Source: "{#MyAppSourceDir}\System.Reflection.TypeExtensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Reflection.dll") != "87d796c61766483eda298e17a12ea591c65711f347853642292c6628dd55e98b"
  #error "Accepted payload mismatch: System.Reflection.dll"
#endif
Source: "{#MyAppSourceDir}\System.Reflection.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Resources.Extensions.dll") != "72011b2dc60c3d71dd6ee34097650ee67934637ae76a649c97c0bb66bd899d0b"
  #error "Accepted payload mismatch: System.Resources.Extensions.dll"
#endif
Source: "{#MyAppSourceDir}\System.Resources.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Resources.Reader.dll") != "5670470b6c8b2036278a1b59172e04ba32270aaee92e047bd8d171e7beba3ce4"
  #error "Accepted payload mismatch: System.Resources.Reader.dll"
#endif
Source: "{#MyAppSourceDir}\System.Resources.Reader.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Resources.ResourceManager.dll") != "ee01c6f19d731ed9ec07617603495debbd6f9cc4457b0ec1ba8a3dbac794e4ee"
  #error "Accepted payload mismatch: System.Resources.ResourceManager.dll"
#endif
Source: "{#MyAppSourceDir}\System.Resources.ResourceManager.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Resources.Writer.dll") != "799f6d9e0d887f00dc7c45f6d938581ef40af17a0dc92195669fa7041ad8ae7e"
  #error "Accepted payload mismatch: System.Resources.Writer.dll"
#endif
Source: "{#MyAppSourceDir}\System.Resources.Writer.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Runtime.CompilerServices.Unsafe.dll") != "a32f2372e5250a57bba485d0dcba87dd7a0864d3c3ce7f8172e7e1bd6ce2a956"
  #error "Accepted payload mismatch: System.Runtime.CompilerServices.Unsafe.dll"
#endif
Source: "{#MyAppSourceDir}\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Runtime.CompilerServices.VisualC.dll") != "7d5ebae58c6e533c3aaaf18a8f428a732be556f6c9fbda53ef6202f7d24c2bb3"
  #error "Accepted payload mismatch: System.Runtime.CompilerServices.VisualC.dll"
#endif
Source: "{#MyAppSourceDir}\System.Runtime.CompilerServices.VisualC.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Runtime.Extensions.dll") != "f90005f24411a5936a3fbc9560894795524e44cb6066572448a7fb3dd28beb23"
  #error "Accepted payload mismatch: System.Runtime.Extensions.dll"
#endif
Source: "{#MyAppSourceDir}\System.Runtime.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Runtime.Handles.dll") != "b80a346b6949c556e2f4b587540dd076d5e3ae544da49faaa7c124eb20523d75"
  #error "Accepted payload mismatch: System.Runtime.Handles.dll"
#endif
Source: "{#MyAppSourceDir}\System.Runtime.Handles.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Runtime.InteropServices.JavaScript.dll") != "0f73165acbe39246bc83ee70706075a94274e5b1563c4ef0277c989d2c6771e7"
  #error "Accepted payload mismatch: System.Runtime.InteropServices.JavaScript.dll"
#endif
Source: "{#MyAppSourceDir}\System.Runtime.InteropServices.JavaScript.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Runtime.InteropServices.RuntimeInformation.dll") != "4145d1653d0357417dcc0eca2e38fffffbc886c035c7201bd721f77bac9d037b"
  #error "Accepted payload mismatch: System.Runtime.InteropServices.RuntimeInformation.dll"
#endif
Source: "{#MyAppSourceDir}\System.Runtime.InteropServices.RuntimeInformation.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Runtime.InteropServices.dll") != "96370a9241de81b737f80a582a05baaf20a5d4746f3cbe5cbdb387ffc5094d75"
  #error "Accepted payload mismatch: System.Runtime.InteropServices.dll"
#endif
Source: "{#MyAppSourceDir}\System.Runtime.InteropServices.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Runtime.Intrinsics.dll") != "3fce5f1e99be36817443704d29548a57b7e51364cf681e89fe477fdd1120a720"
  #error "Accepted payload mismatch: System.Runtime.Intrinsics.dll"
#endif
Source: "{#MyAppSourceDir}\System.Runtime.Intrinsics.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Runtime.Loader.dll") != "66a4e3053c33488bb7b009716b9a729d625b095a5caca90f0acc892c57bbb52f"
  #error "Accepted payload mismatch: System.Runtime.Loader.dll"
#endif
Source: "{#MyAppSourceDir}\System.Runtime.Loader.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Runtime.Numerics.dll") != "d97b61299d9816c8c133eefc0acbddf516d8becc4750aedf9c6b873e38dddc02"
  #error "Accepted payload mismatch: System.Runtime.Numerics.dll"
#endif
Source: "{#MyAppSourceDir}\System.Runtime.Numerics.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Runtime.Serialization.Formatters.dll") != "06859184aefb580bfa4d1367259f8374c7cd845454b3af85a6994c0377a7f294"
  #error "Accepted payload mismatch: System.Runtime.Serialization.Formatters.dll"
#endif
Source: "{#MyAppSourceDir}\System.Runtime.Serialization.Formatters.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Runtime.Serialization.Json.dll") != "2fffd8fc83464cbbcd8f10a4f417c07d35eb933c9c267617081dada767e29ee4"
  #error "Accepted payload mismatch: System.Runtime.Serialization.Json.dll"
#endif
Source: "{#MyAppSourceDir}\System.Runtime.Serialization.Json.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Runtime.Serialization.Primitives.dll") != "a6b432447a3c9516bfa595889923e5c82c65676eac996aeda88b57a1358d7e74"
  #error "Accepted payload mismatch: System.Runtime.Serialization.Primitives.dll"
#endif
Source: "{#MyAppSourceDir}\System.Runtime.Serialization.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Runtime.Serialization.Xml.dll") != "48700492e7b3bf8b9bb6240c42a31ee70b4c25d364302f7754c1cc8b44f24b40"
  #error "Accepted payload mismatch: System.Runtime.Serialization.Xml.dll"
#endif
Source: "{#MyAppSourceDir}\System.Runtime.Serialization.Xml.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Runtime.Serialization.dll") != "dabc0ea3a5b89da914d97d9b1627c71c4f0922e8f0690e0f36c29bed3ff8bd64"
  #error "Accepted payload mismatch: System.Runtime.Serialization.dll"
#endif
Source: "{#MyAppSourceDir}\System.Runtime.Serialization.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Runtime.dll") != "87f6a9a4133e1fb95b0430071ae73d232eb9cb24beb04aca4d7ae3a3d074f9ad"
  #error "Accepted payload mismatch: System.Runtime.dll"
#endif
Source: "{#MyAppSourceDir}\System.Runtime.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Security.AccessControl.dll") != "a10c0fa9fe9cd7eff8e305227de296bd449975aeb298cd28caed63ef475ac60b"
  #error "Accepted payload mismatch: System.Security.AccessControl.dll"
#endif
Source: "{#MyAppSourceDir}\System.Security.AccessControl.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Security.Claims.dll") != "84265624a4c6a31ad6bfc3da3c4f57ec3592d53d1906cf47bb1b8e0d62a223c7"
  #error "Accepted payload mismatch: System.Security.Claims.dll"
#endif
Source: "{#MyAppSourceDir}\System.Security.Claims.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Security.Cryptography.Algorithms.dll") != "459db23d26c80169c9993e641fddcbb8e82bbbc28c55cbff3d6783ee2c988f3f"
  #error "Accepted payload mismatch: System.Security.Cryptography.Algorithms.dll"
#endif
Source: "{#MyAppSourceDir}\System.Security.Cryptography.Algorithms.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Security.Cryptography.Cng.dll") != "c927c883ff03b042a93ee6ae3816f38b4fca7efa72ba02d8c37781d41173537d"
  #error "Accepted payload mismatch: System.Security.Cryptography.Cng.dll"
#endif
Source: "{#MyAppSourceDir}\System.Security.Cryptography.Cng.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Security.Cryptography.Csp.dll") != "ac489e29fd556e28e5c39c9694af51d2b096ec2b526b481cdfea1d2869c581b8"
  #error "Accepted payload mismatch: System.Security.Cryptography.Csp.dll"
#endif
Source: "{#MyAppSourceDir}\System.Security.Cryptography.Csp.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Security.Cryptography.Encoding.dll") != "b755bcccb17f378d940546ad7089205e29e95d4ef7bab140ec797155cb889582"
  #error "Accepted payload mismatch: System.Security.Cryptography.Encoding.dll"
#endif
Source: "{#MyAppSourceDir}\System.Security.Cryptography.Encoding.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Security.Cryptography.OpenSsl.dll") != "939c6a88964420c8a3a6527f8e5d10a8f769561b8eaf14d5b14e522500a64b88"
  #error "Accepted payload mismatch: System.Security.Cryptography.OpenSsl.dll"
#endif
Source: "{#MyAppSourceDir}\System.Security.Cryptography.OpenSsl.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Security.Cryptography.Pkcs.dll") != "4522b7caaea8872d9c4a704f3d4399dd483932c1999e9baee57b2ab0205e2282"
  #error "Accepted payload mismatch: System.Security.Cryptography.Pkcs.dll"
#endif
Source: "{#MyAppSourceDir}\System.Security.Cryptography.Pkcs.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Security.Cryptography.Primitives.dll") != "97cd2c5be283d98848344f5d08f30e9a073c4a1f532057cfa7e0448ce2d79865"
  #error "Accepted payload mismatch: System.Security.Cryptography.Primitives.dll"
#endif
Source: "{#MyAppSourceDir}\System.Security.Cryptography.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Security.Cryptography.ProtectedData.dll") != "6fb702bba95c6146ca4583a3613792b452a21630ef9976937fa70b072c43f286"
  #error "Accepted payload mismatch: System.Security.Cryptography.ProtectedData.dll"
#endif
Source: "{#MyAppSourceDir}\System.Security.Cryptography.ProtectedData.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Security.Cryptography.X509Certificates.dll") != "f9d803364bf3bd1ba764d17c6e1294224366bf74e9008c5369d2163053951be0"
  #error "Accepted payload mismatch: System.Security.Cryptography.X509Certificates.dll"
#endif
Source: "{#MyAppSourceDir}\System.Security.Cryptography.X509Certificates.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Security.Cryptography.Xml.dll") != "ad57ce0ab1b29dd4a8a20391424299778304cffe8d1656330773106d78840cdb"
  #error "Accepted payload mismatch: System.Security.Cryptography.Xml.dll"
#endif
Source: "{#MyAppSourceDir}\System.Security.Cryptography.Xml.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Security.Cryptography.dll") != "56e43c3e486d790c64eeac87a5537dd1510b97830c87f062bc14eabbe82f95be"
  #error "Accepted payload mismatch: System.Security.Cryptography.dll"
#endif
Source: "{#MyAppSourceDir}\System.Security.Cryptography.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Security.Permissions.dll") != "7c53588f217a53d84f06f79ff3b2833a1f8e494461929e13babd38e5eb6de7a5"
  #error "Accepted payload mismatch: System.Security.Permissions.dll"
#endif
Source: "{#MyAppSourceDir}\System.Security.Permissions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Security.Principal.Windows.dll") != "88faa49ce295e777767af6b44d9cf356d9a884e371ae48647e429079d57c5dc2"
  #error "Accepted payload mismatch: System.Security.Principal.Windows.dll"
#endif
Source: "{#MyAppSourceDir}\System.Security.Principal.Windows.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Security.Principal.dll") != "39e6d7720d67af2f03707901a6d0e20496c51e83f848db3e8fd92b5fa72bb091"
  #error "Accepted payload mismatch: System.Security.Principal.dll"
#endif
Source: "{#MyAppSourceDir}\System.Security.Principal.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Security.SecureString.dll") != "9622d7e1433f77b0f237c183f06a145f35df484eece4a91ae9d723f40bea42be"
  #error "Accepted payload mismatch: System.Security.SecureString.dll"
#endif
Source: "{#MyAppSourceDir}\System.Security.SecureString.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Security.dll") != "b2c7b9a1b17bcc644229b0de7be98feda873306f60d895b4ed4fb3ee22b42548"
  #error "Accepted payload mismatch: System.Security.dll"
#endif
Source: "{#MyAppSourceDir}\System.Security.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.ServiceModel.Web.dll") != "76b5b4eae5fc25590311652c7720e08c4008b59c785662609d092b7fb2eb496b"
  #error "Accepted payload mismatch: System.ServiceModel.Web.dll"
#endif
Source: "{#MyAppSourceDir}\System.ServiceModel.Web.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.ServiceProcess.dll") != "26a76069c2a8b77ef7aece50a8fa1d74654167d0a91bcdf0886794f28b770c8a"
  #error "Accepted payload mismatch: System.ServiceProcess.dll"
#endif
Source: "{#MyAppSourceDir}\System.ServiceProcess.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Text.Encoding.CodePages.dll") != "14061806bcc21b8656c27437bd2c6ec4d82b4f08ff4b593a1bd5f36d94289d48"
  #error "Accepted payload mismatch: System.Text.Encoding.CodePages.dll"
#endif
Source: "{#MyAppSourceDir}\System.Text.Encoding.CodePages.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Text.Encoding.Extensions.dll") != "527cec08760564d37da41c123630d2107be80e0d8d289207cc9a184573fb2509"
  #error "Accepted payload mismatch: System.Text.Encoding.Extensions.dll"
#endif
Source: "{#MyAppSourceDir}\System.Text.Encoding.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Text.Encoding.dll") != "019b7aae88661c619998f52bd28fa652068cf4710e4066576f4318ca57bd3de8"
  #error "Accepted payload mismatch: System.Text.Encoding.dll"
#endif
Source: "{#MyAppSourceDir}\System.Text.Encoding.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Text.Encodings.Web.dll") != "a4807681c163b2e62dec0f0ff1b288ae2392fec51a995bb479b16c29760ed023"
  #error "Accepted payload mismatch: System.Text.Encodings.Web.dll"
#endif
Source: "{#MyAppSourceDir}\System.Text.Encodings.Web.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Text.Json.dll") != "1d2e6b506ace3fd86cc9a914040fe309788d8210beec7bf8cbfb62cf67d5772d"
  #error "Accepted payload mismatch: System.Text.Json.dll"
#endif
Source: "{#MyAppSourceDir}\System.Text.Json.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Text.RegularExpressions.dll") != "3ee5c77dbc64d3eb876ab6a86cb2c9c47395bba4c1ba5f9e92313ee0ba0d4905"
  #error "Accepted payload mismatch: System.Text.RegularExpressions.dll"
#endif
Source: "{#MyAppSourceDir}\System.Text.RegularExpressions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Threading.AccessControl.dll") != "04ca8ef1432bea00604f4b065196128819da96af04df6a74afa4ec807b414949"
  #error "Accepted payload mismatch: System.Threading.AccessControl.dll"
#endif
Source: "{#MyAppSourceDir}\System.Threading.AccessControl.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Threading.Channels.dll") != "02f646280c0e83b5a480e62e0894b5a220691c9aac0489c5bc1d767d4e7df3dd"
  #error "Accepted payload mismatch: System.Threading.Channels.dll"
#endif
Source: "{#MyAppSourceDir}\System.Threading.Channels.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Threading.Overlapped.dll") != "4d0348c305a1c6277982ab3f0ac1c47673fb7414d29c35b3bfef574e3554f02f"
  #error "Accepted payload mismatch: System.Threading.Overlapped.dll"
#endif
Source: "{#MyAppSourceDir}\System.Threading.Overlapped.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Threading.Tasks.Dataflow.dll") != "659f14b2576d2d34bf8c24e5d8f7238d190fe91d11d888a89b90b7bcd673b25b"
  #error "Accepted payload mismatch: System.Threading.Tasks.Dataflow.dll"
#endif
Source: "{#MyAppSourceDir}\System.Threading.Tasks.Dataflow.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Threading.Tasks.Extensions.dll") != "569ef32c126b389651092497173bba1b86dce1365c81f8d48755cfc1ede6690a"
  #error "Accepted payload mismatch: System.Threading.Tasks.Extensions.dll"
#endif
Source: "{#MyAppSourceDir}\System.Threading.Tasks.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Threading.Tasks.Parallel.dll") != "d2d14c51694a018189e67ab4d1116982ba78e8d3418377d574713c2687194831"
  #error "Accepted payload mismatch: System.Threading.Tasks.Parallel.dll"
#endif
Source: "{#MyAppSourceDir}\System.Threading.Tasks.Parallel.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Threading.Tasks.dll") != "3d58c971ab9bb4e3b5de43f4706ad9f63b4aea04d1ca7a3762ce93168875a60d"
  #error "Accepted payload mismatch: System.Threading.Tasks.dll"
#endif
Source: "{#MyAppSourceDir}\System.Threading.Tasks.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Threading.Thread.dll") != "51b8ab47c92cae897b99f70a5ec9e3528708b0762ac7d552ccd3970fe4b072ed"
  #error "Accepted payload mismatch: System.Threading.Thread.dll"
#endif
Source: "{#MyAppSourceDir}\System.Threading.Thread.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Threading.ThreadPool.dll") != "0f73cc058dfbd0704102ed999e79026312246dfda630fe2968f542a87ff7c116"
  #error "Accepted payload mismatch: System.Threading.ThreadPool.dll"
#endif
Source: "{#MyAppSourceDir}\System.Threading.ThreadPool.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Threading.Timer.dll") != "b19b79ea8a9198e91117b918d80fc806e6c63deb5859a0d5793e5b14f985ee5e"
  #error "Accepted payload mismatch: System.Threading.Timer.dll"
#endif
Source: "{#MyAppSourceDir}\System.Threading.Timer.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Threading.dll") != "89a81656b39f93950f4ab2882b7ea350892edd7b6074ad67179ecc73ca01a190"
  #error "Accepted payload mismatch: System.Threading.dll"
#endif
Source: "{#MyAppSourceDir}\System.Threading.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Transactions.Local.dll") != "9b1fe23f9dcab3866f9e19bf1793eeee94b36219d848d302ab853090176d427c"
  #error "Accepted payload mismatch: System.Transactions.Local.dll"
#endif
Source: "{#MyAppSourceDir}\System.Transactions.Local.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Transactions.dll") != "4f03efe1d90d139f10e337cd0173a1a223016886b30e977c916bfffe96dac64a"
  #error "Accepted payload mismatch: System.Transactions.dll"
#endif
Source: "{#MyAppSourceDir}\System.Transactions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.ValueTuple.dll") != "e60bcf3660409954a8d0d8b11570407fd973531f8ed2d9f65ff87ed216eb3dd0"
  #error "Accepted payload mismatch: System.ValueTuple.dll"
#endif
Source: "{#MyAppSourceDir}\System.ValueTuple.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Web.HttpUtility.dll") != "09394846262f2d4dfca9def8826d916bd0fe298306ec0962bf87a85987be5cb3"
  #error "Accepted payload mismatch: System.Web.HttpUtility.dll"
#endif
Source: "{#MyAppSourceDir}\System.Web.HttpUtility.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Web.dll") != "eca8a27aa45b6a468b6894e0c479cd4c7fa07948fca4f809f98b36dcbabbfb43"
  #error "Accepted payload mismatch: System.Web.dll"
#endif
Source: "{#MyAppSourceDir}\System.Web.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Windows.Controls.Ribbon.dll") != "a01f9e4cb121730c5add2938d92826768905ecad12477bf1c709f40360062947"
  #error "Accepted payload mismatch: System.Windows.Controls.Ribbon.dll"
#endif
Source: "{#MyAppSourceDir}\System.Windows.Controls.Ribbon.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Windows.Extensions.dll") != "62ae76b38d7356690bbdb7946eb43161c7f8d33881b0b447d98e5cb80bce1a06"
  #error "Accepted payload mismatch: System.Windows.Extensions.dll"
#endif
Source: "{#MyAppSourceDir}\System.Windows.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Windows.Forms.Design.Editors.dll") != "15c7f374df2e83bb1f4a5b772e3d962d52ded9817c50bc5ceb71c8dd2c16c063"
  #error "Accepted payload mismatch: System.Windows.Forms.Design.Editors.dll"
#endif
Source: "{#MyAppSourceDir}\System.Windows.Forms.Design.Editors.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Windows.Forms.Design.dll") != "4f86f6b581ba42c4a900240b5764dcb1ef12d8594dd8a96d3d2fc79552b52533"
  #error "Accepted payload mismatch: System.Windows.Forms.Design.dll"
#endif
Source: "{#MyAppSourceDir}\System.Windows.Forms.Design.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Windows.Forms.Primitives.dll") != "3fb4a95941e63ca12517a46a9c48448158028246173b1730b774ca9044e2bada"
  #error "Accepted payload mismatch: System.Windows.Forms.Primitives.dll"
#endif
Source: "{#MyAppSourceDir}\System.Windows.Forms.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Windows.Forms.dll") != "de94e4ca05769cc090fd7be09c5a3132327e26a91b34dbb02467433dec4a2184"
  #error "Accepted payload mismatch: System.Windows.Forms.dll"
#endif
Source: "{#MyAppSourceDir}\System.Windows.Forms.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Windows.Input.Manipulations.dll") != "7b83fb73a998af9297f45a4f79577af2ae7a9b1cbcd7425c05164e8e10866190"
  #error "Accepted payload mismatch: System.Windows.Input.Manipulations.dll"
#endif
Source: "{#MyAppSourceDir}\System.Windows.Input.Manipulations.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Windows.Presentation.dll") != "ad43eed347e9e0bb4e18b38392408724a71acddadef746512c81a7cfd171e860"
  #error "Accepted payload mismatch: System.Windows.Presentation.dll"
#endif
Source: "{#MyAppSourceDir}\System.Windows.Presentation.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Windows.Primitives.dll") != "644a1b4818afe9f194ee34a75652b7cc4db92f1e3da783164baac4348338b63c"
  #error "Accepted payload mismatch: System.Windows.Primitives.dll"
#endif
Source: "{#MyAppSourceDir}\System.Windows.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Windows.dll") != "84bff30fe6d50354c1db5b43c8abd4325b63c93c9ffdceacf6741172f53f78a0"
  #error "Accepted payload mismatch: System.Windows.dll"
#endif
Source: "{#MyAppSourceDir}\System.Windows.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Xaml.dll") != "fa4c695d225f21894543000701c17160896d4b6f42de7a03677fbb8470317a83"
  #error "Accepted payload mismatch: System.Xaml.dll"
#endif
Source: "{#MyAppSourceDir}\System.Xaml.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Xml.Linq.dll") != "e3b2f21addb2174a05e3ae502e60c8c6293e6867e402616c601fc9eafae53b24"
  #error "Accepted payload mismatch: System.Xml.Linq.dll"
#endif
Source: "{#MyAppSourceDir}\System.Xml.Linq.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Xml.ReaderWriter.dll") != "98e524ceadbdc13867472174989b94138164be1be7e3916b02e003da56d4c3c5"
  #error "Accepted payload mismatch: System.Xml.ReaderWriter.dll"
#endif
Source: "{#MyAppSourceDir}\System.Xml.ReaderWriter.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Xml.Serialization.dll") != "8d128e78cde86c6e8dc76a0114dda1fc6dfaad2c9b9d38126cfaf943d88dd6cd"
  #error "Accepted payload mismatch: System.Xml.Serialization.dll"
#endif
Source: "{#MyAppSourceDir}\System.Xml.Serialization.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Xml.XDocument.dll") != "ec9f74fd02c3bc34e5d39c3956a1e994ca0ca35b76281596e160346f358c3425"
  #error "Accepted payload mismatch: System.Xml.XDocument.dll"
#endif
Source: "{#MyAppSourceDir}\System.Xml.XDocument.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Xml.XPath.XDocument.dll") != "84411475a6a8f4dcb37435eeadf0cee23f89e643381616a4a785aec4e60c5c56"
  #error "Accepted payload mismatch: System.Xml.XPath.XDocument.dll"
#endif
Source: "{#MyAppSourceDir}\System.Xml.XPath.XDocument.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Xml.XPath.dll") != "d182c2f23c3d9ca1d43a2d55fe2bfbc800f4a6fdd4a800e7bd438064dd8faa81"
  #error "Accepted payload mismatch: System.Xml.XPath.dll"
#endif
Source: "{#MyAppSourceDir}\System.Xml.XPath.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Xml.XmlDocument.dll") != "0a7138dbc4afb4103ba2a60d0175195ea4a4d2b5202cc62bbb07520b460cc117"
  #error "Accepted payload mismatch: System.Xml.XmlDocument.dll"
#endif
Source: "{#MyAppSourceDir}\System.Xml.XmlDocument.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Xml.XmlSerializer.dll") != "3a1f60ce44e157d9a0041d522decf3c27613e9793dba25a02eccd26b8ff8a6e1"
  #error "Accepted payload mismatch: System.Xml.XmlSerializer.dll"
#endif
Source: "{#MyAppSourceDir}\System.Xml.XmlSerializer.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.Xml.dll") != "4e08544e819c44bd34f6baedca5e4f3c965b2ad576e095c1f7f7d74f2ac22965"
  #error "Accepted payload mismatch: System.Xml.dll"
#endif
Source: "{#MyAppSourceDir}\System.Xml.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\System.dll") != "bf9b65fcaef5e79d6f8e07597ae964619e86843986b6785bbfa9c03c673189f6"
  #error "Accepted payload mismatch: System.dll"
#endif
Source: "{#MyAppSourceDir}\System.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\THIRD_PARTY_LICENSES\GeoTimeZone.LICENSE") != "23b197502a8b013908f68e903f6c0fac9dcd2c30e0f934957d7f26e519723244"
  #error "Accepted payload mismatch: THIRD_PARTY_LICENSES\GeoTimeZone.LICENSE"
#endif
Source: "{#MyAppSourceDir}\THIRD_PARTY_LICENSES\GeoTimeZone.LICENSE"; DestDir: "{app}\THIRD_PARTY_LICENSES"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\THIRD_PARTY_LICENSES\TimeZoneConverter.LICENSE.txt") != "38183336fbc98bd5d6a3eb391eca942a25a639c188e7a6efc6647bc9b0abc0a2"
  #error "Accepted payload mismatch: THIRD_PARTY_LICENSES\TimeZoneConverter.LICENSE.txt"
#endif
Source: "{#MyAppSourceDir}\THIRD_PARTY_LICENSES\TimeZoneConverter.LICENSE.txt"; DestDir: "{app}\THIRD_PARTY_LICENSES"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\TimeZoneConverter.dll") != "d344e45cfce87c651340205b1ed8d5cb85b9d784a5eb219f43d7076d62a491ea"
  #error "Accepted payload mismatch: TimeZoneConverter.dll"
#endif
Source: "{#MyAppSourceDir}\TimeZoneConverter.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\UIAutomationClient.dll") != "da50f3e3dc670a93451930ebc09c159ace2c34f579322a23713d06ce9493afe4"
  #error "Accepted payload mismatch: UIAutomationClient.dll"
#endif
Source: "{#MyAppSourceDir}\UIAutomationClient.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\UIAutomationClientSideProviders.dll") != "a94bd3bf4c29f42ba5e77c48aeba335c45ee2116529267c73d669e76579f262c"
  #error "Accepted payload mismatch: UIAutomationClientSideProviders.dll"
#endif
Source: "{#MyAppSourceDir}\UIAutomationClientSideProviders.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\UIAutomationProvider.dll") != "691c03d4bd5ad62aca075154454f9112fd83063c351f05ac000dc8a378cf96e0"
  #error "Accepted payload mismatch: UIAutomationProvider.dll"
#endif
Source: "{#MyAppSourceDir}\UIAutomationProvider.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\UIAutomationTypes.dll") != "f067607b8580974165b05e41a7c28fda823ddd8c3211c4d63f894f3b3006470d"
  #error "Accepted payload mismatch: UIAutomationTypes.dll"
#endif
Source: "{#MyAppSourceDir}\UIAutomationTypes.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\WinRT.Runtime.dll") != "bcf3a14e8712a90837fc5c0d8c8a24696af2bd7f74e9767597ec81ededfb23db"
  #error "Accepted payload mismatch: WinRT.Runtime.dll"
#endif
Source: "{#MyAppSourceDir}\WinRT.Runtime.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\WindowsBase.dll") != "eb051d513031cb6d80b219a1de82e8889284aedcd770f399bf3ea726cff43e14"
  #error "Accepted payload mismatch: WindowsBase.dll"
#endif
Source: "{#MyAppSourceDir}\WindowsBase.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\WindowsFormsIntegration.dll") != "1ae30d0b42b29a56a0e9f30cc5bc076145d649faa80d2f5dc897ecd4d91cf5f9"
  #error "Accepted payload mismatch: WindowsFormsIntegration.dll"
#endif
Source: "{#MyAppSourceDir}\WindowsFormsIntegration.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\clretwrc.dll") != "a5be8e59c1a20c993771744bb36fb987ecd93766c0c6251c2716fff6d9bd70b2"
  #error "Accepted payload mismatch: clretwrc.dll"
#endif
Source: "{#MyAppSourceDir}\clretwrc.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\clrgc.dll") != "d497a976cc2f724abdb77b14b433ec684c07248a95c1b0331eafbb3703407b4b"
  #error "Accepted payload mismatch: clrgc.dll"
#endif
Source: "{#MyAppSourceDir}\clrgc.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\clrgcexp.dll") != "f0a36bbbc46fa57d89c978afb43213782ffb8b3e8aa4641498d3f0770430ac12"
  #error "Accepted payload mismatch: clrgcexp.dll"
#endif
Source: "{#MyAppSourceDir}\clrgcexp.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\clrjit.dll") != "ea4c636599d8cd2df65d20120a30466ba276d08d00a0d083f815a7eb7e845f92"
  #error "Accepted payload mismatch: clrjit.dll"
#endif
Source: "{#MyAppSourceDir}\clrjit.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\coreclr.dll") != "331c37f79cce19b37e74ee1adf8a78d741484f8adc1ce46c45cd6ad94d8022b4"
  #error "Accepted payload mismatch: coreclr.dll"
#endif
Source: "{#MyAppSourceDir}\coreclr.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\createdump.exe") != "362af9f57646e184c0c5c4f0ffd7af776771fd045cad734990a7178477bee983"
  #error "Accepted payload mismatch: createdump.exe"
#endif
Source: "{#MyAppSourceDir}\createdump.exe"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\cs\Microsoft.VisualBasic.Forms.resources.dll") != "b1163016cbf56fbec98635d65b6e44bbaf53d2e10c22836641fb9cbf3c67b181"
  #error "Accepted payload mismatch: cs\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\cs\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\cs\PresentationCore.resources.dll") != "057dca33eb6e26eb0cbc17e7cfde7dff848573f8de8ea2c2df35c411c6addf5c"
  #error "Accepted payload mismatch: cs\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDir}\cs\PresentationCore.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\cs\PresentationFramework.resources.dll") != "d009bfeaefd6263ae8e2623d905f124566ed77a5fc36c50e4e28645bd8653466"
  #error "Accepted payload mismatch: cs\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\cs\PresentationFramework.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\cs\PresentationUI.resources.dll") != "3ecc06ee87fc449979e951a859ec0509461aca8512c9bd180ea93bdf4afd29f5"
  #error "Accepted payload mismatch: cs\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDir}\cs\PresentationUI.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\cs\ReachFramework.resources.dll") != "7b173426d8d2cdf434d3b5833b9d6c72ca59e0a2b208560430f1bed2e7cb8384"
  #error "Accepted payload mismatch: cs\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\cs\ReachFramework.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\cs\System.Windows.Controls.Ribbon.resources.dll") != "ff1702880482513128bff514b473daf4e002caf19c6ebe73a1d6c25b52f36403"
  #error "Accepted payload mismatch: cs\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDir}\cs\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\cs\System.Windows.Forms.Design.resources.dll") != "7e176d6573d7fdd6c9ff011a71bd4e82926de3501c8f841be149681603058c5c"
  #error "Accepted payload mismatch: cs\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDir}\cs\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\cs\System.Windows.Forms.Primitives.resources.dll") != "1e5e964f7e718ac6292e938a25d15781b40a974e122f9bc5ad1e5f33a8374ad2"
  #error "Accepted payload mismatch: cs\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDir}\cs\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\cs\System.Windows.Forms.resources.dll") != "dc495b192c095998cbf5b6b1db6469380a7fcf6418c5561187aacd2bc2088f9c"
  #error "Accepted payload mismatch: cs\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\cs\System.Windows.Forms.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\cs\System.Windows.Input.Manipulations.resources.dll") != "3c27b1cf48fb3ae63ace90f34a1e8ad7b99bfc2620fb251eb49c0b79fd43445b"
  #error "Accepted payload mismatch: cs\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDir}\cs\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\cs\System.Xaml.resources.dll") != "15ed290ce2365e453b15e6cc1c663d7ecdcee6fea04a9b494b1d108e16777fbc"
  #error "Accepted payload mismatch: cs\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDir}\cs\System.Xaml.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\cs\UIAutomationClient.resources.dll") != "f8e45d3f78b09d51a2bef78c06014d3fbff7ec820db29555e45abd04f9d179ef"
  #error "Accepted payload mismatch: cs\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDir}\cs\UIAutomationClient.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\cs\UIAutomationClientSideProviders.resources.dll") != "234940a9fd5085c549c87a9a9b00ee8b4995bcef9f2bd92927b1f1defa194f2e"
  #error "Accepted payload mismatch: cs\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDir}\cs\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\cs\UIAutomationProvider.resources.dll") != "28d25f38029127c44a1e042d03825630595bb6c41c42a4bbc066f64555bdd39f"
  #error "Accepted payload mismatch: cs\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDir}\cs\UIAutomationProvider.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\cs\UIAutomationTypes.resources.dll") != "27c58a653332caf3fe462f9072e901acdf81c93e20e255c21fe75524f5aa7d5d"
  #error "Accepted payload mismatch: cs\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDir}\cs\UIAutomationTypes.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\cs\WindowsBase.resources.dll") != "369eae83d68919d7189e730f46f83b35a205edff15331010167d2b4e63befae3"
  #error "Accepted payload mismatch: cs\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDir}\cs\WindowsBase.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\cs\WindowsFormsIntegration.resources.dll") != "780e117003a6d6b40942590451d9d92531c2adbad7b7654a9edab2ab2d57ffe4"
  #error "Accepted payload mismatch: cs\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDir}\cs\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\de\Microsoft.VisualBasic.Forms.resources.dll") != "de995ff2aae42c626381064b39b97cb6bf251bae35b109f6dd4b3d8a9f89e187"
  #error "Accepted payload mismatch: de\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\de\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\de\PresentationCore.resources.dll") != "a9fab4afed764da094ae8475631d671914cd1b46bb707588bf60a3d3666bd59e"
  #error "Accepted payload mismatch: de\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDir}\de\PresentationCore.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\de\PresentationFramework.resources.dll") != "c6600e20a942d6e0398f1ac864b182668db4ab39356a65c44368ebb0a032c13b"
  #error "Accepted payload mismatch: de\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\de\PresentationFramework.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\de\PresentationUI.resources.dll") != "361a6466ef9079b675e0f554686dea885bca0b177a7e4f8972ad631fec0366bd"
  #error "Accepted payload mismatch: de\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDir}\de\PresentationUI.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\de\ReachFramework.resources.dll") != "b58d8443cc5ddf9a6cd5385654a50f7aa74829629d82d62d75c8e759666dd6b8"
  #error "Accepted payload mismatch: de\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\de\ReachFramework.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\de\System.Windows.Controls.Ribbon.resources.dll") != "131cbc1e4d606df3dfa29017d123d176f7570680702a299b4f460e5ee1692dff"
  #error "Accepted payload mismatch: de\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDir}\de\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\de\System.Windows.Forms.Design.resources.dll") != "5ac60d946947e39760ed8c1034cd34afc95ac2c696ff09a2fc8cd3171305ffaa"
  #error "Accepted payload mismatch: de\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDir}\de\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\de\System.Windows.Forms.Primitives.resources.dll") != "3da1690d05b84a6e5adc4539cbdfc464f433db131f3f71f9a0f68f3da9926566"
  #error "Accepted payload mismatch: de\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDir}\de\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\de\System.Windows.Forms.resources.dll") != "a01f89ca4be27a55791f1a13c91e5808e3180c2dd916796e8eb8f7f0a6debcf0"
  #error "Accepted payload mismatch: de\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\de\System.Windows.Forms.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\de\System.Windows.Input.Manipulations.resources.dll") != "0fa9af972c94c8b88a08846068cac89f15d879c930efd8f50b62d2751f620b64"
  #error "Accepted payload mismatch: de\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDir}\de\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\de\System.Xaml.resources.dll") != "821fc638c44ee125d67fed53c96f2b74576fdf79454e995ccf5265544dc49865"
  #error "Accepted payload mismatch: de\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDir}\de\System.Xaml.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\de\UIAutomationClient.resources.dll") != "b0c4ab37648b16f772b1a16fcca559ea507a69dcca37fbf302dc8498b88f3a40"
  #error "Accepted payload mismatch: de\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDir}\de\UIAutomationClient.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\de\UIAutomationClientSideProviders.resources.dll") != "aad6c858008aad993bba2f6c784c1a98c4875e0a5cb6c5fea4602035f0ea3b87"
  #error "Accepted payload mismatch: de\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDir}\de\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\de\UIAutomationProvider.resources.dll") != "12677e43dab5f0351dabfb60ecdd4e6b966cff9ee6e073205e43c9ef3cd30024"
  #error "Accepted payload mismatch: de\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDir}\de\UIAutomationProvider.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\de\UIAutomationTypes.resources.dll") != "d3365a572372e38fe2358748efa62439d9acdc80668ac4259b079ad396440e2b"
  #error "Accepted payload mismatch: de\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDir}\de\UIAutomationTypes.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\de\WindowsBase.resources.dll") != "28ced19eac00f994c83d6860562121ee5687b62f7a59ff02ba16b7c0522e4e61"
  #error "Accepted payload mismatch: de\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDir}\de\WindowsBase.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\de\WindowsFormsIntegration.resources.dll") != "fb64dbdc9f9352c4a690e31c12eb37ddb6d242132b6df22c9408387dd0920773"
  #error "Accepted payload mismatch: de\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDir}\de\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\es\Microsoft.VisualBasic.Forms.resources.dll") != "f46b06c3edece4676e210aa4b8cfc37a612461b22124f3bf4e4fc5c87aa80531"
  #error "Accepted payload mismatch: es\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\es\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\es\PresentationCore.resources.dll") != "9d0d39bf045132c14af504d1b4e749baa6e7846cedccfc39a50bcec91fab3b02"
  #error "Accepted payload mismatch: es\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDir}\es\PresentationCore.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\es\PresentationFramework.resources.dll") != "ea898d8e84b9c739a2f8e0aedba65d84baa4cfc20bf843acf1d59d99aef59268"
  #error "Accepted payload mismatch: es\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\es\PresentationFramework.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\es\PresentationUI.resources.dll") != "c09b2f56868b99395efd64be22adc7da5dc668d64635545d0c0651f939592426"
  #error "Accepted payload mismatch: es\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDir}\es\PresentationUI.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\es\ReachFramework.resources.dll") != "94e05360513f4af7e267e02cf2970386a45e0cd423c91882e8ec5123cbc6f2fe"
  #error "Accepted payload mismatch: es\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\es\ReachFramework.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\es\System.Windows.Controls.Ribbon.resources.dll") != "d5173cc8450533b5ba2daa38f8a3181b2aa4fca270246bee8c01bceb62681e3f"
  #error "Accepted payload mismatch: es\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDir}\es\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\es\System.Windows.Forms.Design.resources.dll") != "03ecad1acd4c69804339146725ebb7c6b8d191417c450910f81a604011f17833"
  #error "Accepted payload mismatch: es\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDir}\es\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\es\System.Windows.Forms.Primitives.resources.dll") != "57825af000a4097607e808c41d8e2485d1b7858a76988322dcaf2337deeafc5a"
  #error "Accepted payload mismatch: es\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDir}\es\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\es\System.Windows.Forms.resources.dll") != "1382425b9aaac485c940b3cb5d2d07aa61c03d188fa00baa23dedb825af19cb9"
  #error "Accepted payload mismatch: es\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\es\System.Windows.Forms.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\es\System.Windows.Input.Manipulations.resources.dll") != "7bd24f98462af82a930a045ae0a8cf6d6f0361c9ffe494db4a74c80af95739a0"
  #error "Accepted payload mismatch: es\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDir}\es\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\es\System.Xaml.resources.dll") != "b6212bf9213eae996d94d7c1b05f85de511837c2100d1064cdf33dbf1dd430a5"
  #error "Accepted payload mismatch: es\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDir}\es\System.Xaml.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\es\UIAutomationClient.resources.dll") != "d7264d3e7dcfbda081b33da7b204efe9d7e31ebf861d3f523a84ac684788e519"
  #error "Accepted payload mismatch: es\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDir}\es\UIAutomationClient.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\es\UIAutomationClientSideProviders.resources.dll") != "e38bc91e4b139d3502287657ec541aadd9d43eee5bc50cbd04718869a6df9dfe"
  #error "Accepted payload mismatch: es\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDir}\es\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\es\UIAutomationProvider.resources.dll") != "5866550fa8b90e632971718c20c95acfa94c96450c9b7c4526839165d25eb4b1"
  #error "Accepted payload mismatch: es\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDir}\es\UIAutomationProvider.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\es\UIAutomationTypes.resources.dll") != "c46c916c3dadddf9dc6515c332c51a670cbb4ce8302e9f5e8515167e8162bdf1"
  #error "Accepted payload mismatch: es\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDir}\es\UIAutomationTypes.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\es\WindowsBase.resources.dll") != "b0680466ef0ab3dbe44b81764800b06869221a8c863e1433f21c7e804ec78a4d"
  #error "Accepted payload mismatch: es\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDir}\es\WindowsBase.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\es\WindowsFormsIntegration.resources.dll") != "0288cc8a13e0eddfc4a2bb7ff4f1702ed1343e75e01347e2f0013e1ea0fb8baa"
  #error "Accepted payload mismatch: es\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDir}\es\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\fr\Microsoft.VisualBasic.Forms.resources.dll") != "d4a1b5dce12e1ca73eb5a5ac1416545e6537df9d3a7dd7dda3f7f6241cd79af5"
  #error "Accepted payload mismatch: fr\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\fr\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\fr\PresentationCore.resources.dll") != "9620d273cd682d5f6d07845509555c38f16672f7ba2464b3bccc3d7cfcc221f0"
  #error "Accepted payload mismatch: fr\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDir}\fr\PresentationCore.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\fr\PresentationFramework.resources.dll") != "a73dda3064bafac4437a3e522af92b21b42bc9ef207f2d1d7525d965d8519cb1"
  #error "Accepted payload mismatch: fr\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\fr\PresentationFramework.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\fr\PresentationUI.resources.dll") != "3abfe86782977127d880e22cd986413e004804c866bd503568b17fa6f978ec3b"
  #error "Accepted payload mismatch: fr\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDir}\fr\PresentationUI.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\fr\ReachFramework.resources.dll") != "e671c5ad3033c80a13c34dae6c338ce21627608dbb61174a31adcd89130014a3"
  #error "Accepted payload mismatch: fr\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\fr\ReachFramework.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\fr\System.Windows.Controls.Ribbon.resources.dll") != "03489ebb86734463af9712409ed4db1e26bf4dea248810be536c688a682d781a"
  #error "Accepted payload mismatch: fr\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDir}\fr\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\fr\System.Windows.Forms.Design.resources.dll") != "67a18c003540fa9936ed269693836772a1c4078af88d33ccf516389798f05afa"
  #error "Accepted payload mismatch: fr\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDir}\fr\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\fr\System.Windows.Forms.Primitives.resources.dll") != "ddb38c0d1fc6976b040637d8ba0037c5d270c7cc54e75e62ce3a73f0660e7920"
  #error "Accepted payload mismatch: fr\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDir}\fr\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\fr\System.Windows.Forms.resources.dll") != "98ec7c806bb1e294cc9842283e8603eb983fd637d0e2500027f3ea3d3d2cace5"
  #error "Accepted payload mismatch: fr\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\fr\System.Windows.Forms.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\fr\System.Windows.Input.Manipulations.resources.dll") != "276e69173e813fa7d7d0c87b3e0bf3a08adf6e14743e36cb6e4a089ef5ad2e1a"
  #error "Accepted payload mismatch: fr\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDir}\fr\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\fr\System.Xaml.resources.dll") != "cba71837ed2bc2782293376e3f84fdefeaaf4e5f01cf134366287ff78ad9b5d4"
  #error "Accepted payload mismatch: fr\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDir}\fr\System.Xaml.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\fr\UIAutomationClient.resources.dll") != "18ff459dde1be3d88918a7cba66043bdc570fa2ed807c4852ae38f864619f7db"
  #error "Accepted payload mismatch: fr\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDir}\fr\UIAutomationClient.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\fr\UIAutomationClientSideProviders.resources.dll") != "b46891466dd7053806f9cc2108031348d55185c40ae1d26c4d6eb5cfdf1c9b2a"
  #error "Accepted payload mismatch: fr\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDir}\fr\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\fr\UIAutomationProvider.resources.dll") != "31b052225ec884e121defd6daf4dcea3d388fb514ca47a834b8cab4b3fbf3062"
  #error "Accepted payload mismatch: fr\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDir}\fr\UIAutomationProvider.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\fr\UIAutomationTypes.resources.dll") != "c6228ad5455aa3f3c3a001c2e38b0d06ccd10536821a451507eb20094e5ef0f6"
  #error "Accepted payload mismatch: fr\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDir}\fr\UIAutomationTypes.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\fr\WindowsBase.resources.dll") != "70912f476d9461a486da4da66b36b817b03fd86aa3b86954294c55016bc37511"
  #error "Accepted payload mismatch: fr\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDir}\fr\WindowsBase.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\fr\WindowsFormsIntegration.resources.dll") != "a86a6538fc8ee82534d0803e0492642bb3d842d8735c0920cdd9eaa4062ea103"
  #error "Accepted payload mismatch: fr\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDir}\fr\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\geonames\NOTICE.txt") != "b116bb5f613ad8b2476eb8dbdee7af5310cc5b396798d2bf9e6e0b56f0a116f2"
  #error "Accepted payload mismatch: geonames\NOTICE.txt"
#endif
Source: "{#MyAppSourceDir}\geonames\NOTICE.txt"; DestDir: "{app}\geonames"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\geonames\cities1000.tsv.gz") != "e377a5ffddd22ad4e47e84452d394bc2227f35eb67dfdcacf186ee9b8462e2f1"
  #error "Accepted payload mismatch: geonames\cities1000.tsv.gz"
#endif
Source: "{#MyAppSourceDir}\geonames\cities1000.tsv.gz"; DestDir: "{app}\geonames"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\hostfxr.dll") != "b41af3ca044a75a77f3bf7883ec586a63e8bd25e74106b21d08131488141581b"
  #error "Accepted payload mismatch: hostfxr.dll"
#endif
Source: "{#MyAppSourceDir}\hostfxr.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\hostpolicy.dll") != "26b4a61642b38f76ec91f545a97c08ac7bd6e7bbf4842cc245cadbfc8b36dee5"
  #error "Accepted payload mismatch: hostpolicy.dll"
#endif
Source: "{#MyAppSourceDir}\hostpolicy.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\it\Microsoft.VisualBasic.Forms.resources.dll") != "8516a41381daecf0ca7044c4dde0832e370ec061a0818fe2072c189cec8884fb"
  #error "Accepted payload mismatch: it\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\it\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\it\PresentationCore.resources.dll") != "c7af9e37a7ab3ae4330e837c1d2038f9c5d5207954c7f46c7e516fab07f8c2fb"
  #error "Accepted payload mismatch: it\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDir}\it\PresentationCore.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\it\PresentationFramework.resources.dll") != "3923f991050b5b2e2001c1f2e813f9b5f88970c9742aba329ed370f637f6ee4e"
  #error "Accepted payload mismatch: it\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\it\PresentationFramework.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\it\PresentationUI.resources.dll") != "c0d8cca4760c169dcc03395ac22c2f2cca98cb7f7e4463e1ebf4d4ae8617c94c"
  #error "Accepted payload mismatch: it\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDir}\it\PresentationUI.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\it\ReachFramework.resources.dll") != "b41198d8898113825cf52cc224363b23511a67d1b5c717490790bfdeeb65b6c3"
  #error "Accepted payload mismatch: it\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\it\ReachFramework.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\it\System.Windows.Controls.Ribbon.resources.dll") != "231b3388b0bfeb46ae79dfc93624cf54594280725a4ce714c432e10ad5ca508f"
  #error "Accepted payload mismatch: it\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDir}\it\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\it\System.Windows.Forms.Design.resources.dll") != "e7dd0816b983543c53e01a4dd1262383f242076b46dacd592916affbd7979ed5"
  #error "Accepted payload mismatch: it\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDir}\it\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\it\System.Windows.Forms.Primitives.resources.dll") != "b94acc16a01b6cd282f4f51ea831b62ea506485d37b3a994ee11b595898b93c6"
  #error "Accepted payload mismatch: it\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDir}\it\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\it\System.Windows.Forms.resources.dll") != "540ee4348a3d35e92b08f86d094bd0c4f8e472322bcb301f9621232973f7c5c4"
  #error "Accepted payload mismatch: it\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\it\System.Windows.Forms.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\it\System.Windows.Input.Manipulations.resources.dll") != "ec0d89b36d7ac00dec3bd6fc9ef3839028d57598b244ccbeaae0e6805034abca"
  #error "Accepted payload mismatch: it\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDir}\it\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\it\System.Xaml.resources.dll") != "0dfbe3b0632ff5f437e36702581f2150b1468c4abb871e43c54e009a75061d3d"
  #error "Accepted payload mismatch: it\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDir}\it\System.Xaml.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\it\UIAutomationClient.resources.dll") != "805bf2285f9f870c424c2623e48f377bcea378453d7ee19983e70bafb16dffc9"
  #error "Accepted payload mismatch: it\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDir}\it\UIAutomationClient.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\it\UIAutomationClientSideProviders.resources.dll") != "ac1f2c932cf30f08cc5ab90ebf1341bb18f9b2c364f26eb9f4d65bda0fa5fc77"
  #error "Accepted payload mismatch: it\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDir}\it\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\it\UIAutomationProvider.resources.dll") != "198a6b63d5331941f6d4e061ebd41258d3abbf1865c443fea3664bcd7272ad13"
  #error "Accepted payload mismatch: it\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDir}\it\UIAutomationProvider.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\it\UIAutomationTypes.resources.dll") != "bb7ceca81056bec661b5dea6cac0508914a8e98ff455f71f6d470a20eb5a6c3c"
  #error "Accepted payload mismatch: it\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDir}\it\UIAutomationTypes.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\it\WindowsBase.resources.dll") != "d036c85e95066039381649d5681b3383b685de5d6b032d1a9e72f1aa5df926ab"
  #error "Accepted payload mismatch: it\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDir}\it\WindowsBase.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\it\WindowsFormsIntegration.resources.dll") != "c286640a2cb1527dfa3e68e47d989885f5bc86c67680d903cf18662d31c1661e"
  #error "Accepted payload mismatch: it\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDir}\it\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ja\Microsoft.VisualBasic.Forms.resources.dll") != "bdeb78cdd58dda626d2200462da7c5f31d60f37dad21d6aba151fbf56c51be78"
  #error "Accepted payload mismatch: ja\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ja\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ja\PresentationCore.resources.dll") != "0dc4990dd2c85e68e90223d334ee4b511222ce35f3c2d1f9ef2100178fb15ea1"
  #error "Accepted payload mismatch: ja\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ja\PresentationCore.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ja\PresentationFramework.resources.dll") != "d000c6ae91fc4c782b3806e64eb1f847bd35a3149b98f17d48b8e613e5328648"
  #error "Accepted payload mismatch: ja\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ja\PresentationFramework.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ja\PresentationUI.resources.dll") != "2267942215681a82eea96ef214a120d76111e1ca1a9be91ca6183c26082d6c1c"
  #error "Accepted payload mismatch: ja\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ja\PresentationUI.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ja\ReachFramework.resources.dll") != "c121844f1ddba25613f27c0a4ada6789d95914925e8aa2e8690ea600be541208"
  #error "Accepted payload mismatch: ja\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ja\ReachFramework.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ja\System.Windows.Controls.Ribbon.resources.dll") != "631b3cbdef6d7ef698a8e22e4b973e7dd2419247c97d6cde4a0e19064f119dab"
  #error "Accepted payload mismatch: ja\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ja\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ja\System.Windows.Forms.Design.resources.dll") != "7ae6117b285500cd451122d74945fcbdcbd8354bcb2a45692838feabcf866a29"
  #error "Accepted payload mismatch: ja\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ja\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ja\System.Windows.Forms.Primitives.resources.dll") != "eccd200bd9ea1483f07325f2733e11849a12b37a3d89a0777a15b64bb722168d"
  #error "Accepted payload mismatch: ja\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ja\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ja\System.Windows.Forms.resources.dll") != "1f65e4c29ed6f19463d1b620e92e197adaf35618c044c82a355006c66d3babb6"
  #error "Accepted payload mismatch: ja\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ja\System.Windows.Forms.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ja\System.Windows.Input.Manipulations.resources.dll") != "eb24e243107beac31a5a4ed2bd493b3da53974b41776ce482aa1584ba408f4fa"
  #error "Accepted payload mismatch: ja\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ja\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ja\System.Xaml.resources.dll") != "9ceafdb6c4bd1eda90ff7cc4f6c62b4a1ab45368ebb72507d53c87145af66035"
  #error "Accepted payload mismatch: ja\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ja\System.Xaml.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ja\UIAutomationClient.resources.dll") != "d3ab5d611c4ed0e7e802e720a41d0c3926305b0675233b1b686f3f5ccdab37f8"
  #error "Accepted payload mismatch: ja\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ja\UIAutomationClient.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ja\UIAutomationClientSideProviders.resources.dll") != "7ae80153914f71513d364cfc0de232d36e99b94a049560d7d123191721b035d6"
  #error "Accepted payload mismatch: ja\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ja\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ja\UIAutomationProvider.resources.dll") != "58e3360c69c192f3e1541be2f3569eac6fe6cb000e3f79e504e94c488bea7317"
  #error "Accepted payload mismatch: ja\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ja\UIAutomationProvider.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ja\UIAutomationTypes.resources.dll") != "6f862dc381286d137e615601cc51531718713dc50bf3a89997f59a9f7d59d67f"
  #error "Accepted payload mismatch: ja\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ja\UIAutomationTypes.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ja\WindowsBase.resources.dll") != "fb945a821ac0fe5f1a4826d510073f62092c417535f39ec166a93c3c7381c44a"
  #error "Accepted payload mismatch: ja\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ja\WindowsBase.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ja\WindowsFormsIntegration.resources.dll") != "b91a85f654cdb942eecdb9df1c84fed60887168623ad91cf1c4c7e80ba7588bc"
  #error "Accepted payload mismatch: ja\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ja\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\ja"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ko\Microsoft.VisualBasic.Forms.resources.dll") != "a6d729c825ab12ae43d1aa8e8d120c919c85bc81a70f55ae294e22612bd1ccf4"
  #error "Accepted payload mismatch: ko\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ko\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ko\PresentationCore.resources.dll") != "88a6f0a639de5880dc3fcddd69322d740225980d3bce076fa236350f9e08d014"
  #error "Accepted payload mismatch: ko\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ko\PresentationCore.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ko\PresentationFramework.resources.dll") != "cb32fd9a4446257b4e698866fe3c1b03c9cee2d5b377efa841903ce48372f26b"
  #error "Accepted payload mismatch: ko\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ko\PresentationFramework.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ko\PresentationUI.resources.dll") != "2ad39fd76c5dd70f9b832c29be5feef1c3afcf98892bfc3b893280c8ce81e2f8"
  #error "Accepted payload mismatch: ko\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ko\PresentationUI.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ko\ReachFramework.resources.dll") != "8d9b4b1dbd9cb0b4bba12e04dcbf4db2e9df57c1c3dde14deb381e0680d9b8df"
  #error "Accepted payload mismatch: ko\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ko\ReachFramework.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ko\System.Windows.Controls.Ribbon.resources.dll") != "1e5155634e4576a5c38361b2de62604b30df467114f7fa16bf0423d87c84b64a"
  #error "Accepted payload mismatch: ko\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ko\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ko\System.Windows.Forms.Design.resources.dll") != "0506b63df9a3527114fa730be830070f785bf023ed4f064619c8ff2e1b6c277a"
  #error "Accepted payload mismatch: ko\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ko\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ko\System.Windows.Forms.Primitives.resources.dll") != "c8a59f9c1b08da1e2c27473d8fc8cefc8ad34dd2aec5a028d879486e8e098577"
  #error "Accepted payload mismatch: ko\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ko\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ko\System.Windows.Forms.resources.dll") != "22176dec3acb77e6dcbd0338e7254ddbd381580f7ab68ea07867fddaa90c7978"
  #error "Accepted payload mismatch: ko\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ko\System.Windows.Forms.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ko\System.Windows.Input.Manipulations.resources.dll") != "dc261e6e77f938925f784dfbdcb0a1bfff25a9b0deb555281bcefafbbdc931e7"
  #error "Accepted payload mismatch: ko\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ko\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ko\System.Xaml.resources.dll") != "df0101752e00f2982a92dcdad29b72acbd2000a69ed5d86f949b381e872d2127"
  #error "Accepted payload mismatch: ko\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ko\System.Xaml.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ko\UIAutomationClient.resources.dll") != "12ce47ea3b8e3e2e4c76bcfc8b6e5515459dc9c02a26c9bb775a59018f959038"
  #error "Accepted payload mismatch: ko\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ko\UIAutomationClient.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ko\UIAutomationClientSideProviders.resources.dll") != "9246742cd9fe1f1beebeb81b676f781119dee15164e75f4394dbe7d30217940d"
  #error "Accepted payload mismatch: ko\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ko\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ko\UIAutomationProvider.resources.dll") != "2780844c26a4a496b962d03de87d773b588150fb8760f91216bd5f428d7ffd60"
  #error "Accepted payload mismatch: ko\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ko\UIAutomationProvider.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ko\UIAutomationTypes.resources.dll") != "3791bd41c44985a7d96523d6fdced4a4522466cd2abde4ae1f5facea9c006ac0"
  #error "Accepted payload mismatch: ko\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ko\UIAutomationTypes.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ko\WindowsBase.resources.dll") != "6f533a2327e7c2fd2440a4be5736d535fdcc768ab39904497d2db1d8893f4cad"
  #error "Accepted payload mismatch: ko\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ko\WindowsBase.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ko\WindowsFormsIntegration.resources.dll") != "97602d52b36dbc0f0c27ba11c8302021a7da3dd34ad2993198df9e4d2793251c"
  #error "Accepted payload mismatch: ko\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ko\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\ko"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\mscordaccore.dll") != "0a24185b73aaba945692f9b9433d295bb726350b28a824bf2fbbeb0bf92be3c4"
  #error "Accepted payload mismatch: mscordaccore.dll"
#endif
Source: "{#MyAppSourceDir}\mscordaccore.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\mscordaccore_amd64_amd64_10.0.926.27113.dll") != "0a24185b73aaba945692f9b9433d295bb726350b28a824bf2fbbeb0bf92be3c4"
  #error "Accepted payload mismatch: mscordaccore_amd64_amd64_10.0.926.27113.dll"
#endif
Source: "{#MyAppSourceDir}\mscordaccore_amd64_amd64_10.0.926.27113.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\mscordbi.dll") != "6944ca87644750563d5a452b3f18feedb26426701b3f80dd7601e3f48e52285c"
  #error "Accepted payload mismatch: mscordbi.dll"
#endif
Source: "{#MyAppSourceDir}\mscordbi.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\mscorlib.dll") != "be4a0fa1ca6b7cea8980aa063ae070565c57488c104e7fc9efa116e1c68bffac"
  #error "Accepted payload mismatch: mscorlib.dll"
#endif
Source: "{#MyAppSourceDir}\mscorlib.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\mscorrc.dll") != "c0fcd2ceebf9885d9bdfe96d543618d5707918b30e908b630121b316c963fa66"
  #error "Accepted payload mismatch: mscorrc.dll"
#endif
Source: "{#MyAppSourceDir}\mscorrc.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\msquic.dll") != "99daf179ee2dfb5c36493543647b64fc58a24aba0b16d5e9232b06003fbc9054"
  #error "Accepted payload mismatch: msquic.dll"
#endif
Source: "{#MyAppSourceDir}\msquic.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\netstandard.dll") != "41ba95895b61a4fe461333e2d401b5ecc2c0d57eca07a3f5287f906908d7f0fd"
  #error "Accepted payload mismatch: netstandard.dll"
#endif
Source: "{#MyAppSourceDir}\netstandard.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pl\Microsoft.VisualBasic.Forms.resources.dll") != "8ff08d76521f1407068bf724d71b859d9cdbd771f8f18991ed2159c59a33bb10"
  #error "Accepted payload mismatch: pl\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pl\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pl\PresentationCore.resources.dll") != "81e9d686c0523a1b917b99a17edbdbe9c3c5ca381ab52ae40f7fac49abcf540f"
  #error "Accepted payload mismatch: pl\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pl\PresentationCore.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pl\PresentationFramework.resources.dll") != "383fd7b703c1c811953e75545e6c3156ae8c2dca7c579ea157bf7756098dcd1b"
  #error "Accepted payload mismatch: pl\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pl\PresentationFramework.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pl\PresentationUI.resources.dll") != "daf770246210c8ecfebf222da4745050ff1cc5385e60c8649bf5b6ce119bbe18"
  #error "Accepted payload mismatch: pl\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pl\PresentationUI.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pl\ReachFramework.resources.dll") != "97cdbc8321c83e6ddcdd77f216f9e9974c0bbc391e40dcca2421c180054ee87a"
  #error "Accepted payload mismatch: pl\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pl\ReachFramework.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pl\System.Windows.Controls.Ribbon.resources.dll") != "e56d9798431f4d88ebe76752094466f8ec5158f39ef720d36c15d50645cfe1f6"
  #error "Accepted payload mismatch: pl\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pl\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pl\System.Windows.Forms.Design.resources.dll") != "a39e419335b3e112724add760586858e2eb6b87253d478043713175f7f3c5153"
  #error "Accepted payload mismatch: pl\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pl\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pl\System.Windows.Forms.Primitives.resources.dll") != "562a49671e28be82595929f744dace7e8a741c845d46c5f99b9765b4b0712bbe"
  #error "Accepted payload mismatch: pl\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pl\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pl\System.Windows.Forms.resources.dll") != "a2c66abbd1722575229972b359c65dda128f6aa85fcc771cb20744baf98bc678"
  #error "Accepted payload mismatch: pl\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pl\System.Windows.Forms.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pl\System.Windows.Input.Manipulations.resources.dll") != "aeadf3833afaf8277008da173720e572a8c694e04d512ae7260eeba15ae3b7c6"
  #error "Accepted payload mismatch: pl\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pl\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pl\System.Xaml.resources.dll") != "0f1954b45211b934965cf7bca5df3bd37a29abd4440001d2eca8194d818adb9b"
  #error "Accepted payload mismatch: pl\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pl\System.Xaml.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pl\UIAutomationClient.resources.dll") != "d17765ef7912eaff7a80e1dea9baf27b02a7ea558c946de64936b8ca22eccc03"
  #error "Accepted payload mismatch: pl\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pl\UIAutomationClient.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pl\UIAutomationClientSideProviders.resources.dll") != "96dc9e91ae95dc3c9323ed1ddf81c6e6f883e582d0569c29703f8a612f036d40"
  #error "Accepted payload mismatch: pl\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pl\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pl\UIAutomationProvider.resources.dll") != "ae27918f74db87b8869817458846ef6b695aeeb46f088c151eb7bf176b7e81a4"
  #error "Accepted payload mismatch: pl\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pl\UIAutomationProvider.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pl\UIAutomationTypes.resources.dll") != "129dc3ee0b5ace93b7a20dca40744125406450a5d36a2ee533220354d7ab7552"
  #error "Accepted payload mismatch: pl\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pl\UIAutomationTypes.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pl\WindowsBase.resources.dll") != "8070602fbad1185115bb89ade2af5955169127133eb3356641e24b9bd529eca3"
  #error "Accepted payload mismatch: pl\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pl\WindowsBase.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pl\WindowsFormsIntegration.resources.dll") != "c7782819f8b222fc8a79d83cabb2f58d2cfbc91eb65d5b0b79e89c58fdd1cbd5"
  #error "Accepted payload mismatch: pl\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pl\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\pl"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\prayer_automation.ico") != "555c890b949528ce48fc8c9cdb5c0ef75286ac41dbfb4df4fa75a772caa11c12"
  #error "Accepted payload mismatch: prayer_automation.ico"
#endif
Source: "{#MyAppSourceDir}\prayer_automation.ico"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\prayer_core.dll") != "7f18fd13ed58430475904d38914575ca421f408381d62c43c6041d1e2e525858"
  #error "Accepted payload mismatch: prayer_core.dll"
#endif
Source: "{#MyAppSourceDir}\prayer_core.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pt-BR\Microsoft.VisualBasic.Forms.resources.dll") != "4fab6ac2db16261a17132684f15dcffcbe15f9f5c7084f0027d98c1f49619441"
  #error "Accepted payload mismatch: pt-BR\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pt-BR\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pt-BR\PresentationCore.resources.dll") != "4653fdb53383fa2bfa470572a04d74ae58bc7e8d7de1c94444eb4a4fe6a4b0e6"
  #error "Accepted payload mismatch: pt-BR\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pt-BR\PresentationCore.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pt-BR\PresentationFramework.resources.dll") != "765e8a7e9c0d2f2ddfabbcee5767fa6c6a62e004a8265710329ee81ecd149ee2"
  #error "Accepted payload mismatch: pt-BR\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pt-BR\PresentationFramework.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pt-BR\PresentationUI.resources.dll") != "d9a82d518145d66999d003c59c1e57ede8b873025fe8d4d0feddea77fed756cd"
  #error "Accepted payload mismatch: pt-BR\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pt-BR\PresentationUI.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pt-BR\ReachFramework.resources.dll") != "bb33dd6dd2733018701233ccffe6a293b689081ab6151555f433c0a8c917fcaa"
  #error "Accepted payload mismatch: pt-BR\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pt-BR\ReachFramework.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pt-BR\System.Windows.Controls.Ribbon.resources.dll") != "210ee6ba923dfa38f753dc9cfc05c0a5fb3df595e7a64e4801c53f5e487a1f7d"
  #error "Accepted payload mismatch: pt-BR\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pt-BR\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pt-BR\System.Windows.Forms.Design.resources.dll") != "bf17ac654eba529211f2ae1d5691904a9b2988e5525fafd84850e053bf3b989b"
  #error "Accepted payload mismatch: pt-BR\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pt-BR\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pt-BR\System.Windows.Forms.Primitives.resources.dll") != "566a98d6a24f1f0ab74aa97fceb3ff868aa7107dd2bc09aaed2f623f41f7db78"
  #error "Accepted payload mismatch: pt-BR\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pt-BR\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pt-BR\System.Windows.Forms.resources.dll") != "54bce898c12f0622f277c94498f4b665e3bcd41b559a4db0d839841e32a0da9d"
  #error "Accepted payload mismatch: pt-BR\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pt-BR\System.Windows.Forms.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pt-BR\System.Windows.Input.Manipulations.resources.dll") != "7a329f69cf88cbb15552dc4235b76a105afc1aafb647ee181c850df45ed724c9"
  #error "Accepted payload mismatch: pt-BR\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pt-BR\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pt-BR\System.Xaml.resources.dll") != "6d85a90dd73ab57504c2fa30275f12116b7274cc1b3a6f98e24591a6a01dfeb2"
  #error "Accepted payload mismatch: pt-BR\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pt-BR\System.Xaml.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pt-BR\UIAutomationClient.resources.dll") != "f15925fcf99d28a8ef958b3671be175a34ddaa6499fcd0c54983590d46c30c66"
  #error "Accepted payload mismatch: pt-BR\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pt-BR\UIAutomationClient.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pt-BR\UIAutomationClientSideProviders.resources.dll") != "9312609d14dd095c766e9d614a477c6fca293f765a9bdd28b2edd4494e0d92e5"
  #error "Accepted payload mismatch: pt-BR\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pt-BR\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pt-BR\UIAutomationProvider.resources.dll") != "dfd9b350809e0dad7e764654d6a50b9ab49ad9042a2ce0175a14145106488f35"
  #error "Accepted payload mismatch: pt-BR\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pt-BR\UIAutomationProvider.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pt-BR\UIAutomationTypes.resources.dll") != "5666a50afe89c57dc03e6bb1e471d05f307a2674912ff3cdbddbff5ecf4d96ce"
  #error "Accepted payload mismatch: pt-BR\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pt-BR\UIAutomationTypes.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pt-BR\WindowsBase.resources.dll") != "d15b8183ba96ef7d63698a9ab5f301b0679eb5f9f12da53e78c89010cf78cc0f"
  #error "Accepted payload mismatch: pt-BR\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pt-BR\WindowsBase.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\pt-BR\WindowsFormsIntegration.resources.dll") != "1f4a4b59751c6ad4b18dd604a47d217524450c2d5da88029ca245899ad9b4737"
  #error "Accepted payload mismatch: pt-BR\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDir}\pt-BR\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ru\Microsoft.VisualBasic.Forms.resources.dll") != "31e5db701958c3c775aa55ad889af1720907b1a60579157d084a291c9065051f"
  #error "Accepted payload mismatch: ru\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ru\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ru\PresentationCore.resources.dll") != "e03905c6a15890c34127741d5c13ed1a1f7a7762d71f9114533e74f540f2071a"
  #error "Accepted payload mismatch: ru\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ru\PresentationCore.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ru\PresentationFramework.resources.dll") != "6786445bd9c6fbf5979a9ca29844ae47e084aef3a85fc915dec3a34ba6a4a1be"
  #error "Accepted payload mismatch: ru\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ru\PresentationFramework.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ru\PresentationUI.resources.dll") != "e58764490d83f130ac2e4a6c7b5c324b0124d743447899553ba84a7738087a92"
  #error "Accepted payload mismatch: ru\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ru\PresentationUI.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ru\ReachFramework.resources.dll") != "ec3caee9bac50c74aa2e7e2d73f8acfa8250babc84aa83a99a218543d90f85d0"
  #error "Accepted payload mismatch: ru\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ru\ReachFramework.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ru\System.Windows.Controls.Ribbon.resources.dll") != "14e3a551d6dcccd2787b6e94e509c47e6cb79f179405c551d54f9549409fe8a9"
  #error "Accepted payload mismatch: ru\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ru\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ru\System.Windows.Forms.Design.resources.dll") != "08a2e7d67a9f953da04f657d86e8d1c76502840de6d1597959836d43ad338f8f"
  #error "Accepted payload mismatch: ru\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ru\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ru\System.Windows.Forms.Primitives.resources.dll") != "6d335bf16d58b5b6969736bb0d975a090b93b92885fc2fe8edfd829425324c43"
  #error "Accepted payload mismatch: ru\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ru\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ru\System.Windows.Forms.resources.dll") != "07e45c301de8330a6d6afa39889d36c500a3ff11f377a7d2592bf7fb7bc76bc1"
  #error "Accepted payload mismatch: ru\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ru\System.Windows.Forms.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ru\System.Windows.Input.Manipulations.resources.dll") != "969c0c5544e3f3b7468de7c6361edbd627de9ac6ef3422470fc6d86b471a1008"
  #error "Accepted payload mismatch: ru\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ru\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ru\System.Xaml.resources.dll") != "c5a9c39ef81d8c7b41c291f7b72462d5b939f70c4d04362296cd2fe0ff4f559e"
  #error "Accepted payload mismatch: ru\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ru\System.Xaml.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ru\UIAutomationClient.resources.dll") != "cb556554871bcf9bc072cb4be5d584e0042f65c8d67f04b5decc0f6b6d74b83a"
  #error "Accepted payload mismatch: ru\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ru\UIAutomationClient.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ru\UIAutomationClientSideProviders.resources.dll") != "c787a4ea9a5e58e856803d38cce5fd27c2c8d59c881f77f1fdeea120f20cf894"
  #error "Accepted payload mismatch: ru\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ru\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ru\UIAutomationProvider.resources.dll") != "c2cf59117586a068d121f881345a739036f72b55ed6b0dcf8c149c1e490168cb"
  #error "Accepted payload mismatch: ru\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ru\UIAutomationProvider.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ru\UIAutomationTypes.resources.dll") != "b470fe212d4da17d51fcb6b170f5f58492357ad31ff9accb991027197bc40eac"
  #error "Accepted payload mismatch: ru\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ru\UIAutomationTypes.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ru\WindowsBase.resources.dll") != "6d2e9a2f51fe8e9ee0fc6607c17a798b367adff904c2131a750ddda6df16531b"
  #error "Accepted payload mismatch: ru\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ru\WindowsBase.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\ru\WindowsFormsIntegration.resources.dll") != "405d704fcc65c4089a0b52b754efc708332c50b44f76ff0a8668c39c5ef0acc3"
  #error "Accepted payload mismatch: ru\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDir}\ru\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\ru"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\sedang_sholat.png") != "e64b8515280119b4e4e7f0e59c74244da5a44a815f1dd4cb5c4e2dbc7868a5df"
  #error "Accepted payload mismatch: sedang_sholat.png"
#endif
Source: "{#MyAppSourceDir}\sedang_sholat.png"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\tr\Microsoft.VisualBasic.Forms.resources.dll") != "7af12bb2e9a2835f8cf85acd3162ceb25537d2acb6959f8ead5d2d3bf91cfa1f"
  #error "Accepted payload mismatch: tr\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\tr\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\tr\PresentationCore.resources.dll") != "1783be5e1a78beabdc715f9f7695d807fde1352c4e94d181879356881bb2081c"
  #error "Accepted payload mismatch: tr\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDir}\tr\PresentationCore.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\tr\PresentationFramework.resources.dll") != "be4c18ba29242f1f97571b94feee186c0850048a2ef5030067d183f971c2718f"
  #error "Accepted payload mismatch: tr\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\tr\PresentationFramework.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\tr\PresentationUI.resources.dll") != "ed462a1cdb74bcede3fdc55d3bc6c3e0a24c9f0250073978e65f60c0d9674947"
  #error "Accepted payload mismatch: tr\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDir}\tr\PresentationUI.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\tr\ReachFramework.resources.dll") != "47f356c1f4b73343adb5ba35452efc1ac62aa73be661e69254d6722ed9d0eb55"
  #error "Accepted payload mismatch: tr\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\tr\ReachFramework.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\tr\System.Windows.Controls.Ribbon.resources.dll") != "0ad4290694de2e43ad2a4bd5dcdb6c48abbb61c7788aaf9dfcfa0e7c6fa64cab"
  #error "Accepted payload mismatch: tr\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDir}\tr\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\tr\System.Windows.Forms.Design.resources.dll") != "2d011841fadf92e3620dd7e9e2eed1db2132bca4f7490951b10952a6612ab140"
  #error "Accepted payload mismatch: tr\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDir}\tr\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\tr\System.Windows.Forms.Primitives.resources.dll") != "0a8fff62db88f86914ceec6f6732e21d9f143b007d85a888c7fc39029e7ccb83"
  #error "Accepted payload mismatch: tr\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDir}\tr\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\tr\System.Windows.Forms.resources.dll") != "22b4ca79bba1245966acdde5d7d46d3b355084edf2aa07b6b47d5b88160f8373"
  #error "Accepted payload mismatch: tr\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\tr\System.Windows.Forms.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\tr\System.Windows.Input.Manipulations.resources.dll") != "667f93fc7fdbb63b1118defa9428abdfa5397e951971cd9c3debdb490dd8fe3d"
  #error "Accepted payload mismatch: tr\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDir}\tr\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\tr\System.Xaml.resources.dll") != "077ee4277f5c0284b53f1c1dcbfc1d4397e382a4f5a0ae220afd83569af7cc7a"
  #error "Accepted payload mismatch: tr\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDir}\tr\System.Xaml.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\tr\UIAutomationClient.resources.dll") != "1f8b3b12f15062bb05ebacf9dc2e72863447c9b6037d219fb9ecde51685c5e98"
  #error "Accepted payload mismatch: tr\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDir}\tr\UIAutomationClient.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\tr\UIAutomationClientSideProviders.resources.dll") != "d188b8cbab4c064aafc9d99585423ecb681bee906e59c7d1f5792a34408620bd"
  #error "Accepted payload mismatch: tr\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDir}\tr\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\tr\UIAutomationProvider.resources.dll") != "ec61f24fa4028f6665661ac6b0479d5fabd9c68cd19a5fe54038765b26bb29a8"
  #error "Accepted payload mismatch: tr\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDir}\tr\UIAutomationProvider.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\tr\UIAutomationTypes.resources.dll") != "65938a18fd51b42d647169500503fdef6bbb60b48b1e0c8d7207a783e947e2c6"
  #error "Accepted payload mismatch: tr\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDir}\tr\UIAutomationTypes.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\tr\WindowsBase.resources.dll") != "9b2cdd62d88b9b3d53cf50e0188b0c40b89717a995c2f07bf51c7c91d482c000"
  #error "Accepted payload mismatch: tr\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDir}\tr\WindowsBase.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\tr\WindowsFormsIntegration.resources.dll") != "55c071c5b69f2867eeea02177a1f5c4f09346665bc88d1a4d3559274c2c27390"
  #error "Accepted payload mismatch: tr\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDir}\tr\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\tr"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\vcruntime140_cor3.dll") != "d5e4d9a3e835fa679450145d6a7d94e36573a509317111904d9b3712c30d9066"
  #error "Accepted payload mismatch: vcruntime140_cor3.dll"
#endif
Source: "{#MyAppSourceDir}\vcruntime140_cor3.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\wpfgfx_cor3.dll") != "098d9cefaca7b1e36460eb28f2725d7afdb8e93855c29f8dd3627a1fb692ae8e"
  #error "Accepted payload mismatch: wpfgfx_cor3.dll"
#endif
Source: "{#MyAppSourceDir}\wpfgfx_cor3.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hans\Microsoft.VisualBasic.Forms.resources.dll") != "7f8b977f32dee7b58fbf6511c52a1825eb541b5007ba98c5105f13291bd7f89d"
  #error "Accepted payload mismatch: zh-Hans\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hans\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hans\PresentationCore.resources.dll") != "a689374665c43ade4442975527a31d97dc24fac93e7964d319171cfd51c7f82d"
  #error "Accepted payload mismatch: zh-Hans\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hans\PresentationCore.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hans\PresentationFramework.resources.dll") != "77cd49ef4764421286087d96a59850bb0bec199a1d86e08311cbce138a1d310f"
  #error "Accepted payload mismatch: zh-Hans\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hans\PresentationFramework.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hans\PresentationUI.resources.dll") != "01c26ec7603b507db1cf963f8e43a0b679bc3555bf56e8ebe570e50afae68ee6"
  #error "Accepted payload mismatch: zh-Hans\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hans\PresentationUI.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hans\ReachFramework.resources.dll") != "3abfe8d211cbc09bb83ec2846a90a7c02ce1ec8112724db21710c041853969a6"
  #error "Accepted payload mismatch: zh-Hans\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hans\ReachFramework.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hans\System.Windows.Controls.Ribbon.resources.dll") != "aa1e3fc7f4349a50fecf2cd8f9673f13de10b21301d4034e8379afa7c287fe49"
  #error "Accepted payload mismatch: zh-Hans\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hans\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hans\System.Windows.Forms.Design.resources.dll") != "532fd6822c22242213b37231d75d067c1eee491aa25fcd60b3c4bb03fbefea0a"
  #error "Accepted payload mismatch: zh-Hans\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hans\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hans\System.Windows.Forms.Primitives.resources.dll") != "d4a1f385a5b9f8986741523a6b70a6f422e71237d578267a76461a6db877e9c2"
  #error "Accepted payload mismatch: zh-Hans\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hans\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hans\System.Windows.Forms.resources.dll") != "10f58f34f5cdb264da4581911f2442944867e8f2d65a62c897161656bca5293d"
  #error "Accepted payload mismatch: zh-Hans\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hans\System.Windows.Forms.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hans\System.Windows.Input.Manipulations.resources.dll") != "ed3c96dc8732dbd0f71b55337d121857f2db16895637e4029f81388337d11ceb"
  #error "Accepted payload mismatch: zh-Hans\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hans\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hans\System.Xaml.resources.dll") != "379ba1ee671e0c26116feebc674e3275d8765d997624fea49a7b613a55a95e0c"
  #error "Accepted payload mismatch: zh-Hans\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hans\System.Xaml.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hans\UIAutomationClient.resources.dll") != "f4349da13b7c252315532265ea46c15b19efe78a6a1678d7ff7de19b80439701"
  #error "Accepted payload mismatch: zh-Hans\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hans\UIAutomationClient.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hans\UIAutomationClientSideProviders.resources.dll") != "ad83b784e4f587fcfe254855bd86d1f2414f37dce5b313db9b53a91cc7a75190"
  #error "Accepted payload mismatch: zh-Hans\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hans\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hans\UIAutomationProvider.resources.dll") != "b272d82c9563f3c0289daea81005f8b58b735fe15cfe65d1980c7fa3a4b2ed92"
  #error "Accepted payload mismatch: zh-Hans\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hans\UIAutomationProvider.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hans\UIAutomationTypes.resources.dll") != "6247fddbd366d2f6a29a46b3749962a6c2b7f70eb87dca5fb8e0eeba38d7d0bd"
  #error "Accepted payload mismatch: zh-Hans\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hans\UIAutomationTypes.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hans\WindowsBase.resources.dll") != "a01bde3f587330f035198b0d62e9814bcf86b2849b7427cc7f1536b86579a825"
  #error "Accepted payload mismatch: zh-Hans\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hans\WindowsBase.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hans\WindowsFormsIntegration.resources.dll") != "bc4a92bb4f154ee3f298d4b85d0e9101174a763b90dc3f16ea1d9691baba09bb"
  #error "Accepted payload mismatch: zh-Hans\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hans\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hant\Microsoft.VisualBasic.Forms.resources.dll") != "7a1f90e2f736c5d2e1483e6d1456c67e4b978a2b798ad828354d89e6046b60cd"
  #error "Accepted payload mismatch: zh-Hant\Microsoft.VisualBasic.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hant\Microsoft.VisualBasic.Forms.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hant\PresentationCore.resources.dll") != "31c835f8397681ef641cb576ab87110b0b43beb74199560539b0856a1e4663a9"
  #error "Accepted payload mismatch: zh-Hant\PresentationCore.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hant\PresentationCore.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hant\PresentationFramework.resources.dll") != "b011e20f3e0ed4cddf3da97bda40d8d5def857fcee3a3ef90c21992043614e0e"
  #error "Accepted payload mismatch: zh-Hant\PresentationFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hant\PresentationFramework.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hant\PresentationUI.resources.dll") != "bf50ac2f8b4b8a2cf753752635e6a1efa7ed73f702326622a971d6576863a921"
  #error "Accepted payload mismatch: zh-Hant\PresentationUI.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hant\PresentationUI.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hant\ReachFramework.resources.dll") != "0c08dc4af298acaa14532c74cfdf79ff5d25ea1f6f1ca7ec62de2dc19e7a3c9b"
  #error "Accepted payload mismatch: zh-Hant\ReachFramework.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hant\ReachFramework.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hant\System.Windows.Controls.Ribbon.resources.dll") != "7330d055942f84ad2682f2dc7f81853a4bb31d57ec03be747b6abb279fc99481"
  #error "Accepted payload mismatch: zh-Hant\System.Windows.Controls.Ribbon.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hant\System.Windows.Controls.Ribbon.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hant\System.Windows.Forms.Design.resources.dll") != "02f8729f491eb1b42496e7b638d016fce3428874aca7e910fd82c0765cc66328"
  #error "Accepted payload mismatch: zh-Hant\System.Windows.Forms.Design.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hant\System.Windows.Forms.Design.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hant\System.Windows.Forms.Primitives.resources.dll") != "08d9179525ffc58ca6b2da4298d3bc38c856a2197b6481484eef412cf2d6fcfb"
  #error "Accepted payload mismatch: zh-Hant\System.Windows.Forms.Primitives.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hant\System.Windows.Forms.Primitives.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hant\System.Windows.Forms.resources.dll") != "9080d4e1cf078b633b553d1ab871236d46c3c52875ee737dafdef98d9b8eb9d6"
  #error "Accepted payload mismatch: zh-Hant\System.Windows.Forms.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hant\System.Windows.Forms.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hant\System.Windows.Input.Manipulations.resources.dll") != "c4fc442b43986cd1f77b3989b0ba176d4cd73340c8be1ed5ca29442cb9254900"
  #error "Accepted payload mismatch: zh-Hant\System.Windows.Input.Manipulations.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hant\System.Windows.Input.Manipulations.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hant\System.Xaml.resources.dll") != "e56c2fe1b29e9d2c8d484d3d92af646908eca38cc980a7b8bd95ef1697f4419c"
  #error "Accepted payload mismatch: zh-Hant\System.Xaml.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hant\System.Xaml.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hant\UIAutomationClient.resources.dll") != "06b2b3aee4999dd82ab9e12a63bd2cd380038bee9f58b4f5129304b77d4cae13"
  #error "Accepted payload mismatch: zh-Hant\UIAutomationClient.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hant\UIAutomationClient.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hant\UIAutomationClientSideProviders.resources.dll") != "85c262c118f23ae66cf1c3b2beaff4ed5f7895257e332db8467630d7a905cda0"
  #error "Accepted payload mismatch: zh-Hant\UIAutomationClientSideProviders.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hant\UIAutomationClientSideProviders.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hant\UIAutomationProvider.resources.dll") != "c2b90ca715f4d9b560a2b74d4c376744bf981a4dea12f1a146ae5133e12eeeff"
  #error "Accepted payload mismatch: zh-Hant\UIAutomationProvider.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hant\UIAutomationProvider.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hant\UIAutomationTypes.resources.dll") != "37a7e838559d8cc37cdfdd836bb9cd7291027d303cfea76a3f9f9549e877c1c5"
  #error "Accepted payload mismatch: zh-Hant\UIAutomationTypes.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hant\UIAutomationTypes.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hant\WindowsBase.resources.dll") != "ced20a2672d9a0416c222ce89ed3cf3c51a7f9d0f7a784eb3a9eb24d83d4f487"
  #error "Accepted payload mismatch: zh-Hant\WindowsBase.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hant\WindowsBase.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite
#if GetSHA256OfFile(MyAppSourceDir + "\zh-Hant\WindowsFormsIntegration.resources.dll") != "c54544abd8039d7d59bc66f26381bacb8af2e75cb12e774cb096da594fd7b149"
  #error "Accepted payload mismatch: zh-Hant\WindowsFormsIntegration.resources.dll"
#endif
Source: "{#MyAppSourceDir}\zh-Hant\WindowsFormsIntegration.resources.dll"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion; Check: CheckPayloadWrite
