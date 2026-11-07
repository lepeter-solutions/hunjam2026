# Robotkarakter

## Cél és források
`Scripts/Characters/GuideRobot.cs`, `Prefabs/GuideRobot.prefab`. Primitívekből álló, placeholder NPC, akivel a játékos beszélgetést kezdeményezhet. A fej a játékos felé fordul; a robot nem járkál.

## Viselkedés
Üres kézzel a test/fej közelében trigger indítja az interakciót; a közeli cél kiválasztását a Meta SDK adja. Várakozáskor elindítja a háromgömbös kört. Játék közben kiírja az aktuális haladást. Befejezés után ugyanígy új kört kezd.

`DemoGame` kezeli az állapotot, a robot csak kommunikál és indít. `dialogue` a világban megjelenő TextMesh; `head` a követő fejrész. A scene-beli példány kapja meg a DemoGame referenciát, a prefab nem hivatkozik scene-objektumra.

## Ellenőrzés és korlátok
Várakozás -> kezdés -> részleges haladás -> teljesítés -> újrakezdés. Távolról és falon keresztül ne lehessen beszélni. Nincs szinkronhang, animációs állapotgép vagy lokalizáció; a demó feliratai angolok. Tényleges teszteredmények: CHANGELOG.
