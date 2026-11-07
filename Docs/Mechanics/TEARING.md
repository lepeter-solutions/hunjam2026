# Dinamikus papírtépés

## Cél, források és használat
`Tearable.cs`, `PaperMesh.cs`, `PaperFragment.cs`, `InteractionFeedback.cs`; TearableContract prefab, ContractMesh.asset, Contract.png és Contract.mat. A DemoWorld hátsó asztalán és a VRInteractionTest munkapadján: fogd meg gripre a két oldalt, majd húzd szét. Csak a Tearable komponenssel jelölt papír téphető.

## Állapotok és algoritmus
Egész → érvényes kétkezes fogás → feszülés → egyszeri geometriai split. Mindkét kéz kontaktpontját és induló távolságát a második csatlakozásakor rögzítjük. Egy kéz, azonos oldali fogás, kis távolság, gyenge húzás nem tép.
A szakadás a két lokális kontaktpont felezőpontján halad át; normálisa az induló fogásirány és a pillanatnyi, papírsíkba vetített húzásirány átlaga. A felosztott XY papír minden háromszögét két félsíkkal metsszük, az új csúcsok UV-jét interpoláljuk, a poligonokat háromszögeljük. Nem előre elkészített felek jelennek meg. A textúra darabokon is ugyanott folytatódik.
A két új mesh és vastagított convex MeshCollider a fogástulajdonlás átadása előtt készül el. Ugyanabban a fizikaframe-ben a megfelelő fragment kerül mindkét anchorhoz; az eredeti renderer, collider és Grabbable kikapcsol. A darabok külön Rigidbodyt és TransferOnly fogást kapnak. Nem téphetők tovább. A reset törli a darabokat, helyreállítja az eredeti lapot.

## Inspector
- `minimumGripSeparation`: 0,12 m; `gripHalfExtents`: (0,22; 0,16) a lokális XY síkban; `centreMargin`: 0,025 m. A két oldal az X előjele szerint különül el.
- `extensionThreshold`: 0,09 m; `springStiffness`: 40; `forceThreshold`: 3; `minimumPullSpeed`: 0,04 m/s.
- Az erő rugómodellből becsült érték (nyúlás × merevség), nem a kontrollertől mért fizikai erő.
- `tearSound`: opcionális AudioClip. Üresen rövid, determinisztikusan generált szakadászaj. Feszüléskor legfeljebb 8 mm-es hullámdeformáció és finom haptika, szakadáskor erősebb rövid impulzus.
- `Tension`, `EstimatedForce`, `LastCutPoint`, `LastCutNormal`: diagnosztikai API.

## Ellenőrzés és korlátok
EditMode: három eltérő vágósík, területmegmaradás, UV-folytonosság, változó fragmentgeometria. PlayMode: egykezes/gyenge/rossz régiós tépés tiltása, valódi húzás, két kéz külön fragmentbirtoklása, külön elengedés, reset. A tényleges futási eredmény a CHANGELOG-ban.
Headseten külön ellenőrizendő a feszülés olvashatósága, haptika/hang időzítése, papírfogás és reset. Ez egyszeri, egyenes vonalú síkbeli split; nincs progresszív szakadásfront, általános 3D mesh-vágó, rongy-/softbody-fizika vagy újratépés. A collider szándékosan 4 mm vastag. Az eredeti mesh felosztott, konvex téglalap és nem skálázott prefab; más geometriákhoz külön validáció kell.
