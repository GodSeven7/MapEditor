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
            return _y;
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

    public GameObject hexMeshGO;
    HexMesh hexMesh;

    public GameObject hexWaterGO;
    HexWater hexWater;

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
        set
        {
            elevation = value;
            SetDirty();
        }
    }

    int elevation;

    public float WaterLevel
    {
        get { return waterLevel; }
        set {
            waterLevel = value;
            SetDirty();
        }
    }

    float waterLevel = 0;

    public Color color = Color.white;

    public TerrainType terrainType = TerrainType.Grass;

    void Awake()
    {
        hexMesh = hexMeshGO.GetComponent<HexMesh>();

        hexWater = hexWaterGO.GetComponent<HexWater>();
    }

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ChangeTerrainType()
    {
        if(hexMesh)
        {
            //Elevation = HexCellConf.height;
            terrainType = HexCellConf.terrainType;
            SetDirty();
        }
    }

    public HexMesh GetHexMesh()
    {
        return hexMesh;
    }

    public HexWater GetWaterMesh()
    {
        return hexWater;
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

    public bool IsUnderWater()
    {
        if(waterLevel > elevation)
        {
            return Mathf.Abs(waterLevel - elevation) > 0.01f;
        }
        return false;
    }

    void SetDirty()
    {
        hexMesh.SetDirty();
        hexWater.SetDirty();

        HexDirection direction = HexDirection.Left;
        for (int i = 0; i < 6; i++)
        {
            HexCell hc = GetNeighbor(direction);
            if (hc)
            {
                hc.GetHexMesh().SetDirty();
                hc.GetWaterMesh().SetDirty();
            }
            direction = direction.Next();
        }
    }
}
