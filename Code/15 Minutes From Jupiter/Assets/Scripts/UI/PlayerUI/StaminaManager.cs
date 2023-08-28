using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaManager : MonoBehaviour
{
    [SerializeField] private GameObject staminaPrefab;
    [SerializeField] private PlayerMovement playerStamina;
    private List<UIState> staminaCrystals = new List<UIState>();
    [SerializeField] private float xSpacing; // Adjust this value as needed for xSpacing between staminaCrystals
    [SerializeField] private float ySpacing; // Adjust this value as needed for spacing between staminaCrystals

    private void OnEnable()
    {
        PlayerMovement.OnPlayerDash += DrawStamina;
    }

    private void OnDisable()
    {
        PlayerMovement.OnPlayerDash -= DrawStamina;
    }

    private void Start()
    {
        DrawStamina();
    }

    public void DrawStamina()
    {
        ClearStamina();

        // determine total staminaCrystals to make based on max health
       
        int crystalsToMake = (int)playerStamina.maxStamina;

        for (int i = 0; i < crystalsToMake; i++)
        {
            CreateEmptyStamina();
        }

        for (int i = 0; i < staminaCrystals.Count; i++)
        {
            // determine heart status
            int staminaRemaining = (int)Mathf.Clamp(playerStamina.stamina - i, 0, 1);
            staminaCrystals[i].SetStaminaImage((UIStaminaStatus)staminaRemaining);
        }

    }

    public void CreateEmptyStamina()
    {
        GameObject newCrystal = Instantiate(staminaPrefab);
        newCrystal.transform.SetParent(gameObject.transform);
        int staminaIndex = staminaCrystals.Count;
       
        if (staminaIndex == 0)
        {
            newCrystal.transform.localPosition = Vector3.zero;
        }
        else // Adjust the position based on the previously spawned heart
        {
            Vector2 prevCrystalPos = staminaCrystals[staminaIndex - 1].transform.GetComponent<RectTransform>().anchoredPosition;

            Vector2 pos = Vector2.zero;

            if (staminaIndex % 2 == 1)
            {
                pos = new Vector2(prevCrystalPos.x + xSpacing, prevCrystalPos.y + ySpacing);
            }
            else
            {
                pos = new Vector2(prevCrystalPos.x + xSpacing, prevCrystalPos.y - ySpacing);
            }

            newCrystal.GetComponent<RectTransform>().anchoredPosition = pos;
        }

        newCrystal.transform.localScale = new Vector3(62, 62, 0);

        UIState newStaminaState = newCrystal.GetComponent<UIState>();
        newStaminaState.SetStaminaImage(UIStaminaStatus.Empty);
        staminaCrystals.Add(newStaminaState);
    }

    public void ClearStamina()
    {
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }
        staminaCrystals = new List<UIState>();
    }
}
