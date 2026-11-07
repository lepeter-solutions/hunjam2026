# Repülőhang függőleges sebesség alapján

## Cél és források
A Training Aircraft meglévő AudioSource hangmagassága süllyedéskor nő, emelkedéskor csökken. Forrás: `Scripts/Player/FlightVehicle.cs`; Manhattan cockpit AudioSource. A `Flight Audio` referencia megadható Inspectorban; üresen először a gép gyökerén, majd gyermekein keresi a meglévő AudioSource-ot. Nem hoz létre új hangforrást.

## Bemenet és állapot
A Rigidbody világkoordinátás függőleges sebessége vezérli, nem a yoke állása vagy a gép dőlésszöge. Sík repülésnél a forrás induláskori pitch értékére áll vissza. Folyamatos SmoothDamp simítás; a kijelző 10 Hz-es frissítése nem korlátozza a hangot. Reset és komponensletiltás visszaállítja az eredeti pitch-et. A lejátszás, loop, hangerő és klip beállítása megmarad.

## Hangolás
`Vertical Pitch Amount`: 0.3, az alap pitch ±30%-a. `Vertical Speed For Full Pitch`: 10 m/s; ennél nagyobb emelkedés/süllyedés már nem módosít tovább. `Audio Pitch Response Time`: 0.4 s. Alap pitch 1 esetén emelkedésnél minimum 0.7, süllyedésnél maximum 1.3. Amount=0 kikapcsolja az eltérést.

## Ellenőrzés és korlátok
Forrás/diff áttekintve, kérésre nincs fordítás vagy teszt. A mentett helyi cockpit AudioSource klip/resource mezője a munka idején üres volt: hallható eredményhez hangklip és lejátszás szükséges. Későbbi próba: sík repülés, emelkedés, süllyedés, ±10 m/s túllépése, reset, hiányzó AudioSource és kijelző nélküli működés. A Doppler-beállítás külön hangmagasság-változást is okozhat; azt nem módosítja a kód.
