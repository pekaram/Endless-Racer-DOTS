using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class LobbyUiCallbacks : MonoBehaviourPunCallbacks
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

    // Callback for UI joining button
    public void OnJoin()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        // For quick testing and possible matchmaking
        PhotonNetwork.NetworkingClient.OpJoinRandomOrCreateRoom(null, new EnterRoomParams{RoomOptions = new RoomOptions() { MaxPlayers = 2 } }); 
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("failed to join +" + message);
        base.OnCreateRoomFailed(returnCode, message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("failed to join +" + message);

        base.OnJoinRoomFailed(returnCode, message);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        this.StartCoroutine(this.WaitForTime());
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();

        var customRoomProps = new ExitGames.Client.Photon.Hashtable();
      
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProps);
    }

    private IEnumerator WaitForTime()
    {
        yield return new WaitUntil(()=> PhotonNetwork.CurrentRoom.PlayerCount > 1 &&  PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Time"));
        this.OnPlayClicked();
    }   
}
