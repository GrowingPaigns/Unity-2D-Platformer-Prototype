using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartManager : MonoBehaviour
{
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private PlayerHealth playerHealth;
    private List<UIState> hearts = new List<UIState>();
    [SerializeField] private float xSpacing; // Adjust this value as needed for xSpacing between hearts
    [SerializeField] private float ySpacing; // Adjust this value as needed for spacing between hearts

    private void OnEnable()
    {
        PlayerHealth.OnPlayerDamage += DrawHearts;
    }

    private void OnDisable()
    {
        PlayerHealth.OnPlayerDamage -= DrawHearts;
    }

    private void Start()
    {
        DrawHearts();
    }

    public void DrawHearts()
    {
        ClearHearts();

        // determine total hearts to make based on max health
        float maxHealthRemainder = playerHealth.maxHealth % 2;
        int heartsToMake = (int)(playerHealth.maxHealth / 2 + maxHealthRemainder);

        for (int i = 0; i < heartsToMake; i++) 
        {
            CreateEmptyHeart();
        }

        for (int i = 0; i < hearts.Count; i++)
        {
            // determine heart status
            int heartStatusRemainder = (int)Mathf.Clamp(playerHealth.health - (i * 2), 0, 2);
            hearts[i].SetHeartImage((UIHeartStatus)heartStatusRemainder);
        }

    }

    public void CreateEmptyHeart()
    {
        GameObject newHeart = Instantiate(heartPrefab);
        newHeart.transform.SetParent(gameObject.transform);
        int heartIndex = hearts.Count;
        
        if (heartIndex == 0)
        {
            newHeart.transform.localPosition = Vector3.zero;
        }
        else // Adjust the position based on the previously spawned heart
        {
            Vector2 prevHeartPos = hearts[heartIndex - 1].transform.GetComponent<RectTransform>().anchoredPosition;
             
            Vector2 pos = Vector2.zero;

            if (heartIndex % 2 == 1)
            {
                pos = new Vector2(prevHeartPos.x + xSpacing, prevHeartPos.y + ySpacing);
            } 
            else
            {
                pos = new Vector2(prevHeartPos.x + xSpacing, prevHeartPos.y  - ySpacing);
            }

            newHeart.GetComponent<RectTransform>().anchoredPosition = pos;
        }

        newHeart.transform.localScale = new Vector3(62,62,0);

        UIState newHeartState = newHeart.GetComponent<UIState>();
        newHeartState.SetHeartImage(UIHeartStatus.Empty);
        hearts.Add(newHeartState);
    }

    public void ClearHearts()
    {
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }
        hearts = new List<UIState>();
    }
}
