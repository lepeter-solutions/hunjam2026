# Szúrási gyakorlóeszköz

## Cél és források
`StabbingTool.cs`, `StabTarget.cs`, `InteractionFeedback.cs`; PracticeKnife prefab és a két műhelyjelenet narancs gyakorlóhabja. Nincs karaktersebzés vagy vér. A régi Blade/Cuttable vágás külön, változatlan mechanika.

## Bemenet és állapot
Grip: fogás és átadás. A hegy tényleges mozgásának 8 mm sugarú söprése választja az első szilárd kontaktot; csak StabTarget felület érvényes. A hegy előreirányának, a mozgásnak és a felület befelé mutató normálisának egyeznie kell. Álló, túl gyenge, oldalirányú, eldobott vagy nem célanyagba történő érintés nem szúrás. Fogás/átadás után 0,15 s az élesedési idő, 50 cm-es trackingugrás nem találat.
Behatoláskor az eszköz a belépési tengelyhez kötött, a kéz előretolása csökkentett arányban növeli a mélységet. Az oldalirányú mozgást és forgást a modell korlátozza. Kihúzáskor 2,5 cm-rel a belépési pont mögé kell húzni. Elengedve a célban marad, ismét megfogható és kihúzható. A saját eszköz–cél colliderpárok átmenetileg figyelmen kívül maradnak, majd visszaállnak; globális rétegek nem változnak. RESET oldja a rögzítést és visszateszi az eszközt.

## Inspector
`tip`: +Z a hegy iránya; `minimumSpeed`: 0,6 m/s; `alignment`: 0,8 dot-küszöb. StabTarget: `maximumDepth` 0,055 m; `insertionRatio` 0,4. A fogás a markolat körüli 13 cm-es régióban engedélyezett. Rövid kontaktzaj és haptika kíséri a belépést.

## Ellenőrzés és korlátok
PlayMode: sebesség-, irány- és céltípus-elutasítás, valós hegy-söprés, mélységhatár, kihúzás és reset. Tényleges eredmény: CHANGELOG. Headseten a kéz–markolat illeszkedés, ellenállásérzet, mindkét kéz, céloldal és haptika ellenőrizendő.
Ez kontrollált kinematic demonstráció, nem anyagtörés vagy fizikailag mért súrlódás. A vizuális kéz követi a valós kontrollert, ezért erőltetett oldalirányú mozgatásnál eltávolodhat a rögzített eszköztől. A cél legyen statikus; mozgó/deformálódó cél külön bővítés.
