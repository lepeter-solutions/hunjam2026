using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HunJam
{
    public class TerroristAi : MonoBehaviour
    {
        [SerializeField]
        private float speed = 2;

        [SerializeField]
        private float restartDelay = 3;

        [SerializeField]
        private Image fade;

        [SerializeField]
        private AudioSource playerAudioSource;

        [SerializeField]
        private AudioClip deathAudioClip;

        private void Update()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.name == "Twins")
            {
                StartCoroutine(GameOver(false));
            }
            else if (collision.gameObject.name == "PlayerAircraft")
            {
                StartCoroutine(GameOver(true));
            }
        }

        private IEnumerator GameOver(bool win)
        {
            Color c = fade.color;
            c.a = 1;

            playerAudioSource.clip = deathAudioClip;
            playerAudioSource.loop = false;
            playerAudioSource.Play();

            yield return new WaitForSeconds(restartDelay);

            if (win)
            {
                Debug.Log("Epilógus pálya loading");
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().ToString());
            }
        }
    }
}
