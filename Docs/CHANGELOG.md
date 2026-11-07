# Változásnapló

## 2026-11-07 — Süllyedési fizika és jobb oldali gázkar
- Fix sebességszabályozás helyett tolóerő, sebességnégyzetes légellenállás és állásszögfüggő, a sebességre merőleges felhajtóerő. Orrlenyomáskor csökkenő/negatív felhajtóerő engedi a tényleges süllyedést; nagy állásszögnél egyszerűsített felhajtóerő-csökkenés.
- TrainingAircraft jobb oldalán megfogható, ±35° végállású, elengedve állástartó gázkar, Meta Grabbable adapterrel. Tolóerő 0–100%, reset 70%; V/S és THR műszerkijelzés. A prefab és Editor-generátor is frissült, a meglévő scene-ek öröklik.
- A cockpit-kezelőszervek kimaradnak a szabad tárgyak Y<-3 resetjéből, így lejjebb helyezett pályán sem áll vissza magától a gázkar.
- Forrás/diff és prefab-referenciák áttekintve; felhasználói kérésre nincs fordítás, teszt vagy build. Repülési hangolás, kar elérhetősége és headsetes működés ellenőrizetlen.

## 2026-11-07 — Minden automatikus repülő-reset kikapcsolható
- Az előző változás csak az alsó magassági feltételt tiltotta. Most az `Automatic Reset Enabled` főkapcsoló alapból kikapcsolja az összes automatikus visszaállítást, az ütközéses ágat is.
- Külön kapcsoló és állítható küszöb az alsó/felső magassághoz, bank/pitch szöghöz, vízszintes távolsághoz és ütközési sebességhez. A kézi RESET / RECENTER független maradt.
- A ResetFlight hívási helyei és a diff áttekintve. Felhasználói kérésre nem futott fordítás, teszt vagy build; a határátlépések és ütközések headsetes próbája még nyitott.

## 2026-11-07 — Állítható és kikapcsolható alsó resetmagasság
- FlightVehicle: az eddigi fix Y<8 resetet Inspectorban kapcsolható `Reset Below Altitude` és szabadon, negatívra is állítható `Reset Altitude` váltja. Alapból kikapcsolt, hogy a lejjebb helyezett pályán elindulhasson a gép.
- A kézi reset és az egyéb automatikus resetfeltételek megmaradnak. A felhasználó scene- és assetmódosításait nem változtattuk meg.
- Forrás/diff áttekintve; a korábbi kérés szerint nincs fordítás, teszt vagy build. A negatív magasságú indulás és a kapcsoló/küszöb futásidejű próbája még nyitott.

## 2026-11-07 — Yoke fogási felület és mozgáshatárok
- Felhasználói hibajelzés: az előző rögzítési javítás után a yoke nem fogható. A yoke LateUpdate-je most a kéz lekérdezése előtt fut, és a fogáskeresés szinkronizálja a látható Transformokat a fizikával.
- A yoke kézkövetésiugrás-védelme cockpit-koordinátákban számol; a repülő haladása nem ad hamis elengedési parancsot. A normál tárgyak világkoordinátás védelme megmarad.
- A tényleges kormányállás is korlátozott: ±9 cm ki-be mozgás, ±40° fordítás. Gravitáció és szabad eldobás nincs, a kézvezérlés és visszacentírozás megmarad.
- Ellenőrzés: forrás és diff áttekintése. Felhasználói kérésre nem futott fordítás, teszt vagy build; a fogás és a többperces repülés headsetes visszaellenőrzése nyitott.

## 2026-11-07 — Rögzített yoke és Manhattan integráció
- A yoke elengedéskor sem vált szabad fizikára. A cockpitből számított kötött pózt fizikában és a repülő interpolált megjelenítése után is felveszi; megmarad a kétkezes fogás, ±40° fordítás, ±9 cm eltolás és centírozás. Prefab és generátor gravitáció nélkül készül.
- A manhatten branch beolvasztása a codex/vr-two-hand-demos ágba; város, művészeti assetek és hangok átvétele, a meglévő kétkezes Meta runtime megtartásával. Részletes konfliktusfeloldási döntések: Mechanics/MANHATTAN_FLIGHT.md.
- TrainingAircraft prefab a Manhattan scene-ben (500, 120, -300) kezdőpózzal, pilóta riggel; a régi kamera inaktív, Manhattan az első build-scene.
- A helyi, eltérő Manhattan scene és editorállapot külön Git stashben és a repo melletti hunjam2026-local-before-flight-merge mappa ZIP-mentéseiben megőrizve, nem commitolva saját munkaként.
- Ellenőrzés: forrás- és YAML-áttekintés. Felhasználói kérésre nincs fordítás, Play mód, teszt vagy build; többperces repülés, városütközés, Quest követés és teljesítmény ellenőrizetlen.

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

## 2026-11-07 — Kétkezes fogás és közvetlen átadás
- A Grabbable kizárólagos fogását közös anchor–tárgy nyilvántartás, elsődleges/másodlagos kéz és explicit állapotok váltják. TransferOnly esetén grip-átfedés, TwoHands esetén kétkezes orientáció. Elengedéskor folyamatos újrabázisolás, tracking/tiltás/megsemmisülés utáni takarítás.
- DemoHand a közös tulajdonlást használja; az átadás nem ejti el a másik kéz tárgyát. Rövid grip-haptika, nyugodt alapujjtartás és tárgyfüggő zárás. A mozgásbemenetek változatlanok.
- Dokumentáció: Mechanics/GRABBING.md frissítve az API-val, állapotokkal és korlátokkal.
- Unity 6000.3.28f1, elkülönített TwoHandValidation másolat: fordítás sikeres; 9/9 PlayMode teszt sikeres (5 meglévő regresszió, 4 új fogáspróba). Helyi bizonyíték: My project/Builds/TwoHandValidation/grab-results.xml és grab-tests.log.
- Headsetes két szem/kontroller, kézpóz-illeszkedés, haptika, dobásérzet és teljesítmény még ellenőrizendő. A papír-, szúrás- és repülésbővítés következő munkalépés.

## 2026-11-07 — Papírtépés, szúrás és repülőgépes demók
- Futásidejű síkbeli mesh-felosztás UV-megőrzéssel, külön collideres és Rigidbody-s, kézben maradó papírdarabokkal; feszülési visszajelzés, hang, haptika és reset.
- Hegyirány-, sebesség- és célfelület-függő szúrás, korlátozott behatolás és kihúzás; egy- vagy kétkezes, korlátozott yoke és valóban mozgó Rigidbody-repülő, üléskalibrációval és újraindítással.
- A meglévő DemoWorld új asztalt kapott. Új VRInteractionTest és FlightYoke scene, hat tárgyprefab, scene-választó gombok, dokumentáció és korábbi állapotot bemutató képek.
- A felhasználó build-/teszttiltása ELŐTT elkülönített másolatban: 10/10 EditMode és 17/17 PlayMode teszt sikeres; három scene statikus előnézete elkészült. Bizonyíték: Builds/TwoHandValidation/final-EditMode.xml és release-PlayMode.xml. Ezek a Meta SDK-ra átállás ELŐTTI változat eredményei.
- A későbbi felhasználói kérés leállítja a buildelést és tesztelést, és Meta XR integrációra módosítja a következő munkalépést. A korábbi eredmények nem igazolják azt. Headsetes ellenőrzés nincs; a jelenlegi Editor-telepítésben nincs Android buildmodul.

## 2026-11-07 — Meta XR, natív kézmodell és Grabbable
- Felhasználói irányváltás: Meta XR Core + Interaction SDK 207.0.1 a hivatalos Meta registryből. A Unity Package Manager letöltötte a csomagokat és frissítette a packages-lock.json fájlt.
- QuestPlayer: Meta OVRCameraRig, Capsense Natural, OVRHandPrefab bal/jobb változat, OpenXR kézcsontváz és URP kézanyag. Az eredeti riggyökér és room-scale mozgás megmarad; a Meta szem-/kontrolleranchorok váltják a korábbi pózforrást. Nincs locomotion prefab.
- HunJam.Grabbable a Meta Grabbable leszármazottja, natív pointer/transformer életciklussal. A saját közelségi inputadapter, átadási állapotok, korlátozott dobás, szúrás és yoke az ITransformer bővítési ponton maradnak; nincs második Rigidbody-mozgató. A papírdarabok is ezt öröklik.
- Meta OpenXR / kézkonfiguráló Editor-menü, szükséges assembly-hivatkozások, dokumentáció és META_XR.md útmutató. A régi kézassetek GUID-ja megmarad; Meta módban nem aktívak. Az átállás után a konfiguráló menü kézzel futtatható, nem írja át a beállításokat minden újratöltéskor.
- A Unity-import lezárult; Android és Standalone Meta XR Feature engedélyezett, Controllers And Hands és OpenXR csontváz mentve. Az SDK által készített közös Oculus/Resources konfigurációk, AndroidManifest és UI-tagek bekerülnek. A géphez kötött fejlesztői/publikálási SDK-beállítások gitignore alatt maradnak. Meta 207 miatt minimum Android API 32; az SDK az install location értékét Auto-ra állítja.
- Áttekintve: hivatalos forrás/API, fájldiff, csomagfeloldás és importüzenetek. A konzol az AndroidPlayer modul hiányát jelzi. A felhasználó tiltása miatt nem indult új build, Test Runner vagy Play mód; headsetes működési siker nincs állítva. A Meta előtti teszt- és képi eredmények ettől elkülönítve szerepelnek.
- Nyitott: Quest/Link kéz- és markolatilleszkedés, kétkezes mechanikák, scene-váltás, repülési komfort és standalone teljesítmény. A Capsense nem tárgyfüggő IK; általános kinematic fogásnál továbbra is lehetséges falba tolás.
