using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.IO;
using DG.Tweening;
using ZXing;
using ZXing.QrCode;
using ZXing.Unity;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Panels Management")]
    public RectTransform[] panels;
    public float moveDistance = 800f;
    public float duration = 0.5f;
    private int currentPanelIndex = 0;

    [Header("Friend Room")]
    public RawImage qrImage;
    public TMP_Text roomNameText;

    [Header("QR Scanner")]
    public RawImage cameraFeed;
    private WebCamTexture camTexture;

    [Header("Online MatchMaking")]
    public Transform playerCarousel;
    public GameObject playerCardPrefab;

    [Header("Room Join")]
    public TMP_InputField roomCodeInput;
    public TMP_Text feedbackText;
    public TMP_Text feedbackTextInvite;
    public GameObject feedbackText1;

    [Header("Toast UI")]
    public GameObject toastPanel;
    public TMP_Text toastText;

    private string currentInviteRoom;
    public GameObject opencamobject;
    public Image mainCharacterImage;
    public Image homePageCharacterImage;

    public Sprite[] characterSprites; // 10 sprites
    public int selectedCharacterIndex = 0;

    public GameObject[] characterSlots; // Parent objects of each character icon (with border inside)
    private int currentSelectedIndex = -1;

    public GameObject confromforquit;

    [Header("Scroll Zoom/Scale")]
    public ScrollRect scrollRect;
    public RectTransform content;
    public List<RectTransform> itemList;  // 5 items (add in inspector)
    public float maxScale = 1.5f;
    public float minScale = 1.0f;
    public float zoomSpeed = 10f;

    [Header("music")]
    public AudioSource musicSource;
    public AudioSource clicksource;

    // Camera control for lobby
    private WebCamTexture webcamTexture;

    private void Awake()
    {
        DontDestroyOnLoad(musicSource);
    }
    void Start()
    {
        for (int i = 1; i < panels.Length; i++)
        {
            panels[i].anchoredPosition += new Vector2(moveDistance, 0);
        }

        for (int i = 0; i < characterSlots.Length; i++)
        {
            int index = i;
            Button btn = characterSlots[i].GetComponent<Button>();
            btn.onClick.AddListener(() => SelectCharacter(index));
            if (characterSlots[4] || characterSlots[10])
            {
                Debug.Log(" chatracter slot " + characterSlots[i]);
                Debug.Log(" Enter to  ");
                RectTransform rt = characterSlots[4].transform.Find("Avathar").GetComponent<RectTransform>();
                RectTransform rt1 = characterSlots[10].transform.Find("Avathar").GetComponent<RectTransform>();
                Vector2 newsize = rt.sizeDelta;
                newsize.x = 160;
                rt.sizeDelta = newsize;
                rt1.sizeDelta = newsize;

                Debug.Log(" change the size ");
            }


            // Initialize character sprite
            Image icon = characterSlots[i].transform.Find("Avathar").GetComponent<Image>();
            icon.sprite = characterSprites[i];

            //PhotonNetwork.NickName = inputNameField.text; // from user input
            PhotonNetwork.NickName = characterSlots[index].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text;

            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                { "CharacterIndex", selectedCharacterIndex }
            });

            // Hide all borders initially
            characterSlots[i].transform.Find("SelectionBorder").gameObject.SetActive(false);
            characterSlots[index].transform.Find("NameText").localScale = Vector3.one;
        }
    }

    private void SelectCharacter(int index)
    {
        // Deselect previous
        if (currentSelectedIndex != -1)
        {
            characterSlots[currentSelectedIndex].transform.Find("SelectionBorder").gameObject.SetActive(false);
            characterSlots[currentSelectedIndex].transform.Find("NameText").localScale = Vector3.one;
        }

        // Select new
        characterSlots[index].transform.Find("SelectionBorder").gameObject.SetActive(true);
        characterSlots[index].transform.Find("NameText").localScale = Vector3.one * 1.2f;
        currentSelectedIndex = index;

        selectedCharacterIndex = index;

        mainCharacterImage.sprite = characterSprites[index];
        homePageCharacterImage.sprite = characterSprites[index];

        //PhotonNetwork.NickName = inputNameField.text; // from user input
        PhotonNetwork.NickName = characterSlots[index].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text;

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
        {
            { "CharacterIndex", selectedCharacterIndex }
        });

        // Set main image width based on avatar index
        RectTransform mainRect = mainCharacterImage.GetComponent<RectTransform>();
        RectTransform homePageRect = homePageCharacterImage.GetComponent<RectTransform>();
        if (index == 4 || index == 10) // <-- yahan apne special index daalein
        {
            Vector2 size = mainRect.sizeDelta;
            Vector2 size1 = homePageRect.sizeDelta;
            size.x = 275;
            size1.x = 210;
            mainRect.sizeDelta = size;
            homePageRect.sizeDelta = size1;
        }
        else
        {
            Vector2 size = mainRect.sizeDelta;
            Vector2 size1 = homePageRect.sizeDelta;
            size.x = 200;
            size1.x = 140;
            size1.y = 200;
            mainRect.sizeDelta = size;
            homePageRect.sizeDelta = size1;
        }

        //Debug.Log("Selected character index: " + index);
    }


    #region Create and Share Friend Room

    public void CreateFriendRoom()
    {
        string randomRoom = "FRIEND_" + Random.Range(1000, 9999);
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(randomRoom, options);
        GenerateQRCode(randomRoom);
        roomNameText.text = randomRoom;
    }

    void GenerateQRCode(string text)
    {
        QRCodeWriter writer = new QRCodeWriter();
        var matrix = writer.encode(text, BarcodeFormat.QR_CODE, 256, 256);
        Texture2D tex = new Texture2D(256, 256);

        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                tex.SetPixel(x, y, matrix[x, y] ? Color.black : Color.white);
            }
        }
        tex.Apply();
        qrImage.texture = tex;
    }

    public void ShareRoomCode()
    {
        string path = Application.persistentDataPath + "/roomQR.png";
        File.WriteAllBytes(path, ((Texture2D)qrImage.texture).EncodeToPNG());

        new NativeShare()
            .SetText("Join my War Field room!")
            .AddFile(path)
            .SetSubject("Join my game")
            .SetTitle("Join Room")
            .Share();
    }

    #endregion

    #region Join Friend Room via QR or Manual Code

    public void JoinRoomWithQRCode()
    {
        camTexture = new WebCamTexture();
        cameraFeed.texture = camTexture;
        camTexture.Play();
        StartCoroutine(ScanQRCode());
    }   

    IEnumerator ScanQRCode()
    {
        BarcodeReader reader = new BarcodeReader();
        while (true)
        {
            if (camTexture.width > 100 && camTexture.height > 100)
            {
                Texture2D snap = new Texture2D(camTexture.width, camTexture.height);
                snap.SetPixels(camTexture.GetPixels());
                snap.Apply();

                var result = reader.Decode(snap.GetPixels32(), snap.width, snap.height);
                if (result != null)
                {
                    camTexture.Stop();
                    PhotonNetwork.JoinRoom(result.Text);
                    yield break;
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void JoinRoomWithCode()
    {
        string code = roomCodeInput.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            feedbackText1.SetActive(true);
            feedbackText.text = "Please enter a valid room code.";
            return;
        }

        PhotonNetwork.JoinRoom(code);
        feedbackText1.SetActive(true);
        feedbackText.text = "Joining room...";
    }

    public void CopyRoomCode()
    {
        GUIUtility.systemCopyBuffer = roomNameText.text;
        ShowToast("Copied");


    }

    public void UploadQRCodeFromFile()
    {
#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select QR Code", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(path))
        {
            LoadAndDecodeQRCode(path);
        }
#else
        NativeFilePicker.PickFile((path) =>
        {
            if (path != null)
            {
                LoadAndDecodeQRCode(path);
            }
        }, new string[] { "image/*" });
#endif
    }

    private void LoadAndDecodeQRCode(string path)
    {
        byte[] imageData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imageData);
        qrImage.texture = tex;

        BarcodeReader reader = new BarcodeReader();
        var result = reader.Decode(tex.GetPixels32(), tex.width, tex.height);
        if (result != null)
        {
            PhotonNetwork.JoinRoom(result.Text);
        }
        else
        {
            ShowToast("Failed to read QR code.");
        }
    }

    #endregion

    #region Panel Navigation

    public void ShowNextPanel()
    {
        clicksource.Play();
        if (currentPanelIndex < panels.Length - 1)
        {
            panels[currentPanelIndex].DOAnchorPosX(-moveDistance, duration).SetEase(Ease.InOutQuad);
            currentPanelIndex++;
            panels[currentPanelIndex].DOAnchorPosX(0, duration).SetEase(Ease.InOutQuad);
        }
    }

    public void ShowPreviousPanel()
    {
        clicksource.Play();
        if (currentPanelIndex > 0)
        {
            panels[currentPanelIndex].DOAnchorPosX(moveDistance, duration).SetEase(Ease.InOutQuad);
            currentPanelIndex--;
            panels[currentPanelIndex].DOAnchorPosX(0, duration).SetEase(Ease.InOutQuad);
        }
        else if (currentPanelIndex == 0)
        {
            confromforquit.SetActive(true);
        }
    }

    public void clicksoundplay()
    {
        clicksource.Play();
    }

    public void conformQuit()
    {
        clicksource.Play();
        Application.Quit();
    }

    public void cancleforquit()
    {
        clicksource.Play();
        confromforquit.SetActive(false);
    }

    #endregion

    #region Toast Notification

    void ShowToast(string message)
    {
        StopAllCoroutines();
        StartCoroutine(ToastRoutine(message));
    }

    IEnumerator ToastRoutine(string message)
    {
        toastPanel.SetActive(true);
        toastText.text = message;
        yield return new WaitForSeconds(2f);
        toastPanel.SetActive(false);
    }

    #endregion

    #region Online Matchmaking

    public void ShowOnlinePlayers()
    {
        foreach (Transform child in playerCarousel)
            Destroy(child.gameObject);

        foreach (Player player in PhotonNetwork.PlayerListOthers)
        {
            GameObject card = Instantiate(playerCardPrefab, playerCarousel);
            card.transform.Find("Username").GetComponent<Text>().text = player.NickName;
            card.transform.Find("InviteButton").GetComponent<Button>().onClick.AddListener(() => InvitePlayer(player));
        }
    }

    void InvitePlayer(Player target)
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            currentInviteRoom = PhotonNetwork.CurrentRoom.Name;
            object[] content = new object[] { PhotonNetwork.LocalPlayer.NickName, currentInviteRoom };
            RaiseEventOptions options = new RaiseEventOptions { TargetActors = new int[] { target.ActorNumber } };
            PhotonNetwork.RaiseEvent(100, content, options, ExitGames.Client.Photon.SendOptions.SendReliable);
            StartCoroutine(StartInviteTimer());
        }
    }

    IEnumerator StartInviteTimer()
    {
        float timer = 30f;
        while (timer > 0f)
        {
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }
        Debug.Log("Invite timed out.");
    }

    // Quick Match button handler for random matchmaking
    public void OnQuickMatchButtonClicked()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    #endregion

    #region Photon Callbacks

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void OnEvent(ExitGames.Client.Photon.EventData photonEvent)
    {
        if (photonEvent.Code == 100)
        {
            object[] data = (object[])photonEvent.CustomData;
            string inviterName = (string)data[0];
            string roomToJoin = (string)data[1];

            Debug.Log($"{inviterName} has invited you to a match! Room: {roomToJoin}");

            // Here you can open a UI to accept or decline the invite
        }
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room Created: " + PhotonNetwork.CurrentRoom.Name);
    }

    // Handle random matchmaking failure by creating a new room
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        string roomName = "Room_" + Random.Range(1000, 10000);
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(roomName, options);
    }

    // When joined a room, start game if 2 players are present
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            PhotonNetwork.LoadLevel("Game");
        }
        else
        {
            // Wait for another player
            feedbackTextInvite.text = "Waiting for another player to join...";
            //ShowToast("Waiting for another player to join...");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        feedbackText.text = "Failed to join room. Try again.";
        Debug.LogError("Join Room Failed: " + message);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        feedbackText.text = "Room creation failed. Try again.";

    }

    // When another player joins, start game if 2 players are present
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName} entered the room.");
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            Debug.Log("2 players joined. Starting the game...");
            PhotonNetwork.LoadLevel("Game");
        }
    }

    #endregion

    void Update()
    {
        // --- ScrollRect Zoom/Scale Logic ---
        if (scrollRect != null && itemList != null && itemList.Count > 0)
        {
            Vector3[] viewWorldCorners = new Vector3[4];
            scrollRect.viewport.GetWorldCorners(viewWorldCorners);
            float center = (viewWorldCorners[2].x + viewWorldCorners[0].x) / 2;
            foreach (RectTransform item in itemList)
            {
                if (item == null) continue;
                Vector3[] itemCorners = new Vector3[4];
                item.GetWorldCorners(itemCorners);
                float itemCenter = (itemCorners[2].x + itemCorners[0].x) / 2;
                float distance = Mathf.Abs(center - itemCenter);
                float scale = Mathf.Lerp(maxScale, minScale, distance / (scrollRect.viewport.rect.width / 2));
                item.localScale = Vector3.Lerp(item.localScale, new Vector3(scale, scale, 1f), Time.deltaTime * zoomSpeed);
            }
        }
    }

    // // Camera control for lobby
    // public void OpenCamera()
    // {
    //     if (webcamTexture == null)
    //         webcamTexture = new WebCamTexture();
    //     webcamTexture.Play();
    // }

    public void CloseQRCodeCamera()
    {
        if (camTexture != null && camTexture.isPlaying)
        {
            camTexture.Stop();
            camTexture = null;
            opencamobject.SetActive(true);
            cameraFeed.texture = null; // Optionally clear the UI
        }
    }

    public void OnBackButtonClicked()
    {
        CloseQRCodeCamera();
        // ...baaki back navigation code...
    }
}
