using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapEditor_LeftPanel : MonoBehaviour {
    public HexGrid hexGrid;
    public InputField xInput;
    public InputField yInput;
    public InputField heightInput;
    public InputField waterInput;

    void Awake()
    {
        if (hexGrid == null)
        {
            Debug.LogError("please choose xInput and yInput");
            enabled = false;
        }
    }

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnAddButtonClick()
    {
        if(xInput && yInput && !string.IsNullOrEmpty(xInput.text) && !string.IsNullOrEmpty(yInput.text))
        {            
            int x = int.Parse(xInput.text);
            int y = int.Parse(yInput.text);
            hexGrid.AddNewCell(x, y);
        }
        else
        {
            Debug.LogError("please input xInput and yInput");
        }
    }

    public void OnDelButtonClick()
    {
        hexGrid.DelCell(hexGrid.curTouch.X, hexGrid.curTouch.Z);
    }

    public void OnSaveButtonClick()
    {
        SaveMapData.SaveCurrentMapData();
    }

    public void OnChangeHeightClick()
    {
        if (xInput && !string.IsNullOrEmpty(heightInput.text))
        {
            int height = int.Parse(heightInput.text);
            HexCellConf.height = height;
            hexGrid.ChangeHeight(hexGrid.curTouch.X, hexGrid.curTouch.Z, height);
        }
        else
        {
            Debug.LogError("please input heightInput");
        }
    }

    public void OnWaterLevelClick()
    {
        if (xInput && !string.IsNullOrEmpty(waterInput.text))
        {
            float waterLevel = float.Parse(waterInput.text);
            hexGrid.ChangeWaterLevel(hexGrid.curTouch.X, hexGrid.curTouch.Z, waterLevel);
        }
        else
        {
            Debug.LogError("please input heightInput");
        }
    }

    public void OnChangeTerrainType(int index)
    {
        HexCellConf.terrainType = (TerrainType)index;
    }

    public void OnChangeYellowColor()
    {
        HexCellConf.color = Color.yellow;
    }
    public void OnChangeRedColor()
    {
        HexCellConf.color = Color.red;
    }
    public void OnChangeGrayColor()
    {
        HexCellConf.color = Color.gray;
    }
    public void OnChangeBlueColor()
    {
        HexCellConf.color = Color.blue;
    }
}
