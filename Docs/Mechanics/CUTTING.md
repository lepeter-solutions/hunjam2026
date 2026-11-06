# Vágás tárggyal
## Cél és források
`Scripts/Interaction/Tool.cs`, `Cuttable.cs`; Blade.prefab, DemoWorld narancssárga vágóblokk.
A penge mozgatása a kijelölt blokkot két külön fizikai darabra bontja, amelyek megfoghatók.
## Bemenetek és állapot
Grip tartása, tényleges lendítés. A penge végén lévő kontaktpont ugyanazt a sebességküszöbös, akadályokat figyelembe vevő söprést használja, mint a kalapács. Álló vagy eldobott penge nem vág.
Egész -> vágott állapot egyszer történik; az eredeti renderer/collider kikapcsol, két Rigidbody darab jelenik meg. RESET megsemmisíti a darabokat és visszakapcsolja az eredeti blokkot.
## Hangolás és korlátok
Minimum 1 m/s; 2,5 cm kontaktpontsugár. A darabok az eredeti blokk anyagát, méretét és forgatását öröklik.
Ez előre meghatározott, középen kétfelé osztó blokkprototípus. Nem tetszőleges mesh vágása és nem a penge teljes élének modellezése; később cserélhető mesh-szeletelésre.
## Ellenőrzés
PlayMode: nyugalmi penge nem vág, valóban mozgatott és fogott penge a fizikai söprésen keresztül vág; darabok/reset ellenőrzése. Headseten mindkét kéz, különböző lendítési irányok, sebességek és akadályok külön ellenőrizendők. Eredmény: CHANGELOG.
