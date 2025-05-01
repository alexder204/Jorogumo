using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayerDialogue
{
    public class MoveScenes2D : MonoBehaviour
    {
        [SerializeField] private string newLevel;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                SceneManager.LoadScene(newLevel);
            }
        }
    }
}
