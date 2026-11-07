# Flight Yoke — ülő járműdemó

## Cél, források és indítás
`Scenes/FlightYoke.unity`, `Prefabs/TrainingAircraft.prefab`; `FlightYoke.cs`, `FlightVehicle.cs`, `FlightSeat.cs`, `DemoSceneButton.cs`; Editor: `InteractionDemoTools.cs`.
Külön nyitható Play módban, vagy a DemoWorld / VRInteractionTest FLIGHT YOKE gombjával (üres kéz, közelség + trigger). Quest Link és két Touch kontroller, illetve standalone APK szükséges a valódi VR-próbához. A gép 120 m magasan, 16 m/s utazósebességgel indul. A játékos közvetlenül a pilótaülésben van.

## Bemenetek és állapot
A két cián markolat egy vagy két kézzel fogható. Óramutató szerinti elfordítás jobbra dönt, előretolás lefelé, hátrahúzás felfelé irányít. A második kéz átlagolt parancsot ad; csatlakozás és kézváltás újrabázisolja a referenciákat. Elengedve rugó/csillapítás jellegű visszacentírozás történik. A yoke nem válik eldobhatóvá.
A kormány állása csak repülési parancs: a gép Rigidbodyje előrehajtást, sebességfüggő felhajtóerőt, légellenállást, oldalcsúszás-csillapítást és helyi forgatónyomatékot kap. A bedöntés koordinált yaw-parancsot ad; elengedve enyhe stabilizáció működik. A kamera nem külön forgatott repüléspótlék: a rig a jármű gyermeke.
A FlightSeat megvárja a fejkövetést és a DemoPlayer padlóbeállítását, majd a valós fejmagasságot az ülés szemponthoz illeszti. Fej-/kézmozgás utána teljes egészében room-scale. Headset nélküli kameranézet is az ülésben indul. Nincs joystickos séta, fordulás vagy teleport.

## Inspector és reset
- Yoke: `maximumAngle` ±40°, `maximumTravel` ±9 cm; `deadZone` 0,06; `responseTime` 0,045 s; `returnTime` 0,25 s. Mounting alatt a lokális Z a push/pull tengely.
- Jármű: `cruiseSpeed` 16 m/s; `pitchRate` 18°/s; `rollRate` 30°/s; `stabilization` 0,65. A műszer 10 Hz-en mutat sebességet, magasságot, pitch/bank értéket.
- RESET / RECENTER: üres kéz + trigger a bal zöld gombon; elengedi a yoke-ot, visszaállítja a gépet és újrailleszti az ülést.
- Automatikus reset: 8 m alatti / 600 m fölötti magasság, 75°-nál nagyobb bank, 50°-nál nagyobb pitch, 1400 m-es vízszintes körzet elhagyása vagy 2 m/s feletti ütközés. Jobb gomb: DemoWorld.

## Ellenőrzés és korlátok
2026-11-07: a yoke rögzített cockpit-vezérlő. Elengedéskor sem kapcsol dinamikus Rigidbodyre, nincs gravitáció vagy dobási sebesség. A helyi alaphelyzetből, korlátozott Z eltolásból és elfordulásból számított póz közvetlenül rögzíti a fizikai testet; LateUpdate a repülő interpolált megjelenítési pózához is illeszti. Így nincs külön, lemaradó világkoordinátás MovePosition-pályája. A prefab és generátor gravitáció nélküli, kinematic, ContinuousSpeculative beállítással készül. A ±40° és ±9 cm végállás, kétkezes átlagolás és visszacentírozás megmarad. Csak forrásáttekintés történt; felhasználói kérésre nem futott fordítás vagy teszt. A többperces repülés és reset utáni rögzítés headseten ellenőrizendő.

Ugyanez a prefab a [Manhattan pályán](MANHATTAN_FLIGHT.md) is használható.

EditMode: dead zone és határolás. PlayMode: üléshelyzet, tényleges haladás, jobb bank és pull-up, egy-/kétkezes fogás, kézváltás folytonossága, kinematic elengedés, centírozás és reset. Tényleges eredmény: CHANGELOG.
Könnyített demonstráció, nem repülésszimulátor: nincs átesésmodell, throttle vagy aerodinamikai szárnyfelület-szimuláció. Az automatikus reset azonnali; headsetes komfortját még ellenőrizni kell. A virtuális repülés érzékenysége, horizont, két szem képe, Link/Quest követés, ülés és újracentírozás, valamint standalone teljesítmény külön hardverteszt. A normál világ rigjének helyváltoztatását a flight komponensek nem módosítják.
