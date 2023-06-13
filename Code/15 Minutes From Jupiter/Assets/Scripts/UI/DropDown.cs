using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropDown : MonoBehaviour
{
    public GameObject Panel;

    public void OpenPanel()
    {
        if (Panel != null && !Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.KeypadEnter))
        {
            Animator animation = Panel.GetComponent<Animator>();

            if (animation != null)
            {
                bool isOpen = animation.GetBool("Open");
                animation.SetBool("Open", !isOpen);
            }
        }
    }
}
