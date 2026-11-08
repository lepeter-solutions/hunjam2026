# Változásnapló

## 2026-11-08 — Döntési fa egyeztetése a story charttal
- A Vérszerződés chartjának megfelelően az aláírás D1 True, a tépés False kimenetet rögzít; a Trianon D2 értékei változatlanul aláírás False, tépés True.
- A Manhattan D4 4. branch-sorának False panorámája `pici_nemet.png` helyett `pici_rom.png`. A 8. sor `no_rom` kimenete a felhasználó kérésére `no_hun.png`-t használja.
- A döntési fa útmutató és VR átvezetési leírás frissítve a konkrét képfájlokkal és az új bool-bekötéssel.
- Ellenőrzés: scene UnityEventek, StoryFlow GUID-ok, branch-sorrend és Build Settings statikusan összevetve. Unity, fordítás, Play mód és headsetes próba nem futott; valós működés headseten még ellenőrizendő.

## 2026-11-08 — Térképek, döntési kilépők és Hitler befejezések
- A térképfelület megőrzi a képarányt, az első követett nézethez igazodik és utána a szobában rögzül. Manhattan után az EndScene ugyanazt a választott térképet tartja láthatóan.
- A Hitler-jelenet fegyvere távolság szerint vizsgálja a találatokat, figyelmen kívül hagyja a saját fegyver/kéz interakciós triggereit, és a falat lövést blokkoló akadályként kezeli. A ciánkapszula a követett játékos szájánál egyszer aktiválja az önhalál döntést.
- Mind a négy story stage döntésszáma, exitje, eseményértéke és képágainak referenciái statikusan ellenőrizve. Unity, fordítás és headsetteszt nem futott; az eszközös ellenőrzés nyitott.

## 2026-11-08 — Döntési fa: példa és kérdések
- Külön Inspector-útmutató készült a Vérszerződés–Hitler példával, az ágankénti képekkel, eseménybekötéssel és C# alternatívával. A függvény- és eseményneveket összevetettük a projekttel. Csak dokumentáció változott.

## 2026-11-08 — Képenként beköthető döntési ágak
- A Branches minden előzményéhez saját False Panorama és True Panorama húzható; üresen a pálya meglévő A/B képei maradnak. Konkrét Vérszerződés–Hitler Inspector-példa a mechanikaleírásban. Az eseménynevek és az ágválasztás statikusan áttekintve; Unity nem futott.

## 2026-11-08 — Előzményfüggő kétágú döntési fa
- Pályánként továbbra is csak A és B kép rendelhető hozzá. Az előző döntések választják a csomópontot, az aktuális döntés annak két ágából választ. Az 1/2/4/8 csomópont mintakiosztását és mind a 16 teljes történetet statikusan ellenőriztük; Unity-próba nem futott.

## 2026-11-08 — Két átvezetőkép pályánként
- A négy pálya saját 0/1 döntése választ két kép közül; kizárólag az EndScene használja a 16 összesített végképet. A konfiguráció átvezetve, a pályasorrend változatlan. Statikus ellenőrzés; Unity és headsetes próba nem futott.

## 2026-11-08 — VR történetátvezetés és EndScene
- Rögzített pályasorrend, négy döntés, pályánkénti 16 képes konfiguráció és UnityEvent-adapter. Az eseményeket a pályák készítője köti be.
- Egy fehér panorámajelenet, teljes nézetű fehér áttűnés, aszinkron betöltés, az utolsó képhez tartós EndScene. Két új scene, közös unlit anyag és 16 cserélhető PNG-minta. Build-lista és dokumentáció frissítve.
- Statikus ellenőrzés; fordítás és headsetes validáció nem történt. Részletek: Mechanics/VR_STORY_TRANSITIONS.md.

## 2026-11-08 — Manhattan víz, távoli part és köd
- A kilenc eredeti vízcsempe újra aktív, a generált vízkorong kikerült a jelenetből. A korábbi Exp2 ködsűrűség ismét 0,006.
- A part és az épületsziluett együtt 4500 méterrel hátrébb került (+Z), a scene látótávja 9000 m. A generátor ezt az elrendezést tartja meg.
- A scene-diff és a korábbi víz- és ködbeállítások áttekintve. Build és teszt nem futott; a cockpitből látható horizont és a Quest teljesítmény még ellenőrizendő.

## 2026-11-08 — Helyi javítások és Manhattan környezet publikálása
- Felhasználói kérésre a tinta/tollfogás, cockpit indulás, yoke-kézrögzítés, köd és távoli környezet javításai commitba kerülnek mainre. A korábbi „nem commitolt” bejegyzések az akkori munkafázist jelölik.
- Origin/main frissítve; nem érkezett új távoli commit. A felhasználó külön helyi cockpit-pozíció/sebesség-hangolása és projektbeállításai nem kerülnek ebbe a javításcommitba, a munkapéldányban megmaradnak. Git-diff ellenőrizve; nincs fordítás vagy futtatott teszt.

## 2026-11-08 — Manhattan kikötői környezet és távoli táj (nem commitolt)
- Új Manhattan Surroundings scene-gyökér: összefüggő víz, szabálytalan partvidék/sziget, alacsony domborzat és 301 távoli épület. 18 statikus mesh, összesen 42 148 háromszög; a külső szárazföld is szabálytalanul fut a vízbe.
- Két könnyű, stereo-kompatibilis URP shader távolsági párával, napfénnyel és vízcsillanással. A korábbi kilenc vízblokk inaktívan megmaradt; skybox és gameplay-köd változatlan. Scene-szintű, kameraátadást kezelő 6000 m látótáv.
- Reprodukálható Python assetgenerátor, saját geometriával, külső letöltés/csomag nélkül. Más helyi változtatások megőrizve. Forrás és diff áttekintve; kérésre nincs fordítás, teszt vagy Unity-indítás. Headsetes látvány és teljesítmény még ellenőrizendő.

## 2026-11-08 — Első cockpit spawn, yoke-kézrögzítés és Trianon tollfogás (nem commitolt)
- A VR ülésirány csak friss Meta anchorfrissítésből, 0,25 s érvényes követés után kalibrálható. Tracking-origin változás és recenter újrakalibrál; az első indításkor túl korán használt kamera/póz útvonala lezárva. PilotPivot forgatása változatlan.
- A meglévő Meta kézmodell yoke-fogás közben a kormányhoz rögzül; a valódi kontrolleranchor és az irányítás változatlan. Elengedve visszatér a követéshez.
- Trianon tollán visszaállítva a Vérszerződés natív Meta Grabbable komponense és mindkét eredeti kézpózága. A papíradapter és az SDK kölcsönös kiválasztásszűrése megakadályozza a toll és papír egy kézzel egyidejű fogását. A korábbi tintajavítás megmaradt.
- Forrás, telepített SDK API-k és scene-diff áttekintve. Kérésre nincs fordítás, teszt vagy Unity-indítás; headsetes működés/látvány ellenőrizendő. Minden módosítás commitolatlan, más helyi munkák megőrizve.

## 2026-11-08 — Manhattan ködsűrűség felezése
- Fog Density 0,006 → 0,003 a Manhattan jelenetben. A többi helyi scene-módosítás megmaradt. Szerializált érték áttekintve; nincs fordítás vagy teszt. A változtatás commitolatlan.

## 2026-11-08 — Trianon közvetlen tinta láthatóságjavítása (nem commitolt)
- A helyi LineRenderer külön, explicit scene-referenciás PaperStrokeInk anyagot használ: URP Unlit, opaque, kétoldalas, papír után rajzolva, megtartott mélységvizsgálattal. A korábbi tintakamera anyagának újrafelhasználása megszűnik ebben a scene-ben.
- Trianon vonalvastagság 2 → 3 mm; a régi scene-ek TrailRenderer útvonala változatlan. Forrás/diff áttekintve, fordítás/teszt nem futott. Headsetes eredmény még ellenőrizendő; kérésre nincs commit/push.

## 2026-11-08 — Papírral együtt mozgó, tollhegyhez igazított írás
- Trianonban a tinta papírhoz szülőzött, helyi koordinátás LineRenderer-vonásokra váltott a meglévő tintaanyaggal. A kész és aktív vonások a lap mozgatását/forgatását követik.
- A tényleges tollhegy felületre vetített pontja hajtja a rajzot és a fedettséget. A hibás képarányú tintakamera/vetítési réteg helyett közvetlen rajzolás, 0,5 mm mélységi emeléssel; felemelés és lapszél megszakítja a vonást.
- A nem téphető Vérszerződés-papír korábbi útvonala megmaradt. Forrás/diff és koordinátaútvonal áttekintve; teszt, Play mód, játékbuild és headsetes ellenőrzés nem futott. Kétkezes lapmozgatás, sarokpont-illesztés és két szem képe eszközön ellenőrizendő.

## 2026-11-08 — Trianon darabok kézben tartása és írási döntés
- A grip külön felvételi/elengedési küszöböt kapott (65%/10%); új tárgy kizárólag új szorításkor választható. A kézben tartott tollhoz közelített papír nem fogható fel ugyanazzal a szorítással.
- A tépés menti a tényleges kezeket és oldalaikat; a szülő Meta takarítása a darabok kiválasztása előtt befejeződik. A darabok tulajdonosa a kéz marad a felengedésig.
- A toll–lap szilárd ütközés kikapcsolva a Trianon adapter indulásakor, az írótriggerek megmaradnak. Az első tintanyom lezárja a tépést, a megkezdett tépés lezárja az írást.
- Forrás/diff áttekintve; Unity 6000.3.28f1 C# fordítás sikeres. Play mód, teszt, játékbuild és headsetes ellenőrzés nem futott. Két kéz külön elengedése, grip-lazítás, írás közbeni laphelyzet és a két döntés kizárása eszközön ellenőrizendő.

## 2026-11-08 — Trianon tinta és többszöri tépés javítása
- Trianonban a tál feltöltve indul, a toll tintával előkészített; a többi scene továbbra is mártást igényel. Az írásrács a tényleges XY érzékelőfelületet használja a régi mesh helyett.
- Toll és papír közös HunJam/Meta Grabbable fogásnyilvántartásba került; a toll párhuzamos scene-interaktorai kikapcsolva. Egy grip egy legközelebbi, akadály nélkül elérhető tárgyat választ.
- A keletkező papírdarabok is kereshetők, és a nyilvántartás ténylegesen kézben tartott darabját engedi el az adapter. Újrafogás és további tépés a meglévő mérethatárokig támogatott.
- Forrás és scene-hivatkozások áttekintve; Unity 6000.3.28f1 C# fordítás sikeres. Play mód, teszt, játékbuild és headsetes ellenőrzés nem futott. A fogás, tintanyom, többgenerációs tépés és komfort headseten visszaellenőrizendő.

## 2026-11-07 — Trianon szerződéses választás
- A Trianon scene a Vérszerződés VR játékosát, asztali környezetét, tollát és tintás tálját kapta; az eredeti írófelület a DemoWorld téphető szerződéséhez kapcsolódik.
- A Touch grip a meglévő Grabbable papírt fogja. Az aláírás teljesítése kizárja a tépést, a megkezdett szakadás kizárja az aláírás teljesítését.
- Editor scene-mentés és C# fordítás történt. Play mód, teszt, Android build és headsetes ellenőrzés nem futott; a két interakció és a VR élmény valódi eszközön ellenőrizendő.

## 2026-11-07 — Helyi Manhattan munka publikálása mainre
- Új felhasználói kérés alapján a korábban commitolatlan javítások és a felhasználó helyi scene/audio/beállítás módosításai mentve (79a16c2), majd a friss origin/main (f695a21) beolvasztva. A korábbi „nem commitolt” bejegyzések az akkori munkafázist jelölik.
- Animation mappa metaütközésénél a main GUID-ja marad (külső assethivatkozás nincs); ProBuilder beállítások azonos értékei egyszer szerepelnek, a main sorrendjében. Scene-ek automatikusan egyesítve.
- Git konfliktusok és változáslista áttekintve; kérésre nincs fordítás, teszt vagy build. VR-futásidejű ellenőrzések továbbra is nyitottak.

## 2026-11-07 — Yoke/gázkar fogás scene-reload után (nem commitolt)
- DemoHand tracking és grip Button actionök kezdeti állapotellenőrzése engedélyezve. Újratöltéskor a továbbra is követett kontrollerek állapota az első inputfrissítésnél beolvasódik, nem kell követésvesztés az új isTracked action aktiválásához.
- A trigger új lenyomást igényel; a fogási távolság és Meta transformer változatlan. Telepített Input System forrás és diff áttekintve; kérésre nincs fordítás vagy teszt. Többszörös headsetes reload és mindkét kéz ellenőrizendő. Nincs commit/push.

## 2026-11-07 — Teljes látómezős fade és cockpit iránykorrekció (nem commitolt)
- A négyzetként látszó UI-panel helyett URP clip-space overlay, stereo instancing támogatással és mélységtől független rajzolással. Alpha=1 már létrehozáskor; követésre várakozás, 0.5 s teljes fekete, majd a meglévő fadeDuration szerinti áttűnés. Game-over ugyanennek az overlaynek ad teljes feketét.
- A korábban hozzáadott PilotPivot +90° forgatás visszavonva a jobbra nézési hibajelentés alapján; Y=0°, a kameraátadás utáni kalibrálás megtartva.
- Forrás/diff áttekintés; nincs Unity-indítás, fordítás vagy teszt. Mindkét szem teljes lefedése, első képkocka, cockpit kezdőirány és ütközés utáni fekete headseten ellenőrizendő. Kérésre minden commitolatlan marad.

## 2026-11-07 — Ütközéses game-over és VR kezdőfade javítása (nem commitolt)
- A felhasználó helyi FlightVehicle/TerroristAi munkájának folytatása: a küldetéses ütközés különválasztva a kikapcsolt gyakorló-reset kapcsolóktól; aircraft felismerés komponens/ős alapján. Egyszeri game-over, startup fade leállítása, gépek megállítása, nullbiztos audio és aktív scene név szerinti újratöltése. Opcionálisan konfigurálható epilógus.
- Kezdés teljes feketével, követésre várakozó 10 másodperces alapfade; meglévő Canvas újrakötése a tényleges Meta szemkamerára. A halálkori fekete szín visszaíródik az Image-re.
- 39 üres MeshCollider azonosítva a scene-ben. Runtime statikus épületcolliderek a meshgyerekeken; enemy dinamikus Rigidbody/FixedUpdate sebesség és szükség esetén meshhatárokhoz illesztett primitív hitbox. Folytonos ütközésdetektálás a gépeken.
- Forrás és sceneadatok áttekintve; kérésre nincs fordítás, teszt vagy build. Headsetes fade, kontaktusok, colliderilleszkedés és runtime mesh-cooking teljesítmény ellenőrizetlen. A felhasználó kifejezetten kérte, hogy minden maradjon commitolatlan; nincs commit vagy push. Részletek: Mechanics/FLIGHT_MISSION.md.

## 2026-11-07 — Sikertelen grip újrapróbálása közelítéskor
- Main frissítve d2fc056-ra; a helyi ProBuilder-módosítás megőrizve. DemoWorld papírprefab, aktív collider, fogáskomponens és scene-override-ok áttekintve.
- DemoHand: a sikertelen grip nem vész el a lenyomás frame-jében; nyomva tartva 0,05 másodpercenként újrakeres. Sikeres fogás után ugyanaz a szorítás nem szerezhet új tárgyat átadás/reset után.
- A hatótáv, akadályvizsgálat és Meta Grabbable útvonal változatlan. Forrás/diff áttekintés; nincs build, fordítás vagy teszt. A felhasználói hibajelzés teljes headsetes reprodukciója és a javítás visszaellenőrzése még nyitott.

## 2026-11-07 — Manhattan kezdőirány 90°-os korrekciója
- A felhasználó visszajelzése szerint a kameraátadás javítása után is 90°-kal balra nézett a játékos. A scene PilotPivotja helyi Y=0° helyett +90°: induláskor és RECENTER-kor jobbra korrigált célirány.
- Kizárólag a pivot forgatása változott, a szemhelyzet és a fejkövetés változatlan. Diff áttekintve; kérésre nincs fordítás vagy teszt. A headsetes eredmény ellenőrizendő.

## 2026-11-07 — Pilóta kezdőiránya Meta kameraátadáskor
- FlightSeat újrakalibrál, amikor a startup kamerát a Meta szemkamera váltja, így a régi kamerához számolt forgatás nem marad végleges. Érvényes Meta követés után a PilotPivot +Z irányát használja; a későbbi fejforgatás szabad.
- Függőleges nézésnél az iránykalibráció újrapróbálkozik, nem jelöl hamis sikert. Forrás/diff áttekintve; kérésre nincs fordítás, teszt vagy build. Quest Linkes kezdőirány még ellenőrizendő.

## 2026-11-07 — Meta kézprefabok referenciajavítása
- A Quest Link naplóban hiányzó prefabreferencia miatt leállt Meta rig inicializálás. A kézvariánsokat a Meta SDK 207.0.1 eredeti kézkomponenseit használó önálló prefabok váltják, explicit gyökér-fileID-val; GUID, Capsense, OpenXR oldal és anyag megmarad.
- A hiányzó referenciák diagnosztikája külön megnevezi a kamera, bal és jobb kéz állapotát. Nincs saját kézmodell vagy ujjanimáció.
- Statikus prefab/diff áttekintés; kérésre nincs fordítás, teszt, Play mód vagy build. A Linkes megjelenés még headseten ellenőrizendő. Más helyi projektbeállítások nem részei a javításnak.

## 2026-11-07 — Papírtépés integrációja mainbe
- A codex/progressive-paper-tearing ág (6e215ce) beolvasztva a friss mainbe (46f2536), a cockpit- és egyéb main-változások megtartásával.
- Egyetlen szöveges ütközés a változásnaplóban: mindkét ág bejegyzése megmaradt. Git/diff áttekintés; kérésre nincs build, fordítás vagy teszt.

## 2026-11-07 — Friss cockpit-módosítások mainbe
- A felhasználó ccfe3af (Add better cockpit) commitja beolvasztva: Manhattan scene, cockpitanyagok, repülőhang és ProBuilder-beállítások.
- Automatikus, ütközésmentes Git-merge; a változáslista áttekintve. Kérésre nem futott fordítás, teszt vagy build; headsetes működés ellenőrizetlen.

## 2026-11-07 — Cockpit integráció mainbe
- A cockpit branch repülőmodellje, PilotPivot ülésrögzítése és függőleges sebességhez kötött hangmagassága beolvasztva a friss mainbe. A recovery metaütközéseknél a main azonosítói maradtak.
- Helyi commitolatlan módosítások érintetlenek; külön worktree-ben készült. Konfliktusok statikusan áttekintve, felhasználói kérésre nincs fordítás vagy teszt.

## 2026-11-07 — Függőleges sebességgel változó repülőhang
- FlightVehicle a meglévő, automatikusan megtalált vagy Inspectorban megadott AudioSource pitchét simítva változtatja: süllyedéskor magasabb, emelkedéskor mélyebb, alapból ±30% eltérés 10 m/s-nál, 0.4 s válaszidővel.
- Reset/letiltás visszaállítja az eredeti pitch-et; a lejátszást és a helyi scene-módosításokat nem írja át.
- Forrás/diff áttekintve; nincs fordítás, teszt vagy build. A mentett helyi AudioSource klip nélkül volt; a hallható eredmény még ellenőrizendő.

## 2026-11-07 — PilotPivot ülésrögzítés a cockpit branchen
- Munka előtt origin/cockpit pull (720c54e). A felhasználó helyi Manhattan-módosításából kizárólag a kért PilotPivot objektum átvéve, eredeti azonosítókkal és pozícióval; a többi helyi cockpit-átrendezés nem része ennek a commitnak.
- FlightSeat: explicit PilotPivot szemhelyzet/indulási irány és scene-szintű helyzetzár. Meta anchorfrissítés után és renderelés előtt korrigált közös rig; fejforgatás és kézvezérlés megmarad, kisétálás nincs. A többi scene room-scale marad.
- Dokumentáció és szabály frissítve a felhasználó által kért kivétellel. Forrás/prefab-override áttekintés történt; kérésre nincs fordítás, Play mód vagy teszt. Headsetes rögzítés és komfort ellenőrizendő.

## 2026-11-07 — Fokozatos, továbbtéphető papír
- A hirtelen split helyett kéztávolság-változással hajtott, megállítható szakadásfront, sűrített vizuális mesh, nyíló rés és szélfelhajlás. Újrafogáskor a megkezdett vonal folytatható.
- A darabok öröklik a kétkezes Meta Grabbable fogást, a téphetőséget és hangbeállításokat; terület- és mélységhatár, területarányos tömeg, teljes darabfa-reset és runtime-mesh takarítás.
- TearableContract prefabban külön húzás közbeni loop- és végső szakadás-hanghely, sebességfüggő térbeli hang és haptika. A közös prefab révén DemoWorld és VRInteractionTest is megkapja, scene-újragenerálás nélkül.
- Forrás/prefab/diff áttekintve. Felhasználói kérésre nincs build, fordítás, Play mód vagy teszt. Headsetes látvány, kézillesztés, audio és Quest teljesítmény ellenőrizetlen; a korábbi azonnali splitre írt PlayMode papírtesztek későbbi frissítést igényelnek. Részletek: Mechanics/TEARING.md.

## 2026-11-07 — Working-Manhattan összeolvasztása a main ággal
- Forrás: origin/Working-Manhattan (bc9f99e); célalap: origin/main (695d98b). Külön worktree-ben végzett helyi merge, az eredeti munkapéldány és staging megőrzésével.
- Hét Git-ütközés rendezve: három XR Hands mintamappa a main GUID-ját tartja meg; a két material a bejövő, kiegészített URP-mezőket használja. Az OpenXR asset egészében a main konzisztens fileID-hálózatát és újabb HandTracking sémáját tartja meg; a feature-k engedélyezése azonos a két ágon.
- A main ágban módosított Assets/_Recovery/0.unity megmarad a bejövő törlés helyett. A többi fájl Git automatikus egyesítésével került be, beleértve Manhattan várost, repülést és a main további tartalmait.
- Csak Git- és szerializált diff-áttekintés történt. Felhasználói kérésre nincs build, teszt vagy Unity-indítás. A szöveges merge sikere nem igazolja a scene-ek, prefabok és runtime-rendszerek együttműködését; ezek később ellenőrizendők.

## 2026-11-07 — Yoke billenés húzáskor és toláskor
- A yoke ±9 cm-es eltolását arányos, legfeljebb ±10° billenés kíséri: húzáskor a pilóta felé, toláskor előre. A collideres test is követi, a fordítás és visszacentírozás megmarad.
- `Maximum Push Pull Tilt` Inspector-paraméter (0–25°), a közös prefabban 10°. A repülő vezérlési bemenete változatlan.
- Forrás/diff áttekintve; felhasználói kérésre nincs fordítás, teszt vagy build. Headsetes fogás és látvány még ellenőrizendő.

## 2026-11-07 — Határozottabb gázreakció
- Maximális tolóerő-gyorsulás 3 → 6 m/s², állítható négyzetes gázgörbe (`throttleExponent=2`). A felső kartartomány nagyobb gyorsuláskülönbséget ad, teljes gázon dupla tolóerővel. A közös repülőprefabban is rögzítve.
- Forrás/diff áttekintve; felhasználói kérésre nincs fordítás, teszt vagy build. Headsetes sebességérzet és magasabb sebességű repülési hangolás még ellenőrizendő.

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
