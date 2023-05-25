using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
   
    private PlayerMovement playerMovement;

    /* -- Removed all player attack content -- 
     * Player Launch felt too slow to be in the game. While the movement of the dash-esk
     * ability was helpful in some cases, mostly it felt like the charging mechanic slowed 
     * the player down
     *
     * Repurposed this idea into a dash mechanic (Can be found in the movement script)
     * Feels a lot more responsive now with the immediate reaction on mouse release
     */

    void Start()
    {
       

    }

    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            
        }

        

        
    }



    
}