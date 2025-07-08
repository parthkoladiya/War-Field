using Photon.Pun;
using UnityEngine;

public class NetworkUtils : MonoBehaviourPunCallbacks
{
    private static NetworkUtils _instance;

    public static NetworkUtils getInstance()
    {
            if (_instance == null)
            {
                GameObject go = new GameObject("NetworkUtils");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<NetworkUtils>();
            }
            return _instance;
    }
    void Start()
    {
        Debug.Log("@@@@@ " + PhotonNetwork.IsConnected);
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        Debug.Log("@@@@@##### " + PhotonNetwork.IsConnected);
    }
   
}


