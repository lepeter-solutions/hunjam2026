# Energiagyűjtő játékkör

## Cél és források
`Scripts/Core/EnergyRound.cs`, `DemoGame.cs`, `Scripts/World/EnergyReceiver.cs`. A játékos három különböző gömböt juttat a zöld központi állomásba.

## Állapotok
`Waiting`: csak a robot indíthatja a kört. `Playing`: az elengedett, a scene három regisztrált gömbje közé tartozó gömb pontot ér, majd eltűnik. `Complete`: mindhárom kézbesítve; a robot új kört indíthat. Desktop Backspace fejlesztői újraindítás.

Az azonosítóhalmaz megakadályozza, hogy az ismétlődő fizikai triggerhívás ugyanazt a gömböt többször számolja. Kézben tartott vagy idegen tárgy nem ér pontot. A célzóna trigger, a talapzat szilárd. Új körben a gömbök eredeti pozíciója és sebessége visszaáll.

## Hangolás és bővítés
A DemoGame `cells` tömbje határozza meg a célmennyiséget. A felirat demóra szabott; célmennyiség változásakor a sikerfeliratot és a robot szövegét is frissíteni kell. Nincs időlimit vagy mentés.

## Ellenőrzés
EditMode tesztek: kezdés előtt nincs pont; azonos gömb nem dupláz; befejezés után nincs új pont; új kör törli az azonosítókat. Játékban: elengedés a gyűjtőbe növeli a számlálót, három után siker, újraindítás mindhárom gömböt visszaadja. Tényleges eredmények: CHANGELOG.
