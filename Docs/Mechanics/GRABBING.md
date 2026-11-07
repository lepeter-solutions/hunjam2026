# Meta SDK közeli fogás
## Cél és források
QuestPlayer, EnergyOrb és eszközprefabok; Scripts/Interaction/GameItem.cs; Meta SDK Grabbable, GrabInteractable, HandGrabInteractable.
A Meta hivatalos rigje és kontrollervezérelt kezei végzik a közeli kiválasztást és fogást. A saját DemoHand, Grabbable és ujjanimáció eltávolítva.
## Bemenetek és állapot
Grip/palm fogás, elengedésre SDK dobás. A fogás hatótávját és jelöltjét az SDK interaktorai adják. Nincs távoli fogás.
A Meta Grabbable kezeli a pozíciót, a kinematic állapot zárolását és a dobási sebességet. Egy objektum egyszerre egy fogási ponttal mozog; a második kéz átvételét az SDK kezeli.
A Rigidbody interpolation kikapcsolt: a Meta transformer közvetlenül a Transformot mozgatja.
GameItem.IsHeld az SDK SelectingPointsCount állapotából származik. Reset megszakítja az SDK-kiválasztást, nullázza a sebességet és visszaállítja a kezdőpozíciót.
## Hangolás és kapcsolatok
Az Inspectorban az SDK komponenseit hangold, ne hozz létre párhuzamos kézpozíció- vagy fogásimplementációt. Az energiagyűjtő csak elengedett gömböt fogad.
A PhysicsCube és PhysicsBall prefab ugyanezzel az SDK-bekötéssel szabadon használható építőelem. Az InteractionSandbox scene hátulsó munkapadján találhatók; tömegük 0.5 és 0.2 kg, méretük 16 és 20 cm. A szabad tárgyak nem vesznek részt az energiagyűjtő körben.
A trigger SDK ControllerSelector eseménye a MetaUseAction adapteren keresztül használja a fogott tárgyat, üres kéznél a közeli robotot/resetet.
## Ellenőrzés
SDK pointereseményes PlayMode tesztek: kiválasztás, mozgatás, elengedés, pontozás, reset és eszközök. Valódi kontrollerekkel: mindkét kéz, fogáspóz, váltás, dobás, követés- és fókuszvesztés. Eredmény: CHANGELOG.
