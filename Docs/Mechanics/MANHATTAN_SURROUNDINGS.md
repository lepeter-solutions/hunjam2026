# Manhattan távoli környezete

## Cél és források
A város ne egy üres, négyszögletes vízmező közepén álljon. A `Manhattan.unity` új `Manhattan Surroundings` gyökere összefüggő vizet, szabálytalan parti szárazföldet, lankás domborzatot és alacsony várossziluettet ad. A meglévő naplementés skybox változatlan. A környezet művészileg összeállított, New York jellegű kikötői háttér, nem valódi térinformatikai Manhattan-rekonstrukció.

Források: `Assets/_Game/Art/ManhattanSurroundings/` statikus mesh- és materialassetek; `Art/Materials/ManhattanLandscape.shader`, `ManhattanHarbor.shader`; `Scripts/World/ManhattanBackdrop.cs`; `Tools/generate_manhattan_surroundings.py`. Nincsenek letöltött assetek, új csomagok vagy külső licencfüggőségek.

## Felépítés és kapcsolatok
- 16 domborzati szegmens, egy összevont távoli városmesh (301 épület), egy 18 km sugarú vízkorong. Összesen 42 148 háromszög, 18 renderer; nincs árnyékvetés vagy futásidejű mesh-generálás.
- A vízszint világ Y=-186, a meglévő városi alap alatt. Nyugati/keleti part, északi szárazföld és délnyugati sziget öleli körül a játékterületet. A domborzat külső széle szabálytalan parttal lesüllyed a víz alá, még a mesh-rács határa előtt.
- A korábbi kilenc WaterBlock_50m példány inaktív scene-override-ot kapott; nem töröltük az eredeti asseteket. A város, repülő, cockpit, ütközések és korábbi helyi módosítások megmaradnak.
- A talaj csúcspontszíneket, felületnormál szerinti napfényt és távolsági párát használ. A víz opaque URP shader: lassú normálhullámzás, Fresnel-szerű horizontfény és napcsillanás, tükrözési kamera nélkül. Mindkét shader stereo instancing makrókat tartalmaz.
- A ManhattanBackdrop a jelenet DemoPlayer kamerájának far clip értékét legalább 6000 m-re emeli. A később létrejövő Meta szemkamerát is kezeli; letiltáskor visszaállítja az előző értéket. A közös QuestPlayer/TrainingAircraft prefab változatlan.

## Hangolás és bővítés
A környezet egyben kapcsolható a scene `Manhattan Surroundings` objektumán; a régi víz külön visszakapcsolható. `View Distance`: kamera látótáv. A Landscape material `Haze Distance` alapértéke 2400 m, `Haze Color` a távoli kékesszürke pára; Harbor material `Water Color` a víz alapszíne. A gameplay RenderSettings ködsűrűsége továbbra is 0,003. A háttér saját távolsági párát használ, hogy a távoli partokat ne nyelje el teljesen ez a sűrű helyi köd.

A generátor a repo gyökeréből `python Tools/generate_manhattan_surroundings.py` paranccsal futtatható. Determinisztikus geometriát/GUID-okat ír és csak a saját, fenntartott 88001108… scene-azonosítóit frissíti; a víz kikapcsolását is beállítja. Nem indít Unityt. A generált objektumok kézi módosításait egy újragenerálás felülírja. Domborzati paraméterek: `margin`, `height`, `relief`; vízszint: `SEA`; várossűrűség és épületmagasság: a generátor rácsa és rögzített seedje.

## Ellenőrzés és korlátok
2026-11-08: assetgenerálás, forrás/scene-diff áttekintése; nincs Unity-indítás, shader/C# fordítás, Play mód, teszt vagy build a felhasználó korábbi kérésének megfelelően. A látvány még nem igazolt headseten. Módosítások commitolatlanok.

Későbbi manuális ellenőrzés: első cockpit spawn és reload után is látható távoli part; körberepülés és magasabbról lefelé nézés négyszögletes vízszél nélkül; városi alap és víz találkozása; napsütés/pára összhangja; két szem és mozgás közbeni csillanás; Quest GPU-idő és teljesítmény a megnövelt látótáv mellett. A háttér dekoráció: nincs collider, leszállópálya, növényfizika vagy részletes épületbelső. Közelről az egyszerűsített távoli épületek és talaj nem részletes játékterületnek készültek. A nagyobb látótáv teljes jelenetre gyakorolt költsége eszközön mérendő.
