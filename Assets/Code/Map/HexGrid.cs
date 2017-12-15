using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
    public HexCell childPrefab;
    public Text textPrefab;
    public int width = 10;
    public int height = 10;
    Dictionary<int, HexCell> childCells;
    Dictionary<int, Text> childTexts;
    Canvas gridCanvas;

    [HideInInspector]
    public HexCoordinates curTouch;

    [HideInInspector]
    public HexCell curHexCell;

    // Use this for initialization
    void Start()
    {
        if (childPrefab == null)
            return;

        gridCanvas = GetComponentInChildren<Canvas>();
        childCells = new Dictionary<int, HexCell>();
        childTexts = new Dictionary<int, Text>();

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                AddCell(i, j);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            TouchCell(hit.point);
        }
    }

    void TouchCell(Vector3 position)
    {
        curHexCell = null;
        position = transform.InverseTransformPoint(position);
        curTouch = HexCoordinates.FromPosition(position);

        int key = HexCoordinates.GetIndex(curTouch.X, curTouch.Z);
        HexCell hc;
        if(childCells.TryGetValue(key, out hc))
        {
            hc.ChangeColor();
            curHexCell = hc;
        }
    }

    public void AddNewCell(int i, int j)
    {
        HexCell hc = AddCell(i, j);
        if(hc)
        {
            SetCellNeighbor(hc, HexDirection.Right);
            SetCellNeighbor(hc, HexDirection.RightUp);
            SetCellNeighbor(hc, HexDirection.LeftUp);
        }
    }

    void SetCellNeighbor(HexCell hexCell, HexDirection direction)
    {
        HexCoordinates coordinates = hexCell.coordinates.GetNeighbor(direction);
        int key = HexCoordinates.GetIndex(coordinates.X, coordinates.Z);
        HexCell hc;
        if (childCells.TryGetValue(key, out hc))
        {
            hexCell.SetNeighbor(direction, hc);
        }
    }

    HexCell AddCell(int i, int j)
    {
        int key = HexCoordinates.GetIndex(i, j);
        if (childCells.ContainsKey(key))
        {
            Debug.LogError(string.Format("exist cell, i = {0}, j = {1}", i, j));
            return null;
        }

        Vector3 pos = new Vector3();
        pos.x = (j % 2) * HexCellConf.innerRadius + HexCellConf.innerRadius * 2 * i;
        pos.y = 0;
        pos.z = HexCellConf.outerRadius / 2 * 3 * j;

        HexCell hc = GameObject.Instantiate<HexCell>(childPrefab);
        hc.gameObject.transform.SetParent(this.transform);
        hc.gameObject.transform.localPosition = pos;
        hc.coordinates = HexCoordinates.FromOffsetCoordinates(i, j);
        hc.Number = key;
        childCells[key] = hc;

        SetCellNeighbor(hc, HexDirection.Left);
        SetCellNeighbor(hc, HexDirection.LeftDown);
        SetCellNeighbor(hc, HexDirection.RightDown);

        if (childTexts.ContainsKey(key)) return hc;

        Text text = GameObject.Instantiate<Text>(textPrefab);
        text.rectTransform.SetParent(gridCanvas.transform, false);
        text.rectTransform.anchoredPosition = new Vector2(pos.x, pos.z);
        text.text = string.Format("{0},{1}", i, j);
        childTexts[key] = text;

        return hc;
    }

    public void DelCell(int i, int j)
    {
        int key = HexCoordinates.GetIndex(i, j);
        HexCell hc;
        if (childCells.TryGetValue(key, out hc))
        {
            GameObject.Destroy(hc.gameObject);
            childCells.Remove(key);
        }
    }

    public void ChangeHeight(int i, int j, int height)
    {
        int key = HexCoordinates.GetIndex(i, j);
        HexCell hc;
        if (childCells.TryGetValue(key, out hc))
        {
            hc.Elevation = height;
        }

        Text text;
        if (childTexts.TryGetValue(key, out text))
        {
            Vector3 uiPosition = text.rectTransform.localPosition;
            uiPosition.z = height * -HexCellConf.elevationStep;
            text.rectTransform.localPosition = uiPosition;
        }
    }

    public HexCell GetCell(int index)
    {
        return childCells[index];
    }
}
