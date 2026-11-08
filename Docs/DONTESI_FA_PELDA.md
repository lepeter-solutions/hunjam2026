# Döntési fa és scene bekötések

A történet sorrendjét a `Resources/StoryFlow` **Stages** listája adja: `01_Vérszerződés` → `04_Trianon` → `02_Hitler` → `Manhattan` → `EndScene`. Minden pályán a `Decision Number` 1, 2, 3, 4 sorrendben áll. A döntések a képet választják, a következő pályát nem.

A StoryFlow-ban a sor (**Element**) az előző döntések kombinációja; a soron belüli False/True a mostani pálya két kimenete. `false=0`, `true=1`. Element indexe: `D1 + 2×D2 + 4×D3` (csak a már megtörtént döntéseket számold).

## A négy pálya ágtáblái

### D1 — Vérszerződés, 1 branch sor

| Element | False kimenet | True kimenet |
|---:|---|---|
| 0 | False | True |

A jelenlegi bekötésben aláírás = **False**, tépés = **True**. Ez az utoljára feltöltött képkiosztást követi.

### D2 — 04_Trianon, 2 branch sor

| Element | D1 | False kimenet | True kimenet |
|---:|---|---|---|
| 0 | False | False | True |
| 1 | True | False | True |

D2 scene exit száma 2. Aláírás = False, tépés = True.

### D3 — 02_Hitler, 4 branch sor

| Element | D1 | D2 | False kimenet | True kimenet |
|---:|---|---|---|---|
| 0 | False | False | False | True |
| 1 | True | False | False | True |
| 2 | False | True | False | True |
| 3 | True | True | False | True |

D3 scene exit száma 3. A legfrissebb térképbeállítás szerint Hitler lelövése = **False**; a játékos saját lelövése vagy a ciánkapszula = **True**. A fegyver és a kapszula ugyanazt a D3 kilépőt hívja.

### D4 — Manhattan, 8 branch sor

| Element | D1 | D2 | D3 | False kimenet | True kimenet |
|---:|---|---|---|---|---|
| 0 | False | False | False | False | True |
| 1 | True | False | False | False | True |
| 2 | False | True | False | False | True |
| 3 | True | True | False | False | True |
| 4 | False | False | True | False | True |
| 5 | True | False | True | False | True |
| 6 | False | True | True | False | True |
| 7 | True | True | True | False | True |

D4 scene exit száma 4. A Manhattan `TerroristAi` befejezési eseménye rögzíti a választott ágat. Ezután az EndScene a Manhattanből kiválasztott térképet tartja láthatóan.

## Inspector: döntés és továbbmenet

A képeket a `My project/Assets/_Game/Resources/StoryFlow.asset` fájlban állítod. Az eseményeket a scene `StoryExit` objektumának `StoryLevelExit` komponensére kötöd. A scene-ben levő Decision Number egyezzen a StoryFlow ugyanazon stage-ével.

A UnityEventekben a `SetDecision(false/true)` fusson először, utána a `CompleteLevel()`. Kódból használd a `CompleteWithDecision(bool)` műveletet, amely ezt a két lépést egyben hajtja végre. A játék megjegyzi azt is, ha a kiválasztott érték False volt; az alapértelmezett false önmagában nem számít meghozott döntésnek. Döntés nélküli befejezés vagy a StoryFlow-tól eltérő scene sorszám hibaüzenetet ad.

A Hitler-jelenet három befejezése: Hitler lelövése (False), saját lelövés (True), ciánkapszula használata (True). A fegyver a csőtorkolatból mér, kihagyja a saját fegyver és kéz interakciós triggereit, a fal viszont blokkolja a lövést. A kapszulát a követett játékos szája közelében kell tartani rövid ideig.

## Ellenőrzés

A StoryFlow 1/2/4/8 ágszáma, a képreferenciák GUID-jai, a négy scene döntésszáma, scene exitjei, eseményértékei és a Build Settings scene listája statikusan áttekintve. A 04_Trianon scene frissen pullolva. Unity, fordítás, Play mód és headsetes próba nem futott; a VR fegyver, kapszula, döntések és végső térkép eszközön ellenőrizendő.
