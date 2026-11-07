# Rögzített pilótaülés

## Cél és források
A felhasználó kifejezett kérése szerint a cockpit Manhattan jelenetében nem lehet fizikailag kisétálni a repülőből. A PilotPivot a pilóta szemhelyzetét és kezdeti előreirányát jelöli. Források: `FlightSeat.cs`, `DemoPlayer.cs`, `Scenes/Manhattan.unity`. Más scene-ekben az alap room-scale viselkedés megmarad.

## Bemenetek és állapotok
A FlightSeat megvárja az érvényes fejkövetést, a nézés vízszintes irányát induláskor a PilotPivothoz igazítja, majd a teljes játékosrig helyzetét úgy korrigálja, hogy a szemközép a pivoton maradjon. A fej forgatása és a kézmozgás megmarad; a fej helyváltoztatása, lehajolás és séta sem viszi ki a kamerát az ülésből. A kezek a fejhez viszonyítva követnek, nem maradnak hátra a valós séta során. A játékos a yoke-kal és a gázkarral irányít.

A korrekció Meta UpdatedAnchors után, LateUpdate-ben és renderelés előtt is megtörténik, a natív fej-/kontrollerpózok felülírása nélkül. Tracking-vesztés alatt nincs új kalibráció. RESET / RECENTER újrakalibrálja a kezdő nézésirányt.

## Hangolás
Manhattan: a felhasználó által jelzett 90°-os balra nézést a PilotPivot helyi Y=+90° forgatása korrigálja. Ez az indulási és RECENTER célirányt jobbra fordítja; a pivot pozíciója változatlan. Más jelenetekre nem vonatkozik. A korrekció utáni szélvédőirány headseten még ellenőrizendő.
FlightSeat: `Pilot Pivot` explicit Transform-referencia, `Lock To Pilot Pivot` csak a cockpit scene-ben engedélyezve. A pivot legyen a repülő gyermeke, a kívánt szemmagasságon, +Z iránya a pilóta előreiránya. A közös prefab alapból nem tiltja a room-scale mozgást. Referencia nélkül a régi Seat Eye a pozíciótartalék.

## Ellenőrzés és korlátok
2026-11-07: kameraátadáskor külön újrakalibrálás történik. A korábbi statikus/fallback kamerával végzett kalibráció nem érvényes az új Meta kamerára; a FlightSeat megvárja annak érvényes követését, majd újra a PilotPivot +Z irányába igazít. Függőleges nézésnél nincs használható vízszintes irány, ezért később próbálja újra. Statikus forrásáttekintés történt; fordítás és headsetes próba kérésre nem. Ellenőrizendő: eltérő valós szobairányból indított Link Play, később csatlakozó headset, indulás után szabad fejfordítás és RESET / RECENTER.
Forrásáttekintés; felhasználói kérésre nincs fordítás, Play mód vagy build. Későbbi Quest-próba: kezdőpozíció és előreirány; fizikai séta/lehajolás után változatlan szemhelyzet; szabad fejforgatás és kézvezérlés; gyors fejmozgás, tracking-visszatérés, RESET és gépfordulás közbeni rögzítés; többi scene-ben továbbra is room-scale. A teljes fejpozíció-zárolás szándékosan megszünteti a valós fejeltolódás virtuális leképezését; komfortja hardveren ellenőrizendő.
