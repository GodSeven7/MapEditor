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
public struct SimpleTransform
{
    public Vector3 position;
    public Vector3 scale;
    public Quaternion rotation;
}

[Serializable]
public struct MapObj
{
    public string prefabName;
    public SimpleTransform transform;
}

[Serializable]
public class MapData : ScriptableObject
{
    public int width;
    public int height;
    public List<CellData> cellDatas;
    public List<MapObj> mapObjs;
}
