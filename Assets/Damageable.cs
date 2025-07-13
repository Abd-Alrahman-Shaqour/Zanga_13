using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    [Header("Main components")]
    [SerializeField] ThirdPersonController_RobotSphere robotSphere_controller;

    [Header("Main properties")]
    [SerializeField] int health = 1;
    [SerializeField] int shield;

    void Awake()
    {
         
    }

    public void Init()
    {
        shield = robotSphere_controller.has_Shield;
    }

    public void TakeDamage()
    {
        robotSphere_controller.TakeDamage();

        // if(shield > 0)
        // {
        //     shield--;
        //     return;
        // }

        // if(health > 0)
        // {
        //     health--;

        //     if(health <= 0)
        //     {
        //         // Death
        //     }
        // }
    }

    public void PlayerDied()
    {
        robotSphere_controller.canPlay = false;
    }
}
