# Manhattan repülőpálya

## Cél és assetek
A `manhatten` branch városa a `Assets/_Game/Scenes/Manhattan.unity` scene-ben, a közös `Prefabs/TrainingAircraft.prefab` egy példányával. A gép (500, 120, -300) világpozícióból, +Z irányban, 16 m/s sebességgel indul. A prefab tartalmazza a pilótaülést, QuestPlayer/Meta riget, yoke-ot, műszereket és reset gombot. A korábbi külön Main Camera inaktív. Manhattan az első engedélyezett build-scene; a régi demók továbbra is elérhetők.

## Bemenetek, állapotok és hangolás
Touch grip: egy- vagy kétkezes yoke-fogás. Fordítás: bedöntés; húzás/tolás: emelkedés/süllyedés. Bal zöld gomb üres kézzel és triggerrel: gép reset és üléskalibráció. Jobb gomb: DemoWorld. Részletek és végállások: [FLIGHT_YOKE.md](FLIGHT_YOKE.md). A játékos saját mozgása room-scale; a jármű szállítja a riget. Nincs joystickos locomotion.

A kezdőpóz a scene prefab-override-ja. A sebesség és a kormány érzékenysége a közös aircraft prefabban hangolható. Lejjebb helyezett pályához a FlightVehicle `Reset Below Altitude` kapcsolója alapból kikapcsolt; bekapcsolva a `Reset Altitude` világkoordinátás Y értékét a kezdőpont alá kell állítani (negatív érték is megadható). A 600 m felső és 1400 m vízszintes reset-határ változatlan.

## Merge-döntések
A `codex/vr-two-hand-demos` fogási, kéz-, tépési, szúrási és repülési rendszere maradt az integrált runtime. A `manhatten` városassetjei, BlenderScenes, hangok és Vérszerződés pálya átkerültek. Az eltérő GameItem/MetaUseAction alapú demó-runtime és InteractionSandbox nem váltja fel a kétkezes branch rendszereit; ezek az eredeti branch történetében megmaradnak.

A csomagmanifest és a Unity által korábban létrehozott lockfile együtt a manhatten verziója: az érkező XRI/Hands minták függőségei megmaradnak, de nem vezérlik a repülő rigjét. A BlenderScenes mappa GUID-ja ütközött a meglévő UnityHands mappáéval; csak az érkező mappa kapott új GUID-ot, az assetek referenciái változatlanok.

## Ellenőrzési állapot
2026-11-07: forrás- és YAML-áttekintés, prefab-bekötés, külön kamera kikapcsolása és build-lista szerkesztése. A felhasználó kérésére nem indult Unity, fordítás, Play mód, teszt vagy build.

Későbbi manuális ellenőrzés: Manhattan megnyitása; pilótaülésből indulás; többperces repülés közben a yoke helyben maradása; egy/két kéz, elengedés, végállások, reset; épületütközés; Quest két szem, követés és képkockaidő. A kezdőpozíció látványa, a város ütközői és Quest teljesítménye futás közben még nem ellenőrzött.
