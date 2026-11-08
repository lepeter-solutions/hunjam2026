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
        [Header("01")]
        [SerializeField]
        private string TrianonLevel = "03_Trianon";
        [SerializeField]
        private int good_waitTime = 0;
        [SerializeField]
        private int bad_waitTime = 3;
        [SerializeField]
        private UnityEvent good_endingEvents_i;
        [SerializeField]
        private UnityEvent bad_endingEvents_i;



        [SerializeField] private OVRScreenFade screenFade;
        public void WaitSwitcherToTrianon(bool goodEnding)
        {
            Debug.Log("Starting ienumerator");
            var waitTime = goodEnding ? good_waitTime : bad_waitTime;
            StartCoroutine(LoadSceneAfterFade(TrianonLevel, goodEnding, waitTime));
        }

        private IEnumerator LoadSceneAfterFade(string sceneName, bool goodEnding, int waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            screenFade.FadeOut();
            yield return new WaitForSeconds(screenFade.fadeTime);
            if(goodEnding) good_endingEvents_i?.Invoke();
            else bad_endingEvents_i?.Invoke();
            yield return new WaitForSeconds(6);
            SceneManager.LoadScene(sceneName);
        }
    }
}
