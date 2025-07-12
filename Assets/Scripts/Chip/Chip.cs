
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using TMPro;

[CreateAssetMenu]
public class Chip : ScriptableObject
{
    public string partName;
    public string description;
    public Sprite icon;
    public ChipType chipType;
    public RarityType rarityType;
    public int value = 1;
}