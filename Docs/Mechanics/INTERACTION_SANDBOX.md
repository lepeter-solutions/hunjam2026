# Meta XR fejlesztői demóscene

## Cél és források
`Assets/_Game/Scenes/InteractionSandbox.unity`: külön, szerkeszthető kiindulópont a játék építéséhez. A DemoWorld meglévő Meta SDK rigjére és működéseire épül; nem igényel scene-generálást.

Közös prefabok: QuestPlayer, PhysicsCube, PhysicsBall, Hammer, Blade, Lamp, EnergyOrb és GuideRobot. A kézmodelleket, kontrollerkövetést, kézpózokat, fogást és dobást a Meta XR SDK adja. A korábban választott kontrollervezérelt kezek maradnak; mozgás kizárólag fizikailag.

## Elrendezés és használat
- Kezdőpont: (0, 0, 0). Elöl kalapácscél, vágható blokk és eszköz-RESET; jobbra kalapács, penge és lámpa.
- Balra az energiagömbös feladat, a robot indítja és újraindítja a kört.
- Hátul, a (0, 0.78, -1.15) munkapadon négy kocka és két labda: megfogás, elengedés, dobás, gurítás, egymásra rakás és ütközés próbálható.
- Grip: közeli fogás; elengedés: SDK-dobás; trigger: fogott eszköz használata vagy közeli robot/gomb interakciója.

## Fizikai tárgyak és bővítés
PhysicsCube: 16 cm-es kocka, 0.5 kg, BoxCollider. PhysicsBall: 20 cm átmérő, 0.2 kg, SphereCollider. Mindkettő gravitációs, szabad Rigidbody, Continuous Dynamic ütközésfigyeléssel, kikapcsolt interpolációval. A Meta Grabbable / GrabInteractable / HandGrabInteractable komponensek és a meglévő GameItem adapter vannak bekötve; nincs új fogásimplementáció.

A két prefabot húzd át másik pályára vagy készíts belőlük prefab variantot. Tömeg és collider hangolható az Inspectorban; új használati viselkedés a meglévő GameItem.Use bővítési pontjára kapcsolható. A szabad tárgyak nem számítanak energiagömbnek. A RESET az eszközöket kezeli; a kockák és labdák Play újraindításakor állnak vissza, illetve a GameItem -3 méter alatt automatikusan visszateszi az elengedett tárgyat.

Az új scene kézzel bővíthető. A HunJam demógenerátor és Build Quest APK parancs továbbra is a DemoWorldot célozza; az új pálya Android buildjéhez a Unity Build Profiles scene-listájában ezt a scene-t válaszd.

## Ellenőrzés állapota
A felhasználó kérésére nem indult Unity, fordítás, build vagy teszt. A scene és prefabok közvetlenül a meglévő Unity assetekből készültek. Headsetes működés nincs igazolva.

Későbbi kézi ellenőrzés: nyisd meg az InteractionSandbox scene-t; Quest Link alatt mindkét kézzel fogj meg és engedj el egy tárgyat, rakd egymásra a kockákat, gurítsd a labdát, használd a lámpát, kalapácsot, pengét és robotot. A kézhez kötött tárgy mozgatása kinematic; ez nem teljes erő-visszacsatolásos fizikai kézszimuláció.

Kapcsolódó leírások: [fogás](GRABBING.md), [eszközhasználat](TOOL_USE.md), [ütés](HITTING.md), [vágás](CUTTING.md), [Meta SDK](../META_XR.md).
