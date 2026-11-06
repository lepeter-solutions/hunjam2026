# Ütés tárggyal
## Cél és források
`Scripts/Interaction/Tool.cs`, `ImpactTarget.cs`; Hammer.prefab és DemoWorld cián céltömb.
A fogott kalapács fizikai lendítése növeli a cél fölötti ütésszámlálót. A tárgyak szilárd Rigidbody-ütközői a mozgatható tárgyakat is meglökhetik.
## Bemenetek és állapot
Nincs támadógomb: grip tartása mellett lendítsd a kontrollert. A fej kontaktpontjának előző és jelenlegi helye közötti SphereCast vizsgálja az érintést.
Csak fogott, legalább 0,15 s óta stabilan tartott eszköz számít. Minimum 1 m/s, 2,5 cm sugár, 0,25 s céloldali ismétlésgátlás. Az első szilárd akadály blokkolja a távolabbi célokat; 50 cm-es frame-ugrás nem ütés.
## Hangolás és korlátok
Tool minimumSpeed/contactRadius és kontaktpont. Csak a kalapács fejének kijelölt kontaktpontja számít találatnak; nincs sérülésrendszer vagy haptika. Az ütésszámláló a sebességküszöböt mutatja, nem hiteles erőmérés.
## Ellenőrzés
Automatizált küszöb- és ismétlésgátlás-ellenőrzés; hardveren tényleges lendítés, nyugalmi érintés, tracking ugrás és cél RESET ellenőrzendő. Eredmény: CHANGELOG.
