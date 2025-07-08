using System.Collections.Generic;
using UnityEngine;
public class gridManager : MonoBehaviour
{
    public RectTransform gridParent;
    public int gridSize;
    public GameObject gridItem;
    public List<List<Box>> gridItems = new List<List<Box>>();
    public List<List<Box>> opponentGridItems = new List<List<Box>>();
    public RectTransform opponentParent;
    public Sprite[] soliderSprite;
    public Sprite boom1;
    //public Sprite boom2;
    public Sprite boom3;


    public static gridManager gridManagerObj;

    public static gridManager getInstance()
    {
        return gridManagerObj;
    }

    private void Awake()
    {
        gridManagerObj = this;
    }

    void Start()
    {
        //gridParent.sizeDelta = new Vector2(gridSize * gridItem.GetComponent<RectTransform>().sizeDelta.x, gridSize * gridItem.GetComponent<RectTransform>().sizeDelta.y); ;
        genrateGrid();
    }

    void genrateGrid()
    {
        for (int i = 0; i < gridSize; i++)
        {
            gridItems.Add(new List<Box>());
            opponentGridItems.Add(new List<Box>());

            List<Sprite> availableSprites = new List<Sprite>(soliderSprite);
            ShuffleList(availableSprites);

            for (int j = 0; j < gridSize; j++)
            {
                Box gridEl = Instantiate(gridItem, gridParent.transform).GetComponent<Box>();
                gridEl.myImage.gameObject.SetActive(false);
                gridEl.boxMatrix.Add(i);
                gridEl.boxMatrix.Add(j);
                gridItems[i].Add(gridEl);

                Box opponentGridEl = Instantiate(gridItem, opponentParent.transform).GetComponent<Box>();

                Sprite selectedSprite = null;
                foreach (var sprite in availableSprites)
                {
                    bool found = false;
                    for (int row = 0; row < i; row++)
                    {
                        if (opponentGridItems[row][j].myImage.sprite == sprite)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        selectedSprite = sprite;
                        break;
                    }
                }
                if (selectedSprite == null)
                    selectedSprite = availableSprites[0];

                opponentGridEl.myImage.sprite = selectedSprite;
                opponentGridEl.myImage.gameObject.SetActive(true);
                opponentGridEl.boxMatrix.Add(i);
                opponentGridEl.boxMatrix.Add(j);
                opponentGridItems[i].Add(opponentGridEl);

                availableSprites.Remove(selectedSprite);
            }
        }
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }

    public void RegenerateGrid()
    {
        // Destroy all children of gridParent
        foreach (Transform child in gridParent)
        {
            GameObject.Destroy(child.gameObject);
        }
        // Destroy all children of opponentParent
        foreach (Transform child in opponentParent)
        {
            GameObject.Destroy(child.gameObject);
        }
        // Clear the lists
        gridItems.Clear();
        opponentGridItems.Clear();
        // Regenerate the grid
        genrateGrid();
    }
}
