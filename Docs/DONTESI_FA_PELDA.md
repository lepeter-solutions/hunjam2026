# Döntési fa és scene bekötések

A történet sorrendjét a `Resources/StoryFlow` **Stages** listája adja: `01_Vérszerződés` → `04_Trianon` → `02_Hitler` → `Manhattan` → `EndScene`. Minden pályán a `Decision Number` 1, 2, 3, 4 sorrendben áll. A döntések a képet választják, a következő pályát nem.

A StoryFlow-ban a sor (**Element**) az előző döntések kombinációja; a soron belüli False/True a mostani pálya két kimenete. `false=0`, `true=1`. Element indexe: `D1 + 2×D2 + 4×D3` (csak a már megtörtént döntéseket számold).

## A négy pálya ágtáblái

### D1 — Vérszerződés, 1 branch sor

| Element | False kimenet | True kimenet |
|---:|---|---|
| 0 | `nagy_hun.png` | `no_hun.png` |

A chart jelölése szerint aláírás = **True**, tépés = **False**. A False kimenet `nagy_hun.png`, a True kimenet `no_hun.png`.

### D2 — 04_Trianon, 2 branch sor

| Element | D1 | False kimenet | True kimenet |
|---:|---|---|---|
| 0 | False | `kis_hun.png` | `nagy_hun.png` |
| 1 | True | `pici_rom.png` | `nagy_rom.png` |

D2 scene exit száma 2. Aláírás = False, tépés = True.

### D3 — 02_Hitler, 4 branch sor

| Element | D1 | D2 | False kimenet | True kimenet |
|---:|---|---|---|---|
| 0 | False | False | `kis_hun.png` | `becsi_hun.png` |
| 1 | True | False | `kis_nemet.png` | `nemet_bir.png` |
| 2 | False | True | `europa_hun.png` | `nemet_bir.png` |
| 3 | True | True | `nagy_rom.png` | `pici_rom.png` |

D3 scene exit száma 3. A legfrissebb térképbeállítás szerint Hitler lelövése = **False**; a játékos saját lelövése vagy a ciánkapszula = **True**. A fegyver és a kapszula ugyanazt a D3 kilépőt hívja.

### D4 — Manhattan, 8 branch sor

| Element | D1 | D2 | D3 | False kimenet | True kimenet |
|---:|---|---|---|---|---|
| 0 | False | False | False | `kis_hun.png` | `nagy_hun.png` |
| 1 | True | False | False | `pici_nemet.png` | `nemet_bir.png` |
| 2 | False | True | False | `arab_vilag.png` | `magyar_vilag.png` |
| 3 | True | True | False | `pici_rom.png` | `roman_europa.png` |
| 4 | False | False | True | `kis_hun.png` | `nagy_hun.png` |
| 5 | True | False | True | `ceausescu_allamok.png` | `pici_rom.png` |
| 6 | False | True | True | `no_usa.png` | `tomcat_land.png` |
| 7 | True | True | True | `no_hun.png` | `nagy_rom.png` |

D4 scene exit száma 4. A chart `Kiiktatás` kimenete True, a `Betelepülés` kimenete False. A chart 8. sorának `no_rom` térképe a felhasználó kérése szerint `no_hun.png`-t használja. A Manhattan `TerroristAi` befejezési eseménye rögzíti a választott ágat. Ezután az EndScene a Manhattanből kiválasztott térképet tartja láthatóan.

## Inspector: döntés és továbbmenet

A képeket a `My project/Assets/_Game/Resources/StoryFlow.asset` fájlban állítod. Az eseményeket a scene `StoryExit` objektumának `StoryLevelExit` komponensére kötöd. A scene-ben levő Decision Number egyezzen a StoryFlow ugyanazon stage-ével.

A UnityEventekben a `SetDecision(false/true)` fusson először, utána a `CompleteLevel()`. Kódból használd a `CompleteWithDecision(bool)` műveletet, amely ezt a két lépést egyben hajtja végre. A játék megjegyzi azt is, ha a kiválasztott érték False volt; az alapértelmezett false önmagában nem számít meghozott döntésnek. Döntés nélküli befejezés vagy a StoryFlow-tól eltérő scene sorszám hibaüzenetet ad.

A Hitler-jelenet három befejezése: Hitler lelövése (False), saját lelövés (True), ciánkapszula használata (True). A fegyver a csőtorkolatból mér, kihagyja a saját fegyver és kéz interakciós triggereit, a fal viszont blokkolja a lövést. A kapszulát a követett játékos szája közelében kell tartani rövid ideig.

## Ellenőrzés

A charttal egyeztetve a D1 aláírás True, tépés False; D2 aláírás False, tépés True. A D4 3. sorának False képe `pici_rom.png`; a 8. sor `no_rom` felirata a felhasználó kérésére `no_hun.png`-re mutat. A StoryFlow 1/2/4/8 ágszáma, aktív térképreferenciák GUID-jai, a négy scene döntésszáma és a Build Settings scene listája statikusan áttekintve. Unity, fordítás, Play mód és headsetes próba nem futott.
