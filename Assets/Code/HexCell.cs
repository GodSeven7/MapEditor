using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HexDirection
{
    RightUp, Right, RightDown, LeftDown, Left, LeftUp
}

public static class HexDirectionExtensions
{
    public static HexDirection Opposite(this HexDirection direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }

    public static HexDirection Previous(this HexDirection direction)
    {
        return direction == HexDirection.RightUp ? HexDirection.LeftUp : (direction - 1);
    }

    public static HexDirection Next(this HexDirection direction)
    {
        return direction == HexDirection.LeftUp ? HexDirection.RightUp : (direction + 1);
    }
}

[System.Serializable]
public struct HexCoordinates
{
    public const int GridWidth = 3;
    public const int GridHeight = 3;
    public const int CellWidth = 10;
    public const int CellHeight = 10;

    public const int Coefficient = 1000;

    [SerializeField]
    private int _x, _y, _z;

    public int X
    {
        get
        {
            return _x;
        }
    }

    public int Y
    {
        get
        {
            return 0;
        }
    }

    public int Z
    {
        get
        {
            return _z;
        }
    }

    public HexCoordinates(int x, int z, int y = 0)
    {
        this._x = x;
        this._z = z;
        this._y = y;
    }

    public override string ToString()
    {
        return "(" +
            X.ToString() + ", " + Z.ToString() + ", " + Y.ToString() + ")";
    }

    public static HexCoordinates FromOffsetCoordinates(int x, int z)
    {
        return new HexCoordinates(x, z);
    }

    public static HexCoordinates FromPosition(Vector3 position)
    {
        float z = position.z / (HexCellConf.outerRadius * 3 / 2);
        
        int iZ = Mathf.RoundToInt(z);

        float x = (position.x - ((iZ % 2 == 1) ? HexCellConf.innerRadius : 0)) / (HexCellConf.innerRadius * 2);
        int iX = Mathf.RoundToInt(x);

        return new HexCoordinates(iX, iZ);
    }

    public static int GetIndex(int x, int z)
    {
        return z * Coefficient + x; 
    }
    
    public HexCoordinates GetNeighbor(HexDirection direction)
    {
        int x = X;
        int z = Z;
        switch(direction)
        {
            case HexDirection.Left:
                x = x - 1;
                break;
            case HexDirection.LeftDown:
                x = (z % 2 == 0) ? x - 1 : x;
                z = z - 1;
                break;
            case HexDirection.LeftUp:
                x = (z % 2 == 0) ? x - 1 : x;
                z = z + 1;
                break;
            case HexDirection.Right:
                x = x + 1;
                break;
            case HexDirection.RightDown:
                x = (z % 2 == 0) ? x : x + 1;
                z = z - 1;
                break;
            case HexDirection.RightUp:
                x = (z % 2 == 0) ? x : x + 1;
                z = z + 1;
                break;
            default:
                break;
        }
        return HexCoordinates.FromOffsetCoordinates(x, z);
    }
}

public class HexCell : MonoBehaviour {

    public HexCoordinates coordinates;
    HexMesh hexMesh;

    [SerializeField]
    HexCell[] neighbors = new HexCell[6];


    public int Number
    {
        get { return number; }
        set { number = value; }
    }

    int number;

    public int Elevation
    {
        get{ return elevation; }
        set{ elevation = value; }
    }

    int elevation;

    void Awake()
    {
        hexMesh = GetComponent<HexMesh>();
    }

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ChangeColor()
    {
        if(hexMesh)
        {
            hexMesh.ChangeColor();
        }
    }

    public HexMesh GetHexMesh()
    {
        return hexMesh;
    }

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public HexEdgeType GetEdgeType(HexDirection direction)
    {
        return HexCellConf.GetEdgeType(elevation, neighbors[(int)direction].elevation);
    }
}
