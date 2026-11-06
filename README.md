# HunJam 2026 — Quest Playground

Unity **6000.3.28f1**, URP, Meta Quest standalone / OpenXR / Touch kontrollerek.

![A DemoWorld gyakorlópálya](Docs/Images/DemoWorld.png)

## Indítás
1. Unity Hubban a `My project` mappát add hozzá; ugyanazt az Editor-verziót használd.
2. Várd meg a csomagok importját és a fordítást.
3. Nyisd meg: `Assets/_Game/Scenes/DemoWorld.unity`, majd Play.
4. Headset nélküli mód: WASD mozgás, jobb egérgomb nyomva tartva körülnézés, E beszélgetés, bal egérgomb nyomva tartva tárgyfogás, Backspace új kör.
5. Beszélj a robottal, gyűjtsd össze a három energiagömböt, és engedd őket a központi célállomásba. Beszélj újra a robottal az új körhöz.

Questen: bal stick mozgás; jobb stick oldalra 30° fordulás; grip tárgyfogás; trigger beszélgetés; jobb A gomb a padlóra mutatva teleportálás. A teleport célját a jobb kéz sugara mutatja.

A demó szerkeszthető scene-t, prefabokat és materialokat tartalmaz. A `HunJam/Generate Demo World` menü csak tudatos újragenerálásra való: felülírja a demó generált tartalmát. A `HunJam/Configure Quest` a Quest buildbeállításokat alkalmazza.

Munkakezdés: [AGENTS.md](AGENTS.md). Dokumentáció: [Docs/README.md](Docs/README.md). Build és eszközteszt: [Docs/QUEST_SETUP.md](Docs/QUEST_SETUP.md).
