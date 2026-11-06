# Demóvilág

## Forrás és elrendezés
`Scenes/DemoWorld.unity`, `Editor/DemoProjectTools.cs`, `Art/Materials/`. 22 × 22 m-es körbejárható udvar, alacsony határolófalakkal, két kerülhető akadállyal és útjelzőkkel. Kezdőpont a déli részen, robot előtte, célállomás középen, gömbök az udvar három pontján.

Valódi, szerkeszthető scene; a játékos, robot és gömb külön prefab. Egy directional light, egyszerű URP anyagok, postprocess nélkül. A World hierarchia tartalmazza a statikus környezetet. A `WalkableSurface` markerrel ellátott padló teleportálható.

## Generálás és közös szerkesztés
`HunJam/Generate Demo World` újragenerálja a demóvilágot és a hozzá tartozó prefabokat/anyagokat. Meglévő scene esetén figyelmeztet, mert a kézi szerkesztéseket felülírná. Nem fut automatikusan projektmegnyitáskor. Batch ellenőrzésre: `HunJam.Editor.DemoProjectTools.Prepare`, kizárólag ellenőrzési másolatban.

A scene-nek egyszerre egy szerkesztője legyen; új pályához külön scene készüljön. A beépített primitívek placeholder assetek, nincs külső grafikai licencfüggőség.

## Ellenőrzés és korlátok
Scene megnyitása hiba és hiányzó script nélkül; a játékos a padlón indul; robot és táblák olvashatók; mindhárom gömb és a cél körbejárható. A validátor ellenőrzi a scriptreferenciákat, a játékost és a három gömböt. A render és Quest teljesítmény külön ellenőrzendő. Eredmények: CHANGELOG.

2026-11-06: Unity batch render alapján a geometria, a feliratok és a különböző színű anyagok megjelenése ellenőrizve. [Előnézet](../Images/DemoWorld.png). A képi ellenőrzés Editor-render; nem headsetes teljesítménymérés.
