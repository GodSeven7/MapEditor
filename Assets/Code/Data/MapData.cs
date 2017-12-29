using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct CellData
{
    public int x;
    public int y;
    public int elevation;
    public float waterLevel;
    public TerrainType terrainType;
}

[Serializable]
public class MapData : ScriptableObject
{
    public int width;
    public int height;
    public List<CellData> cellDatas;
}
