# Demóvilág
## Forrás és elrendezés
`Scenes/DemoWorld.unity`, `Editor/DemoProjectTools.cs`, `Art/Materials/`, `Prefabs/`.
A nagy udvar megmarad háttérként, de minden játékmechanika a kezdőpont körüli kis műhelyben van: balra gömbök és gyűjtő, elöl munkapad és ütő/vágó cél, jobbra kalapács, penge, lámpa. A robot balra hátul áll.
A kezdőpont (0,0,0). A fő eszközök körülbelül egy méteren belül helyezkednek el; elérésükhöz fizikai lépés, fordulás, nyúlás szükséges. A távoli udvar bejárása nem feladat.
## Generálás és szerkesztés
A scene valódi Unity asset, a játékos, robot, gömb és eszközök prefabok. HunJam/Generate Demo World felülírja a generált demót; kézi módosítások előtt commit. Batch Prepare csak ellenőrzési másolatban.
A kéz egyszerű tenyér és ujjak, primitív placeholder, nem külső modell. Egy directional light, egyszerű URP anyagok; a használható lámpa helyi spotfényt ad.
## Ellenőrzés
Scriptreferenciák, három gömb, játékos, PlayMode mechanikatesztek és képi előnézet. Hardveren a rendelkezésre álló biztonságos játéktérhez igazítani kell az elrendezést és a munkapad magasságát. Teljesítmény és ergonomikus elérhetőség külön Quest-ellenőrzés.
[Előnézet](../Images/DemoWorld.png). Eredmények: CHANGELOG.
