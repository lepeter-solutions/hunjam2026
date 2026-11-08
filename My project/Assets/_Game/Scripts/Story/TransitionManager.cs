using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HunJam
{
    public static class TransitionManager
    {
        public static bool IsRevealing(string scene) => VRFadeManager.Instance &&
            GameManager.Instance.InTransition && GameManager.Instance.NextScene==scene;

        // With no argument the fixed order in StoryFlow is used. An explicit target must match it.
        public static void StartTransition(string nextScene=null)
        {
            var game=GameManager.Instance;
            if(game.InTransition) return;
            var flow=Resources.Load<StoryFlow>("StoryFlow");
            string source=SceneManager.GetActiveScene().name;
            int i=flow ? flow.IndexOf(source) : -1;
            if(i<0) { Debug.LogError($"Story: {source} has no entry in Resources/StoryFlow."); return; }
            if(!game.HasDecision(flow.stages[i].decisionNumber))
            { Debug.LogError($"Story: {source} has no recorded decision. Set true or false before completing the level."); return; }
            string next=flow.Next(i);
            if(string.IsNullOrWhiteSpace(next) || (nextScene!=null && nextScene!=next) || next==flow.transitionScene ||
               !Application.CanStreamedLevelBeLoaded(next) ||
               (next!=flow.endScene && !Application.CanStreamedLevelBeLoaded(flow.transitionScene)))
            { Debug.LogError($"Story: invalid route from {source}. Expected '{next}', requested '{nextScene}'. Check StoryFlow and Build Profiles."); return; }
            game.CompletedScene=source;
            game.NextScene=next;
            game.SavedStatus=game.GetStatusIndex();
            for(int previous=1;previous<flow.stages[i].decisionNumber;previous++)
                if(!game.HasDecision(previous))
                    Debug.LogWarning($"Story: decision {previous} was not played; using false (direct scene preview).");
            game.InTransition=true;
            var fade=VRFadeManager.Instance;
            if(!fade) fade=game.gameObject.AddComponent<VRFadeManager>();
            game.StartCoroutine(Run(game,flow,fade));
        }

        private static IEnumerator Run(GameManager game,StoryFlow flow,VRFadeManager fade)
        {
            // sceneLoaded runs after Awake/OnEnable and before Start, including on reload.
            void PrepareIncoming(Scene scene, LoadSceneMode mode)
            {
                if(scene.name!=game.NextScene && scene.name!=flow.transitionScene) return;
                foreach(var root in scene.GetRootGameObjects())
                    foreach(var legacy in root.GetComponentsInChildren<OVRScreenFade>(true))
                        legacy.fadeOnStart=false;
            }
            SceneManager.sceneLoaded+=PrepareIncoming;
            try
            {
                if(flow.debugLogging) Debug.Log($"Story: {game.CompletedScene} → {game.NextScene}, status {game.SavedStatus:D2}");
                yield return fade.FadeTo(1,flow.fadeOut);
                yield return null; // Submit an opaque frame before starting the load.
                // The final room shows the last selected map indefinitely, with no second map/load.
                bool final=game.NextScene==flow.endScene;
                var load=SceneManager.LoadSceneAsync(final ? flow.endScene : flow.transitionScene);
                if(load==null) { Debug.LogError("Story: transition scene could not be loaded."); yield break; }
                yield return load;
                yield return WaitForView(fade);
                yield return fade.FadeTo(0,flow.fadeIn);
                if(final) yield break;
                yield return new WaitForSecondsRealtime(Mathf.Max(0,flow.hold));
                yield return fade.FadeTo(1,flow.fadeOut);
                yield return null;
                load=SceneManager.LoadSceneAsync(game.NextScene);
                if(load==null) { Debug.LogError($"Story: could not load {game.NextScene}."); yield break; }
                yield return load;
                yield return WaitForView(fade);
                yield return fade.FadeTo(0,flow.fadeIn);
            }
            finally { SceneManager.sceneLoaded-=PrepareIncoming; fade.SetOpacity(0); game.InTransition=false; }
        }
        private static IEnumerator WaitForView(VRFadeManager fade)
        {
            yield return null; // allow Start, rig creation, and panorama assignment
            var scene=SceneManager.GetActiveScene();
            while(fade.RenderedScene!=scene.handle) yield return null;
            var player=Object.FindFirstObjectByType<DemoPlayer>();
            while(player && player.IsVR && !player.TrackingReady) yield return null;
            var panorama=Object.FindFirstObjectByType<StoryPanorama>();
            while(panorama && !panorama.IsReady) yield return null;
            if(UnityEngine.XR.XRSettings.isDeviceActive)
            {
                var head=UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.Head);
                while(!head.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked,out bool tracked) || !tracked)
                { yield return null; head=UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.Head); }
            }
        }
    }
}
