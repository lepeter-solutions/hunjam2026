# Kézben tartott tárgy használata
## Cél és források
`Scripts/Interaction/Tool.cs`, `GameItem.cs`, `WorkshopReset.cs`; Lamp.prefab, DemoWorld.
A lámpa a használható tárgy prototípusa.
## Bemenetek és állapot
Grip: fogás. Fogás közben trigger: lámpafény be/ki. Letett lámpán a Use nem kapcsol. Elengedve megtartja a fényállapotát.
Üres kézzel a zöld RESET gomb közelében trigger: eszközök eredeti helyre, lámpa kikapcsolva, vágóblokk és ütésszámláló alaphelyzetbe. Az energiakört külön a robot kezeli.
## Hangolás és bővítés
Spot light: 5 m, intenzitás 5, 60 fok. ToolKind.Lamp és Light referencia. További tárgyak GameItem.Use felülírásával bővíthetők.
## Ellenőrzés
PlayMode ellenőrzi, hogy csak fogva kapcsolható, kétszer kapcsolva kiállapotot kap, és reset elengedi. Hardveres trigger/megvilágítás ellenőrzés szükséges; eredmény CHANGELOG.
