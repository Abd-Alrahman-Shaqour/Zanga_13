using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotSFX : MonoBehaviour
{
    public void Play_Step()
    {
        AudioManager.Instance?.PlayWalk();
    }
}
