# Hitler: fegyver, ciánkapszula és döntés

## Cél és bekötés
A `02_Hitler` jelenet a jelenlegi StoryFlow **3. döntése**. Az utoljára feltöltött térképbeállításokhoz igazodva Hitler lelövése `false` (0); a játékos saját lelövése vagy a ciánkapszula használata `true` (1). A három esemény ugyanazon StoryLevelExit `CompleteWithDecision(bool)` függvényét hívja, így a döntés mentése és a pályaváltás egy művelet.

Érintett fájlok: `Assets/_Game/Scripts/VRGun.cs`, `PillDetectInMouth.cs`, `Scenes/02_Hitler.unity`. Fogás: meglévő Meta Grabbable / HandGrabInteractable és kézpózok.

## Fegyver
Fogás közben a fogó kéz indextriggere lő. A sugár a jelenet meglévő `pointer` csőtorkolatának +Z irányát követi. A találatok távolság szerint rendezettek; saját fegyver, játékos kéz-/interakciós colliderei és nem céltárgy triggerjei kimaradnak. A legközelebbi szilárd környezeti tárgy blokkol. Hitler teljes megadott hierarchiája célpont. A játékosnak külön, CenterEyeAnchorhoz kapcsolt fejcollidere van; a fejen belül kezdődő sugár is találat. A korábbi kézcollider-referencia és kizárólagos tagvizsgálat megszűnt. Hiányzó muzzleFlash nem akadályozza a találatot; az effekt 5 s után takarítódik.

Hangolás: `maxDistance`, `hitLayers`, `muzzleTransform`, fejcollider sugara (0,12 m). A piros Gizmo mutatja a lövés tényleges irányát. A találatot követő további lövések tiltottak.

## Ciánkapszula
A kézben tartott kapszulát a játékos követett szája közelében kell tartani 0,35 s-ig. `playerHead` a CenterEyeAnchor, `mouthOffset=(0;-0,07;0,04)`, `mouthRadius=0,09 m`, `holdTime=0,35 s`. Eltávolítás megszakítja az időzítést. Elfogyasztva egyszer hívja a true kimenetet. A régi, Hitler szájára illesztő ZoneInteractable ebben a scene-ben kikapcsolt; a Meta SDK kezeli továbbra is a tárgy fogását. Nincs kézzel indított vagy frame-enként újraindított coroutine.

## Ellenőrzés és korlátok
Forrás, telepített Meta SDK API, scene fileID-k és a három bool-esemény statikusan áttekintve. Unity-indítás, fordítás, Play mód és headsetes próba a felhasználó kérésére nem futott. Headseten ellenőrizendő: mindkét kézzel lövés, üres ravaszhúzás, akadály blokkolása, Hitler fej/törzs találat, fejhez érintett fegyver, kapszula használata és a három út után a D3 helyes értéke. A meglévő csőtorkolat vizuális illesztése és a 9 cm-es szájzóna komfortja eszközön ellenőrizendő.
