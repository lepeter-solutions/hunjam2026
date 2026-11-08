# Manhattan távoli környezete

A Manhattan jelenetben a kikötő vizét a kilenc eredeti `WaterBlock_50m` prefabpéldány rajzolja ki a korábbi helyén. A `Manhattan Surroundings` gyökérhez a 16 parti szegmens és az egyetlen meshbe összevont, 301 épületből álló távoli várossziluett tartozik. Ezek dekorációk, játékosi bemenet és collider nélkül.

A háttér gyökere világkoordinátában (0, 0, 4500). Ez 4,5 km-rel tolja el a teljes partvidéket a repülőpálya +Z irányában. A szegmensek és az épületek együtt mozdulnak, így a méretük, magasságuk és egymáshoz igazításuk megmarad. A város és a víz önálló objektumok. A jelenet a korábbi szürke, Exp2 módú ködöt használja 0,006 sűrűséggel. A háttér saját shaderében a távolsági pára a Landscape material Haze Distance és Haze Color értékeivel hangolható.

A források a `My project/Assets/_Game/Scenes/Manhattan.unity` jelenet, az `Art/ManhattanSurroundings/` statikus assetjei, az `Art/Materials/ManhattanLandscape.shader` és a `Scripts/World/ManhattanBackdrop.cs` komponens. Utóbbi a pilóta és a később létrejövő Meta szemkamera látótávját a scene-ben megadott 9000 méterre emeli. A repo gyökeréből futtatható `Tools/generate_manhattan_surroundings.py` a fenntartott 88001108… azonosítókon építi újra a hátteret. A part alakja a margin és height függvényekkel hangolható.

2026-11-08. A jelenet és a generátor diffjét összevetettük a környezet bevezetése előtti jelenettel. Unityben és headseten még ellenőrizendő a kilenc vízcsempe illeszkedése, a part láthatósága a cockpitből induláskor és forduláskor, valamint a köd erőssége és a nagyobb látótáv képkockaideje mindkét szemen. A korábbi kérés szerint build és teszt nem futott.
