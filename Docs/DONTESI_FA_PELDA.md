# Döntési fa bekötése Unityben: Vérszerződés → Hitler

**Példa:** a játékos az első pályán aláírja vagy széttépi a szerződést. Mindkét esetben a Hitler-pályára jut. Ott megölheti vagy életben hagyhatja Hitlert, de **ugyanaz a tett más térképet eredményezhet** attól függően, mi történt a szerződéssel.

## 1. Mit hova húzzak?

Két helyen dolgozol. A [StoryFlow asset](../My%20project/Assets/_Game/Resources/StoryFlow.asset) Inspectorába a **Project ablakból PNG-ket** húzol: itt adod meg, mit lásson a játékos. A jelenetben egy esemény célmezőjébe a **Hierarchyból egy GameObjectet** húzol: itt adod meg, mikor született meg a döntés.

**Vérszerződés.** Jelöld ki a StoryFlow assetet. Nyisd le: **Stages → Element 0** (`01_Vérszerződés`). Állítsd a **Decision Number** értékét **1**-re. A **Branches** mérete **1**: a játék kezdetén csak egy kiinduló helyzet van. A **Branches → Element 0** sorba húzd a következő képeket:

| Mező | A játékos döntése | A hozzárendelt PNG |
|---|---|---|
| False Panorama | Széttépi a szerződést | Nincs Magyarország |
| True Panorama | Aláírja a szerződést | Létrejön Magyarország |

**Hitler.** Ugyanott nyisd le a **Stages → Element 1** elemet (`02_Hitler`). **Decision Number = 2**, **Branches mérete = 2**. A **sor** az előző döntés, az **oszlop** a mostani:

| Branches sor | Előzmény | False Panorama: életben hagyod | True Panorama: megölöd |
|---|---|---|---|
| Element 0 | Széttépted | Egész Európa a tiéd | Németország lesz Magyarország |
| Element 1 | Aláírtad | Még nincs egyértelműen megadva | Kisebb lesz Magyarország |

A példában a **„csak Románia”** is megölés után szerepelt, de mindkét megöléses mezőhöz tartozik már eredmény. Ha aláírás és életben hagyás után szeretnéd, az **Element 1 → False Panorama** helyre húzd. Ha megölés után szeretnéd, döntsd el, melyik előzményhez tartozzon, és annak a sornak a **True Panorama** képét cseréld le.

## 2. Mi indítja el a következő pályát?

A képek hozzárendelése önmagában nem vált pályát. A játékbeli esemény két műveletet hív, ebben a sorrendben: **SetDecision** rögzíti a döntést, **CompleteLevel** elindítja az átvezetést.

1. Nyisd meg a **01_Vérszerződés** jelenetet. A Hierarchyban válaszd a **Create Empty** lehetőséget, nevezd el az objektumot `StoryExit`-nek. Az Inspectorban **Add Component → StoryLevelExit**, majd **Decision Number = 1**.
2. Jelöld ki a **paper** objektumot. A **PaperWritingDetector → On Writing Completed** eseménylistában cseréld le a régi **GameManagerFader → WaitSwitcherToTrianon** hívást. A hang- és látványeseményeket hagyd meg.
3. Az eseménylistán a **+** gombbal adj hozzá két sort. **Mindkettő** célmezőjébe a Hierarchyban lévő **StoryExit** objektumot húzd. Az első függvény legyen **StoryLevelExit → SetDecision(bool)**, a jelölőnégyzet **bepipálva**. A második legyen **StoryLevelExit → CompleteLevel()**.
4. A **paper → PaperWritingDetector → On Paper Torn** eseménynél ugyanígy cseréld le a régi váltóhívást. Most a **SetDecision(bool)** négyzete **üres** legyen; utána jön a **CompleteLevel()**. Mentsd a jelenetet.
5. Nyisd meg a **02_Hitler** jelenetet. Hozz létre benne egy saját **StoryExit** objektumot ugyanilyen komponenssel, de itt **Decision Number = 2**. A megölést jelző eseményhez kapcsold a **SetDecision(true) → CompleteLevel()** hívásokat. Az életben hagyást véglegesítő eseményhez kapcsold a **SetDecision(false) → CompleteLevel()** hívásokat. E két eseményt te jelölöd ki. Az életben hagyáshoz is kell befejező esemény, például távozás a kijáraton: attól, hogy a játékos *még* nem ölt, a pálya nem ér véget.

**Egy teljes út:** széttéped → az első döntés hamis → megjelenik a „Nincs Magyarország” kép → betölt a Hitler-pálya. Megölöd → a második döntés igaz → a rendszer az előzmény miatt az **Element 0**, a mostani döntés miatt annak **True Panorama** mezőjét olvassa → „Németország lesz Magyarország” → betölt Trianon. Egyik váltáshoz sem kell külön továbbgomb.

## 3. Gyakori kérdések

**Miért állítható 1–4 között a Decision Number?** Ez a döntés *sorszáma*, nem az értéke. A második pályán mindkét befejezés a 2. döntést írja. Az értéket a SetDecision jelölőnégyzete adja: üres = 0, bepipálva = 1. A StoryFlow adott pályáján és a scene StoryLevelExit komponensén ugyanaz a sorszám legyen.

**Mi az a branch, és miért van két sor a Hitler-pályán?** Egy sor egy korábbi történetet jelöl. A játékos már csak a saját sorában járhat, és azon belül két ága van: hamis vagy igaz. Ezért a Hitler-pályán egy adott játékosnak két lehetősége van, miközben a két különböző előzményhez összesen négy külön kép adható meg. Ha két út ugyanahhoz az eredményhez vezet, mindkét mezőbe húzhatod ugyanazt a PNG-t.

**Miért van még Image A/B és Endings is?** Az **Image A/B** pályánként két közösen használható alapkép. Ha egy ág saját panorámája üres, az **If False / If True** választó szerinti alapkép jelenik meg. Ha a fenti táblázat szerint közvetlenül PNG-t húzol a **False Panorama / True Panorama** mezőbe, az elsőbbséget élvez. Az **Endings** külön lista: csak az EndScene használja. Négy kétértékű döntésből 16 teljes történet lehet; ezért van ott 16 végképhely.

**Hogyan folytassam Trianonnál és Manhattannél?** Trianon a 3. döntés: a négy előzménysor sorrendje **0** = tépés + életben hagyás, **1** = aláírás + életben hagyás, **2** = tépés + megölés, **3** = aláírás + megölés. Mindegyik sorhoz a trianoni döntés két képét rendeld. Manhattan a 4. döntés: ugyanez a négy sor ismétlődik, először hamis trianoni döntéssel (0–3), majd igazzal (4–7). A pályák sorrendje egyik ágon sem változik.

**Muszáj függvényt hívnom a kódban?** Nem kell új kód, ha van UnityEvent: a fenti Inspector-bekötés hívja a függvényeket. Ha a döntés már egy saját scriptedben dől el, ugyanazt így indíthatod onnan:

```csharp
// A sikeres aláírást kezelő függvény törzsében:
HunJam.GameManager.Instance.SetDecision(1, true);
HunJam.TransitionManager.StartTransition();
```

Egy befejezéshez válaszd az Inspectoros **vagy** a kódos utat. A működés további részletei: [technikai leírás, időzítések és ellenőrzés](Mechanics/VR_STORY_TRANSITIONS.md).
