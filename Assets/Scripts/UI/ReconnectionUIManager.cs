using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class ReconnectionUIManager : MonoBehaviour
{
    public static ReconnectionUIManager Instance { get; private set; }

    public TextMeshProUGUI opponentStatusText;
    public TextMeshProUGUI reconnectionTimerText;
    public Image backgroundPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        gameObject.SetActive(false);
    }

    public void ShowReconnectionUI(string opponentName)
    {
        gameObject.SetActive(true);
        if (opponentStatusText != null)
            opponentStatusText.text = $"{opponentName} disconnected!";
        if (reconnectionTimerText != null)
            reconnectionTimerText.gameObject.SetActive(true);
    }

    public void UpdateTimer(float remainingTime)
    {
        if (reconnectionTimerText != null)
        {
            if (remainingTime >= 0)
            {
                reconnectionTimerText.text = $"Reconnecting in: {Mathf.CeilToInt(remainingTime)}s";
            }
            else
            {
                reconnectionTimerText.text = "Connection failed.";
                // photonView.RPC("RPC_GameOver", RpcTarget.Others, false);
                StartCoroutine(HideAfterDelay(0.5f));
            }
        }
    }
    
    public void ShowReconnected(string opponentName)
    {
        Debug.Log(" Reconnected  ");
        if (opponentStatusText != null)
            opponentStatusText.text = $"{opponentName} reconnected!";
        if (reconnectionTimerText != null)
            reconnectionTimerText.text = "";
        StartCoroutine(HideAfterDelay(2f));
    }

    public void HideUI()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideUI();
    }
}