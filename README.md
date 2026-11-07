# HunJam 2026 — Quest Playground

Unity **6000.3.28f1**, URP, Meta Quest standalone / Meta XR Interaction SDK 207.0.1 / OpenXR / Touch kontrollerek.

![A DemoWorld gyakorlópálya](Docs/Images/DemoWorld.png)

## Indítás
1. Unity Hubban a `My project` mappát add hozzá; ugyanazt az Editor-verziót használd.
2. Várd meg a csomagok importját és a fordítást.
3. Új fejlesztői demó: `Assets/_Game/Scenes/InteractionSandbox.unity`, majd Play. A korábbi `DemoWorld.unity` is megmarad.
4. Unity Play módban Quest Linkkel tesztelj. Headset nélkül csak álló előnézet van; a mechanikákat a PlayMode tesztek ellenőrzik.
5. Beszélj a robottal, gyűjtsd össze a három energiagömböt, és engedd őket a központi célállomásba. Beszélj újra a robottal az új körhöz.

Questen: kizárólag fizikai séta és fordulás. Grip: közeli tárgy megfogása; trigger: használat vagy közeli beszélgetés. Kalapáccsal üthetsz, pengével vághatod a blokkot, a lámpát triggerrel kapcsolhatod. Üres kézzel érintsd meg a RESET gombot és nyomj triggert az eszközpróba újraindításához. Nincs joystickmozgás vagy teleport.

A demó szerkeszthető scene-t, prefabokat és materialokat tartalmaz. A `HunJam/Generate Demo World` menü csak tudatos újragenerálásra való: felülírja a demó generált tartalmát. A `HunJam/Configure Quest` a Quest buildbeállításokat alkalmazza.

Munkakezdés: [AGENTS.md](AGENTS.md). Dokumentáció: [Docs/README.md](Docs/README.md). Build és eszközteszt: [Docs/QUEST_SETUP.md](Docs/QUEST_SETUP.md).

Az [InteractionSandbox](Docs/Mechanics/INTERACTION_SANDBOX.md) a Meta kezek és eszközök mellett hátul külön fizikai munkapadot tartalmaz négy rakható kockával és két dobható labdával. A `PhysicsCube` és `PhysicsBall` prefab új pályákhoz is használható. Az új scene kérésre tesztelés és fordítás nélkül került be.
