using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SaveMapData
{
    public static void SaveCurrentMapData()
    {
        GameObject go = GameObject.Find("HexGrid");
        if (go)
        {
            HexGrid hexGrid = go.GetComponent<HexGrid>();
            MapData mapData = ScriptableObject.CreateInstance<MapData>();
            mapData.width = hexGrid.width;
            mapData.height = hexGrid.height;

            //analysis cell data
            HexCell[] results = hexGrid.transform.GetComponentsInChildren<HexCell>();
            if (results.Length > 0)
            {
                mapData.cellDatas = new List<CellData>(mapData.width* mapData.height);
                foreach (HexCell hexCell in results)
                {
                    CellData cellData = new CellData();
                    cellData.x = hexCell.coordinates.X;
                    cellData.y = hexCell.coordinates.Z;
                    cellData.waterLevel = hexCell.WaterLevel;
                    cellData.elevation = hexCell.Elevation;
                    cellData.terrainType = hexCell.terrainType;

                    mapData.cellDatas.Add(cellData);
                }
            }

            //analysis environment
            Transform env = go.transform.Find("Environment");
            int count = env.childCount;
            if (count > 0)
            {
                mapData.mapObjs = new List<MapObj>(count);
                for (int i = 0; i < count; i++)
                {
                    Transform child = env.GetChild(i);
                    MapObj mapObj = new MapObj();
                    mapObj.prefabName = "Prefabs/Tree01";
                    SimpleTransform simpleTransform = new SimpleTransform();
                    simpleTransform.position = child.localPosition;
                    simpleTransform.scale = child.localScale;
                    simpleTransform.rotation = child.localRotation;
                    mapObj.transform = simpleTransform;

                    mapData.mapObjs.Add(mapObj);
                }
            }

            //save mapData
#if UNITY_EDITOR
            string path = EditorUtility.SaveFilePanel("Save Map Data", Application.dataPath, "", "asset");
            if (string.IsNullOrEmpty(path)) return;

            path = path.Replace(Application.dataPath, "Assets");
            
            AssetDatabase.CreateAsset(mapData, path);
#endif

        }
    }
	
}
