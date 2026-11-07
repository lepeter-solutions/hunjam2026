# Fokozatos, újratéphető papír

## Cél és használat
Források: `Tearable.cs`, `PaperMesh.cs`, `PaperFragment.cs`, `InteractionFeedback.cs`, `WorkshopReset.cs`. Assetek: TearableContract prefab, ContractMesh.asset, Contract.png és Contract.mat. A DemoWorld hátsó asztala és a VRInteractionTest munkapadja ugyanazt a prefab-beállítást örökli; scene-újragenerálás nem szükséges.

Fogd meg a lap két külön pontját, majd fokozatosan távolítsd a kezeket. Rövid előfeszítés után a széltől induló rés a további húzással halad végig. Megálló húzásnál a front megáll; a kéz közelítése nem gyógyítja vissza a szakadás előrehaladását. Elengedéskor a részleges rés keskenyre záródik, de megmarad. Újrafogáskor az elkezdett vonal két oldalát kell megfogni. A levált darabokat külön felveheted és két kézzel tovább tépheted, a méret- és mélységhatárig.

## Állapotok és működés
Ép → kétkezes előfeszítés → részleges szakadás → két önálló, újratéphető darab. A második kéz csatlakozásakor rögzülnek a kontaktpontok és a kéztávolság. A vonal a két kontaktpont felezőpontján, a közöttük lévő XY irányra merőlegesen halad; tetszőleges irányból lehet fogni, nincs világ-X szerinti oldaltiltás. A geometriai területvizsgálat elutasítja az üres vagy túl apró eredményt.

A háromszögvágás megtartja az interpolált UV-koordinátákat. A két vágott felület sűrített vizuális másolata jelenik meg; a szakadásfront előtti rész zárt, mögötte a felület a kézkontaktus elmozdulását követi. A front körül simított nyílás és lokális szélfelhajlás látható. A vizuális háromszögek kétszeri felosztása csak egyszer, a szakadás indulásakor történik. A mozgó csúcstömbök újrahasznosítottak; a fizikához és a következő vágáshoz a kisebb eredeti vágott mesh marad.

Az előrehaladást a kéztávolság pozitív változása adja, legfeljebb 2,5 teljes szakadás/másodperc sebességgel. Egyetlen hirtelen rántás nem válthat azonnal két tárgyra; a limit fölötti mozdulat nem tárolódik később lejátszandó animációként, tovább kell húzni. A kisebb lap rövidebb húzóutat igényel. A feszítés rugómodellből becsült érték, nem mért kontrollererő.

A front végén a darabok a végső vizuális helyzetükön jönnek létre, középre rendezett lokális geometriával, területarányos tömeggel, külön Rigidbodyval és 4 mm vastag convex colliderrel. A meglévő Meta Grabbable adapter ugyanabban a frame-ben adja át őket a megfelelő kezeknek. Mindkettő TwoHands fogást és örökölt Tearable/hangbeállításokat kap. A szétszakadt szülő rejtve őrzi a darabfát; a WorkshopReset csak a gyökérlapot reseteli, amely rekurzívan felszabadítja az összes darabot, fogást és runtime mesht.

## Inspector és hangbekötés
A `03_Trianon.unity` jelenetben ugyanez a prefab az aláírható laphoz kapcsolódik. A `PaperWritingDetector` teljesített aláírása után nem indul szakadás; a szakadás megkezdése után az aláírás nem teljesíthető. Az asztali elrendezést és az ellenőrzési korlátokat a [Trianon leírás](TRIANON_CONTRACT.md) rögzíti.

A `TearableContract` prefab Tearable komponensében:
- `Minimum Grip Separation`: 0,06 m lokális kontaktustávolság.
- `Extension Threshold`: 0,025 m; `Spring Stiffness`: 40; `Force Threshold`: 0,8; `Minimum Pull Speed`: 0,005 m/s az induláshoz. Elindult szakadás lassú húzással is folytatható.
- `Tear Travel`: 0,18 m az eredeti 0,42 × 0,30 m-es laphoz; területtel arányosan skálázott, legalább 0,04 m.
- `Minimum Fragment Area`: 0,0018 m² darabonként, lokális mesh-terület. `Maximum Tear Depth`: 5; legfeljebb 32 levéldarab gyökérlaponként, a területhatár ezt tovább korlátozza. Az ennél kisebb/végső darab megfogható marad.
- `Edge Curl`: 0,012 m maximális felhajlás a front körül.
- `Tearing Loop`: ide húzd a folyamatos papírszakadás AudioClipjét. Csak előrehaladáskor szól, sebességfüggő hangerővel/hangmagassággal és rövid hangerőátmenettel. Üresen a folyamat néma.
- `Tear Sound`: végső elszakadás egyszeri AudioClipje; üresen a meglévő generált demózaj szól.
- `Tear Volume`: 0,4. A folyamat hangja térbeli, Doppler nélkül, 0,15–4 m távolságtartománnyal. Finom haptika kíséri az előrehaladást, rövid erősebb impulzus a leválást.

Saját hangasset nem került be; a két Inspector-slot bekötésre kész. A generátor a komponens azonos alapértékeit használja. Diagnosztika: `IsTorn`, `TearProgress`, `Tension`, `EstimatedForce`, `LastCutPoint`, `LastCutNormal`, `Fragments`.

## Ellenőrzési állapot és későbbi lépések
2026-11-07: kizárólag forrás-, prefab- és diff-áttekintés. A felhasználó kérése szerint nem indult fordítás, build, Play mód vagy teszt. Futásidejű és headsetes működés nincs igazolva. A régi PlayMode papírtesztek az azonnali splitet, X-oldaltiltást és nem téphető fragmentet várják; az új viselkedés elfogadásához később frissítendők. A korábbi teszteredmények erre a változatra nem érvényesek.

Később ellenőrizendő:
1. Egy kéz, kis húzás, közel egybeeső fogások és túl kicsi darab ne indítson tépést.
2. Lassú húzásnál végig látható rés/front; megállás, visszaközelítés, újrafogás után megmaradó előrehaladás; gyors rántás se legyen azonnali split.
3. Mindkét kezdőkéz, különböző fogási irányok; leváláskor kép- és fogásfolytonosság, külön elengedés/átadás.
4. Legalább három egymás utáni tépés ugyanabból a lapból; gyerekdarabok textúrája, ütközése, hangöröklése és mérethatára.
5. RESET részleges szakadáskor és többgenerációs darabok kézben tartásakor; scene-váltás, követésvesztés, letiltás, takarítás.
6. Mindkét hangslot külön és üresen, hangerő 0, lassú/gyors húzás, szünet és folytatás.
7. Quest: két szem, két kontroller, komfort, késleltetés, szakadáskezdeti mesh-készítés költsége, sok darab képkockaideje.

Korlátok: síkbeli, egyenes szakadásnyom, nem rostszimuláció vagy általános softbody. A folyamat közben a collider még az összefüggő eredeti lapot képviseli; az új önálló colliderek a végén készülnek. A finom szélfelhajlás a leválás végére kisimul. Egy lapon egyszerre egy megkezdett vonal folytatható. A hang, vizuális sűrűség és küszöbök headseten hangolandók; támogatott alap a felosztott, konvex XY lap, egységnyi skálával.

## 2026-11-07 — DemoWorld fogási hibajelzés
A friss main (d2fc056) papírprefabján a collider és a Grabbable aktív, a scene nem írja ezeket felül. A fogáskeresés korábban kizárólag a grip lenyomási frame-jében futott: a hatótávon kívül megkezdett szorítás közelítéskor nem próbált újra. A DemoHand most sikerig vagy felengedésig periodikusan újrakeres, változatlan közelségi és akadályszabállyal. Ez az azonosított bemeneti eset javítása; a jelentett headsetes hiba teljes oka futtatás nélkül nem igazolt.
Későbbi ellenőrzés: grip lenyomása a laptól távol, majd közelítés felengedés nélkül; normál közeli felvétel, kétkezes fogás, reset utáni új lenyomás és fal mögötti tárgy tiltása. Csak forrás/prefab/diff áttekintés történt; kérésre nincs build vagy teszt.
