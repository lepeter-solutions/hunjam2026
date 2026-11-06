# Fizikai mozgás és követés
## Cél és források
`Scripts/Player/DemoPlayer.cs`, `Prefabs/QuestPlayer.prefab`, `Scenes/DemoWorld.unity`.
A játékos csak a valós térben sétálva, fordulva és lehajolva mozog. A rig gyökere változatlan; az OpenXR TrackedPoseDriver követi a fejet.
## Bemenetek és állapot
Nincs joystickmozgás, snap turn, teleport, WASD vagy egérrel fordulás. Nincs CharacterController vagy gravitációs rigmozgatás.
Floor tracking origin kérése; ha nem támogatott, 1,65 m magassági offset. Headset nélkül a kamera álló szerkesztői nézetet ad, a kezek rejtettek.
## Hangolás és kapcsolatok
A scene kezdőpontja a műhely közepe. A virtuális padló és a valós padló egyezzen. A headset saját játéktérhatárát kell használni, a virtuális fal nem állítja meg a fizikai testet.
A fallback magasság később kalibrációs beállítás lehet; nincs automatikus visszateleport.
## Ellenőrzés
PlayMode: rig pozíció és forgatás változatlan, nincs CharacterController és teleport API. Hardveren külön: stick és A nem mozgat/fordít, fizikai lépés és fejfordítás követése, újracsatlakozás, padlószint. Eredmény: CHANGELOG.
