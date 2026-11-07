# Cockpit gázkar

## Cél és források
A jobb oldali, talapzatra rögzített gázkar a motor tolóerejét szabályozza. Forrás: `Scripts/Interaction/FlightThrottle.cs`, `Scripts/Player/FlightVehicle.cs`; asset: `Prefabs/TrainingAircraft.prefab`; generátor: `Editor/InteractionDemoTools.cs`. A Manhattan és FlightYoke scene ugyanazt a módosított prefabot örökli.

## Bemenetek és állapotok
Touch grip a kar közelében, majd kézzel előre/hátra billentés. Előre: több gáz; hátra: kevesebb. A meglévő Meta Grabbable/ITransformer adapter kezeli a fogást és átadást. TransferOnly: második kéz átveheti, de az első elengedéséig nem vezérel. Elengedve a kar megtartja állását; nincs rugós középállás, gravitáció vagy dobás. A cockpit helyi koordinátáiban rögzített, ±35° végállással. RESET / RECENTER 70%-ra állítja.

A FlightVehicle 0–100% között olvassa a Value értéket. A kar tolóerőt szabályoz, nem közvetlen sebességet: gyorsítás/lassítás fokozatos, a gravitáció és légellenállás is számít; nulla gázon siklik, nem áll meg a levegőben. A műszer THR százalékot és V/S függőleges sebességet mutat (negatív = süllyedés).

## Hangolás és kapcsolatok
Kar: `maximumAngle` 35°, `initialThrottle` 0.7. Jobb oldali pivot: (0.55, 0.65, 0.15) a géphez képest. FlightVehicle: `maximumThrustAcceleration` 3 m/s²; `dragCoefficient` 0.0082; a 16 m/s `cruiseSpeed` a kezdősebesség és a felhajtóerő referenciaértéke, nem automatikusan tartott sebesség. Kar nélküli régi scene-ben 70% gáz az alapérték.

A kinematic cockpit-kezelőszervek kimaradnak a Grabbable Y<-3 tárgymentő resetjéből, így negatív világmagasságon is megmarad a kar állása. A gép automatikus resetkapcsolója változatlanul alapból kikapcsolt.

## Ellenőrzés és korlátok
Tényleges eredmény: forrás/diff és prefab-hivatkozások statikus áttekintése. Felhasználói kérésre nincs fordítás, Play mód, teszt vagy build; hardveres működés nem igazolt.

Későbbi próba: jobb kézzel grip, mindkét végállás, újrafogás ugrás nélkül, kézváltás, elengedés után állástartás, reset 70%-ra. Sebességváltozás sík repülésben 0/70/100%-nál; negatív Y-nál állástartás; yoke előretolására negatív V/S és csökkenő ALT; húzásra emelkedés. Egyszerűsített aerodinamika; nincs hajtómű-késleltetés, szél vagy részletes propeller-/átesésmodell. Quest komfort és teljesítmény még ellenőrizendő.
