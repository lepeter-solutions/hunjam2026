# Meta XR integráció

## Csomagok és cél
A felhasználó kérésére a projekt a hivatalos Meta XR Core SDK és Interaction SDK `207.0.1` verzióját használja. Forrásuk a manifestben rögzített `https://npm.developer.oculus.com` scoped registry. A `packages-lock.json` fájlt a Unity Package Manager frissíti. A meglévő Unity OpenXR loader, Touch bemenetek és room-scale szabály maradnak.

## Természetes kontrollerkezek
A `QuestPlayer` prefab `MetaRoomScaleRig` komponense XR kapcsolatkor létrehozza a csomag eredeti `OVRCameraRig` példányát. Az `OVRManager.controllerDrivenHandPosesType` értéke `Natural`: a Meta Capsense a Touch érintés- és gombadataiból készít természetes ujjpózt. A `MetaLeftHand` és `MetaRightHand` a csomag `OVRHandPrefab` prefabjának változatai, OpenXR bal/jobb csontvázzal, `ControllerInHand` láthatósággal és URP bőrszínű anyaggal. Az SDK adja a kézgeometriát és a csontanimációt. Nincs saját ujjfizika, optikai grip vagy pinch-vezérlés.

A Meta rig ugyanazon játékosgyökér gyermeke. A régi kamera kikapcsol, a Meta középső szemkamerája veszi át a scene kamerabeállításait. A DemoHand a Meta kontrolleranchor világpózát követi. A régi Unity kézmodell és ControllerHandPose nem fut a Meta kezek mellett. Headset nélkül az eredeti statikus kamera marad. Nincs locomotion/teleport prefab. FlightYoke-ban a meglévő járműgyökér szállítja a teljes riget; az üléskalibráció megvárja az első érvényes Meta anchorfrissítést.

## Grabbable és játékszabályok
A `HunJam.Grabbable` most az SDK `Oculus.Interaction.Grabbable` osztályából származik, és annak `ITransformer` bővítési pontját valósítja meg. A meglévő 12 cm-es közelségi/akadályvizsgálatot végző DemoHand inputadapter a `Grab`/`Release` API-n keresztül Meta Hover, Select, Move, Unselect és Unhover eseményeket küld. A tényleges Rigidbody-mozgatás a Meta által meghívott transformerben történik, fizikaframe-enként egyszer.

Az átadási szerepek, fogási régiók, kétkezes elsődleges kontaktpont, yoke-végállás, szúráskorlátozás és korlátozott dobási sebesség a játék bővítései. A Meta automatikus második-kéz-átadása ki van kapcsolva, maximum két grip szerepel. A natív automatikus Rigidbody-lock és dobás ki van kapcsolva, hogy a járműhöz kötött yoke és a beszúrt eszköz ne kapjon második fizikavezérlőt. A papírdarabok is ugyanilyen Meta Grabbable-leszármazottak; a létrehozási frame-ben inicializálhatók és átvehetők.

A projekt adapterét használd a fogáshoz. Külön SDK GrabInteractor/GrabInteractable pár hozzáadása ehhez a prefabhoz további eseményadaptert igényel; jelenleg a meglévő közelségi kiválasztást tartottuk meg. A Capsense nem tárgygeometriára illesztett IK: a pontos tenyér/markolat és ujjilleszkedés hardveren hangolandó.

## Import és beállítás
A mentett projektbeállításokban a Meta XR Core OpenXR feature Android és Standalone célon engedélyezett, az OpenXR kézcsontváz és a Capsense-hez szükséges Controllers And Hands támogatás beállított. Újraalkalmazás: `HunJam/Configure Meta XR Hands` (`Editor/MetaXrSetup.cs`). Ez minimum API 32-t is beállít a Meta 207 követelménye szerint; nem buildel, nem indít Play módot és nem futtat tesztet. A menü nem fut automatikusan minden domain reloadkor. Ha az SDK saját „Enable Meta XR Feature Set” ablakot mutatja, a Meta feature engedélyezése szükséges.

A `HunJam/Generate Demo World` generátora az új rig-referenciákat is beköti. A scene-ek a közös QuestPlayer prefab módosítását öröklik; az eredeti pályaobjektumokat emiatt nem kell újragenerálni.

## Ellenőrzési állapot
2026-11-07: hivatalos csomagforrás és API-k áttekintése, manifest/assembly/prefab bekötés; a Unity Package Manager letöltötte a két `207.0.1` csomagot, és elkészítette a lockfile-t. Az import lezárult, a mentett Android/Standalone Meta feature és kézbeállítások létrejöttek. Az import konzoljában hiányzó AndroidPlayer buildmodul szerepel; ezt a feladat nem telepítette. A felhasználó tiltása miatt ehhez az átálláshoz nem indult build, Test Runner vagy Play mód. A korábbi teszteredmények és képek a Meta előtti állapotra vonatkoznak.

Később ellenőrizendő: Quest és Link Capsense, két szem, padlószint, két kontroller, tracking/fókuszvesztés; átadás és tépett darabok; yoke/ülés/scene-váltás; dobás, haptika, komfort és standalone képkockaidő. A jelenlegi Editor-telepítésben nincs Android buildmodul. Nincs headsetes siker állítva.

Az SDK létrehozott közös Oculus/Resources konfigurációi, AndroidManifestje és UI-tagjei verziózottak. A géphez kötött DevAgentSettings és OVRPlatformToolSettings asset/meta párok a `.gitignore` részei; a csomag ezeket helyben újra létrehozhatja.

## Hivatalos források
- [Meta: Capsense, természetes kontrolleres kézpózok](https://developers.meta.com/vr/documentation/unity/unity-capsense/)
- [Meta: Interaction SDK beállítás](https://developers.meta.com/vr/documentation/unity/unity-isdk-setup/)
- [Meta: Unity Package Manager](https://developers.meta.com/vr/documentation/unity/unity-package-manager/)

A Meta assetek a csomagokban maradnak, saját licencükkel; a projekt csak prefabváltozatokat és hivatkozásokat tárol.
