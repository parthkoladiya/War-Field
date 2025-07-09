using DG.Tweening;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.Playables;
using ExitGames.Client.Photon;

public enum stringType
{
    None,
    Tank,
    AC,
    Str
}

public class heightInfo
{
    public bool isAccseptable = true;
    public List<Box> heighlitedAreas = new List<Box>();
}

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    // --- Add this flag for weapon set phase ---
    public bool isWeaponSetActive = false;

    [Header("Game Settings")]
    public float placementTime = 120f;

    [Header("References")]
    public Transform gridParent;
    public Sprite highLightSprite;

    public Transform opponentGridParent;

    public TextMeshProUGUI timerText;
    public TextMeshProUGUI weaponText;
    public GameObject playButton;

    public GameObject tossPanel;
    private Tween flipTween;
    public GameObject spriteObj;
    public TextMeshProUGUI headOrTail;
    public GameObject[] headTailCoin;
    [SerializeField] TextMeshProUGUI tossWonOrLossText;
    public TextMeshProUGUI YourCallText;

    public GameObject topInGame;
    public GameObject topPlacement;

    public Image playerIcon;
    public Image OpponentIcon;
    public Sprite[] characterSprites;
    public int CharacterIndex;
    public Sprite mySprite;
    public Sprite opponentSprite;

    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI opponentNameText;

    private float remainingTime;

    private bool isPlayerReady = false;
    private bool isOpponentReady = false;
    private string userChoice = "";

    public List<heightInfo> CurruntHeightInfos = new List<heightInfo>();
    public Sprite boomSprite;
    public Sprite explosionSprite;
    public TextMeshProUGUI winText;
    public GameObject winLosePanel;
    public GameObject areyousurepanel;
    public GameObject opponentNotLongerPanel;
    public GameObject ReconnectField;


    public string curruntStringType;
    public Box currentSelectedBox = null;
    public Weponselector curruntBtn = null;
    public List<Weponselector> btn = new List<Weponselector>();
    public static GameManager GameManagerobj;

    public bool isCliked = false;
    public int attckSize = 3;
    public int curruntAttack = 0;
    public bool isMyTurn = false;
    public History cuuruntHistory = null;
    public List<List<int>> curruntHistoryList = new List<List<int>>();
    public Transform opponentLogTransform;
    public Transform playerLogTransform;
    public GameObject LogPanel;
    public List<List<Box>> finalMap = new List<List<Box>>();
    public List<List<Box>> opponentFinalMap = new List<List<Box>>();
    public int[] marksForMap = { 4, 4, 3, 2, 2, 2 };
    public int[] opponentMarksForMap = { 4, 4, 3, 2, 2, 2 };
    public Sprite blastTile;
    public GameObject allBtnNode;
    public Sprite Defect;
    public Sprite Victory;
    public Image VictoryBadgeAvtarFrem;
    public Sprite[] DefectedImage;
    public Text turnTimer;
    public int TimerValue = 0;
    public int TotalTimer = 45;
    public CountdownTimer countdownTimer;
    public Text messageText;
    public Coroutine mYCoroutine = null;
    public GameObject[] allFrems;
    public GameObject[] allFremBtn;

    [Header("Sound effects")]
    public AudioSource winsound;
    public AudioSource lostsound;
    public AudioSource coinflipsound;
    public AudioSource normalblast;
    public AudioSource BoomBlast;

    [Header("Reconnection Settings")]
    public float reconnectionTimeLimit = 30f;
    private float currentReconnectionTime;
    private bool isReconnecting = false;
    private Coroutine reconnectionCoroutine;
    private bool wasInGame = false;

    public TextMeshProUGUI myTriesText;
    public TextMeshProUGUI opponentTriesText;
    private int playerTries = 15;
    private int opponentTries = 15;

    private bool isFinalChancePhase = false;
    private bool isFinalChanceDoneByPlayer = false;
    private bool isFinalChanceDoneByOpponent = false;



    public GameObject[] finalChancePanels;   // 3 panels ke liye
    public GameObject[] finalChanceButtons;  // 3 extra buttons ke liye

    //[Header("WEAPON BTN")]
    //public GameObject tankBTN;
    //public GameObject acBTN;
    //public GameObject strBTN;

    // List to save opponent weapon positions
    public List<Vector2Int> savedOpponentWeaponPositions = new List<Vector2Int>();

    private string myFinalChanceWeapon = "";
    private string opponentFinalChanceWeapon = "";
    private bool isMyFinalChanceWeaponSet = false;
    private bool isOpponentFinalChanceWeaponSet = false;
    private bool isFinalChanceSubmitter = false;

    // public WeaponLogImage[] weaponLogImages; // Inspector me assign karein (order: Tank1, Tank2, AC1, AC2, AC3, Str)

    // Track hit counts for each weapon (by type and index)
    public Dictionary<string, int> weaponHitCounts = new Dictionary<string, int>();

    private string lastRoomName = ""; // Store last room name for rejoin

    private float inactivityTimer = 0f;
    public float inactivityLimit = 180f; // 1 minute

    private bool isTimerPaused = false;

    [Header("Turn Timer")]
    public float timerDuration = 30f; // Default timer duration in seconds

    // Add this for the popup
    public GameObject opponentSettingPopup; // Assign in inspector

    public enum GamePhase { Placement, Gameplay }
    public GamePhase currentPhase = GamePhase.Placement;

    public static GameManager getInstance()
    {
        return GameManagerobj;
    }

    private void Awake()
    {
        GameManagerobj = this;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.KeepAliveInBackground = 60;
    }

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //odinHandler.JoinRoom("TestRoom");

        myTriesText.text = "Your Tries: " + playerTries.ToString();
        opponentTriesText.text = "Opponent Tries: " + opponentTries.ToString();
        remainingTime = placementTime;
        weaponText.text = "Select Weapon";

        foreach (Transform child in tossPanel.transform)
        {
            Button btn = child.GetComponent<Button>();
            if (btn) btn.interactable = false;
        }

        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("CharacterIndex", out object myCharObj))
        {
            int myCharIndex = (int)myCharObj;
            if (myCharIndex >= 0 && myCharIndex < characterSprites.Length)
            {
                playerIcon.sprite = characterSprites[myCharIndex];
            }
        }
        playerNameText.text = PhotonNetwork.NickName;

        if (PhotonNetwork.PlayerListOthers.Length > 0)
        {
            Photon.Realtime.Player opponent = PhotonNetwork.PlayerListOthers[0];
            if (opponent.CustomProperties.TryGetValue("CharacterIndex", out object oppCharObj))
            {
                int opponentCharIndex = (int)oppCharObj;
                if (opponentCharIndex >= 0 && opponentCharIndex < characterSprites.Length)
                {
                    OpponentIcon.sprite = characterSprites[opponentCharIndex];
                }
            }
            opponentNameText.text = opponent.NickName;
        }

        currentPhase = GamePhase.Placement; // Set phase to placement at start
    }

    public void closeAllLayer(int index)
    {
        for (int i = 0; i < allFrems.Length; i++)
        {
            allFrems[i].SetActive(false);
            allFremBtn[i].transform.localScale = Vector3.one;
        }

        if (index >= 0 && index < allFrems.Length)
        {
            allFrems[index].gameObject.SetActive(true);
            allFremBtn[index].transform.localScale = new Vector3(1.2f, 1.2f);
        }
    }

    void Update()
    {
        // Inactivity timer logic
        inactivityTimer += Time.deltaTime;
        if (inactivityTimer >= inactivityLimit)
        {
            Debug.Log("Player inactive for 1 minute. Leaving room...");
            PhotonNetwork.LeaveRoom();
            inactivityTimer = 0f; // Prevent multiple calls
        }
        if (!isPlayerReady)
        {
            if (!isTimerPaused) // Only update timer if not paused
            {
                if (remainingTime >= 0)
                {
                    remainingTime -= Time.deltaTime;
                    timerText.text = "Time Left: " + Mathf.Ceil(remainingTime).ToString();
                }
                else
                {
                    isPlayerReady = true;
                    StartCoroutine(disconnected());
                }
            }
        }
        // Call the synchronized timer update every frame
        UpdateSynchronizedTimer();
    }

    void LateUpdate()
    {
        // Only reset timer during placement phase
        if (!isPlayerReady && (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.touchCount > 0))
        {
            inactivityTimer = 0f;
        }
    }

    public void heighLigteIdentify(Box box)
    {
        int i = box.boxMatrix[0];
        int j = box.boxMatrix[1];
        CurruntHeightInfos.Clear();

        for (int k = 0; k < gridManager.getInstance().gridItems.Count; k++)
        {
            for (int l = 0; l < gridManager.getInstance().gridItems[k].Count; l++)
            {
                Box item = gridManager.getInstance().gridItems[k][l];
                item.heighlightme(false);
            }
        }

        if (curruntStringType == "AC")
        {
            int total = i + j;
            heightInfo hDataUpRight = new heightInfo();
            for (int k = i; k < i + curruntStringType.Length; k++)
            {
                if (k > gridManager.getInstance().gridSize - 1 || (total - k) < 0 || (gridManager.getInstance().gridItems[k][total - k].myChar != "" && gridManager.getInstance().gridItems[k][total - k].myChar != curruntStringType[k - i].ToString()))
                {
                    hDataUpRight.isAccseptable = false;
                    break;
                }
                Box item = gridManager.getInstance().gridItems[k][total - k];
                hDataUpRight.heighlitedAreas.Add(item);
            }
            if (hDataUpRight.isAccseptable && !hDataUpRight.heighlitedAreas.All(item => item.isItFixed)) { heigLight(hDataUpRight.heighlitedAreas); CurruntHeightInfos.Add(hDataUpRight); }

            // Diagonal Left-Down
            heightInfo hDataLeftDwon = new heightInfo();
            for (int k = i; k > i - curruntStringType.Length; k--)
            {
                if (k < 0 || total - k > gridManager.getInstance().gridSize - 1 || (gridManager.getInstance().gridItems[k][total - k].myChar != "" && gridManager.getInstance().gridItems[k][total - k].myChar != curruntStringType[i - k].ToString()))
                {
                    hDataLeftDwon.isAccseptable = false;
                    break;
                }
                Box item = gridManager.getInstance().gridItems[k][total - k];
                hDataLeftDwon.heighlitedAreas.Add(item);
            }
            if (hDataLeftDwon.isAccseptable && !hDataLeftDwon.heighlitedAreas.All(item => item.isItFixed)) { heigLight(hDataLeftDwon.heighlitedAreas); CurruntHeightInfos.Add(hDataLeftDwon); }

            int a = j;
            // Diagonal Right-Down
            heightInfo hDataRightDwon = new heightInfo();
            for (int k = i; k < i + curruntStringType.Length; k++)
            {
                if (k > gridManager.getInstance().gridSize - 1 || a > gridManager.getInstance().gridSize - 1 || (gridManager.getInstance().gridItems[k][a].myChar != "" && gridManager.getInstance().gridItems[k][a].myChar != curruntStringType[k - i].ToString()))
                {
                    hDataRightDwon.isAccseptable = false;
                    break;
                }
                Box item = gridManager.getInstance().gridItems[k][a];
                hDataRightDwon.heighlitedAreas.Add(item);
                a++;
            }
            if (hDataRightDwon.isAccseptable && !hDataRightDwon.heighlitedAreas.All(item => item.isItFixed)) { heigLight(hDataRightDwon.heighlitedAreas); CurruntHeightInfos.Add(hDataRightDwon); }

            int b = j;
            // Diagonal Right-Up
            heightInfo hDataRightUp = new heightInfo();
            for (int k = i; k > i - curruntStringType.Length; k--)
            {
                if (k < 0 || b < 0 || gridManager.getInstance().gridItems[k][b].myChar != "" && gridManager.getInstance().gridItems[k][b].myChar != curruntStringType[i - k].ToString())
                {
                    hDataRightUp.isAccseptable = false;
                    break;
                }
                Box item = gridManager.getInstance().gridItems[k][b];
                hDataRightUp.heighlitedAreas.Add(item);
                b--;
            }
            if (hDataRightUp.isAccseptable && !hDataRightUp.heighlitedAreas.All(item => item.isItFixed)) { heigLight(hDataRightUp.heighlitedAreas); CurruntHeightInfos.Add(hDataRightUp); }
        }
        else
        {
            // Vertical Down
            heightInfo hDataDwon = new heightInfo();
            for (int k = i; k < i + curruntStringType.Length; k++)
            {
                if (k > gridManager.getInstance().gridSize - 1 || (gridManager.getInstance().gridItems[k][j].myChar != "" && gridManager.getInstance().gridItems[k][j].myChar != curruntStringType[k - i].ToString()))
                {
                    hDataDwon.isAccseptable = false;
                    break;
                }
                Box item = gridManager.getInstance().gridItems[k][j];
                hDataDwon.heighlitedAreas.Add(item);
            }
            if (hDataDwon.isAccseptable && !hDataDwon.heighlitedAreas.All(item => item.isItFixed)) { heigLight(hDataDwon.heighlitedAreas); CurruntHeightInfos.Add(hDataDwon); }

            // Vertical Up
            heightInfo hDataUp = new heightInfo();
            for (int k = i; k > i - curruntStringType.Length; k--)
            {
                if (k < 0 || (gridManager.getInstance().gridItems[k][j].myChar != "" && gridManager.getInstance().gridItems[k][j].myChar != curruntStringType[i - k].ToString()))
                {
                    hDataUp.isAccseptable = false;
                    break;
                }
                Box item = gridManager.getInstance().gridItems[k][j];
                hDataUp.heighlitedAreas.Add(item);
            }
            if (hDataUp.isAccseptable && !hDataUp.heighlitedAreas.All(item => item.isItFixed)) { heigLight(hDataUp.heighlitedAreas); CurruntHeightInfos.Add(hDataUp); }

            // Horizontal Right
            heightInfo hDataRigt = new heightInfo();
            for (int k = j; k < j + curruntStringType.Length; k++)
            {
                if (k > gridManager.getInstance().gridSize - 1 || (gridManager.getInstance().gridItems[i][k].myChar != "" && gridManager.getInstance().gridItems[i][k].myChar != curruntStringType[k - j].ToString()))
                {
                    hDataRigt.isAccseptable = false;
                    break;
                }
                Box item = gridManager.getInstance().gridItems[i][k];
                hDataRigt.heighlitedAreas.Add(item);
            }
            if (hDataRigt.isAccseptable && !hDataRigt.heighlitedAreas.All(item => item.isItFixed)) { heigLight(hDataRigt.heighlitedAreas); CurruntHeightInfos.Add(hDataRigt); }

            // Horizontal Left
            heightInfo hDataLeft = new heightInfo();
            for (int k = j; k > j - curruntStringType.Length; k--)
            {
                if (k < 0 || (gridManager.getInstance().gridItems[i][k].myChar != "" && gridManager.getInstance().gridItems[i][k].myChar != curruntStringType[j - k].ToString()))
                {
                    hDataLeft.isAccseptable = false;
                    break;
                }
                Box item = gridManager.getInstance().gridItems[i][k];
                hDataLeft.heighlitedAreas.Add(item);
            }
            if (hDataLeft.isAccseptable && !hDataLeft.heighlitedAreas.All(item => item.isItFixed)) { heigLight(hDataLeft.heighlitedAreas); CurruntHeightInfos.Add(hDataLeft); }
        }

        if (CurruntHeightInfos.Count > 0)
        {
            box.myChar = curruntStringType[0].ToString();
            box.myLabel.text = curruntStringType[0].ToString();
            currentSelectedBox = box;
            return;
        }

        currentSelectedBox = null;
        return;
    }

    IEnumerator disconnected()
    {
        Debug.Log(" Disconnect ");
        PhotonNetwork.LeaveRoom();
        while (PhotonNetwork.InRoom)
        {
            yield return null;
        }
        // Optional: PhotonNetwork.Disconnect(); // Only if you want to fully disconnect from Photon
        SceneManager.LoadScene(0);
    }

    public void heigLight(List<Box> heighlitedAreas)
    {
        foreach (var item in heighlitedAreas)
        {
            item.heighlightme(true);
        }
    }

    public void placeAstring(string selectedString, Box box)
    {
        if (string.IsNullOrEmpty(selectedString))
        {
            return;
        }

        List<Box> finalList = new List<Box>();

        for (int i = 0; i < CurruntHeightInfos.Count; i++)
        {
            if (CurruntHeightInfos[i].heighlitedAreas.Contains(box))
            {
                finalList = CurruntHeightInfos[i].heighlitedAreas;
                break;
            }
        }

        if (finalList.Count > 0)
        {
            finalMap.Add(finalList);
            for (int i = 0; i < finalList.Count; i++)
            {
                Box Mbox = finalList[i];
                Mbox.isItFixed = true;
                Mbox.myChar = curruntStringType[i].ToString();
                Mbox.myLabel.text = Mbox.myChar.ToString();
            }

            // Decrement weapon count only after successful placement
            if (curruntBtn != null && curruntBtn.currentCount > 0)
            {
                curruntBtn.currentCount--;
                curruntBtn.UpdateCountText();
                if (curruntBtn.currentCount == 0)
                {
                    curruntBtn.OnAllUsed();
                }
            }
        }

        currentSelectedBox = null;
        curruntStringType = "";
        CurruntHeightInfos.Clear();

        for (int k = 0; k < gridManager.getInstance().gridItems.Count; k++)
        {
            for (int l = 0; l < gridManager.getInstance().gridItems[k].Count; l++)
            {
                Box item = gridManager.getInstance().gridItems[k][l];
                item.heighlightme(false);
            }
        }

        // Reset weapon button state
        if (curruntBtn != null)
        {
            curruntBtn.myImage.color = Color.white;
            curruntBtn.myImage.gameObject.GetComponent<Toggle>().interactable = true;
        }

        if (Weponselector.instance.currentCount == 0)
        {
            btn.All(item => item.GetComponent<Toggle>().interactable = false);
        }

        foreach (var weaponBtn in btn)
        {
            if (weaponBtn.currentCount == 0)
            {
                weaponBtn.OnAllUsed();
            }
        }

        if (btn.All(item => item.myImage.GetComponent<Toggle>().interactable == false))
        {
            Debug.Log(" All btn are off ");
            finalMap.Sort((a, b) => b.Count.CompareTo(a.Count));
            isPlayerReady = true;

            photonView.RPC("RPC_OpponentReady", RpcTarget.Others);

            // Only show the popup to self if opponent is not ready
            if (!isOpponentReady)
            {
                if (opponentSettingPopup != null)
                    opponentSettingPopup.SetActive(true);
            }
            else
            {
                if (opponentSettingPopup != null)
                    opponentSettingPopup.SetActive(false);
                playButton.SetActive(true);
                EnableTossButtonsIfMaster();
            }
        }
        // Save state after placement
        SaveStateToRoom();
    }

    [PunRPC]
    void RPC_OpponentReady()
    {
        Debug.Log(" RPC Call ");
        isOpponentReady = true;

        SaveOpponentWeaponPositions();

        // Only hide the popup and show play button if local player is also ready
        if (isPlayerReady)
        {
            if (opponentSettingPopup != null)
                opponentSettingPopup.SetActive(false);
            playButton.SetActive(true);
            EnableTossButtonsIfMaster();
        }
        // Do not show the popup here if local player is not ready
    }

    void EnableTossButtonsIfMaster()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (Transform child in tossPanel.transform)
            {
                Button btn = child.GetComponent<Button>();
                if (btn) btn.interactable = true;

            }
        }
    }

    public void OnClickPlay()
    {
        playButton.SetActive(false);
        allBtnNode.SetActive(false);
        gridParent.gameObject.SetActive(false);

        tossPanel.SetActive(true);
        if (!PhotonNetwork.IsMasterClient)
        {
            YourCallText.text = "Waiting for opponent's toss...";
        }
    }

    public void setLog(Transform parentTransform, string lastData, List<List<Box>> oppositSideGrid, List<List<Box>> Map, int[] allMarksforMap)
    {
        if (!string.IsNullOrEmpty(lastData))
        {
            History node = Instantiate(LogPanel, parentTransform).GetComponent<History>();
            // Weapon type mapping (order: Map[0]=Tank1, Map[1]=Tank2, AC1, AC2, AC3, Str)
            string[] weaponTypes = { "Tank", "Tank", "AC", "AC", "AC", "Str" };

            // Use weponsImage array from Historyi,
            WeaponLogImage[] logImages = new WeaponLogImage[6];
            for (int i = 0; i < node.weponsImage.Length && i < 6; i++)
            {
                var img = node.weponsImage[i];
                if (img != null)
                {
                    WeaponLogImage wli = img.GetComponent<WeaponLogImage>();
                    if (wli == null)
                        wli = img.gameObject.AddComponent<WeaponLogImage>();
                    wli.weaponType = weaponTypes[i];
                    wli.weaponIndex = weaponTypes.Take(i).Count(t => t == weaponTypes[i]);
                    // TODO: Assign sprites here if needed
                    logImages[i] = wli;
                }
            }

            for (int weaponIdx = 0; weaponIdx < Map.Count; weaponIdx++)
            {
                var weaponBoxes = Map[weaponIdx];
                int totalAlphabets = weaponBoxes.Count;
                int hitCount = weaponBoxes.Count(box => box.isScrached);

                // Find the correct log image by type and index (case-insensitive)
                string type = weaponTypes[weaponIdx];
                int typeIndex = 0;
                for (int i = 0; i < weaponIdx; i++)
                    if (weaponTypes[i] == type) typeIndex++;

                Debug.Log($"Looking for WeaponLogImage: type={type}, typeIndex={typeIndex}");
                foreach (var img in logImages)
                {
                    Debug.Log($"logImages: type={img.weaponType}, index={img.weaponIndex}");
                }

                var logImage = logImages.FirstOrDefault(img =>
                    string.Equals(img.weaponType, type, System.StringComparison.OrdinalIgnoreCase) &&
                    img.weaponIndex == typeIndex);
                if (logImage != null)
                {
                    logImage.SetState(hitCount, totalAlphabets);
                }
                else
                {
                    Debug.LogError($"WeaponLogImage not found for type={type}, typeIndex={typeIndex}. logImages count={logImages.Length}");
                    foreach (var img in logImages)
                    {
                        Debug.Log($"logImages: type={img.weaponType}, index={img.weaponIndex}");
                    }
                }
            }

            // (Optional) Existing log text update for each move
            List<List<int>> hData = JsonConvert.DeserializeObject<List<List<int>>>(lastData);
            for (int i = 0; i < hData.Count; i++)
            {
                int x = hData[i][0];
                int y = hData[i][1];
                Box temp = oppositSideGrid[x][y];
                node.historyInfos[i].text = (x + 1).ToString() + "" + (char)('A' + y);
            }
        }
    }

    public void showWarningPopup(Text text, string message)
    {
        if (text != null && text.transform.parent != null)
        {
            text.transform.parent.gameObject.SetActive(true);
            text.text = message;
        }
    }

    [PunRPC]
    public void SetTurn(bool myTurn, string lastData = "")
    {
        Debug.Log(" My Turn " + myTurn);
        countdownTimer.timerText.text = "0";
        isMyTurn = myTurn;
        curruntAttack = 0; // Reset attack count for new turn

        // --- TIMER LOGIC: Only start timer if it's my turn and not in placement/toss phase ---
        if (mYCoroutine != null)
        {
            StopCoroutine(mYCoroutine);
            mYCoroutine = null;
        }
        // --- SYNC TIMER: Set timerDuration for every turn ---
        timerDuration = 120f; // Set your desired turn timer here
        if (isMyTurn && !topPlacement.activeSelf && !tossPanel.activeSelf)
        {
            mYCoroutine = StartCoroutine(countdownTimer.StartCountdown());
            // Synchronized turn timer: MasterClient sets the turn start time
            if (PhotonNetwork.IsMasterClient)
            {
                ExitGames.Client.Photon.Hashtable turnProps = new ExitGames.Client.Photon.Hashtable();
                turnProps["TurnStartTime"] = PhotonNetwork.Time;
                PhotonNetwork.CurrentRoom.SetCustomProperties(turnProps);
            }
        }

        setLog(opponentLogTransform, lastData, gridManager.getInstance().gridItems, finalMap, marksForMap);
        YourCallText.text = isMyTurn ? "Your call" : "Opponent call";
        OnTurnChanged(myTurn);
        StartCoroutine(UpdateUI());
        // Save state after turn change
        SaveStateToRoom();
    }

    [PunRPC]
    public void RPC_ReceiveAttack(int x, int y)
    {
        Box item = gridManager.getInstance().gridItems[x][y];
        item.Boom(true);

        if (item.myChar.Length == 0)
        {
            item.myImage.gameObject.SetActive(true);
        }
    }

    public void backtothelobby()
    {
        areyousurepanel.SetActive(true);
        isTimerPaused = true; // Pause the timer
    }

    public void confortoleave()
    {
        StartCoroutine(LeaveRoomAndDisconnect());
    }

    public void canclegame()
    {
        areyousurepanel.SetActive(false);
        isTimerPaused = false; // Resume the timer
    }

    private IEnumerator LeaveRoomAndDisconnect()
    {
        PhotonNetwork.LeaveRoom();
        while (PhotonNetwork.InRoom)
            yield return null;
        // Optional: PhotonNetwork.Disconnect(); // Only if you want to fully disconnect from Photon
        SceneManager.LoadScene(1);
    }

    public void OnHeadButtonClicked()
    {
        userChoice = "Heads";
        photonView.RPC("RPC_ShowTossChoice", RpcTarget.All, "Heads", PhotonNetwork.LocalPlayer.ActorNumber);
        bool isHeads = Random.Range(0, 2) == 0;
        photonView.RPC("StartToss", RpcTarget.Others, isHeads, true);
        StartToss(isHeads);
    }

    public void OnTailButtonClicked()
    {
        userChoice = "Tails";
        photonView.RPC("RPC_ShowTossChoice", RpcTarget.All, "Tails", PhotonNetwork.LocalPlayer.ActorNumber);
        bool isHeads = Random.Range(0, 2) == 0;
        photonView.RPC("StartToss", RpcTarget.Others, isHeads, true);
        StartToss(isHeads);
    }

    [PunRPC]
    void StartToss(bool isHeads, bool isFromOther = false)
    {
        Debug.Log("isFromOther : " + isFromOther);
        flipTween = spriteObj.transform.DOScaleX(-1f, 0.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        coinflipsound.Play();
        DOVirtual.DelayedCall(2f, () =>
        {
            flipTween.Kill();
            spriteObj.transform.localScale = Vector3.one;
            string tossResult = isHeads ? "Heads" : "Tails";
            bool didMasterWin = (tossResult == userChoice);
            if (!isFromOther)
            {
                photonView.RPC("RPC_ReceiveTossResult", RpcTarget.All, tossResult, didMasterWin);
            }
        });
    }

    [PunRPC]
    void RPC_ReceiveTossResult(string result, bool didMasterWin)
    {
        coinflipsound.Play();
        bool isMe = PhotonNetwork.IsMasterClient;
        bool iWonToss = (isMe && didMasterWin) || (!isMe && !didMasterWin);
        headOrTail.text = result;
        headTailCoin[0].SetActive(result == "Heads");
        headTailCoin[1].SetActive(result == "Tails");

        tossWonOrLossText.text = iWonToss ? "You Won the Toss!" : "You Lost the Toss!";
        YourCallText.text = iWonToss ? "Your Call" : "Opponent Call";

        SetTurn(iWonToss);

        DOVirtual.DelayedCall(2f, () =>
        {
            ReadyForBattle();
        });
    }

    [PunRPC]
    public void RPC_ShowTossChoice(string choice, int actorNumber)
    {
        Button headButton = tossPanel.transform.Find("H").GetComponent<Button>();
        Button tailButton = tossPanel.transform.Find("T").GetComponent<Button>();
        headButton.interactable = false;
        tailButton.interactable = false;
        if (choice == "Heads")
        {
            headButton.GetComponent<Image>().color = Color.green;
            tailButton.GetComponent<Image>().color = Color.white;
        }
        else
        {
            tailButton.GetComponent<Image>().color = Color.green;
            headButton.GetComponent<Image>().color = Color.white;
        }
    }

    public void ReadyForBattle()
    {
        if (!isPlayerReady) return;

        if (isOpponentReady)
        {
            SendMyUnitsToOpponent();

            StartBattle();
        }
    }

    void SendMyUnitsToOpponent()
    {
        List<List<string>> gridChars = new List<List<string>>();
        for (int i = 0; i < gridManager.getInstance().gridSize; i++)
        {
            gridChars.Add(new List<string>());
            for (int j = 0; j < gridManager.getInstance().gridSize; j++)
            {
                string myChar = gridManager.getInstance().gridItems[i][j].myChar;
                gridChars[i].Add(myChar);
            }
        }

        List<List<int[]>> tempMap = new List<List<int[]>>();
        for (int i = 0; i < finalMap.Count; i++)
        {
            tempMap.Add(new List<int[]>());
            for (int j = 0; j < finalMap[i].Count; j++)
            {
                tempMap[i].Add(new int[2]);
                tempMap[i][j][0] = finalMap[i][j].boxMatrix[0];
                tempMap[i][j][1] = finalMap[i][j].boxMatrix[1];
            }
        }
        tempMap.Sort((a, b) => b.Count.CompareTo(a.Count));


        photonView.RPC("RPC_ReceiveOpponentUnits", RpcTarget.Others, JsonConvert.SerializeObject(gridChars), JsonConvert.SerializeObject(tempMap));
    }


    [PunRPC]
    void RPC_ReceiveOpponentUnits(string jsonData, string opponentFinalMapString)
    {
        Debug.Log("Receiving Opponent Units: " + opponentFinalMapString);

        List<List<string>> opponentUnitChars = JsonConvert.DeserializeObject<List<List<string>>>(jsonData);
        for (int i = 0; i < opponentUnitChars.Count; i++)
        {
            for (int j = 0; j < opponentUnitChars[i].Count; j++)
            {
                gridManager.getInstance().opponentGridItems[i][j].myChar = opponentUnitChars[i][j];
                Debug.Log($"Setting opponentGridItems[{i}][{j}].myChar = {opponentUnitChars[i][j]}");
            }
        }

        opponentFinalMap.Clear(); // Ensure we clear before adding new data
        List<List<int[]>> opponentMapCoords = JsonConvert.DeserializeObject<List<List<int[]>>>(opponentFinalMapString);
        for (int i = 0; i < opponentMapCoords.Count; i++)
        {
            opponentFinalMap.Add(new List<Box>());
            for (int j = 0; j < opponentMapCoords[i].Count; j++)
            {
                int x = opponentMapCoords[i][j][0];
                int y = opponentMapCoords[i][j][1];
                Debug.Log($"Opponent Unit Box: [{x}, {y}]");
                Box el = gridManager.getInstance().opponentGridItems[x][y];
                opponentFinalMap[i].Add(el);
            }
        }
        closeAllLayer(1);
        SaveOpponentWeaponPositions();
    }


    void StartBattle()
    {
        tossPanel.SetActive(false);
        topPlacement.SetActive(false);
        topInGame.SetActive(true);
        //SetTurnCoroutine();
        Debug.Log("Battle Started");
        closeAllLayer(0);
        //odinRoomName = PhotonNetwork.CurrentRoom.Name;
        //JoinOdinRoom(odinRoomName);

        // Set timer for both players
        timerDuration = 120f; // or your desired value

        // MasterClient sets the start time
        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable turnProps = new ExitGames.Client.Photon.Hashtable();
            turnProps["TurnStartTime"] = PhotonNetwork.Time;
            PhotonNetwork.CurrentRoom.SetCustomProperties(turnProps);
        }

        // Make sure timer is visible
        if (countdownTimer != null && countdownTimer.timerText != null)
            countdownTimer.timerText.gameObject.SetActive(true);

        // Immediately update the timer so it doesn't show 0 at start
        UpdateSynchronizedTimer();
    }

    [PunRPC]
    public void RPC_GameOver(bool didWin)
    {
        winLosePanel.SetActive(true);
        Debug.Log(" bool is:: " + didWin);
        if (!didWin)
        {
            Debug.Log(" bool is:: " + didWin);
            VictoryBadgeAvtarFrem.sprite = Defect;
            lostsound.Play();
        }
        else
        {
            VictoryBadgeAvtarFrem.sprite = Victory;
            winsound.Play();
        }

        winText.text = didWin ? "YOU WON!" : "YOU LOST!";
    }

    public void OnClickExitToMainMenu()
    {
        // Save the current room name before leaving
        if (PhotonNetwork.CurrentRoom != null)
            lastRoomName = PhotonNetwork.CurrentRoom.Name;
        PhotonNetwork.LeaveRoom();
        StartCoroutine(LoadMainMenuAfterLeft());
    }

    IEnumerator LoadMainMenuAfterLeft()
    {
        while (PhotonNetwork.InRoom)
            yield return null;
        // Optional: PhotonNetwork.Disconnect(); // Only if you want to fully disconnect from Photon
        SceneManager.LoadScene(0);
    }

    public void ShowPlayerScreen()
    {
        gridParent.gameObject.SetActive(true);
        opponentGridParent.gameObject.SetActive(false);
    }

    public void ShowEnemyScreen()
    {
        gridParent.gameObject.SetActive(false);
        opponentGridParent.gameObject.SetActive(true);
    }

    public void EnemyLog()
    {
        gridParent.gameObject.SetActive(false);
        opponentGridParent.gameObject.SetActive(false);
    }

    // --- CUSTOM PLAYER COUNT AND PER-ACTOR DATA ---
    // Track original ActorNumbers for reconnecting players
    private Dictionary<string, int> originalActorNumbers = new Dictionary<string, int>();
    private Dictionary<int, string> actorNumberToPlayerName = new Dictionary<int, string>();
    // Track the last disconnected player's ActorNumber
    private int lastDisconnectedActorNumber = -1;
    
    // Increment ActivePlayerCount in room properties
    void IncrementActivePlayerCount()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null)
        {
            int count = 0;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("ActivePlayerCount", out object value))
                count = (int)value;
            count++;
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props["ActivePlayerCount"] = count;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
    }

    // Decrement ActivePlayerCount in room properties
    void DecrementActivePlayerCount()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null)
        {
            int count = 0;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("ActivePlayerCount", out object value))
                count = (int)value;
            count = Mathf.Max(0, count - 1);
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props["ActivePlayerCount"] = count;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
    }

    // Store original ActorNumber when player leaves
    void StoreOriginalActorNumber(Player player)
    {
        if (!originalActorNumbers.ContainsKey(player.NickName))
        {
            originalActorNumbers[player.NickName] = player.ActorNumber;
            actorNumberToPlayerName[player.ActorNumber] = player.NickName;
            Debug.Log($"Stored original ActorNumber {player.ActorNumber} for {player.NickName}");
        }
    }

    // Get original ActorNumber for a player
    int GetOriginalActorNumber(string playerName)
    {
        if (originalActorNumbers.ContainsKey(playerName))
        {
            return originalActorNumbers[playerName];
        }
        return -1; // Not found
    }

    // Check if this is a reconnecting player
    bool IsReconnectingPlayer(Player player)
    {
        return originalActorNumbers.ContainsKey(player.NickName);
    }

    // Save state for a specific player using NickName
    void SaveStateForPlayer(Player player)
    {
        string key = $"GameState_{player.NickName}";
        var state = GetCurrentGameState();
        string json = JsonConvert.SerializeObject(state);
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props[key] = json;
        if (PhotonNetwork.CurrentRoom != null)
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    // Load state for a specific player using NickName
    void LoadStateForPlayer(Player player)
    {
        string key = $"GameState_{player.NickName}";
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out object stateJson))
        {
            var state = JsonConvert.DeserializeObject<GameStateData>(stateJson.ToString());
            ApplyGameState(state);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"OnPlayerLeftRoom: {otherPlayer.NickName}, ActorNumber: {otherPlayer.ActorNumber}");
        // Store the ActorNumber of the player who just left
        lastDisconnectedActorNumber = otherPlayer.ActorNumber;
        // Store original ActorNumber for potential reconnection
        StoreOriginalActorNumber(otherPlayer);
        
        if (PhotonNetwork.IsMasterClient)
        {
            DecrementActivePlayerCount();
        }
        StartReconnectionTimer(otherPlayer);
    }

    private void StartReconnectionTimer(Player disconnectedPlayer)
    {
        if (reconnectionCoroutine != null)
            StopCoroutine(reconnectionCoroutine);

        isReconnecting = true;
        currentReconnectionTime = reconnectionTimeLimit;

        if (ReconnectionUIManager.Instance != null)
            ReconnectionUIManager.Instance.ShowReconnectionUI(disconnectedPlayer.NickName);

        reconnectionCoroutine = StartCoroutine(ReconnectionTimerRoutine(disconnectedPlayer));
    }

    private System.Collections.IEnumerator ReconnectionTimerRoutine(Player disconnectedPlayer)
    {
        while (currentReconnectionTime > 0 && isReconnecting)
        {
            currentReconnectionTime -= Time.deltaTime;

            if (ReconnectionUIManager.Instance != null)
                ReconnectionUIManager.Instance.UpdateTimer(currentReconnectionTime);

            if (PhotonNetwork.CurrentRoom != null)
            {
                Player[] players = PhotonNetwork.PlayerList;
                foreach (Player player in players)
                {
                    if (player.ActorNumber == disconnectedPlayer.ActorNumber)
                    {
                        HandlePlayerReconnected(player);
                        yield break;
                    }
                }
            }
            yield return null;
        }

        if (isReconnecting)
        {
            Debug.Log("Reconnection time limit reached. Declaring winner.");
            isReconnecting = false;
            // If in gameplay phase, declare the remaining player as winner
            if (currentPhase == GamePhase.Gameplay && PhotonNetwork.PlayerList.Length == 1)
            {
                // Only one player left, declare them as winner
                Player winner = PhotonNetwork.LocalPlayer;
                photonView.RPC("RPC_GameOver", RpcTarget.All, true);
            }
            else
            {
                Player winner = PhotonNetwork.LocalPlayer;
                photonView.RPC("DeclareWinner", RpcTarget.All, winner);
            }
        }
    }

    private void HandlePlayerReconnected(Player reconnectedPlayer)
    {
        
        Debug.Log($"HandlePlayerReconnected called for: {reconnectedPlayer.NickName}, ActorNumber: {reconnectedPlayer.ActorNumber}");
        isReconnecting = false;
        ReconnectionUIManager.Instance.ShowReconnected(reconnectedPlayer.NickName   );

        // Hide the reconnection UI immediately
        if (ReconnectionUIManager.Instance != null)
        {
            ReconnectionUIManager.Instance.HideUI();
        }

        if (reconnectionCoroutine != null)
        {
            StopCoroutine(reconnectionCoroutine);
            reconnectionCoroutine = null;
        }
        // Game resume logic can be added here if needed
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"OnPlayerEnteredRoom: {newPlayer.NickName}, ActorNumber: {newPlayer.ActorNumber}, isReconnecting: {isReconnecting}");
        if (PhotonNetwork.IsMasterClient)
        {
            IncrementActivePlayerCount();
        }
        // Placement phase: reset game for all if rejoin during placement
        if (currentPhase == GamePhase.Placement)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }
        // Gameplay phase: show reconnect popup to local player
        else if (currentPhase == GamePhase.Gameplay)
        {
            if (newPlayer.IsLocal && ReconnectField != null)
            {
                ReconnectField.SetActive(true);
            }
            return;
        }
        if (isReconnecting)
        {
            Debug.Log($"ActorNumber match! Calling HandlePlayerReconnected for {newPlayer.NickName}");
            HandlePlayerReconnected(newPlayer);
        }
        // Always hide the reconnection UI if any player comes back
        if (ReconnectionUIManager.Instance != null)
        {
            ReconnectionUIManager.Instance.HideUI();
        }
        if (PhotonNetwork.PlayerListOthers.Length > 0 && opponentNameText.text == "")
        {
            Photon.Realtime.Player opponent = PhotonNetwork.PlayerListOthers[0];
            if (opponent.CustomProperties.TryGetValue("CharacterIndex", out object oppCharObj))
            {
                int opponentCharIndex = (int)oppCharObj;
                if (opponentCharIndex >= 0 && opponentCharIndex < characterSprites.Length)
                {
                    OpponentIcon.sprite = characterSprites[opponentCharIndex];
                }
            }
            opponentNameText.text = opponent.NickName;
        }
        // Load state for the new player (if needed)
        LoadStateForPlayer(newPlayer);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        // Save the last room name if still available
        if (PhotonNetwork.CurrentRoom != null)
            lastRoomName = PhotonNetwork.CurrentRoom.Name;
        wasInGame = !string.IsNullOrEmpty(lastRoomName);
        base.OnDisconnected(cause);
    }

    public override void OnConnectedToMaster()
    {
        // Try to rejoin the last room if we were in a game
        if (wasInGame && !string.IsNullOrEmpty(lastRoomName))
        {
            PhotonNetwork.RejoinRoom(lastRoomName);
        }
        else
        {
            Debug.LogWarning("No previous room to rejoin or not flagged as wasInGame.");
            wasInGame = false;
        }
    }

    public override void OnJoinedRoom()
    {
        if (wasInGame)
        {
            photonView.RPC("OnPlayerReconnected", RpcTarget.Others, PhotonNetwork.LocalPlayer);
            wasInGame = false;
        }
        // Save state for this player
        SaveStateForPlayer(PhotonNetwork.LocalPlayer);
        // Load state for this player
        LoadStateForPlayer(PhotonNetwork.LocalPlayer);
        // Optionally, update UI or state to resume the game
    }

    [PunRPC]
    private void OnPlayerReconnected(Player reconnectedPlayer)
    {
        if (isReconnecting && reconnectedPlayer.ActorNumber == lastDisconnectedActorNumber)
        {
            HandlePlayerReconnected(reconnectedPlayer);
        }
    }

    [PunRPC]
    private void DeclareWinner(Player winner)
    {
        Debug.Log("Winner declared: " + winner.NickName);
        bool localPlayerWon = (winner.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);
        winLosePanel.SetActive(true);

        if (localPlayerWon)
        {
            winText.text = "YOU WON!";
            VictoryBadgeAvtarFrem.sprite = Victory;
            ReconnectionUIManager.Instance.reconnectionTimerText.gameObject.SetActive(!true);
            winsound.Play();
        }
        else
        {
            winText.text = "YOU LOST!";
            VictoryBadgeAvtarFrem.sprite = Defect;
            ReconnectionUIManager.Instance.reconnectionTimerText.gameObject.SetActive(!true);
            lostsound.Play();
        }
    }

    // UI update for turn-based map switching
    public IEnumerator UpdateUI()
    {
        Debug.Log("HEre in the Update UI");
        if (isMyTurn)
        {
            Debug.Log("HEre in the Update UI in iF");
            Debug.Log(" bool is : " + isMyTurn);
            if (opponentGridParent != null) opponentGridParent.gameObject.SetActive(true);
            if (gridParent != null) gridParent.gameObject.SetActive(false);
            playerTries--;
            myTriesText.text = "Your Tries: " + playerTries.ToString();
            if (playerTries == 0)
            {
                Debug.Log(" Player Enter ");
                chanceareover();
            }
            //isMyTurn = false;
        }
        else
        {
            Debug.Log("HEre in the Update UI in else");
            // Opponent's Turn: Show player map
            if (opponentGridParent != null) opponentGridParent.gameObject.SetActive(false);
            if (gridParent != null) gridParent.gameObject.SetActive(true);
            opponentTries--;
            opponentTriesText.text = " Opponent tires: " + opponentTries.ToString();
            if (opponentTries == 0)
            {
                Debug.Log(" Opponent Enter ");
                chanceareover();
            }
        }
        yield return new WaitForSeconds(0.5f);
    }

    public void OnTurnChanged(bool myTurn)
    {
        isMyTurn = myTurn;
        UpdateUI();
    }

    public void chanceareover()
    {
        if (playerTries == 0 && opponentTries == 0 && !isFinalChancePhase)
        {
            // Check if any player has found all opponent weapons
            bool playerFoundAllOpponentWeapons = opponentFinalMap.All(unit => unit.All(box => box.isScrached));
            bool opponentFoundAllPlayerWeapons = finalMap.All(unit => unit.All(box => box.isScrached));

            if (!playerFoundAllOpponentWeapons && !opponentFoundAllPlayerWeapons)
            {
                ShowDrawResult();
            }
            // else
            // {
            //     StopBattle(); // Fight stops immediately when tries are overs
            //     isFinalChancePhase = true;
            //     StartFinalChancePhase();
            // }
        }
    }

    public void ShowDrawResult()
    {
        winLosePanel.SetActive(true);
        winText.text = "MATCH DRAW!";
        VictoryBadgeAvtarFrem.sprite = null;
        // Optionally, play a draw sound or show a special UI
    }

    private void StopBattle()
    {
        if (topInGame != null)
            topInGame.SetActive(false);
        //if (mYCoroutine != null)
        //{
        //    StopCoroutine(mYCoroutine);
        //    mYCoroutine = null;
        //}
        isMyTurn = false;
    }

    private void StartFinalChancePhase()
    {
        Debug.Log(" StartFinalChancePhase ");
        gridManager.getInstance().RegenerateGrid();
        StopBattle();
        //tankBTN.SetActive(true);
        //acBTN.SetActive(true);
        //strBTN.SetActive(true);
        var grid = gridManager.getInstance().gridItems;
        for (int i = 0; i < grid.Count; i++)
        {
            for (int j = 0; j < grid[i].Count; j++)
            {
                Box box = grid[i][j];
                box.myChar = "";
                box.isItFixed = false;
                if (box.myLabel != null) box.myLabel.text = "";
                if (box.myImage != null) box.myImage.gameObject.SetActive(false);
            }
        }

        foreach (var weaponBtn in btn)
        {
            if (weaponBtn != null)
            {
                weaponBtn.gameObject.SetActive(true);

                // Restore original sprite
                if (weaponBtn.myImage != null && weaponBtn.originalSprite != null)
                    weaponBtn.myImage.sprite = weaponBtn.originalSprite;

                var toggle = weaponBtn.myImage.gameObject.GetComponent<Toggle>();
                if (toggle != null) toggle.interactable = true;

                if (weaponBtn.countText != null) weaponBtn.countText.gameObject.SetActive(true);

                switch (weaponBtn.myselection)
                {
                    case stringType.Tank:
                        weaponBtn.currentCount = 2;
                        break;
                    case stringType.AC:
                        weaponBtn.currentCount = 3;
                        break;
                    case stringType.Str:
                        weaponBtn.currentCount = 1;
                        break;
                    default:
                        weaponBtn.currentCount = 0;
                        break;
                }
                weaponBtn.UpdateCountText();
            }
        }

        allBtnNode.SetActive(true);
        gridParent.gameObject.SetActive(true);
        opponentGridParent.gameObject.SetActive(false);



        // Show all final chance panels
        if (finalChancePanels != null)
        {
            foreach (var panel in finalChancePanels)
                if (panel != null) panel.SetActive(true);
        }
        // Show all extra buttons
        if (finalChanceButtons != null)
        {
            foreach (var btn in finalChanceButtons)
                if (btn != null) btn.SetActive(true);
        }

        // Set allowed weapon types based on opponent's initial placements
        SetAllowedWeaponsForFinalChance();

        // Show message
        if (messageText != null)
            messageText.text = "Final Chance! Place your weapon as per opponent's types.";

        // Reset state for this phase
        isFinalChanceDoneByPlayer = false;
        isFinalChanceDoneByOpponent = false;
    }

    private void SetAllowedWeaponsForFinalChance()
    {
        HashSet<string> allowedTypes = new HashSet<string>();
        foreach (var unit in opponentFinalMap)
        {
            foreach (var box in unit)
            {
                if (!string.IsNullOrEmpty(box.myChar))
                    allowedTypes.Add(box.myChar);
            }
        }
        foreach (var weaponBtn in btn)
        {

            weaponBtn.gameObject.SetActive(true);
            Debug.Log(" BTNON ");

            // if (allowedTypes.Contains(weaponBtn.weaponType))
            // {
            //     Debug.Log(" BTNON ");
            //     weaponBtn.gameObject.SetActive(true);
            // }
            // else
            // {
            //     Debug.Log(" BTNOFF ");
            //     weaponBtn.gameObject.SetActive(false);
            // }
        }
    }

    public void OnFinalChancePlacementDone()
    {
        // Hide the play button if present
        if (playButton != null)
            playButton.SetActive(false);

        myFinalChanceWeapon = curruntStringType;
        isMyFinalChanceWeaponSet = true;
        isFinalChanceSubmitter = true;

        // Send your weapon to opponent
        photonView.RPC("RPC_ReceiveFinalChanceWeapon", RpcTarget.Others, myFinalChanceWeapon);

        isFinalChanceDoneByPlayer = true;
        //photonView.RPC("RPC_FinalChanceDone", RpcTarget.Others);
        CheckFinalChancePhaseEnd();
    }

    [PunRPC]
    public void RPC_ReceiveFinalChanceWeapon(string weapon)
    {
        opponentFinalChanceWeapon = weapon;
        isOpponentFinalChanceWeaponSet = true;
        CheckFinalChancePhaseEnd();
    }

    private void CheckFinalChancePhaseEnd()
    {
        Debug.Log(" CheckFinal ");
        if (isFinalChanceDoneByPlayer && isFinalChanceDoneByOpponent
            && isMyFinalChanceWeaponSet && isOpponentFinalChanceWeaponSet)
        {
            Debug.Log(" if working  ");
            allBtnNode.SetActive(false);
            gridParent.gameObject.SetActive(false);
            // Hide all final chance panels
            if (finalChancePanels != null)
            {
                Debug.Log(" Second If Working ");
                foreach (var panel in finalChancePanels)
                    if (panel != null) panel.SetActive(false);
            }
            // Hide all extra buttons
            if (finalChanceButtons != null)
            {
                Debug.Log(" BTN if working");
                foreach (var btn in finalChanceButtons)
                    if (btn != null) btn.SetActive(false);
            }
            if (messageText != null)
                messageText.text = "Final chance phase complete!";

            // --- Weapon comparison logic ---
            if (myFinalChanceWeapon == opponentFinalChanceWeapon)
            {
                Debug.Log(" if work");
                if (isFinalChanceSubmitter)
                {
                    // You win
                    photonView.RPC("RPC_GameOver", RpcTarget.Others, false);
                }
                else
                {
                    photonView.RPC("RPC_GameOver", RpcTarget.Others, true); // Tell opponent they won
                    winLosePanel.SetActive(true);
                    winText.text = "YOU LOST!";
                    VictoryBadgeAvtarFrem.sprite = Defect;
                    lostsound.Play();
                }
            }
            else
            {
                photonView.RPC("RPC_GameOver", RpcTarget.Others, true); // Tell opponent they won
                winLosePanel.SetActive(true);
                winText.text = "YOU LOST!";
                VictoryBadgeAvtarFrem.sprite = Defect;
                lostsound.Play();
            }

        }
        else
        {
            photonView.RPC("RPC_GameOver", RpcTarget.Others, true); // Tell opponent they won
            winLosePanel.SetActive(true);
            winText.text = "YOU LOST!";
            VictoryBadgeAvtarFrem.sprite = Defect;
            lostsound.Play();
        }
    }

    // Mark opponent weapon positions and make them non-editable
    public void MarkOpponentWeaponPositions()
    {
        int count = 0;
        foreach (var row in gridManager.getInstance().opponentGridItems)
        {
            foreach (var box in row)
            {
                Debug.Log($"Box [{box.boxMatrix[0]},{box.boxMatrix[1]}] myChar: {box.myChar}, isMarked: {box.isMarked}");
                if (!box.isMarked && !string.IsNullOrEmpty(box.myChar))
                {
                    box.isMarked = true;
                    box.isItFixed = true;
                    if (box.myImage != null)
                    {
                        box.myImage.gameObject.SetActive(true);
                        box.myImage.color = Color.red;
                    }
                    count++;
                }
            }
        }
        Debug.Log($"Total Marked: {count}");
    }

    // Allow editing of marked opponent weapon positions after watching an ad
    public void AllowEditAfterAd()
    {
        foreach (var row in gridManager.getInstance().opponentGridItems)
        {
            foreach (var box in row)
            {
                if (box.isMarked)
                {
                    box.isItFixed = false;
                    if (box.myImage != null)
                        box.myImage.color = Color.white;
                }
            }
        }
    }

    // 1. Save opponent's weapon positions (call after opponent's positions are set)
    public void SaveOpponentWeaponPositions()
    {
        Debug.Log("OPPONENT DATA SAVE: opponentFinalMap count: " + opponentFinalMap.Count);
        savedOpponentWeaponPositions.Clear();
        foreach (var unit in opponentFinalMap)
        {
            Debug.Log("unit count: " + unit.Count);
            foreach (var box in unit)
            {
                Debug.Log("box.myChar: " + box.myChar + " [" + box.boxMatrix[0] + "," + box.boxMatrix[1] + "]");
                if (!string.IsNullOrEmpty(box.myChar))
                {
                    Debug.Log("Enter to save in list");
                    savedOpponentWeaponPositions.Add(new Vector2Int(box.boxMatrix[0], box.boxMatrix[1]));
                }
            }
        }
    }

    [PunRPC]
    public void RPC_TriggerOpponentFinalChance()
    {
        Debug.Log(" RPC_TriggerOpponentFinalChance called ");
        if (!isFinalChancePhase)
        {
            Debug.Log(" condition is called ");
            isFinalChancePhase = true;
            StartFinalChancePhase();
        }
    }

    public void JoinOdinRoom(string roomName)
    {

        Debug.Log("Odin room is null, joining...");
        //odinHandler.JoinRoom(roomName);
        Debug.Log("Odin room join requested: " + roomName);
    }

    //public override void OnLeftRoom()
    //{
    //    // Existing logic if any...
    //    if (odinHandler != null && !string.IsNullOrEmpty(odinRoomName))
    //        odinHandler.LeaveRoom(odinRoomName);
    //}

    // Add this method to update the synchronized timer
    private void UpdateSynchronizedTimer()
    {
        if (countdownTimer != null && countdownTimer.timerText != null && PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("TurnStartTime"))
        {
            double turnStartTime = (double)PhotonNetwork.CurrentRoom.CustomProperties["TurnStartTime"];
            double elapsed = PhotonNetwork.Time - turnStartTime;
            double timeLeft = timerDuration - elapsed;
            if (timeLeft > 0)
            {
                countdownTimer.timerText.gameObject.SetActive(true);
                countdownTimer.timerText.text = $"{Mathf.CeilToInt((float)timeLeft)}";
            }
            else
            {
                countdownTimer.timerText.text = "0";
                // Optionally: handle turn end here
            }
        }
    }

    public void OnBackButtonClicked() {
        // ...existing code...
    }

    // --- GAME STATE SAVE/RESTORE FOR RECONNECTION ---
    [System.Serializable]
    public class GameStateData
    {
        public List<List<string>> playerBoard;
        public List<List<string>> opponentBoard;
        public int playerTries;
        public int opponentTries;
        public bool isMyTurn;
        // Add more fields as needed
    }

    // Save current game state to Photon room properties
    public void SaveStateToRoom()
    {
        var state = GetCurrentGameState();
        string json = JsonConvert.SerializeObject(state);
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["GameState"] = json;
        if (PhotonNetwork.CurrentRoom != null)
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    // Load game state from Photon room properties
    public void LoadStateFromRoom()
    {
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("GameState", out object stateJson))
        {
            var state = JsonConvert.DeserializeObject<GameStateData>(stateJson.ToString());
            ApplyGameState(state);
        }
    }

    // Get current game state from GameManager
    public GameStateData GetCurrentGameState()
    {
        var state = new GameStateData();
        // Player board
        state.playerBoard = new List<List<string>>();
        for (int i = 0; i < finalMap.Count; i++)
        {
            state.playerBoard.Add(new List<string>());
            for (int j = 0; j < finalMap[i].Count; j++)
            {
                state.playerBoard[i].Add(finalMap[i][j].myChar);
            }
        }
        // Opponent board
        state.opponentBoard = new List<List<string>>();
        for (int i = 0; i < opponentFinalMap.Count; i++)
        {
            state.opponentBoard.Add(new List<string>());
            for (int j = 0; j < opponentFinalMap[i].Count; j++)
            {
                state.opponentBoard[i].Add(opponentFinalMap[i][j].myChar);
            }
        }
        state.playerTries = playerTries;
        state.opponentTries = opponentTries;
        state.isMyTurn = isMyTurn;
        return state;
    }

    // Apply loaded state to GameManager
    public void ApplyGameState(GameStateData state)
    {
        // Player board
        for (int i = 0; i < state.playerBoard.Count; i++)
        {
            for (int j = 0; j < state.playerBoard[i].Count; j++)
            {
                finalMap[i][j].myChar = state.playerBoard[i][j];
            }
        }
        // Opponent board
        for (int i = 0; i < state.opponentBoard.Count; i++)
        {
            for (int j = 0; j < state.opponentBoard[i].Count; j++)
            {
                opponentFinalMap[i][j].myChar = state.opponentBoard[i][j];
            }
        }
        playerTries = state.playerTries;
        opponentTries = state.opponentTries;
        isMyTurn = state.isMyTurn;
        // Optionally update UI here
        // Load state after applying (for safety)
        LoadStateFromRoom();
    }

    // Restored: ClearAllPreviews method
    public void ClearAllPreviews()
    {
        // Clear highlights/previews from the player's grid
        if (gridManager.getInstance() != null && gridManager.getInstance().gridItems != null)
        {
            foreach (var row in gridManager.getInstance().gridItems)
            {
                foreach (var box in row)
                {
                    box.heighlightme(false); // Remove highlight
                    // If you have a preview sprite or state, reset it here as well
                    // box.ClearPreview(); // Uncomment if such a method exists
                }
            }
        }
        // Optionally, clear from opponent grid as well
        if (gridManager.getInstance() != null && gridManager.getInstance().opponentGridItems != null)
        {
            foreach (var row in gridManager.getInstance().opponentGridItems)
            {
                foreach (var box in row)
                {
                    box.heighlightme(false);
                    // box.ClearPreview();
                }
            }
        }
    }

    // Restored: ClearCurrentSelection method
    public void ClearCurrentSelection()
    {
        // Deselect the currently selected box
        if (currentSelectedBox != null)
        {
            currentSelectedBox.heighlightme(false);
            // Remove preview text if any
            if (currentSelectedBox.myLabel != null)
                currentSelectedBox.myLabel.text = "";
            currentSelectedBox.myChar = "";
            currentSelectedBox = null;
        }

        // Reset the current string type
        curruntStringType = "";

        // Clear any highlights/previews
        ClearAllPreviews();

        // Reset the current weapon button
        if (curruntBtn != null)
        {
            curruntBtn.myImage.color = Color.white;
            curruntBtn.myImage.gameObject.GetComponent<Toggle>().interactable = true;
            curruntBtn = null;
        }

        // Clear the current height infos
        CurruntHeightInfos.Clear();
    }

    // // Photon callback: Save state when a player leaves
    // public override void OnPlayerLeftRoom(Player otherPlayer)
    // {
    //     SaveStateToRoom();
    // }

    // // Photon callback: Load state when rejoining
    // public override void OnJoinedRoom()
    // {
    //     LoadStateFromRoom();
    // }

public void OnHintButtonClicked()
{
    if (!isMyTurn) return; // Only allow on player's turn
    StartCoroutine(ShowHintCoroutine());
}

private IEnumerator ShowHintCoroutine()
{
    // 1. Sahi line (weapon) select karo (opponentFinalMap)
    if (opponentFinalMap.Count == 0) yield break;
    var correctLine = opponentFinalMap[Random.Range(0, opponentFinalMap.Count)];
    if (correctLine.Count == 0) yield break;

    // Us line me se ek random box lo
    int correctBoxIndex = Random.Range(0, correctLine.Count);
    var correctBox = correctLine[correctBoxIndex];

    // Save previous label (if any)
    string prevLabel = correctBox.myLabel != null ? correctBox.myLabel.text : "";

    // Highlight only (no alphabet change)
    correctBox.heighlightme(true);
    //if (correctBox.myLabel != null)
    //    correctBox.myLabel.text = randomChar.ToString();

    // 2. Random grid cell select karo (opponentGridItems me se, lekin correctBox se alag)
    var grid = gridManager.getInstance().opponentGridItems;
    int randI, randJ;
    Box randomBox;
    do
    {
        randI = Random.Range(0, grid.Count);
        randJ = Random.Range(0, grid[randI].Count);
        randomBox = grid[randI][randJ];
    } while (randomBox == correctBox);

    randomBox.heighlightme(true);

    // 3. 4 second wait karo
    yield return new WaitForSeconds(4f);

    // 4. Highlights & label hatao
    correctBox.heighlightme(false);
    if (correctBox.myLabel != null)
        correctBox.myLabel.text = prevLabel;
    randomBox.heighlightme(false);
}

}