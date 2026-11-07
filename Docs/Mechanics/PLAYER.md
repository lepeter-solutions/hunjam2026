# Fizikai mozgás és követés
## Cél és források
QuestPlayer.prefab, DemoWorld.unity, Editor/MetaProjectTools.cs; Meta SDK OVRCameraRig és OVRComprehensiveInteractionRig.
A Meta XR Core végzi a fej és kontrollerek követését. A játékos csak valós sétával, fordulással és lehajolással mozog.
## Bemenetek és állapot
Floor Level követési origó. Kontrollervezérelt Meta kézmodellek (ConformingToController).
Az összes SDK locomotion komponens letiltott: nincs joystickos séta, fordulás vagy teleport. Nincs saját kamerakövető kód.
## Hangolás és kapcsolatok
A kezdőpont (0,0,0), a műhely köré rendezett tárgyakkal. A headset rendszerhatárát kell használni: a virtuális fal nem állítja meg a valós testet.
Beállítások és csomagverziók: ../META_XR.md.
## Ellenőrzés
Automatikus teszt: SDK rig, két kéz interaktorai, változatlan rigpozíció és tiltott PlayerLocomotor. Headseten: padlószint, két szem, kontrollerek, visszatérés és a stickek hatástalansága. Tényleges eredmény: CHANGELOG.
