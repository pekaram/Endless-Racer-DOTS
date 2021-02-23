using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyUiCallbacks : MonoBehaviour
{
    public void OnPlayClicked()
    {
        SceneManager.LoadScene(1);
    }

    // Start is called before the first frame update
    private void Awake()
    {
        Unity.Entities.World.DefaultGameObjectInjectionWorld.QuitUpdate = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
