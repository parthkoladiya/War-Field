using Newtonsoft.Json;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CountdownTimer : MonoBehaviour
{
    public Text timerText; // Assign in inspector (optional)
    public float countdownTime = 120f;
    public float worningTime = 45;

    public IEnumerator StartCountdown()
    {
        float remaining = countdownTime;

        while (remaining > 0)
        {
            Debug.Log(" timer work ");
            if (timerText != null)
                timerText.text = Mathf.CeilToInt(remaining).ToString();
            if (remaining == 16)
            {
                GameManager.getInstance().showWarningPopup(GameManager.getInstance().messageText, "take turn otherwise loos the game");
            }
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }

        if (timerText != null)
            timerText.text = "0";

        OnTimerEnd();
    }

    void OnTimerEnd()
    {
        var gm = GameManager.getInstance();
        // Only switch turn if it's my turn and not in placement/toss phase
        if (gm.isMyTurn && !gm.topPlacement.activeSelf && !gm.tossPanel.activeSelf)
        {
            gm.isMyTurn = false;
            Debug.Log("⏰ Timer completed! Switching turn.");
            gm.YourCallText.text = "Opponent Turn";
            gm.StartCoroutine(gm.UpdateUI());
            gm.photonView.RPC("SetTurn", Photon.Pun.RpcTarget.Others, true, JsonConvert.SerializeObject(gm.curruntHistoryList));
        }
        // else: do nothing (timer end during placement/toss)
    }
}
