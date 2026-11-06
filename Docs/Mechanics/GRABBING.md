# Közeli tárgyfogás
## Cél és források
`Scripts/Interaction/DemoHand.cs`, `Grabbable.cs`, `EnergyCell.cs`; QuestPlayer, EnergyOrb és eszközprefabok.
Hivatalos Unity XR Hands 1.5.0 HandVisualizer bal/jobb kézmodell, Touch kontrollerrel követve. A ControllerHandPose a grip analóg értékével hajlítja az ujjakat, a triggert a mutatóujj követi. Nincs optikai kézkövetés. Modellek és eredeti licenc: Assets/_Game/Art/UnityHands/.
## Bemenetek és állapot
A kéz 12 cm sugarú környezetében a legközelebbi elérhető tárgyat a grip lenyomása fogja meg; a grip elengedése eldobja. Nincs távoli sugárfogás. Szilárd köztes geometria blokkolja a kiválasztást.
A fogás megtartja az érintési pontot és a tárgy orientációját. Egy tárgynak egy birtokosa lehet. Fogva kinematic, az ütközések bekapcsolva maradnak; elengedve dinamikus Rigidbody, legfeljebb 8 m/s és 20 rad/s.
Fókusz- és követésvesztés, kézletiltás: elengedés dobási impulzus nélkül. 50 cm-nél nagyobb egy fizikaframe-es ugrás szintén elenged. Pálya alá eséskor a tárgy visszaáll.
## Kapcsolatok és hangolás
A Grabbable az energiagömb és Tool közös alapja. A trigger a fogott tárgy Use metódusát, üres kéznél közeli DemoInteractable.Interact metódust hív.
Hatótáv: DemoHand.reach. A kéz vizuális, nincs fizikai kézütköző. A kinematic tárgy erőltetett kézmozdulattal statikus falba tolható; fizikai erővisszacsatolás nincs.
## Ellenőrzés
PlayMode: közeli/távoli tárgy, kizárólagos fogás, fogott gömb nem pontoz, reset és aktív tárgyütközés. Hardveren: mindkét kéz gripje, eszközfogás, dobás és tracking loss. Eredmény: CHANGELOG.

Ujjhajlítás: 70°, hüvelykujj 35°. A kéztartás vizuális; a közeli fogási anchor és a meglévő interakciók változatlanok. Headseten ellenőrizendő a csuklóillesztés és fogáspóz.
