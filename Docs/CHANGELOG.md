# Változásnapló

## 2026-11-07 — InteractionSandbox scene és fizikai építőelemek
- Új, önálló InteractionSandbox.unity scene a közös Meta QuestPlayer riggel, kontrollervezérelt kezekkel és a meglévő robot-, energia-, kalapács-, penge-, lámpa- és resetdemóval.
- Hátulsó fizikai munkapad, négy megfogható/rakható kocka és két megfogható/dobható/gurítható labda. Új PhysicsCube és PhysicsBall prefabok, kész Meta SDK komponensekkel, Rigidbodyval és szilárd colliderrel; új C# kód nélkül.
- Külön scene-leírás, prefabparaméterek és bővítési útmutató. A korábbi scene-ek megmaradnak.
- A felhasználó kérésére nem futott Unity-import, fordítás, build vagy teszt. Futás közbeni és headsetes működés nincs ellenőrizve; a scene és a hozzá tartozó assetek közvetlenül kerülnek commitba és GitHub mainre.

## 2026-11-07 — Meta XR SDK átállás
- Meta XR Core, Interaction SDK és OVR Integration 207.0.1. A QuestPlayer és DemoWorld a hivatalos OVRCameraRig / OVRComprehensiveInteractionRig prefabokat használja. A saját követés, kézmodell, ujjanimáció és fogásmozgatás eltávolítva.
- Kizárólag fizikai mozgás; SDK locomotion, távoli ray és distance grab letiltva. Grip fogás, trigger használat; a játékspecifikus robot, energiakör, kalapács, penge és lámpa megmarad.
- GameItem és MetaUseAction adapterek kötik a játékot az SDK-hoz. A tárgyak visszaállítása inaktív állapotban is visszaírja a Transformot. Az SDK mozgatásával ütköző Rigidbody interpolation kikapcsolva.
- A csapat friss main commitjai beépítve, új pályák és importált minták megőrizve. Android minimum API 32; SDK helyi debuggerkonfiguráció kizárva a verziókövetésből.
- Előzetes Unity-fordítás és scene-validáció sikeres. A korábbi teljes PlayMode futás 2/5 volt; a felismert tesztpozicionálási és reseteltérések javítása után a célzott SDK-kalapácsteszt 1/1 sikeres. A végleges, egyesített projekt teljes ellenőrzése nincs lezárva: a felhasználó kérésére az importot és a hátralévő teszteket leállítottuk, további fordítás nélkül kerül Gitre.
- Új APK és valódi headsetes ellenőrzés nem készült. A robot-újrakezdés és penge teljes regressziós újrafuttatása még hátravan.

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

## 2026-11-06 — Fizikai mozgás és kézből használható műhely
- Kizárólag room-scale követés: joystickmozgás, snap turn, teleport, WASD és CharacterController eltávolítva. Az AGENTS.md rögzíti ezt a követelményt.
- Két kontrollerrel követett egyszerű alapkéz; 12 cm-es közeli fogás és interakció, távoli sugár nélkül. Közös Grabbable alap az energiagömbökhöz és eszközökhöz; fogva is aktív ütközések, elengedési sebesség, tracking-ugrás elleni védelem.
- Kalapács és fizikai lendítésre működő ütésszámláló; penge és két megfogható fizikai darabra bontható blokk; triggerrel kapcsolható lámpa; közeli RESET gomb.
- A mechanikák a kezdőpont körüli műhelybe kerültek. A scene, kéz- és eszközprefabok, előnézet, vezérlési útmutató és mechanikánkénti Markdown-leírások frissültek.
- Ellenőrzés Unity 6000.3.28f1 elkülönített másolatában: sikeres fordítás, scene-validáció, képi előnézet; 3/3 EditMode és 5/5 PlayMode teszt sikeres.
- PlayMode: teljes energiakör, kizárólagos fogás, közeli/távoli tárgykiválasztás, rögzített rig, lámpakapcsolás, eszközreset, mozgó penge tényleges kontaktussöprése, kalapácsütés és követési ugrás miatti elengedés.
- Helyi bizonyítékok: My project/Builds/Validation/roomscale-playmode-final.xml, roomscale-editmode.xml és a kapcsolódó naplók.
- Korlát: primitív alapkéz, előre meghatározott kétfelé vágás a penge kontaktpontján; nincs általános mesh-vágás, haptika vagy fizikai kéz. Kinematic eszköz statikus falba tolható.
- Valós Quest/Link-kontrolleres teszt és teljesítménymérés még szükséges. Ehhez a változathoz új APK nem készült; a korábbi helyi APK a régi demót tartalmazza. Unity Play + Quest Linkkel az új scene közvetlenül próbálható.
- A felhasználó kérésére az ellenőrzött módosítás a GitHub main ágára kerül, a korábbi demóalap commitjaival együtt.

## 2026-11-06 — Hivatalos Unity kézmodellek
- A primitív kezeket a Unity XR Hands 1.5.0 HandVisualizer LeftHand/RightHand csontozott modellje váltja, eredeti meta és licenc mellékelve az Art/UnityHands mappában.
- Touch kontrolleres követés; gripre ujjhajlítás, triggerre mutatóujj-hajlítás. A közeli fogás, használat, ütés és vágás ugyanazokhoz a kezekhez kapcsolódik. Nincs joystickos mozgás.
- A QuestPlayer prefab és a demógenerátor is az új modelleket használja. A teljes XR Hands runtime nem szükséges.
- Unity fordítás és scene-validáció sikeres; 5/5 PlayMode regressziós teszt sikeres. Kontrolleres póz és csuklóillesztés valódi headseten még ellenőrizendő; új APK nem készült.
