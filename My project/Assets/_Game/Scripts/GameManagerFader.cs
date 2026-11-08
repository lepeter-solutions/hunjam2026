using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Animations;
using UnityEngine.Events;
using System.ComponentModel;

namespace HunJam
{
    public class GameManagerFader : MonoBehaviour
    {
        [Category("01")]
        [SerializeField]
        private string TrianonLevel = "03_Trianon";
        [SerializeField]
        private UnityEvent endingEvents_i;



        [SerializeField] private OVRScreenFade screenFade;
        public void WaitSwitcherToTrianon()
        {
            Debug.Log("Starting ienumerator");
            StartCoroutine(LoadSceneAfterFade(TrianonLevel));
        }

        private IEnumerator LoadSceneAfterFade(string sceneName)
        {
            yield return new WaitForSeconds(3);
            screenFade.FadeOut();
            yield return new WaitForSeconds(screenFade.fadeTime);
            endingEvents_i?.Invoke();
            yield return new WaitForSeconds(6);
            //SceneManager.LoadScene(sceneName);
            Debug.Log("scene will load...");
        }
    }
}
