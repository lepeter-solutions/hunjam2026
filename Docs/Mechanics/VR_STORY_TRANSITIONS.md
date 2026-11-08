# VR történetátvezetés és végkép

A sorrend a `My project/Assets/_Game/Resources/StoryFlow.asset` **Stages** listája: Vérszerződés (D1) → 04_Trianon (D2) → Hitler (D3) → Manhattan (D4) → EndScene. A köztes pályák között VR_Transition fut; Manhattan után közvetlenül az EndScene nyílik meg. Az EndScene ugyanazt a Manhattan-ágképet tartja a szobában, időkorlát nélkül.

## Képek, sorrend és időzítés

1. Importáld a térképeket az Assets alá. Színes PNG-hez Texture Type: Default, sRGB: be. A felület a teljes képet használja, a normál térképet nem kell körbefutó grafikává varrni.
2. A StoryFlow Inspectorában a **Branches** sorai a korábbi döntéstörténetek: D1–D4 pályákon rendre 1, 2, 4 és 8 sor. A sor indexe a korábbi bitek összege: `D1 + 2×D2 + 4×D3`, ahol true=1 és false=0. A sor False Panorama / True Panorama mezője a mostani pályadöntés képe, és közvetlenül felülírja az Image A/B választást. A teljes négy döntésállapotot a GameManager 0–15 között őrzi. Az EndScene nem az Endings lista egy 16. képét használja: a Manhattan aktuális Stage.Map képét tartja meg.
3. A Fade In / Hold / Fade Out alapértéke 0,8 / 5 / 0,8 másodperc, valós időben. A Hold a teljesen látható panorámával kezdődik. Debug Logginggal kiírható a kiinduló pálya, a következő pálya és a mentett index.
4. A **Status Panorama** a PNG képarányához igazítja az íves felületet; a normál térképet nem feszíti körbe 360 fokban. A maximális ív alapból 100°. Az első követett nézési irányhoz és szemmagassághoz igazodik, majd a szobában rögzítve marad. Felfedés előtt megvárja ezt a pozíciót.
5. Új pályát sorrendben adj a Stages listához, pontos scene-névvel, döntésszámmal és két képpel, majd engedélyezd a Build Profiles jelenetlistájában is. Ha külön profil felülírja a globális listát, abba is vedd fel. A következő jelenet a lista következő eleme, utoljára az **End Scene**. A döntések nem módosítják ezt.

## Bekötés UnityEventből vagy C#-ból

**A jelenlegi sorrend:** Vérszerződés → 04_Trianon → 02_Hitler → Manhattan. A Branches False/True panorámáit a StoryFlow asset már az előzménykombinációkhoz rendeli; a pályák UnityEventjei rögzítik a döntéseket.

1. Vérszerződés: `StoryExit.decisionNumber=1`; aláírás esemény `SetDecision(true)`, tépés `SetDecision(false)`. Mindkettő után `CompleteLevel()`.
2. 04_Trianon: `decisionNumber=2`; aláírás `false`, tépés `true`, majd `CompleteLevel()`.
3. 02_Hitler: `decisionNumber=3`; a friss térképbeállítás szerint Hitler lelövése `false`, a játékos saját lelövése vagy a ciánkapszula `true`. A jelenet fegyvere és kapszulája a `CompleteWithDecision(bool)` hívással ment és vált egyszerre.
4. Manhattan: `decisionNumber=4`; a `TerroristAi` a 4. döntési kilépőt használja, és a pálya game-over eseményén hívja a döntésrögzítést. Ne állíts be scene reloadot vagy közvetlen scene-betöltést a story eseménnyel párhuzamosan.
5. Az EndScene-hez nem kell új döntés vagy kilépő. A Manhattan döntés után az utolsó térkép látszik a szobában.

A `SetDecision` 0 vagy 1 értéket rögzít; az explicit `recordedDecisions` maszk azt is megkülönbözteti, ha a 0 már valóban kiválasztott döntés volt. A `CompleteLevel()` csak beállított döntés után indul. Kódból a `CompleteWithDecision(bool)` egyben állítja a döntést és indítja a történetváltást. A StoryLevelExit ellenőrzi, hogy a komponens döntésszáma egyezik-e az aktuális scene StoryFlow stage-ével. A közvetlen régi scene-betöltő hívásokat el kell távolítani.

```csharp
using HunJam;

// A jelenet StoryExit komponenséről, ha kódból dől el az eredmény:
storyExit.CompleteWithDecision(true);
```

A négy döntés rendre az 1, 2, 4, 8 súlyú bit. Csak D3 igaz → index 4; mind igaz → 15. A `Stages` sorrendje határozza meg a következő jelenetet, a döntések csak a képet választják. Az állapot alkalmazásbezáráson túl nem mentődik. Új történetnél hívd a `ResetStory()` metódust, majd nyisd meg az első pályát.

## Fájlok

- Scripts/Story: GameManager.cs (állapot), StoryFlow.cs (konfiguráció), StoryLevelExit.cs (eseményadapter), TransitionManager.cs (folyamat), VRFadeManager.cs (fehér réteg), StoryPanorama.cs (henger).
- Scenes/VR_Transition.unity és Scenes/EndScene.unity; Resources/StoryFlow.asset; a pályánkénti térkép PNG-k és közös StoryPanorama anyag.
- Közös meglévő fájlok: Assets/Resources/FullViewFade.shader és Scripts/Player/FullViewFade.cs (konfigurálható szín, alapból fekete); Scripts/Player/FlightVehicle.cs (történetből érkezésnél nincs párhuzamos fekete induló fade); ProjectSettings/EditorBuildSettings.asset (jelenetlista). A fekete repülős újraindítás közvetlen betöltéskor megmarad.

## Ellenőrzés és korlátok

Statikus ellenőrzés: konfigurált jelenetek, státuszindexek, kép-/material-/prefabreferenciák, a henger UV-iránya és a projektben elérhető Meta API-k. A fehér réteg vágótérben fedi a teljes képet, sztereó makrókkal; a manager nem a lecserélt szemkamera gyermeke. Aszinkron betöltés és érvényes képkocka előtt nem indul felfedés. XR-ben fejkövetésre is vár: levett vagy nem követett headsetnél az áttűnés várakozhat. A textúrák közvetlen assetreferenciák: nagyméretű, különböző végleges készleteknél a Quest textúramemóriáját mérni kell.

Statikus ellenőrzés: scene-sorrend, 1/2/4/8 branch méretek, map asset GUID-ok és EndScene Manhattan stage képének kiválasztása. A Vérszerződés aláírása True, tépése False. A Manhattan D4 3. sorában False=`pici_rom.png`, True=`roman_europa.png`; a 8. sorban False=`no_hun.png` (a diagram `no_rom` feliratának kért helyettesítése), True=`nagy_rom.png`. Unity-indítás és headsetes próba nem futott. Questen ellenőrizendő a kezdeti fejirány, képarány, fehér átmenet, mind a négy pálya kimenete, valamint hogy a Manhattan után a szobában a térkép időkorlát nélkül látható marad.
