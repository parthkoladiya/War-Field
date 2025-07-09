using Newtonsoft.Json;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Box : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerEnterHandler
{
    public List<int> boxMatrix = new List<int>();
    public string myChar = "";
    public Text myLabel;
    public bool isItFixed = false;
    public Image myImage;
    public bool isScrached = false;
    public GameObject myFrem;
    public bool isMarked = false;
    // public Image blueBackgroundImage;


    public void OnPointerDown(PointerEventData eventData)
    {
        GameManager.getInstance().isCliked = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GameManager.getInstance().isCliked = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.getInstance().isCliked)
        {
            onClik();
        }
        Debug.Log("Pointer Entered: " + gameObject.name);
    }

    public void heighlightme(bool CanI)
    {
        myFrem.SetActive(CanI);
    }

    public void onClik()
    {
        if (GameManager.getInstance().curruntStringType.Length == 0)
        {
            return;
        }

        bool isContains = false;
        for (int i = 0; i < GameManager.getInstance().CurruntHeightInfos.Count; i++)
        {
            List<Box> singleInfo = GameManager.getInstance().CurruntHeightInfos[i].heighlitedAreas;
            for (int j = 0; j < singleInfo.Count; j++)
            {
                if (singleInfo[j].boxMatrix == boxMatrix)
                {
                    isContains = true;
                }
            }
        }

        if (!isContains && GameManager.getInstance().currentSelectedBox != null)
        {
            if (!GameManager.getInstance().currentSelectedBox.isItFixed)
            {
                GameManager.getInstance().currentSelectedBox.myChar = "";
                GameManager.getInstance().currentSelectedBox.myLabel.text = "";
                GameManager.getInstance().currentSelectedBox = null;
                // Also clear this box's preview if invalid
                this.myChar = "";
                if (this.myLabel != null) this.myLabel.text = "";
            }
            GameManager.getInstance().heighLigteIdentify(this);
            return;
        }

        if (GameManager.getInstance().currentSelectedBox != null && GameManager.getInstance().currentSelectedBox != this && GameManager.getInstance().isCliked == false)
        {
            GameManager.getInstance().placeAstring(GameManager.getInstance().curruntStringType, this);
        }
        else
        {
            if (GameManager.getInstance().currentSelectedBox != null && !GameManager.getInstance().currentSelectedBox.isItFixed)
            {
                GameManager.getInstance().currentSelectedBox.myChar = "";
                GameManager.getInstance().currentSelectedBox.myLabel.text = "";
                GameManager.getInstance().currentSelectedBox = null;
            }
            GameManager.getInstance().heighLigteIdentify(this);
        }

    }

    public void Boom(bool isFromOpponent = false)
    {
        if (!isFromOpponent)
        {
            if (GameManager.getInstance().BoomBlast != null)
            {
                GameManager.getInstance().BoomBlast.Play();
            }
        }
        
        if (isScrached || (!isFromOpponent && !GameManager.getInstance().isMyTurn)) { return; }

        isScrached = true;

        // Show blue background image when damaged
        //if (blueBackgroundImage != null)
        //{
        //    blueBackgroundImage.gameObject.SetActive(true);
        //}

        // --- Weapon hit tracking and log image update ---
        WeaponLogImage weaponLog = GetComponent<WeaponLogImage>();
        if (weaponLog != null && !string.IsNullOrEmpty(weaponLog.weaponType))
        {
            // Unique key for each weapon (type + index)
            string weaponKey = weaponLog.weaponType + weaponLog.weaponIndex;
            int totalAlphabets = 1;
            if (weaponLog.weaponType.ToUpper() == "TANK") totalAlphabets = 2;
            if (weaponLog.weaponType.ToUpper() == "AC") totalAlphabets = 1;
            if (weaponLog.weaponType.ToUpper() == "STR") totalAlphabets = 1;

            // Update hit count
            if (!GameManager.getInstance().weaponHitCounts.ContainsKey(weaponKey))
                GameManager.getInstance().weaponHitCounts[weaponKey] = 0;
            GameManager.getInstance().weaponHitCounts[weaponKey]++;

            int hitCount = GameManager.getInstance().weaponHitCounts[weaponKey];

            // Log to console
            Debug.Log($"Weapon hit: {weaponLog.weaponType} {weaponLog.weaponIndex + 1} | Hit count: {hitCount}/{totalAlphabets}");

            // Update all WeaponLogImage with same type and index
            foreach (var logImg in GameObject.FindObjectsOfType<WeaponLogImage>())
            {
                if (logImg.weaponType == weaponLog.weaponType && logImg.weaponIndex == weaponLog.weaponIndex)
                {
                    logImg.SetState(hitCount, totalAlphabets);
                }
            }
        }
        // --- End weapon log update ---

        if (myChar.Length == 0)
        {
            gameObject.GetComponent<Image>().sprite = GameManager.getInstance().blastTile;
            myImage.sprite = gridManager.getInstance().boom1;
        }
        else if (!isFromOpponent)
        {
            gameObject.GetComponent<Image>().sprite = GameManager.getInstance().blastTile;
            myImage.sprite = gridManager.getInstance().boom1;
            
        }
        else
        {
            gameObject.GetComponent<Image>().sprite = gridManager.getInstance().boom3;
        }

        if (!isFromOpponent)
        {
            GameManager.getInstance().photonView.RPC("RPC_ReceiveAttack", RpcTarget.Others, boxMatrix[0], boxMatrix[1]);
            GameManager.getInstance().curruntHistoryList.Add(boxMatrix);
            if (GameManager.getInstance().curruntAttack <= GameManager.getInstance().attckSize && GameManager.getInstance().isMyTurn)
            {
                GameManager.getInstance().curruntAttack++;
                if (GameManager.getInstance().curruntAttack == (GameManager.getInstance().attckSize))
                {
                    if (GameManager.getInstance().mYCoroutine != null)
                    {
                        StopCoroutine(GameManager.getInstance().mYCoroutine);
                    }
                    GameManager.getInstance().countdownTimer.timerText.text = "0";
                    GameManager.getInstance().isMyTurn = false;
                    GameManager.getInstance().YourCallText.text = "Opponent Call";
                    GameManager.getInstance().curruntAttack = 0;
                    StartCoroutine(GameManager.getInstance().UpdateUI());
                    GameManager.getInstance().setLog(GameManager.getInstance().playerLogTransform, JsonConvert.SerializeObject(GameManager.getInstance().curruntHistoryList), gridManager.getInstance().opponentGridItems, GameManager.getInstance().opponentFinalMap, GameManager.getInstance().opponentMarksForMap);
                    GameManager.getInstance().photonView.RPC("SetTurn", RpcTarget.Others, true, JsonConvert.SerializeObject(GameManager.getInstance().curruntHistoryList));
                    GameManager.getInstance().curruntHistoryList.Clear();
                }

                if (GameManager.getInstance().opponentFinalMap.All(el => el.All(el2 => el2.isScrached)))
                {
                    Debug.Log("Opponent Final Chance");
                    GameManager.getInstance().photonView.RPC("RPC_TriggerOpponentFinalChance", Photon.Pun.RpcTarget.Others);
                }

            }
        }
    }

}
