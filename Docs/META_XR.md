# Meta XR integráció
## Rögzített csomagok
Meta XR Core SDK, Interaction SDK Essentials és Interaction SDK OVR Integration: **207.0.1**, Unity Package Managerből, manifestben és lockfile-ban rögzítve. Unity 6000.3.28f1, OpenXR 1.16.1. A csomagforrásokat nem másoljuk Assets alá.
Az Android minimum API 32. A csapat által importált Unity XRI minták megmaradnak; a DemoWorld és QuestPlayer interakcióit a Meta SDK végzi. Az SDK helyi DevAgentSettings konfigurációja nem kerül Gitbe.
## Rig
A QuestPlayer prefab a Meta OVRCameraRig és OVRComprehensiveInteractionRig prefabjait tartalmazza. A Meta kezeli a fej/kontrollerek követését, kézmodelleket és kontrollervezérelt kézpózokat. Floor Level origin, ConformingToController póz. Nincs saját TrackedPoseDriver, DemoHand vagy ControllerHandPose.
Az SDK locomotion névterének minden komponense letiltott; a távoli ray/distance grab szintén letiltott. A cél továbbra is kizárólag fizikai mozgás és közeli fogás.
## Objektumok
Oculus.Interaction.Grabbable: mozgatás, kinematic állapot és dobási sebesség.
GrabInteractable / HandGrabInteractable: közeli kiválasztás és fogás. Palm/grip alapú fogás; a trigger a használaté. Az SDK irányítja a fogási állapotot és kézváltást.
GameItem: kizárólag játékállapot, reset és az ütés rövid élesítési késleltetése. Nem követi és nem mozgatja a kezet.
MetaUseAction: az SDK ControllerSelector triggereseményét kapcsolja a kiválasztott tárgy Use vagy a közeli DemoInteractable.Interact metódusára. Nincs saját sugár, távolságkeresés vagy kézbemenet.
Az ütés, előre meghatározott blokkvágás és energiagyűjtő kör játékspecifikus kód marad.
## Szerkesztés
A HunJam/Generate Demo World az SDK-s riget hozza létre. A MetaProjectTools.Migrate egyszeri migrációs belépési pont, a meglévő demót módosítja; ellenőrzési másolaton futtatandó, mentés után. Általános új objektumokhoz az SDK hivatalos Quick Action / Building Blocks eszközeit használd.
## Teszt
Unity Play + Quest Link. Androidon a Meta XR feature és a Touch profil legyen aktív. A tesztek SDK pointereseményekkel ellenőrzik a tárgyállapotot és a játékmechanikákat; nem igazolják a valódi headsetkövetést vagy a kontrollergesztusokat. Aktuális eredmény: CHANGELOG.
## Hivatalos források
- [Meta: Comprehensive Interaction Rig](https://developers.meta.com/vr/documentation/unity/unity-isdk-add-comprehensive-interaction-rig/)
- [Meta: Interaction SDK kezdés](https://developers.meta.com/vr/documentation/unity/unity-isdk-getting-started/)
- [Meta: SDK telepítése](https://developers.meta.com/vr/documentation/unity/unity-isdk-setup/)
