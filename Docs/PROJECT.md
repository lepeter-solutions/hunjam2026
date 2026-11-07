# Projekt

## Cél és hatókör
Egyjátékos, rövid VR gyakorlópálya Quest Touch kontrollerekhez. A játékos kizárólag fizikailag mozog, eszközöket használ, üt és kijelölt blokkot vág, beszél egy robotkarakterrel, megfog három energiagömböt, és egy gyűjtőállomásba helyezi őket. Ez technikai demó és közös fejlesztési alap, nem a végleges jam-játék.

Kezdeti célhardver: Quest 2/3/3S család; valódi eszközön külön validálandó. A játék kontrolleres; a Meta Capsense biztosítja a kézpózokat. Nincs optikai kézzel történő játékmenet-vezérlés, multiplayer, mentés vagy végleges művészeti asset.

## Struktúra
```
Assets/_Game/
  Art/Materials/      közös demóanyagok
  Audio/             későbbi saját hangok
  Prefabs/           játékos, robot, energiagömb
  Scenes/            DemoWorld
                     VRInteractionTest, FlightYoke
  Scripts/
    Core/            játékkör állapota
    Player/          követés és helyváltoztatás
    Interaction/     kéz, tárgyfogás, interakció
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
OpenXR 1.16.1, XR Management 4.4.0, Input System 1.20.1. Meta XR Core és Interaction SDK 207.0.1, hivatalos registryből. A HunJam.Grabbable a Meta Grabbable leszármazottja, saját közelségi inputadapterrel és az átadás/yoke/szúrás szabályait megtartó ITransformerrel. A kezek Meta Capsense Natural / OVRHandPrefab alapúak. Nincs XR Interaction Toolkit. Részletek és ellenőrzési korlátok: [Meta XR](META_XR.md).

A világ valódi Unity assetekből áll, nem csak Play módban létrejövő geometriából. A mobil URP profil és egyszerű primitívek csökkentik a kezdeti renderköltséget. A teljesítmény céleszközön mérendő.
