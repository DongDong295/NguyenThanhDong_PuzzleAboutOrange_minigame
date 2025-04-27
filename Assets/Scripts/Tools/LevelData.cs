using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class LevelDataItem
{
    public int index;
    public int boardWidth;
    public int boardHeight;
    public int numberOfBlock;
    public float levelTime;
    public float oneStarTime;
    public float twoStarTime;
}

public class LevelData : SerializedScriptableObject
{
    public LevelDataItem[] LevelDataItems;
}
