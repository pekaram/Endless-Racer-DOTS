using UnityEngine;
using UnityEngine.SceneManagement;

using Unity.Entities;

public class IntroScenePlayer : MonoBehaviour
{
    private void Awake()
    {
        // Hold all systems till game starts
        World.Active.QuitUpdate = true;

        Screen.autorotateToPortrait = false;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToLandscapeLeft = true;
        Screen.orientation = ScreenOrientation.AutoRotation;

        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    private void WaitAndLoadScene()
    {
        SceneManager.LoadScene(1);
    }
}
