# VR történetátvezetés és végkép

A sorrend a `My project/Assets/_Game/Resources/StoryFlow.asset` **Stages** listája: Vérszerződés → Hitler → Trianon → Manhattan → EndScene. Két szomszédos pálya között mindig ugyanaz a VR_Transition fut. Az utolsó átvezetés után az EndScene a négy mentett döntés kombinációjához tartozó végképet tartja a hengeren, időkorlát nélkül. A közvetlenül megnyitott két új scene előnézetként használható (Preview Stage / Preview Status); ezek nem indítanak maguktól új történetet.

## Képek, sorrend és időzítés

1. Importáld a panorámákat az Assets alá. Színes PNG-hez Texture Type: Default, sRGB: be; körbefutó grafikához illeszd össze a bal és jobb szélet. Az U tengely ismétlődhet, a V tengelyhez Clamp ajánlott. A panoráma közepe a rig +Z irányában van, a varrat hátul.
2. A StoryFlow Inspectorában **Stages → Image A / Image B** a pálya két képe. A **Branches** sorai a korábbi döntéstörténetek: a négy pályán rendre 1, 2, 4 és 8 csomópont. Az adott sor **If False / If True** mezője választ A-t vagy B-t az aktuális döntés alapján. A sorszám az előző igaz döntések bitsúlyainak összege (1, 2, 4). A Decision Number a történetbeli sorrend: 1–4. Példa: a második pályán a 0. sor érvényes, ha az első döntés hamis, az 1. sor, ha igaz. A minta a 0. sorban A/B-t, az 1. sorban B/A-t mutat: ugyanaz a második döntés így eltérő képet adhat. Ezek szabadon szerkeszthető mintaszabályok. A négy döntés teljes története akkor is megmarad, ha két út ugyanazt a képet mutatja. Csak az **Endings** lista használ 16 külön végképet. Hiányzó kép vagy csomópont esetén Console-hiba és fehér háttér jelenik meg.
3. A Fade In / Hold / Fade Out alapértéke 0,8 / 5 / 0,8 másodperc, valós időben. A Hold a teljesen látható panorámával kezdődik. Debug Logginggal kiírható a kiinduló pálya, a következő pálya és a mentett index.
4. Az új jelenetek **Status Panorama** objektumán állítható a sugár, magasság, középmagasság, elfordítás és szegmensszám. Mindkét jelenet ugyanazt az URP Unlit materialt és oldalpalást-komponenst használja. Az átlátszó PNG-részek mögött, felfelé és lefelé tiszta fehér kameraháttér látszik, nincsenek szobafalak vagy hengersapkák. A fejkövetést az SDK OVRCameraRig prefabja adja.
5. Új pályát sorrendben adj a Stages listához, pontos scene-névvel, döntésszámmal és két képpel, majd engedélyezd a Build Profiles jelenetlistájában is. Ha külön profil felülírja a globális listát, abba is vedd fel. A következő jelenet a lista következő eleme, utoljára az **End Scene**. A döntések nem módosítják ezt.

## Bekötés UnityEventből vagy C#-ból

A befejezést és a döntési eseményeket a pálya készítője köti be; a rendszer nem találja ki, mi számít döntésnek. Adj egy üres GameObjecthez **StoryLevelExit** komponenst. Válaszd ki a Decision Number értékét (1–4). Egy döntési esemény UnityEvent-listájában válaszd a **StoryLevelExit → SetDecision(bool)** függvényt, és állítsd be az értéket. A befejezési esemény hívja a **CompleteLevel()** függvényt. Ha ugyanaz az esemény végzi mindkettőt, a döntés legyen előbb. A korábbi közvetlen scene-betöltő hívást cseréld le az adott eseményben: például a Vérszerződés GameManagerFader váltója vagy a Manhattan újratöltése ne induljon a történetváltással együtt. A meglévő pályák eseménykötései szándékosan érintetlenek.

```csharp
using HunJam;

// Például a Trianon döntési eseményében:
GameManager.Instance.SetDecision(3, true);
// A befejezési eseményben; cél az Inspector sorrendjéből:
TransitionManager.StartTransition();
// Alternatív hívás Trianonból: TransitionManager.StartTransition("Manhattan");
```

A négy döntés rendre az 1, 2, 4, 8 súlyú bit. Csak a harmadik döntés igaz → GetStatusIndex() = 4; mind igaz → 15. A GameManager scene-váltás előtt pillanatképet készít az indexről, a forrásról és a célról; folyamatban lévő átvezetés nem indítható újra. Az állapot a játékmenet memóriájában él, alkalmazásbezáráson túl nem mentődik. Új történet kezdésekor hívd a **ResetStory()** metódust vagy az adapter azonos nevű UnityEvent-műveletét, majd töltsd be az első pályát.

## Fájlok

- Scripts/Story: GameManager.cs (állapot), StoryFlow.cs (konfiguráció), StoryLevelExit.cs (eseményadapter), TransitionManager.cs (folyamat), VRFadeManager.cs (fehér réteg), StoryPanorama.cs (henger).
- Scenes/VR_Transition.unity és Scenes/EndScene.unity; Resources/StoryFlow.asset; Art/StoryMaps/StoryPanorama.mat és Status_00.png … Status_15.png. A fenti útvonalak az Assets/_Game almappái.
- Közös meglévő fájlok: Assets/Resources/FullViewFade.shader és Scripts/Player/FullViewFade.cs (konfigurálható szín, alapból fekete); Scripts/Player/FlightVehicle.cs (történetből érkezésnél nincs párhuzamos fekete induló fade); ProjectSettings/EditorBuildSettings.asset (jelenetlista). A fekete repülős újraindítás közvetlen betöltéskor megmarad.

## Ellenőrzés és korlátok

Statikus ellenőrzés: konfigurált jelenetek, státuszindexek, kép-/material-/prefabreferenciák, a henger UV-iránya és a projektben elérhető Meta API-k. A fehér réteg vágótérben fedi a teljes képet, sztereó makrókkal; a manager nem a lecserélt szemkamera gyermeke. Aszinkron betöltés és érvényes képkocka előtt nem indul felfedés. XR-ben fejkövetésre is vár: levett vagy nem követett headsetnél az áttűnés várakozhat. A textúrák közvetlen assetreferenciák: nagyméretű, különböző végleges készleteknél a Quest textúramemóriáját mérni kell.

Fordítás, Unity-indítás és headsetes próba nem futott. Későbbi ellenőrzés Linken és Questen: 0/15 és vegyes bitminta; jobb felé olvasható, körbeérő kép; fejfordítás mindkét szemmel fehér áttűnés alatt; teljes kép után 5 másodperc; Time.timeScale=0; ismételt befejezési esemény; kameraátadás; hiányzó PNG; Manhattan A/B képe után az EndScene a kombinációhoz tartozó végképet tartja; új játék után index 0.
