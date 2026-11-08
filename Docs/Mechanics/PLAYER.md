# Fizikai mozgás és követés
Kifejezett felhasználói kivétel: a cockpit Manhattan scene-ben a FlightSeat PilotPivothoz rögzíti a szemhelyzetet, így nincs kisétálás a gépből; fejforgatás és kézvezérlés megmarad. Részletek: [pilótaülés](PILOT_SEAT.md). Más jelenetekben továbbra is room-scale a mozgás. A DemoPlayer TrackingUpdated eseménye a Meta anchorfrissítés után értesíti az üléskomponenst.
## Cél és források
`Scripts/Player/DemoPlayer.cs`, `MetaRoomScaleRig.cs`, `Prefabs/QuestPlayer.prefab`, MetaLeftHand/MetaRightHand, mindhárom demóscene.
A játékos csak a valós térben sétálva, fordulva és lehajolva mozog. A rig gyökere változatlan; XR kapcsolatkor a Meta OVRCameraRig követi a fejet, és letiltja a régi kamerát/TrackedPoseDrivert. A kézpózokat a Meta Capsense Natural állítja elő. [Integráció](../META_XR.md).
## Bemenetek és állapot
Nincs joystickmozgás, snap turn, teleport, WASD vagy egérrel fordulás. Nincs CharacterController vagy gravitációs rigmozgatás.
Meta módban OVRManager Stage origin; a saját floor-offset kód ilyenkor nem írja a tracking space-t. A Meta nélküli kompatibilitási út floor origint kér, sikertelenségnél 1,65 m offsetet használ. Headset nélkül a kamera álló szerkesztői nézetet ad, a kezek rejtettek.
## Hangolás és kapcsolatok
A scene kezdőpontja a műhely közepe. A virtuális padló és a valós padló egyezzen. A headset saját játéktérhatárát kell használni, a virtuális fal nem állítja meg a fizikai testet.
A fallback magasság később kalibrációs beállítás lehet; nincs automatikus visszateleport.

Kizárólag a FlightYoke scene-ben a rig egy Rigidbody-jármű gyermeke. A FlightSeat a padlókövetés kész állapotát (`TrackingReady`) megvárva az üléshez igazítja a gyökeret; utána a valós fej-/kézmozgás marad az egyetlen játékosi mozgásbemenet. A gép mozgása járműfizika, nem új world locomotion. Részletek: [Flight Yoke](FLIGHT_YOKE.md).
## Ellenőrzés
2026-11-07: Quest Link hibajelentésnél az Editor naplója hiányzó Meta prefabreferenciát jelzett, ezért a rig nem indult el. A kézzel szerializált kézvariánsok helyett az SDK eredeti OVRHandPrefab komponenseiből önálló prefabok készültek, valódi 100000 gyökérobjektummal, változatlan asset GUID-dal és bal/jobb OpenXR/Capsense beállításokkal. A hibaüzenet most külön jelzi a kamera/bal/jobb referencia állapotát. Statikus áttekintés történt, futásidejű siker még nem igazolt. Következő kézi ellenőrzés: Manhattan Quest Link Play módban, mindkét kéz látható és követ, yoke/gázkar megfogható, nincs missing-reference hiba.
Az SDK-átálláshoz a felhasználó kérésére nem indult teszt vagy build; a korábbi eredmények a Meta előtti rigre vonatkoznak.
PlayMode: rig pozíció és forgatás változatlan, nincs CharacterController és teleport API. Hardveren külön: stick és A nem mozgat/fordít, fizikai lépés és fejfordítás követése, újracsatlakozás, padlószint. Eredmény: CHANGELOG.
