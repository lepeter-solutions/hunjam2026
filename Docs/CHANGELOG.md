# Változásnapló

## 2026-11-06 — Közös fejlesztési alap és Quest demó
- Kötelező dokumentációolvasás, mechanikánkénti leírás, változásnapló és commit az AGENTS.md-ben.
- Saját játékstruktúra, szerkeszthető demóvilág, robot, mozgás, tárgyfogás és háromgömbös játékkör.
- OpenXR/Quest konfiguráció, desktop tesztvezérlés és reprodukálható Editor-eszközök.
- Ellenőrzés Unity 6000.3.28f1 alatt, elkülönített projektmásolatban: C# fordítás sikeres; DemoWorld scriptreferenciái és három gömbje rendben; 3/3 EditMode és 2/2 PlayMode teszt sikeres.
- PlayMode tesztek: scene-betöltés, robot által indított kör, kizárólagos tárgyfogás, kézben tartott gömb pontozásának tiltása, tényleges fizikai triggeres gyűjtés, teljesítés és újraindítás; nyílt padlóra teleportálás és akadálycél elutasítása.
- Quest APK: sikeres Android/ARM64/IL2CPP fejlesztői build. Az `adb devices` listája üres; headsetes kép, kontrollerbemenetek, komfort és képkockaidő még nem validált.

### Képi ellenőrzés és Unity-import
- A batch előnézet valódi, szinkron lefordított shaderekkel elkészült; a színek, geometria és világfeliratok ellenőrizve. Az előnézet a README-ben és a demóvilág leírásában elérhető.
- Rögzítve az Editor által létrehozott OpenXR Editor Settings asset, a mobil URP asset verziófrissítése és a ShaderGraph alapértelmezett sémamezője. Ezeket a Unity csomagimportja hozta létre.
- A Quest útmutatóban szerepelnek az OpenXR opcionális bemeneti ajánlásai és az SSAO-figyelmeztetés oka; a mobil rendererben nincs SSAO feature.

### Android-build lezárása
- Unity builderedmény: `Success`; APK: `My project/Builds/Quest/HunJam.apk` (50 999 617 bájt). Helyi buildtermék, a Git szándékosan kizárja.
- APK-alapadatok ellenőrizve az Android SDK aapt eszközével: `com.lepetersolutions.hunjam2026`, verzió `0.1.0` / kód `1`, minimum API 29, target API 36, `arm64-v8a`, UnityPlayerGameActivity.
- SHA-256: `02857d4093f0de42ac17fcf9788929a9e1b740782a4a9372f174e1bd5d707848`.
- Helyi bizonyítékok: `My project/Builds/Validation/android-build.log`, `editmode-results.xml`, `playmode-results.xml`. Ezek szintén kizárt buildtermékek.
- Következő elfogadási lépés: APK telepítése egy valós Questre, majd a QUEST_SETUP.md eszköztesztlistájának végrehajtása. Ezt hardver nélkül nem tekintjük teljesítettnek.
