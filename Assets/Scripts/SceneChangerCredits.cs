using UnityEngine;
using UnityEngine.SceneManagement; // This is vital!

public class SceneChangerCredits : MonoBehaviour
{
    // Method to load a scene by its name
    public void ChangeSceneByName()
    {
        SceneManager.LoadScene("MainMenu");
    }

}