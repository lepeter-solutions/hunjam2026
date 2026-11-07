# Movement SDK játékos-testkövetés

## Cél és játékosi viselkedés
A Vérszerződés scene-ben a játékos saját testének helyén egy teljes humanoid avatárt lát. A Meta Movement SDK a headset- és kézkövetésből becsült body tracking pózt a karakter csontvázára retargeteli; nincs hozzá joystickos mozgás vagy külön játékosrig.

Az első személyű nézet elől az avatar fej- és arcmesh-ei rejtettek, hogy ne takarják a kamerát. A törzs, lábak és a Movement SDK kézmesh marad látható.

## Érintett fájlok
- `My project/Assets/_Game/Scenes/01_Vérszerződés.unity`
- `My project/Assets/Samples/Meta XR Movement SDK/207.0.0/Body Tracking Samples/Body/Prefabs/RealisticCharacter.prefab`
- `My project/Assets/Resources/OculusRuntimeSettings.asset`
- `My project/Assets/Oculus/OculusProjectConfig.asset`
- `My project/Packages/manifest.json` és `packages-lock.json`

## Bemenetek, állapotok és kapcsolatok
- A scene `OVRManager` komponense induláskor kéri a Body Tracking engedélyt. Engedélyezés nélkül az avatár nem kap érvényes body pózt.
- A `MetaSourceDataProvider` FullBody joint setet kér; a `CharacterRetargeter` az SDK pózt a RealisticCharacter csontvázára alakítja.
- A `MetaSourceDataProvider` az SDK `TrackingSpace` transzformjával világkoordinátára alakítja a body pózt. Emiatt az avatar prefab gyökere scene-szintű, nem lehet a `TrackingSpace` gyermeke: különben a tracking origin eltolása kétszer adódna hozzá.
- A projekt Body Tracking támogatása engedélyezett. A globális runtime joint set FullBody; más scene-ben nincs automatikusan avatárpéldány.
- A body tracking becsült póz, nem külön testre szerelt szenzorokból származó mozgásmérés. A fizikai room-scale mozgás továbbra is az egyetlen játékosi helyváltoztatás.

## Hangolás és bővítés
- `OculusRuntimeSettings.bodyTrackingJointSet`: `FullBody`. A prefab `_providedSkeletonType` értékének ezzel egyeznie kell.
- `OculusRuntimeSettings.bodyTrackingFidelity`: jelenleg a projekt korábbi, alacsonyabb alapértékén marad; nagyobb fidelity eszközön külön teljesítményméréssel hangolható.
- Az avatar megjelenése és retargetelése a Meta Movement SDK `RealisticCharacter` prefabból jön. A fej, száj és két cornea renderer kizárólag a Vérszerződés scene példányában van kikapcsolva.

## Ellenőrzés
2026-11-07, Unity 6000.3.28f1: a scene-ben a prefab scene-szintű gyökérként van példányosítva, az SDK Body Tracking Samples `MovementBody` scene-jének mintáját követve. A forráskód alapján a provider már világkoordinátára transzformálja a pózt a Meta `TrackingSpace` segítségével; a scene- és dokumentációs módosítás ellenőrzése megtörtént.

Nyitott headsetes teszt: Questen engedélyezni a body trackinget, majd induláskor összevetni a test és a headset pozícióját, fizikai séta közben pedig ellenőrizni, hogy az avatar nem mozdul-e a headsethez képest gyorsabban. Guggoláskor és kar-/kézmozgáskor mindkét szem képén ellenőrizendő a törzs, lábak és karok illeszkedése, a fejmesh hiánya, a követésvesztés utáni visszatérés, komfort és teljesítmény. Android build és fizikai eszközteszt ebben a módosításban nem futott; a korábbi eltérés headseten még visszaellenőrzendő.

## Ismert korlátok
- Az avatar fej/arc mesh-e önnézetben szándékosan nem látható; nincs külső kamera vagy tükörnézet.
- A body tracking minősége és követési tartománya headset/runtime függő, a becsült ízületek eltérhetnek a tényleges testpóztól.
- A Unity Editor preview headset nélkül nem igazolja a runtime body tracking működését.
