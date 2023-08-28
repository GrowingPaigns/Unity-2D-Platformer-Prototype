using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIState : MonoBehaviour
{
    [SerializeField] private Sprite fullStamina, emptyStamina;
    [SerializeField] private Sprite fullHeart, halfHeart, emptyHeart;
    private SpriteRenderer heartImage;
    private SpriteRenderer staminaImage;

    private void Awake()
    {
        staminaImage = GetComponent<SpriteRenderer>();
        heartImage = GetComponent<SpriteRenderer>();
    }

    public void SetStaminaImage(UIStaminaStatus status)
    {
        switch (status)
        {
            case UIStaminaStatus.Empty:
                staminaImage.sprite = emptyStamina;
                break;
            case UIStaminaStatus.Full:
                staminaImage.sprite = fullStamina;
                break;

        }
    }
   

    public void SetHeartImage(UIHeartStatus status)
    {
        switch (status)
        {
            case UIHeartStatus.Empty:
                heartImage.sprite = emptyHeart;
                break;
            case UIHeartStatus.Half:
                heartImage.sprite = halfHeart;
                break;
            case UIHeartStatus.Full:
                heartImage.sprite = fullHeart;
                break;
        }
    }
}

public enum UIHeartStatus
{
    Empty = 0,
    Half = 1,
    Full = 2
}


public enum UIStaminaStatus
{
    Empty = 0,
    Full = 1
}