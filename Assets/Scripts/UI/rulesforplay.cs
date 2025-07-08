using UnityEngine;
using TMPro;
using System.Collections;

public class rulesforplay : MonoBehaviour
{
    public TextMeshProUGUI rulesText;       
    public string fileName = "rules";       
    public float lineDelay = 0.2f;          

    void Start()
    {
        StartCoroutine(LoadRulesLineByLine());
    }

    IEnumerator LoadRulesLineByLine()
    {
        TextAsset ruleData = Resources.Load<TextAsset>(fileName);

        if (ruleData == null)
        {
            rulesText.text = "⚠️ Rules file not found!";
            yield break;
        }

        rulesText.text = ""; // Clear before loading

        string[] lines = ruleData.text.Split('\n');

        foreach (string line in lines)
        {
            rulesText.text += line + "\n";
            yield return new WaitForSeconds(lineDelay);
        }
    }
}
