# Kétkezes VR-bővítés — megvalósítási leltár

## Felmérés és döntések
A kiinduló commit 9364ccd. Unity 6000.3.28f1, URP 17.3.0, OpenXR 1.16.1, XR Management 4.4.0, Input System 1.20.1, Test Framework 1.6.0. Nincs XRI, inventory, holster, hálózati tulajdonlás vagy teleport. Az egykezes kizárólagosság oka a Grabbable.Grab `IsHeld` elutasítása és a DemoHand teljes tárgyat elengedő hívása volt.
A javítás ugyanazt a Grabbable réteget bővíti. Az anchor–tárgy kapcsolat közös, a Rigidbodynek egy mozgatója marad. Input asset, globális mozgás- és fizikabeállítás nem változott. A buildlistába két scene került, a buildkód az engedélyezett listát használja. Az első munkalépésben az eredeti OpenXRPackageSettings.asset és ProjectSettings.asset megőrzését SHA-256 összehasonlítás ellenőrizte.

A későbbi felhasználói kérés Meta XR SDK-t és beépített kezeket ír elő, és megtiltja a további buildet/tesztet. Emiatt a Grabbable a Meta SDK leszármazottja lett, a QuestPlayer Meta OVRCameraRig / Capsense Natural kezeket kapott; a manifest, Unity által készített lockfile és az OpenXR Meta feature konfigurációja változik. A room-scale szabály továbbra is változatlan. [Pontos architektúra és korlátok](META_XR.md).

## Források és assetek
Minden alábbi útvonal a `My project/Assets/_Game/` alatt értendő.

| Terület | Fájlok |
| --- | --- |
| Fogás és kéz | Scripts/Interaction/Grabbable.cs, DemoHand.cs, ControllerHandPose.cs |
| Meta XR | Scripts/Player/MetaRoomScaleRig.cs, Editor/MetaXrSetup.cs; MetaLeftHand/MetaRightHand prefabváltozatok, Art/Materials/MetaHandSkin.mat; QuestPlayer és DemoPlayer; Packages/manifest.json és packages-lock.json, runtime/teszt assembly-hivatkozások |
| Papírtépés | Scripts/Interaction/Tearable.cs, PaperMesh.cs, PaperFragment.cs, InteractionFeedback.cs |
| Szúrás és reset | Scripts/Interaction/StabbingTool.cs, StabTarget.cs, WorkshopReset.cs |
| Repülés | Scripts/Interaction/FlightYoke.cs, DemoSceneButton.cs; Scripts/Player/FlightVehicle.cs, FlightSeat.cs, DemoPlayer.cs (TrackingReady lekérdezés) |
| Scene/build eszközök | Editor/InteractionDemoTools.cs, DemoProjectTools.cs |
| Tesztek | Tests/Editor/PaperGeometryTests.cs; Tests/PlayMode/GrabTransitionTests.cs, InteractionDemoTests.cs; a meglévő tesztek is futnak |
| Új prefabok | TransferCube, Bottle, TwoHandRod, TearableContract, PracticeKnife, TrainingAircraft |
| Új papírassetek | Art/Contract.png, Art/ContractMesh.asset, Art/Materials/Contract.mat |
| Scene-ek | Scenes/DemoWorld.unity (hozzáadott zóna), VRInteractionTest.unity, FlightYoke.unity |

A QuestPlayer és a régi Unity kézmodellek eredeti GUID-jai megmaradtak; az utóbbiak Meta módban nem jelennek meg. A Meta kézmodellek az SDK-csomagból származnak. Minden új Unity assethez .meta tartozik. Külön input action asset nem készült; a meglévő grip/trigger útvonalak használatosak. A fogási mód, régió, tearing-küszöbök és yoke-tengelyek Inspector-beállításait a mechanikaleírások tartalmazzák. A régi fingerClosure paraméter a Capsense ujjpózát nem írja felül.

Az SDK importja az `_Game` mappán kívül saját `Assets/Oculus`, `Assets/Resources`, `Assets/Plugins/Android` beállításokat is létrehozott. A projekt közös konfigurációi verziózottak, a helyi DevAgent/PlatformTool fájlokat a gitignore kizárja. OpenXR feature-bejegyzések, PackageManagerSettings és SDK UI-tagek kerültek a beállításokba; a Meta minimum Android API 32 követelménye és Auto telepítési helye is rögzített.

## Dokumentációs leltár
Elolvasva: gyökér AGENTS.md és README.md; Docs/README.md, PROJECT.md, CHANGELOG.md, QUEST_SETUP.md; az összes meglévő Mechanics dokumentum (PLAYER, GRABBING, CHARACTER, ENERGY_GAME, DEMO_WORLD, TOOL_USE, HITTING, CUTTING, _TEMPLATE); Art/UnityHands README.md és LICENSE.md. A repositoryban más helyi AGENTS.md nem volt.
Frissítve: README, dokumentációs index, PROJECT, QUEST_SETUP, CHANGELOG, GRABBING, PLAYER, DEMO_WORLD, TOOL_USE. Indok: új fogási állapotok, tárgyak, jelenetek, reset, flight-kivétel és ellenőrzések. Új: TEARING, STABBING, FLIGHT_YOKE, VR_INTERACTION_TESTS és ez a leltár. Az érintetlen robot-, energia-, ütés- és blokk-vágási szabályokat nem másoltuk párhuzamos leírásba.

## Validáció és határok
A konkrét korábbi tesztszámokat a CHANGELOG rögzíti; a bizonyítékok az ignorált `My project/Builds/TwoHandValidation/` alatt vannak. A Meta-átállás után nem indult build, Play mód vagy teszt. A három scene képi ellenőrzése a Meta előtti változat asztali kamerarendere, nem headsetes validáció. A DemoWorld eredeti szerializált objektumai változatlanok: csak új objektumok és a SceneRoots új gyökérbejegyzése került hozzá; a közös QuestPlayer prefab külön kapta a Meta rig-bekötést.

A papír kézmozgással előrehaladó, egyenes vonal mentén szakad; a darabok méret- és mélységhatárig tovább téphetők. Nem általános 3D vágás vagy softbody. A 2026-11-07-i fokozatos változat nem futtatott/tesztelt; részletek: Mechanics/TEARING.md. A szúrás kontrollált kinematic ellenállásmodell. A kézpózok kontrolleres ujjanimációk, nem teljes tárgyfüggő IK. Általános kinematic eszköz továbbra is falba tolható. A yoke és a repülő könnyített demó, nem teljes aerodinamika. A fizikai room-scale mozgás minden jelenetben megmarad.

Nyitott hardverpróbák: két szem, követés, bal/jobb kontroller, kéz- és markolatillesztés, dobás és átadás érzete, haptika/hang, üléskalibráció, repülési komfort és reset, Quest teljesítmény. Indítás és manuális lépések: [VR_INTERACTION_TESTS](VR_INTERACTION_TESTS.md).
