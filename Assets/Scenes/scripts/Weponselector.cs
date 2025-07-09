using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Weponselector : MonoBehaviour
{
    public static Weponselector instance;

    public stringType myselection;
    private string myStringType;
    public string weaponType; // For GameManager filtering

    public Image myImage;
    public TextMeshProUGUI countText;
    public Sprite newSpriteWhenZero;
    public Sprite originalSprite; // Store the original sprite

    public int currentCount;

    private void Awake()
    {
        instance = this;

        // First, try to get count from countText
        if (countText != null)
        {
            string cleaned = countText.text.Replace("*", "");
            if (!int.TryParse(cleaned, out currentCount))
            {
                Debug.LogError("Failed to parse number from countText.text");
                currentCount = 0;
            }
        }
        else
        {
            Debug.LogError("countText is null! Please assign it in Inspector.");
            currentCount = 0;
        }

        // If countText is empty or not valid, assign count based on selection
        if (currentCount <= 0)
        {
            switch (myselection)
            {
                case stringType.Tank:
                    currentCount = 2;
                    if (currentCount == 0)
                    {
                        GameManager.Instance.curruntBtn.myImage.gameObject.GetComponent<Toggle>().interactable = false;
                    }
                    break;
                case stringType.AC:
                    currentCount = 3;
                    if (currentCount == 0)
                    {
                        GameManager.Instance.curruntBtn.myImage.gameObject.GetComponent<Toggle>().interactable = false;
                    }
                    break;
                case stringType.Str:
                    currentCount = 1;
                    if (currentCount == 0)
                    {
                        GameManager.Instance.curruntBtn.myImage.gameObject.GetComponent<Toggle>().interactable = false;
                    }
                    break;
                default:
                    currentCount = 0;
                    break;
            }
        }

        UpdateCountText();

        if (currentCount == 0 && newSpriteWhenZero != null)
        {
            myImage.sprite = newSpriteWhenZero;
            if (countText != null)
                countText.gameObject.SetActive(false);
        }

        // Set weaponType string for GameManager filtering
        switch (myselection)
        {
            case stringType.Tank:
                weaponType = "Tank";
                break;
            case stringType.AC:
                weaponType = "AC";
                break;
            case stringType.Str:
                weaponType = "Str";
                break;
            default:
                weaponType = "";
                break;
        }

        // Store the original sprite
        if (myImage != null)
            originalSprite = myImage.sprite;
    }

    public void btnCliked()
    {
        GameManager.getInstance().ClearAllPreviews();
        GameManager.getInstance().ClearCurrentSelection();
        if (currentCount <= 0)
        {
            Debug.Log("No uses left for this weapon.");
            return;
        }

        // Only set current selected string type and highlight, do NOT decrement count here
        switch (myselection)
        {
            case stringType.Tank:
                myStringType = "TANK";
                break;
            case stringType.AC:
                myStringType = "AC";
                break;
            case stringType.Str:
                myStringType = "STR";
                break;
            case stringType.None:
                myStringType = "";
                break;
        }

        GameManager.getInstance().curruntStringType = myStringType;

        // Reset all button colors
        foreach (var item in GameManager.getInstance().btn)
        {
            item.myImage.color = Color.white;
        }

        // Set this button's color to green
        myImage.color = Color.gray;

        GameManager.getInstance().curruntBtn = this;
    }

    public void UpdateCountText()
    {
        if (countText != null)
        {
            countText.text = "*" + currentCount;
            countText.gameObject.SetActive(currentCount > 0);
        }
    }

    // Additions for GameManager integration
    public int GetCurrentCount()
    {
        return currentCount;
    }

    public void OnAllUsed()
    {
        if (newSpriteWhenZero != null)
        {
            myImage.sprite = newSpriteWhenZero;
            myImage.gameObject.GetComponent<Toggle>().interactable = false;
        }
        if (countText != null)
            countText.gameObject.SetActive(false);
        else
            Debug.LogWarning("Toggle not found on: " + gameObject.name);
    }
}