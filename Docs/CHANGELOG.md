# Változásnapló

## 2026-11-06 — Közös fejlesztési alap és Quest demó
- Kötelező dokumentációolvasás, mechanikánkénti leírás, változásnapló és commit az AGENTS.md-ben.
- Saját játékstruktúra, szerkeszthető demóvilág, robot, mozgás, tárgyfogás és háromgömbös játékkör.
- OpenXR/Quest konfiguráció, desktop tesztvezérlés és reprodukálható Editor-eszközök.
- Ellenőrzés Unity 6000.3.28f1 alatt, elkülönített projektmásolatban: C# fordítás sikeres; DemoWorld scriptreferenciái és három gömbje rendben; 3/3 EditMode és 2/2 PlayMode teszt sikeres.
- PlayMode tesztek: scene-betöltés, robot által indított kör, kizárólagos tárgyfogás, kézben tartott gömb pontozásának tiltása, tényleges fizikai triggeres gyűjtés, teljesítés és újraindítás; nyílt padlóra teleportálás és akadálycél elutasítása.
- Quest APK: külön buildellenőrzés folyamatban. Az `adb devices` listája üres; headsetes kép, kontrollerbemenetek, komfort és képkockaidő még nem validált.

### Képi ellenőrzés és Unity-import
- A batch előnézet valódi, szinkron lefordított shaderekkel elkészült; a színek, geometria és világfeliratok ellenőrizve. Az előnézet a README-ben és a demóvilág leírásában elérhető.
- Rögzítve az Editor által létrehozott OpenXR Editor Settings asset, a mobil URP asset verziófrissítése és a ShaderGraph alapértelmezett sémamezője. Ezeket a Unity csomagimportja hozta létre.
- A Quest útmutatóban szerepelnek az OpenXR opcionális bemeneti ajánlásai és az SSAO-figyelmeztetés oka; a mobil rendererben nincs SSAO feature.
