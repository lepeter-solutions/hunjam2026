# Közeli tárgyfogás, két kéz és átadás

2026-11-07: a Grabbable `KeepKinematicOnRelease` és `ApplyTargetPose` bővítési pontjai engedik a FlightYoke cockpit-rögzítését. Normál tárgyak továbbra is dinamikusan elengedhetők és MovePosition-t használnak. A yoke elengedve is kinematic marad, és a rögzítési pontból kapja a pózát. Csak forrásáttekintés történt, teszt/fordítás nem indult a felhasználó kérésére.

## Cél és források
`Scripts/Interaction/Grabbable.cs`, `DemoHand.cs`, `Scripts/Player/MetaRoomScaleRig.cs`; QuestPlayer, MetaLeftHand/MetaRightHand, EnergyOrb és eszközprefabok. Meta XR Interaction SDK 207.0.1, XRI nélkül. A HunJam.Grabbable a natív Oculus.Interaction.Grabbable leszármazottja; annak ITransformer hívása mozgatja a Rigidbodyt. Ugyanaz az anchor nem birtokolhat két tárgyat. [Pontos SDK-határok és beállítás](../META_XR.md).

## Bemenetek és állapotok
A grip lenyomása a 12 cm-es közelségben kiválasztott tárgyhoz csatlakozik. Sikertelen első keresés után a nyomva tartott grip 0,05 másodpercenként újrapróbálkozik, így a már szorított kézzel megközelített vékony papír is felvehető. Sikeres fogás után az adott grip már felhasználtnak számít: átadás, reset vagy kényszerített elengedés után új lenyomás szükséges, nem kap fel automatikusan másik tárgyat. Köztes szilárd geometria blokkol. Trigger: fogott tárgy Use, üres kéz: közeli interakció. Nincsenek új input action assetek vagy mozgásbemenetek.
- Free → HeldLeft/HeldRight: első kéz. A nem DemoHand tesztanchor HeldRightként jelenik meg.
- TwoHands mód: második grip → HeldBoth. Két kézből számított orientáció, elsődleges kontaktpont, változatlan méret.
- TransferOnly (alapértelmezés): második grip → TransferPending. Csak az eredeti kéz mozgat; az átfedést bármelyik kéz elengedése zárja. Az eredeti grip elengedése fejezi be az átadást.
- Másodlagos elengedés: elsődleges marad. Elsődleges elengedés: másodlagos előlép. Az aktuális világpózból új attach offset készül; nincs szabad fizikai köztes frame.
- Utolsó kéz: Releasing → Free/Thrown. A tárgymozgásból számított sebesség legfeljebb 8 m/s és 20 rad/s. Azonos frame-es két elengedés is csak egyszer indít fizikát.

`Release(anchor, throwObject)` egy kezet old; a kompatibilis `Release(bool)` az összeset, például resetkor. Tiltás, megsemmisült anchor, tracking/fókuszvesztés takarítja a nyilvántartást. 50 cm-es egy fizikaframe-es kézugrás az érintett kezet dobási impulzus nélkül oldja. Két kéz 4 cm-nél kisebb távolságánál az utolsó orientáció marad; távolodáskor új referencia készül. A forgatási up-vektor továbbvitele kerüli a függőleges irány miatti átfordulást.

## Inspector és kapcsolatok
`gripMode`: TransferOnly vagy TwoHands. `primaryGrip`, `secondaryGrip`: opcionális fogási régiók, `gripRadius`: 14 cm. Üres referenciával szabad felületi fogás; másodlagos kéznél collider-közelség is kell. A régió nem kényszeríti ugrásszerűen a tárgyat egy pózra. Gizmo mutatja a zónákat. A korábbi `fingerClosure` csak a régi Unity kézanimáció kompatibilitási mezője; a Meta Capsense ujjait a natív kontrolleradatok hajtják.

Hivatalos Meta OVRHandPrefab kézmodellek, Capsense Natural pózokkal és Touch követéssel. A régi ControllerHandPose nem animálja ezeket. Nincs optikai grip-bemenet vagy ujjfizika. Gripkor rövid, támogatott eszközön működő haptika. A Capsense nem tárgyfüggő IK; a markolatok illeszkedése headseten hangolandó.

## Ellenőrzés és korlátok
A tartósan kinematic cockpit-kezelőszervek (yoke és throttle) nem kapják meg a szabad tárgyak Y<-3 mentőresetjét. Így a gázkar elengedett állása a világorigó alatt is megmarad; a normál eldobható tárgyak mentése változatlan. A throttle a meglévő Meta transformer bővítési pontot és cockpit-relatív követésvédelmet használja.

2026-11-07: a DemoHand közelségi keresése először szinkronizálja a Transformokat a fizikával. Ez szükséges a mozgó repülő interpolált cockpitjében látható yoke megfogásához. A Grabbable TrackingPosition bővítési pontja a yoke követésiugrás-védelmét a rögzítés helyi koordinátáiban számolja; normál tárgyaknál továbbra is világkoordinátában működik. A küszöb 50 cm, a valódi követésvesztés továbbra is elenged. Forrásáttekintés történt, futtatott teszt/fordítás nem.

Az SDK-átállás után a felhasználó kérésére nem futott teszt vagy build. Az alábbi tesztek korábbi, Meta előtti eredményei nem igazolják az átállás működését.
`GrabTransitionTests`: kétirányú átadás, kétkezes forgatás, elengedési folytonosság, egybeeső kezek, range, letiltás, anchor-megsemmisülés, egy anchor–egy tárgy és dobás. `DemoWorldTests`: energiakör, közeli kiválasztás, eszközök és room-scale regresszió. Tényleges eredmények: CHANGELOG.

Headseten külön ellenőrizendő a bal/jobb csukló, tenyér és fogáspont illeszkedése, kézváltás, gyors újrafogás, haptika, remegés és dobás. A kinematic tárgyak továbbra is falba tolhatók; nincs erővisszacsatolt kéz vagy általános fizikai megfogás. Az átadási átfedés a grip elengedéséig tarthat; az eredeti kezet nem engedjük el automatikusan.
