# Projekt

## Cél és hatókör
Egyjátékos, rövid VR gyakorlópálya Quest Touch kontrollerekhez. A játékos kizárólag fizikailag mozog, eszközöket használ, üt és kijelölt blokkot vág, beszél egy robotkarakterrel, megfog három energiagömböt, és egy gyűjtőállomásba helyezi őket. Ez technikai demó és közös fejlesztési alap, nem a végleges jam-játék.

Kezdeti célhardver: Quest 2/3/3S család; valódi eszközön külön validálandó. Nincs multiplayer, kézkövetés, mentés vagy végleges művészeti asset.

## Struktúra
```
Assets/_Game/
  Art/Materials/      közös demóanyagok
  Audio/             későbbi saját hangok
  Prefabs/           játékos, robot, energiagömb
  Scenes/            InteractionSandbox, DemoWorld és a csapat pályái
  Scripts/
    Core/            játékkör állapota
    Interaction/     Meta SDK adapterek, eszközhasználat, ütés és vágás
    Characters/      robotkarakter
    World/           gyűjtő és járható felület
  Editor/            scene-generálás, Quest konfiguráció, ellenőrzés
  Tests/Editor/      játékkör regressziós tesztjei
  Tests/PlayMode/    scene, fizikai gyűjtés, közeli fogás, eszközök és reset
Docs/Mechanics/      mechanikánkénti leírások
```

## Munkamegosztás
Egy ember/agent a játékosé, egy a mechanikáké, egy a pályáé lehet; az aktuális feladattábla jelöli a tényleges felelőst. Közös scene és projektbeállítás integrátori terület. A prefabok külön szerkeszthetők. A generátor a kezdeti demó reprodukálására szolgál; kézzel szerkesztett scene-re nem futtatjuk rá automatikusan.

## Technikai döntések
Meta XR Core + Interaction SDK + OVR Integration 207.0.1, OpenXR 1.16.1. A Meta SDK kezeli a követést, a kézmodelleket, a közeli fogást és dobást. Saját kód kizárólag a játéklogika és az SDK-események bekötése. Részletek: META_XR.md.

A csapat által hozzáadott környezetek (például 01_Vérszerződés) külön scene-ek. Az InteractionSandbox a továbbépíthető interakciós alap, a DemoWorld a korábbi demó. Az új pályákhoz a közös QuestPlayer, PhysicsCube és PhysicsBall prefab használható. Az aktuális ellenőrzési állapot a CHANGELOG-ban szerepel.

A világ valódi Unity assetekből áll, nem csak Play módban létrejövő geometriából. A mobil URP profil és egyszerű primitívek csökkentik a kezdeti renderköltséget. A teljesítmény céleszközön mérendő.
