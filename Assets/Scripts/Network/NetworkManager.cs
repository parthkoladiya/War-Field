using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;


public class NetworkManager : MonoBehaviourPunCallbacks
{

    [SerializeField] Slider loadingSlider;


    void Start()
    {
        loadingSlider.DOValue(1, 1f);

        // If not connected yet, reconnect to Photon
        Debug.Log("@@@@@ "+PhotonNetwork.IsConnected);
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        Debug.Log("@@@@@##### " + PhotonNetwork.IsConnected);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server.");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby. Now setting custom properties.");

        // Safely set custom properties
        Hashtable props = new Hashtable
        {
            { "selectedCharacter", PlayerPrefs.GetInt("selectedCharacter", 0) }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        SceneManager.LoadScene("Lobby");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected from Photon: " + cause.ToString());
    }
}
