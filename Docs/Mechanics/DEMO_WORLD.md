# Demóvilág
## Forrás és elrendezés
`Scenes/DemoWorld.unity`, `Editor/DemoProjectTools.cs`, `Art/Materials/`, `Prefabs/`.
A nagy udvar megmarad háttérként, de minden játékmechanika a kezdőpont körüli kis műhelyben van: balra gömbök és gyűjtő, elöl munkapad és ütő/vágó cél, jobbra kalapács, penge, lámpa. A robot balra hátul áll.
A kezdőpont (0,0,0). A fő eszközök körülbelül egy méteren belül helyezkednek el; elérésükhöz fizikai lépés, fordulás, nyúlás szükséges. A távoli udvar bejárása nem feladat.
## Generálás és szerkesztés
A scene valódi Unity asset, a játékos, robot, gömb és eszközök prefabok. HunJam/Generate Demo World felülírja a generált demót; kézi módosítások előtt commit. Batch Prepare csak ellenőrzési másolatban.
A kéz a Meta OVRHandPrefab natív modellje, Capsense Natural kontrolleres ujjpózokkal; bekötés a közös QuestPlayer prefabban. Egy directional light, egyszerű URP anyagok; a használható lámpa helyi spotfényt ad.
## Ellenőrzés
Scriptreferenciák, három gömb, játékos, PlayMode mechanikatesztek és képi előnézet. Hardveren a rendelkezésre álló biztonságos játéktérhez igazítani kell az elrendezést és a munkapad magasságát. Teljesítmény és ergonomikus elérhetőség külön Quest-ellenőrzés.
[Korábbi, Meta előtti előnézet](../Images/DemoWorld.png). Eredmények: CHANGELOG. A Meta-átállás után a felhasználó kérésére nem indult új teszt vagy build.

## Kétkezes interakciós bővítés
Az `InteractionDemoTools.Build` az eredeti scene-t nyitja meg, és csak akkor adja hozzá a `Two Hand Interaction Zone` gyökeret, ha még nincs benne. Nem generálja újra az udvart, robotot vagy energiakört. Az új asztal a kezdőpont mögött van, fizikailag megfordulva érhető el. Kocka és palack: átadás; rúd: kétkezes forgatás; szerződés: dinamikus tépés; tompa eszköz és narancs hab: szúrás. Saját RESET és két zöld scene-választó gomb tartozik hozzá.

A `VRInteractionTest` külön, egyszerű munkapados scene ugyanilyen referenciatárgyakkal és kalapáccsal; a `FlightYoke` ülő repülésdemó. Mindhárom valódi scene/build asset. A `HunJam/Build Interaction Demos` a két új scene-t és saját generált prefabjait felülírja, ezért kézi változtatások után ne futtasd rá vakon. A meglévő zónát nem frissíti automatikusan. A `Generate Demo World` továbbra is teljes, tudatos újragenerálás; utána az interakciós bővítést is újra kell építeni.

Indítás, kezelőszervek, reset és ellenőrzés: [VR interakciós tesztek](../VR_INTERACTION_TESTS.md).
