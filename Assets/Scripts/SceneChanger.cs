using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Method to load a scene by its name
    public void OnStart()
    {
        SceneManager.LoadScene("Level1");
    }
    public void OnCredits(){
        SceneManager.LoadScene("Credits");
    }
}