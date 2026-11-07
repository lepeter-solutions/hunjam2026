# Közeli tárgyfogás, két kéz és átadás

## Cél és források
`Scripts/Interaction/Grabbable.cs`, `DemoHand.cs`, `ControllerHandPose.cs`; QuestPlayer, EnergyOrb és eszközprefabok. Saját interakciós rendszer, XRI nélkül. Egyetlen Grabbable mozgatja a Rigidbodyt; ugyanaz az anchor nem birtokolhat két tárgyat.

## Bemenetek és állapotok
A grip új lenyomása a 12 cm-es közelségben kiválasztott tárgyhoz csatlakozik. Köztes szilárd geometria blokkol. Trigger: fogott tárgy Use, üres kéz: közeli interakció. Nincsenek új input action assetek vagy mozgásbemenetek.
- Free → HeldLeft/HeldRight: első kéz. A nem DemoHand tesztanchor HeldRightként jelenik meg.
- TwoHands mód: második grip → HeldBoth. Két kézből számított orientáció, elsődleges kontaktpont, változatlan méret.
- TransferOnly (alapértelmezés): második grip → TransferPending. Csak az eredeti kéz mozgat; az átfedést bármelyik kéz elengedése zárja. Az eredeti grip elengedése fejezi be az átadást.
- Másodlagos elengedés: elsődleges marad. Elsődleges elengedés: másodlagos előlép. Az aktuális világpózból új attach offset készül; nincs szabad fizikai köztes frame.
- Utolsó kéz: Releasing → Free/Thrown. A tárgymozgásból számított sebesség legfeljebb 8 m/s és 20 rad/s. Azonos frame-es két elengedés is csak egyszer indít fizikát.

`Release(anchor, throwObject)` egy kezet old; a kompatibilis `Release(bool)` az összeset, például resetkor. Tiltás, megsemmisült anchor, tracking/fókuszvesztés takarítja a nyilvántartást. 50 cm-es egy fizikaframe-es kézugrás az érintett kezet dobási impulzus nélkül oldja. Két kéz 4 cm-nél kisebb távolságánál az utolsó orientáció marad; távolodáskor új referencia készül. A forgatási up-vektor továbbvitele kerüli a függőleges irány miatti átfordulást.

## Inspector és kapcsolatok
`gripMode`: TransferOnly vagy TwoHands. `primaryGrip`, `secondaryGrip`: opcionális fogási régiók, `gripRadius`: 14 cm. Üres referenciával szabad felületi fogás; másodlagos kéznél collider-közelség is kell. A régió nem kényszeríti ugrásszerűen a tárgyat egy pózra. Gizmo mutatja a zónákat. `fingerClosure`: tárgyfüggő ujjzárás, alap 0,75.

Hivatalos Unity XR Hands 1.5.0 HandVisualizer modellek Touch követéssel. `ControllerHandPose.relaxedCurl` 0,14; `response` 28/s; a mutatóujj fogáskor is hajlik, triggerrel tovább zárható. Nincs optikai kézkövetés vagy ujjfizika. Gripkor rövid, támogatott eszközön működő haptika.

## Ellenőrzés és korlátok
`GrabTransitionTests`: kétirányú átadás, kétkezes forgatás, elengedési folytonosság, egybeeső kezek, range, letiltás, anchor-megsemmisülés, egy anchor–egy tárgy és dobás. `DemoWorldTests`: energiakör, közeli kiválasztás, eszközök és room-scale regresszió. Tényleges eredmények: CHANGELOG.

Headseten külön ellenőrizendő a bal/jobb csukló, tenyér és fogáspont illeszkedése, kézváltás, gyors újrafogás, haptika, remegés és dobás. A kinematic tárgyak továbbra is falba tolhatók; nincs erővisszacsatolt kéz vagy általános fizikai megfogás. Az átadási átfedés a grip elengedéséig tarthat; az eredeti kezet nem engedjük el automatikusan.
