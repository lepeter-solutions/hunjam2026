# Quest build és ellenőrzés

## Rögzített környezet
- Unity 6000.3.28f1, Android Build Support + SDK/NDK + OpenJDK modulok.
- OpenXR 1.16.1, XR Management 4.4.0, Input System 1.20.1.
- Android: OpenXR loader, Meta Quest Support és Oculus Touch Controller Profile; Initialize XR on Startup.
- ARM64, IL2CPP, Vulkan, Linear, minimum Android API 29; target SDK az installált SDK szerint automatikus. Ez fejlesztői APK-konfiguráció, nem Store-leadás.
- Single Pass Instanced; mobil URP profil, HDR kikapcsolva, 4× MSAA.
- Standalone OpenXR profil is beállítva Quest Linkhez; desktop fallback headset nélkül.

## Build
1. A Hubban ellenőrizd az Android modulokat az adott Editor mellett.
2. Nyisd meg a DemoWorld scene-t. Project Settings > XR Plug-in Management > Project Validation: az Android cél hibáit javítsd, és dokumentáld a változtatást.
3. File > Build Profiles > Android > Switch Platform.
4. `HunJam/Build Quest APK`: `Builds/Quest/HunJam.apk` fejlesztői APK-t készít. A menü alkalmazza a fenti közös beállításokat.
5. Fejlesztői módú Quest USB-kapcsolattal: engedélyezd a headseten az USB debuggingot. `adb devices`, majd `adb install -r <APK útvonala>`. Indítás az Unknown Sources alkalmazáslistából.

## Kézi elfogadási lista
- Hidegindításkor valódi sztereó kép és megfelelő padlómagasság.
- Bal/jobb kontroller követése, grip, trigger, A gomb és thumbstick működik.
- Bal stickkel körbejárás, 30° fordulás, zöld padlócélra teleportálás; akadályba nem teleportál.
- Robot indítja a kört; három gömb megfogható, elengedhető, kézbesíthető; pontszám és újrakezdés helyes.
- Követésvesztés/fókuszvesztés után nincs kézhez ragadt tárgy; alkalmazás-visszatérés működik.
- Headsetenként mérd a képkockaidőt. 72 Hz esetén 13,9 ms a teljes képkockakeret; a stabil teljesítmény igazolása profileres mérés.
- A ténylegesen tesztelt modell, build és eredmény kerüljön a CHANGELOG-ba.

## Automatikus ellenőrzések
Unity Test Runner > EditMode > HunJam.Tests, illetve PlayMode > HunJam.PlayModeTests. Scene ellenőrzés: `HunJam/Validate Demo` (mentési lehetőség után megnyitja a DemoWorld scene-t). A batch ellenőrzést külön projektmásolatban futtatjuk, ha a fejlesztői projekt már nyitva van.

Parancssori ellenőrzés az adott Unity.exe-vel: `-batchmode -projectPath <másolat> -runTests -testPlatform EditMode -assemblyNames HunJam.Tests -testResults <eredmény.xml> -logFile <napló>`. PlayMode esetén a platform `PlayMode`, az assembly `HunJam.PlayModeTests`. A tesztparancshoz ne add meg a `-quit` kapcsolót; a tesztfuttató lép ki.

Android build: `-batchmode -quit -projectPath <másolat> -buildTarget Android -executeMethod HunJam.Editor.DemoProjectTools.BuildQuest -logFile <napló>`. Grafikus batch környezetben `Builds/DemoWorld-preview.png` előnézetet is készít; ezt nem menti bele a scene-be.

## Jelenlegi validációs figyelmeztetések
Az OpenXR opcionális PoseControl/StickControl és input polling ajánlásokat jelez. Ezek csomagszintű beállítások; változtatásuk után újra kell ellenőrizni a kontrollerbemeneteket. Az SSAO-ajánlás az összes konfigurált render pipeline-t, így a sablon PC-profilt is vizsgálja. A Questhez rendelt `Mobile_Renderer` feature-listája üres, azon nincs SSAO. A figyelmeztetések nem helyettesítik a tényleges Quest-profilozást.

## Források
- [Unity: Meta Quest fejlesztési folyamat](https://docs.unity.com/en-us/engine/6000.0/manual/xr/configuring-project-for/meta-quest-develop)
- [Meta: XR Plugin Management](https://developers.meta.com/vr/documentation/unity/unity-xr-plugin/)

Az aktuális gépi ellenőrzések eredménye és a még szükséges hardvertesztek a [változásnaplóban](CHANGELOG.md) szerepelnek.
