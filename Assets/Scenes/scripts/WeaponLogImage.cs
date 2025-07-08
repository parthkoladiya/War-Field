using UnityEngine;
using UnityEngine.UI;

public class WeaponLogImage : MonoBehaviour
{
    public Sprite normalSprite;
    public Sprite damagedSprite;
    public Sprite destroyedSprite;
    public string weaponType; // e.g. "Tank", "AC", "Str"
    public int weaponIndex;   // 0 for Tank1, 1 for Tank2, etc.

    public Image img;
    public GameObject blueBackground; // Assign in Inspector (should be behind weapon image)

    private void Awake()
    {
        img = GetComponent<Image>();
        if (img == null)
        {
            Debug.LogError("WeaponLogImage: No Image component found!");
        }
    }

    void SetBlueImageActive(bool active)
    {
        if (blueBackground != null)
        {
            blueBackground.SetActive(true   );
            var imgComp = blueBackground.GetComponent<Image>();
            if (imgComp != null)
                imgComp.enabled = active;
        }
    }

    /// <summary>
    /// Set the weapon image state based on hit count.
    /// </summary>
    /// <param name="hitCount">How many alphabets of this weapon are hit</param>
    /// <param name="totalAlphabets">Total alphabets in this weapon</param>
    public void SetState(int hitCount, int totalAlphabets)
    {
        if (img == null) {
            Debug.LogError("WeaponLogImage: Image component missing!");
            return;
        }

        Debug.Log($"SetState called: hitCount={hitCount}, totalAlphabets={totalAlphabets}, weaponType={weaponType}, weaponIndex={weaponIndex}");

        if (hitCount == 0)
        {
            img.sprite = normalSprite;
            Debug.Log("Set to normalSprite");
            SetBlueImageActive(false);
        }
        else if (hitCount < totalAlphabets)
        {
            img.sprite = damagedSprite;
            Debug.Log("Set to damagedSprite");
            SetBlueImageActive(true);
        }
        else // hitCount == totalAlphabets
        {
            img.sprite = destroyedSprite;
            Debug.Log("Set to destroyedSprite");
            SetBlueImageActive(true);
        }
    }
}