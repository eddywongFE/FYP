using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManiMenuController : MonoBehaviour
{
    public void PlayGame() { // Start game
        SceneManager.LoadScene(1);
    }

    public void ExitGame() { // Exit application
        Application.Quit();
    }
}
