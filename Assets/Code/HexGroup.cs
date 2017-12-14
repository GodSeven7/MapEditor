using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGroup : MonoBehaviour
{
    public HexGrid[] childPrefab;
    public int width = 2;
    public int height = 2;

    // Use this for initialization
    void Start ()
    {
        float gridWidth = 2 * HexCellConf.innerRadius * 10;
        float gridHeight = (1 - HexCellConf.blendFactor) * HexCellConf.outerRadius * 2 * 10;
        int count = childPrefab.Length;
        for(int i = 0; i < count; i++)
        {
            HexGrid hexGrid = childPrefab[i];
            if(hexGrid)
            {
                hexGrid.Init(i);
                hexGrid.transform.localPosition = new Vector3((i % width) * gridWidth, hexGrid.transform.localPosition.y, i / height * gridHeight);
                ConnectGridEdge(i, hexGrid);
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void ConnectGridEdge(int gridIndex, HexGrid hexGrid)
    {
        //left
        int left = gridIndex - 1;
        if(left > 0)
        {
            HexGrid leftGrid = childPrefab[left];

        }
        //down

    }

    void ConnectCellEdge()
    {

    }
}
