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

            HexCell[] results = hexGrid.transform.GetComponentsInChildren<HexCell>();
            if (results.Length > 0)
            {
                mapData.cellData = new CellData[results.Length];
                int index = 0;
                foreach (HexCell hexCell in results)
                {
                    CellData cellData = ScriptableObject.CreateInstance<CellData>();
                    cellData.x = hexCell.coordinates.X;
                    cellData.y = hexCell.coordinates.Z;
                    cellData.waterLevel = hexCell.WaterLevel;
                    cellData.elevation = hexCell.Elevation;

                    mapData.cellData[index] = cellData;
                    ++index;
                }
            }

            //save mapData
#if UNITY_EDITOR
            string path = EditorUtility.SaveFilePanel("Save Map Data", Application.dataPath, "", "asset");
            if (string.IsNullOrEmpty(path)) return;

            path = path.Replace(Application.dataPath, "Assets");
            
            AssetDatabase.CreateAsset(mapData, path);
#endif

            MapData a =AssetDatabase.LoadAssetAtPath<MapData>(path);
            foreach (CellData cell in a.cellData)
            {
                Debug.Log(string.Format("x = {0}, y = {1}, evevation = {2}, water = {3}", cell.x, cell.y, cell.elevation, cell.waterLevel));
            }

        }
    }
	
}
