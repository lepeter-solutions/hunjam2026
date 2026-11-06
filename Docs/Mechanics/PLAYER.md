# Játékos, követés és helyváltoztatás

## Cél és források
`Assets/_Game/Scripts/Player/DemoPlayer.cs`, `Prefabs/QuestPlayer.prefab`, `Scenes/DemoWorld.unity`. CharacterController alapú játékos, OpenXR HMD-követés és két kontroller. Az Input System TrackedPoseDriver frissíti a kamerát.

## Vezérlés és állapot
- Quest: bal stick kamera vízszintes irányához képest mozgat; jobb stick oldalra 30° snap turn. Új forduláshoz a sticknek vissza kell térnie középre.
- Jobb A: a jobb kéz sugarának zöld, járható célpontjára teleportál. A célpont felett legyen hely a testnek. Falon át nem célozhatunk.
- Desktop: WASD; jobb egérgomb + egér körülnézés. Headset aktív állapotában a desktop mozgás kikapcsol.
- Floor tracking origin kérése; nem támogatott floor esetén 1,65 m kameraoffset. A fizikai szobamozgást a kamera követi; a kapszula követi a fej vízszintes eltolását.
- Mélyre eséskor visszakerül a kezdőpontra. A fizikai fej falon átnézését a demó még nem takarja el.

## Paraméterek
`speed`: 1,8 m/s; `snapAngle`: 30°; kapszulasugár: 0,25 m. A sima mozgás opcionálisan később kikapcsolható komfortmenüből; jelenleg teleport is használható mellette. Ugrás, futás és sima fordulás nincs.

## Ellenőrzés
Desktop: falak megállítják a WASD mozgást, körbe lehet járni a két akadályt. Quest: padlómagasság, room-scale eltolás, két szem képe, fordulás fejkörül, teleport üres célra és tiltás akadálynál. Headset reconnect és alkalmazás-felfüggesztés is kézzel ellenőrizendő. A tényleges futtatási eredmények a változásnaplóban vannak.
