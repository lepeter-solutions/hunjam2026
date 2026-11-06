# Tárgyfogás

## Cél és források
`Scripts/Interaction/DemoHand.cs`, `EnergyCell.cs`, `DemoInteractable.cs`; `Prefabs/EnergyOrb.prefab`, `QuestPlayer.prefab`. Mindkét kontroller rövid sugárral tárgyat foghat meg. A szürke sugár akadályt/üres teret, a cián interaktálható célt, a zöld teleportcélt jelöl.

## Bemenetek és életciklus
Grip lenyomása a megcélzott energiagömböt a kéz előtti anchorhoz rögzíti; elengedése visszaadja a fizikának. Desktopon a kamera céloz, bal egérgomb tartja a tárgyat. Hatótáv 3 m, anchor 0,55 m. A világ geometriája blokkolja a sugarat.

Egyszerre egy kéz birtokolhat egy gömböt. Fogáskor kinematic és ütközésmentes; elengedéskor legfeljebb 6 m/s sebességet kap. Követésvesztés, fókuszvesztés és a kéz letiltása elengedi. A játékos saját kapszulája nem lökdösi a gömböket. A pálya alá eső gömb visszaáll a kiindulóhelyére.

## Kapcsolatok és bővítés
Az interakciós trigger `DemoInteractable.Interact()`-ot hív. A gyűjtő csak elengedett gömböt fogad. A játékkör újraindítása minden gömböt visszaállít. Haptika, fizikai kéz, komplex tárgymarkolat és kézkövetés későbbi bővítés.

## Ellenőrzés
Bal és jobb kéz külön megfog/elenged; két kéz nem birtokolhat egyszerre egy tárgyat; fal mögül nincs fogás; reset közben sem maradhat kinematic tárgy. Desktopon bal gombbal ugyanez; Quest esetén a grip és a kéz orientációja külön hardvertesztet igényel. Futási eredmény: CHANGELOG.
