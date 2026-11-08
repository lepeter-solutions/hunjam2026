# Manhattan repülőpálya

## Cél és assetek
A `manhatten` branch városa a `Assets/_Game/Scenes/Manhattan.unity` scene-ben, a közös `Prefabs/TrainingAircraft.prefab` egy példányával. A gép (500, 120, -300) világpozícióból, +Z irányban, 16 m/s sebességgel indul. A prefab tartalmazza a pilótaülést, QuestPlayer/Meta riget, yoke-ot, műszereket és reset gombot. A korábbi külön Main Camera inaktív. Manhattan az első engedélyezett build-scene; a régi demók továbbra is elérhetők.

## Bemenetek, állapotok és hangolás
2026-11-08: a négyszögletes vízblokkok helyett a jelenet `Manhattan Surroundings` gyökere összefüggő vizet, szabálytalan partokat, távoli várossziluettet és lankás domborzatot ad. A korábbi vízpéldányok inaktívan megmaradnak. A scene kamera látótávolsága 6000 m; [részletes hangolás és ellenőrzési korlátok](MANHATTAN_SURROUNDINGS.md).

Köd: a jelenet RenderSettings exponenciális négyzetes ködöt használ; Fog Density 0,003 (2026-11-08: a korábbi 0,006 fele). A szín és a ködmód változatlan. A szerializált érték ellenőrizve, futásidejű látvány nem tesztelt.
Cockpit branch, felhasználói kérésre: a Training Aircraft alatti PilotPivot a rögzített szemhelyzet és indulási előreirány. A FlightSeat scene-override-jában `pilotPivot` erre mutat, `lockToPilotPivot=1`. Így nincs fizikai kisétálás/lehajolás miatti helyváltoztatás; fejforgatás és kézvezérlés megmarad. Ez felülírja az alábbi régebbi room-scale leírást kizárólag ebben a scene-ben. [Részletes működés és ellenőrzés](PILOT_SEAT.md).
Touch grip: egy- vagy kétkezes yoke-fogás. Fordítás: bedöntés; húzás/tolás: emelkedés/süllyedés. Bal zöld gomb üres kézzel és triggerrel: gép reset és üléskalibráció. Jobb gomb: DemoWorld. Részletek és végállások: [FLIGHT_YOKE.md](FLIGHT_YOKE.md). A játékos saját mozgása room-scale; a jármű szállítja a riget. Nincs joystickos locomotion.

A kezdőpóz a scene prefab-override-ja. A sebesség és a kormány érzékenysége a közös aircraft prefabban hangolható. A FlightVehicle `Automatic Reset Enabled` főkapcsolója alapból kikapcsolt: ilyenkor magasság, bedőlés, távolság és ütközés sem reseteli a gépet. Bekapcsolva minden feltétel külön kapcsolható és hangolható; a `Reset Altitude` világkoordinátás Y értékét a kezdőpont alá kell állítani (negatív is lehet). A kézi reset mindig használható. Részletek: [Flight Yoke](FLIGHT_YOKE.md).

## Merge-döntések
A `codex/vr-two-hand-demos` fogási, kéz-, tépési, szúrási és repülési rendszere maradt az integrált runtime. A `manhatten` városassetjei, BlenderScenes, hangok és Vérszerződés pálya átkerültek. Az eltérő GameItem/MetaUseAction alapú demó-runtime és InteractionSandbox nem váltja fel a kétkezes branch rendszereit; ezek az eredeti branch történetében megmaradnak.

A csomagmanifest és a Unity által korábban létrehozott lockfile együtt a manhatten verziója: az érkező XRI/Hands minták függőségei megmaradnak, de nem vezérlik a repülő rigjét. A BlenderScenes mappa GUID-ja ütközött a meglévő UnityHands mappáéval; csak az érkező mappa kapott új GUID-ot, az assetek referenciái változatlanok.

## Ellenőrzési állapot
2026-11-07: forrás- és YAML-áttekintés, prefab-bekötés, külön kamera kikapcsolása és build-lista szerkesztése. A felhasználó kérésére nem indult Unity, fordítás, Play mód, teszt vagy build.

Későbbi manuális ellenőrzés: Manhattan megnyitása; pilótaülésből indulás; többperces repülés közben a yoke helyben maradása; egy/két kéz, elengedés, végállások, reset; épületütközés; Quest két szem, követés és képkockaidő. A kezdőpozíció látványa, a város ütközői és Quest teljesítménye futás közben még nem ellenőrzött.
