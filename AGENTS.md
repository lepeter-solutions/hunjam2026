# Közös munkaszabályok

## Minden munkamenetben
1. Olvasd el ezt a fájlt, a `README.md`, `Docs/README.md`, `Docs/PROJECT.md`, `Docs/CHANGELOG.md` fájlokat és az érintett alkönyvtárak AGENTS.md fájljait.
2. Nézd meg a Git állapotát. Más ember vagy agent munkáját ne írd felül, ne töröld és ne commitold saját változtatásként.
3. Módosítás előtt olvasd végig az érintett forrásfájlt és a hozzá tartozó mechanikaleírást. Munka közben, feladat- vagy hatókörváltáskor olvasd újra az érintett dokumentációt.
4. Minden új mechanikához készíts külön `Docs/Mechanics/<mechanika>.md` fájlt. Meglévő mechanikánál annak dokumentumát frissítsd; ne hozz létre párhuzamos, ellentmondó leírást.
5. Új rendszerhez, buildfolyamathoz vagy fontos döntéshez készíts megfelelő Markdown-dokumentumot, és vedd fel a `Docs/README.md` indexbe.
6. A kóddal együtt frissítsd a dokumentációt és a `Docs/CHANGELOG.md` változásnaplót: mi változott, miért, milyen ellenőrzés történt, mi maradt ellenőrizetlen.
7. Minden összefüggő, ellenőrzött módosítás után készíts commitot. A feladat végén ne maradjon saját, commitolatlan módosítás. Ne commitolj minden billentyűleütést; egy működő munkalépés legyen egy commit. Hibás vagy blokkolt állapotot egyértelműen dokumentálj. A commitazonosítót a végső beszámolóban add meg; saját commitazonosítóját ne próbáld a commitba írni.
8. A commit nem jelent automatikus push/merge jogosultságot. Feltöltést a felhasználó kérése vagy a megbeszélt csapatfolyamat szerint végezz.

## Projekt és együttműködés
- Unity: **6000.3.28f1**, URP. A Unity projekt gyökere: `My project/`.
- Elsődleges cél: Meta Quest standalone Android, OpenXR, Touch kontrollerek. A desktop mód fejlesztői segítség; nem headsetes validáció.
- Saját játékfájlok: `Assets/_Game/`. A sablonfájlokat ne mozgasd vagy töröld indokolatlanul.
- Egy feladatnak egy felelőse legyen. Párhuzamos agentek külön worktree/klón és ág alatt dolgozzanak. Új ág alapértelmezett előtagja `codex/`.
- Ugyanazt a scene-t, prefabot, Package manifestet vagy projektbeállítást egyszerre egy felelős módosítsa. Közös API változását előre jelezd az érintetteknek.
- `.meta` fájlokat mindig együtt kezeld az assettel; meglévő GUID-ot ne generálj újra. Force Text és Visible Meta Files maradjon bekapcsolva.
- Library, Temp, Logs, UserSettings, buildtermék és helyi ellenőrzési másolat nem kerülhet Gitbe.
- Kód: `HunJam` névtér, egy fő MonoBehaviour fájlonként, beszédes nevek, szerializált hangolható értékek. Editor-kód csak Editor mappában/assemblyben.
- Ne vezess be új csomagot vagy absztrakciót ok nélkül. Verziókat rögzítsd; lockfile-t Unityvel frissítsd.

## Mechanikadokumentum kötelező tartalma
- Cél és játékosi viselkedés; érintett források/scene-ek/prefabok.
- Bemenetek, állapotok, más rendszerekkel való kapcsolat.
- Hangolható értékek és bővítési pontok.
- Konkrét ellenőrzési lépések, tényleges eredmény, ismert korlátok.

## Készültség
- Fordítás sikeres, az érintett működéshez illő teszt vagy manuális ellenőrzés megtörtént.
- VR változásnál headseten ellenőrizendő: két szem képe, követés, bal/jobb kontroller, mozgás, interakció, komfort és teljesítmény.
- Ha nincs eszköz vagy buildkörnyezet, ezt pontosan írd le. Ne állíts headsetes sikert pusztán fordítás vagy desktop teszt alapján.
- Dokumentáció, változásnapló és commit elkészült; a végső beszámoló tartalmazza az eredményt és a nyitott ellenőrzéseket.
- Játékosi mozgás kizárólag fizikai room-scale követésből származhat. Ne adj joystickos mozgást, mesterséges fordulást vagy teleportot új felhasználói kérés nélkül. A kezek Touch kontrollerekkel követett alapkezek.
- Követéshez, kézmodellekhez, kézpózokhoz és fogáshoz a Meta XR Core / Interaction SDK kész komponenseit használd. Ne építs mellé saját riget, ujjanimációt vagy fogásmozgatást; lásd Docs/META_XR.md.
