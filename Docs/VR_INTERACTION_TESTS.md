# VR interakciók — indítás és elfogadás

**Állapot 2026-11-07:** a legutóbbi felhasználói kérés tiltja a buildelést és tesztelést. A lista későbbi elfogadáshoz készült; a Meta XR-átállás után nem hajtottuk végre. A korábbi 10 EditMode / 17 PlayMode siker és a scene-képek a Meta előtti verzióból származnak.

## Jelenetek
| Scene az Assets/_Game/Scenes alatt | Indítás és tartalom | Reset |
| --- | --- | --- |
| DemoWorld.unity | Nyisd meg és Play, vagy az APK első scene-je. Eredeti robot/energiakör, kalapács, vágópenge, lámpa. Mögötted új interakciós asztal és két scene-gomb. | Zöld RESET + üres kéz trigger; energiakör: robot. |
| VRInteractionTest.unity | Külön Play vagy INTERACTION LAB gomb. Kocka, palack, kétkezes rúd, téphető lap, nem téphető referenciatárgyak, tompa szúróeszköz és céltömb. | Bal zöld RESET + üres kéz trigger. |
| FlightYoke.unity | Külön Play vagy FLIGHT YOKE gomb. Indulás az ülésben, gép már stabil előrehaladással. | Bal RESET / RECENTER gomb + trigger; jobb gomb visszavisz a demóba. |

Unity 6000.3.28f1, OpenXR 1.16.1, XR Management 4.4.0, Input System 1.20.1, Meta XR Core + Interaction SDK 207.0.1. Nincs XR Interaction Toolkit. Valódi interakcióhoz Quest Link + Touch vagy a standalone Quest build kell. Headset nélkül statikus scene-nézet van; desktop billentyűs vezérlés nincs. [Meta import, Capsense és Grabbable](META_XR.md).

## Manuális headsetes tesztlista
1. Ellenőrizd külön a két szem képét, padlószintet, bal/jobb követést és kézpózt. Grip nélkül az ujjak enyhén hajlítottak; fogva a markolat a tenyér környékén maradjon.
2. Kocka felvétele bal kézzel, jobb grip csatlakozás, bal elengedés, jobb elengedés; ugyanez fordítva. Átadáskor ne legyen szabadon eső frame vagy ugrás.
3. Rúd: mindkét kezdési sorrend; másodlagos és elsődleges elengedés külön; az elsődleges elengedésekor a másik tartsa meg. A második kéz irányítsa a forgatást; méret ne változzon.
4. Majdnem egyidejű grip és elengedés, gyors újrafogás, közel egybeeső kezek, távoli második kéz; két külön tárgy ne cseréljen birtokost magától.
5. Dobás, földre ejtés, tárgyreset, kontroller követésvesztése, alkalmazásfókusz elvesztése/visszatérése. Ne maradjon foglalt anchor.
6. Papír: egy kéz és gyenge húzás ne tépjen. Két különböző, ellentétes oldali pontból húzva más szakadásgeometria keletkezzen. Mindkét darab saját kézben maradjon, külön elengedhető és átadható legyen. RESET állítsa vissza az eredeti lapot. Kocka/rúd nem téphető.
7. Szúrás: csak heggyel, megfelelő sebességgel a kijelölt habba; oldalsó érintés és lassú mozdulat ne aktiválja. Nyomás mélysége korlátozott, kihúzás működik. Fogás közbeni átadás és reset se hagyjon tiltott colliderkapcsolatot.
8. Régi lámpa trigger, kalapácsütés, penge-vágás, energiakör és robot működjön tovább.
9. Mindhárom scene külön nyitható, a gombok mindkét irányban működjenek az APK-ban is.
10. Flight: ülésből indulás, egy-/kétkezes yoke, csatlakozás/elengedés ugrás nélkül, jobbra/balra bank, push-down és pull-up, centírozás, valódi előrehaladás és műszerek. Reset, újracentírozás és visszalépés próbája.
11. Stickek egyik scene-ben se mozgassák a játékos karakterét. A fizikai fej-/kézmozgás a repülés alatt is maradjon meg.
12. Komfort és teljesítmény: repülés érzékenysége, látóhatár, reset átmenet, követési késés, fogásremegés; Quest profilerrel mérd a képkockaidőt és jelöld a készüléket.

## Automatikus ellenőrzés
A `HunJam.Tests` EditMode és `HunJam.PlayModeTests` PlayMode assembly a Unity Test Runnerből futtatható. A Quest útmutató batch parancsai változatlanok. Jelenet- és képi ellenőrzés: batch `HunJam.Editor.InteractionDemoTools.ValidateAndPreview`; a PNG-k helye `Builds/Previews`.
A `HunJam/Build Interaction Demos` mentett asseteket készít, nem Play módú geometria-bootstrap. A buildlista megőrzi más scene-ek bejegyzéseit, és hozzáadja a három demót. A Quest build az engedélyezett buildlistát használja.

A tényleges eredmények és ismert hiányzó hardverellenőrzések a [változásnaplóban](CHANGELOG.md) vannak. Az automatizált teszt nem igazolja a headsetes kényelmet, kézpózt vagy standalone teljesítményt.
