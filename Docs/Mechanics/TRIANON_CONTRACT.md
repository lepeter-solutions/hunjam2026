# Trianon: aláírás vagy tépés

## Cél és scene
A `03_Trianon.unity` a Vérszerződés jelenet VR játékosát, asztalát, tollát és tintás tálját használja. Az asztalon a DemoWorld `TearableContract` prefabja van. A játékos a tollat a tintába mártva aláírhatja a lapot, vagy két kontrollerrel megfogva széttépheti. A két döntés kizárja egymást.

## Bemenet és állapot
A toll és a tinta a Vérszerződés scene meglévő Meta interakcióit használja. A `QuillWriter` a `Paper` címkéjű, a téphető laphoz rögzített triggeren kezd vonalat; a `PaperWritingDetector` a vonalak által fedett területet és az írás utáni szünetet figyeli. A teljes aláírás után a `Tearable` nem indít szakadást. A szakadás megkezdése után az írásdetektor már nem fogad új aláírást. A tollal rajzolt vonal önmagában, a teljesítés előtt még nem végleges döntés.

A téphető laphoz a Vérszerződés Meta rig bal és jobb Touch kontrollerének gripje kapcsolódik a `TrianonPaperGrip` adapterrel. A lap fogását és mozgását a meglévő `Grabbable`, a szakadást a `Tearable` végzi. A két kéz ugyanazt a prefab-példányt fogja; a kezdeti húzás után a szakadás megállítható és folytatható. A Vérszerződés eredeti írófelülete a lap gyermeke: a trigger érzékel, az átlátszó rendertextúrás réteg és a tintakamera mutatja az írást. A réteg a tépés kezdetekor eltűnik; az alaplapot a téphető prefab rajzolja.

## Érintett források és hangolás
`03_Trianon.unity`, `TearableContract.prefab`, `PaperWritingDetector.cs`, `Tearable.cs`, `TrianonPaperGrip.cs`, valamint a Vérszerződés változatlan toll- és tintaassetjei. A jelenet a forrásscene asztali elrendezését örökli. A tépés küszöbei a prefab `Tearable` komponensében, a tintafedettség és várakozás a papír `PaperWritingDetector` komponensében, a fogási hatótáv a rig `TrianonPaperGrip` komponensében állítható. A későbbi kimenethez a `PaperWritingDetector.onWritingCompleted` esemény és a `Tearable.IsTorn` állapot használható.

## Ellenőrzés
Az Editor a scene-t a forrásokból mentette, és a C# fordítást elvégezte. Manuális ellenőrzés szükséges Quest Linkkel: a játékos az asztal előtt indul; a toll megfogható és tintába mártható; a lapra írt jelek látszanak; 20% lefedettség és 3 másodperc szünet után a lap nem téphető; új indításban a két kézzel elkezdett szakadás után az aláírás nem teljesíthető. A két szem, kontrollerek, követés, komfort és teljesítmény headseten ellenőrizendő. Play mód, teszt és Android build nem futott.
