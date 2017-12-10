using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HexEdgeType
{
    Flat, Slope, Cliff
}

public static class HexCellConf
{

    public static Color color = Color.magenta;
    public const float outerRadius = 5f;
    public const float innerRadius = outerRadius * 0.866025404f;

    public const float solidFactor = 0.75f;
    public const float blendFactor = 1f - solidFactor;

    public const float elevationStep = 1f;

    public const int terracesPerSlope = 2;
    public const int terraceSteps = terracesPerSlope * 2 + 1;

    public static Vector3[] corners = {
        new Vector3(0f, 0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius)
        };

    public static Vector3 GetFirstCorner(HexDirection direction)
    {
        return corners[(int)direction];
    }

    public static Vector3 GetSecondCorner(HexDirection direction)
    {
        int index = (int)direction + 1;
        index = index == 6 ? 0 : index;
        return corners[index];
    }

    public static Vector3 GetFirstSolidCorner(HexDirection direction)
    {
        return corners[(int)direction] * solidFactor;
    }

    public static Vector3 GetSecondSolidCorner(HexDirection direction)
    {
        int index = (int)direction + 1;
        index = index == 6 ? 0 : index;
        return corners[index] * solidFactor;
    }

    public static Vector3 GetBridge(HexDirection direction)
    {
        int index = (int)direction + 1;
        index = index == 6 ? 0 : index;
        return (corners[(int)direction] + corners[index]) * blendFactor;
    }

    public static Color TerraceLerpColor(Color begin, Color end, int step, int stepCount)
    {
        if (0 == step) return begin;

        int allStep = stepCount * 2 + 1;
        if (allStep == step) return end;

        return begin + (end - begin) * step / allStep;
    }

    public static Vector3 TerraceLerp(Vector3 begin, Vector3 end, int step, int stepCount)
    {
        float x = HexCellConf.HorizontalTerraceLerp(begin.x, end.x, step, stepCount);
        float y = HexCellConf.VerticalTerraceLerp(begin.y, end.y, step, stepCount);
        float z = HexCellConf.HorizontalTerraceLerp(begin.z, end.z, step, stepCount);
        return new Vector3(x, y, z);
    }

    public static float HorizontalTerraceLerp(float begin, float end, int step, int stepCount)
    {
        if (0 == step) return begin;

        int allStep = stepCount * 2 + 1;
        if (allStep == step) return end;

        return begin + (end - begin) * step / allStep;
    }

    public static float VerticalTerraceLerp(float begin, float end, int step, int stepCount)
    {
        if (0 == step) return begin;

        int allStep = stepCount * 2 + 1;
        if (allStep == step) return end;

        int verticalStep = stepCount + 1;
        int tmpStep = (step % 2 == 1) ? (step / 2 + 1) : (step / 2);
        return begin + (end - begin) * tmpStep / verticalStep;
    }

    public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
    {
        if (elevation1 == elevation2)
        {
            return HexEdgeType.Flat;
        }
        int delta = elevation2 - elevation1;
        if (delta == 1 || delta == -1)
        {
            return HexEdgeType.Slope;
        }
        return HexEdgeType.Cliff;
    }
}

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    Mesh hexMesh;
    MeshCollider meshCollider;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Color> colors = new List<Color>();

    public Color defaultColor = Color.white;
    public Color touchedColor = Color.magenta;
    public Color color = Color.white;

    HexCell hexCell;

    void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        meshCollider = GetComponent<MeshCollider>();
        hexMesh.name = "Hex Mesh";
        hexCell = GetComponent<HexCell>();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Render();
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void Render()
    {
        hexMesh.Clear();
        vertices.Clear();
        triangles.Clear();
        colors.Clear();

        CaculateGrahpic();

        hexMesh.vertices = vertices.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.colors = colors.ToArray();
        hexMesh.RecalculateNormals();

        meshCollider.sharedMesh = hexMesh;
    }

    void CaculateGrahpic()
    {
        for(int i = 0; i < 6; i++)
        {
            Triangulate((HexDirection)i);
        }
    }

    void Triangulate(HexDirection direction)
    {
        Vector3 center = Vector3.zero + new Vector3(0, hexCell.Elevation, 0);

        //Triangle
        Vector3 v1, v2;
        Color c1;
        InnerTriangle(direction, center, out v1, out v2, out c1);

        if (direction == HexDirection.Left || direction == HexDirection.LeftDown || direction == HexDirection.RightDown)
        {
            //Quat
            QuatBridge(direction, center, v1, v2, c1);
        }
    }

    void InnerTriangle(HexDirection direction, Vector3 center, out Vector3 v1, out Vector3 v2, out Color c1)
    {
        v1 = center + HexCellConf.GetFirstSolidCorner(direction);
        v2 = center + HexCellConf.GetSecondSolidCorner(direction);
        AddTriangle(center, v1, v2);

        c1 = color;
        AddTriangleColor(c1);
    }

    void QuatBridge(HexDirection direction, Vector3 center, Vector3 v1, Vector3 v2, Color c1)
    {
        HexCell neigbor_middle = hexCell.GetNeighbor(direction);
        if (neigbor_middle != null)
        {
            Vector3 bridge = HexCellConf.GetBridge(direction);
            Vector3 v3 = v1 + bridge;
            Vector3 v4 = v2 + bridge;
            v3.y = v4.y = neigbor_middle.Elevation * HexCellConf.elevationStep;

            Color c2 = neigbor_middle.GetHexMesh().color;

            if (hexCell.GetEdgeType(direction) == HexEdgeType.Slope)
            {
                TriangulateEdgeTerracesQuat(v1, v2, c1, v3, v4, c2);
            }
            else
            {
                TriangulateEdgeQuat(v1, v2, c1, v3, v4, c2);
            }

            //Bridge
            ShareTriangle(direction, neigbor_middle, v2, v4, c1, c2);
        }
    }

    void ShareTriangle(HexDirection direction, HexCell neigbor_middle, Vector3 v2, Vector3 v4, Color c1, Color c2)
    {
        if (direction == HexDirection.Left) return;

        HexCell neigbor_next = hexCell.GetNeighbor(direction.Next());
        if (neigbor_next != null)
        {
            Vector3 v5 = v2 + HexCellConf.GetBridge(direction.Next());
            v5.y = neigbor_next.Elevation * HexCellConf.elevationStep;

            if (hexCell.GetEdgeType(direction) == HexEdgeType.Slope || hexCell.GetEdgeType(direction.Next()) == HexEdgeType.Slope || neigbor_next.GetEdgeType(direction.Next().Opposite().Next()) == HexEdgeType.Slope)
            {
                if ((neigbor_middle.Elevation > hexCell.Elevation && neigbor_middle.Elevation > neigbor_next.Elevation) || (neigbor_middle.Elevation < hexCell.Elevation && neigbor_middle.Elevation < neigbor_next.Elevation))
                {
                    TriangulateEdgeTerracesTriangle(v2, c1, hexCell, v5, neigbor_next.GetHexMesh().color, neigbor_next, v4, c2, neigbor_middle);
                }
                else if ((neigbor_next.Elevation > hexCell.Elevation && neigbor_next.Elevation > neigbor_middle.Elevation) || (neigbor_next.Elevation < hexCell.Elevation && neigbor_next.Elevation < neigbor_middle.Elevation))
                {
                    TriangulateEdgeTerracesTriangle(v4, c2, neigbor_middle, v2, c1, hexCell, v5, neigbor_next.GetHexMesh().color, neigbor_next);
                }
                else
                {
                    TriangulateEdgeTerracesTriangle(v5, neigbor_next.GetHexMesh().color, neigbor_next, v4, c2, neigbor_middle, v2, c1, hexCell);
                }
            }
            else
            {
                AddTriangle(v4, v5, v2);
                AddTriangleColor(c2, neigbor_next.GetHexMesh().color, c1);
            }
        }
    }

    void TriangulateCornerTerracesCliff(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        float b = 1f / (rightCell.Elevation - beginCell.Elevation);
        Vector3 boundary = Vector3.Lerp(begin, right, b);
        Color boundaryColor = Color.Lerp(beginCell.color, rightCell.color, b);

        Vector3 v2 = HexMetrics.TerraceLerp(begin, left, 1);
        Color c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);

        AddTriangle(begin, v2, boundary);
        AddTriangleColor(beginCell.color, c2, boundaryColor);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v2;
            Color c1 = c2;
            v2 = HexMetrics.TerraceLerp(begin, left, i);
            c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
            AddTriangle(v1, v2, boundary);
            AddTriangleColor(c1, c2, boundaryColor);
        }

        AddTriangle(v2, left, boundary);
        AddTriangleColor(c2, leftCell.color, boundaryColor);
    }

    void TriangulateEdgeTerracesTriangle(Vector3 beginLeft, Color beginLeftColor, HexCell leftCell, Vector3 beginRight, Color beginRightColor, HexCell rightCell, Vector3 end, Color endColor, HexCell endCell)
    {
        if (leftCell.Elevation != rightCell.Elevation && rightCell.Elevation != endCell.Elevation && leftCell.Elevation != endCell.Elevation)
        {
            if(endCell.Elevation > leftCell.Elevation)
            {
                if (HexCellConf.GetEdgeType(leftCell.Elevation, rightCell.Elevation) == HexEdgeType.Slope)
                {
                    Vector3 lastVertex = beginRight;
                    Color lastColor = beginRightColor;
                    for (int i = 1; i <= HexCellConf.terraceSteps; i++)
                    {
                        Vector3 v3 = HexCellConf.TerraceLerp(beginRight, beginLeft, i, HexCellConf.terracesPerSlope);
                        Color c2 = HexCellConf.TerraceLerpColor(beginRightColor, beginLeftColor, i, HexCellConf.terracesPerSlope);

                        AddTriangle(end, lastVertex, v3);
                        AddTriangleColor(endColor, lastColor, c2);

                        lastVertex = v3;
                        lastColor = c2;
                        Debug.Log("12321312");
                    }
                }
            }
            else
            {
                // end left right
                if (HexCellConf.GetEdgeType(leftCell.Elevation, rightCell.Elevation) == HexEdgeType.Slope)
                {
                    Vector3 lastVertex = beginRight;
                    Color lastColor = beginRightColor;
                    for (int i = 1; i <= HexCellConf.terraceSteps; i++)
                    {
                        Vector3 v3 = HexCellConf.TerraceLerp(beginRight, beginLeft, i, HexCellConf.terracesPerSlope);
                        Color c2 = HexCellConf.TerraceLerpColor(beginRightColor, beginLeftColor, i, HexCellConf.terracesPerSlope);

                        AddTriangle(end, lastVertex, v3);
                        AddTriangleColor(endColor, lastColor, c2);

                        lastVertex = v3;
                        lastColor = c2;
                    }
                }
            }
            return;
        }

        Vector3 lastLeft = beginLeft;
        Vector3 lastRight = beginRight;
        Color lastLeftColor = beginLeftColor;
        Color lastRightColor = beginRightColor;
        for (int i = 1; i <= HexCellConf.terraceSteps; i++)
        {
            Vector3 v3 = HexCellConf.TerraceLerp(beginLeft, end, i, HexCellConf.terracesPerSlope);
            Vector3 v4 = HexCellConf.TerraceLerp(beginRight, end, i, HexCellConf.terracesPerSlope);
            Color c2 = HexCellConf.TerraceLerpColor(beginLeftColor, endColor, i, HexCellConf.terracesPerSlope);
            Color c3 = HexCellConf.TerraceLerpColor(beginRightColor, endColor, i, HexCellConf.terracesPerSlope);
            if (i == HexCellConf.terraceSteps)
            {
                AddTriangle(lastRight, lastLeft, end);
                AddTriangleColor(lastRightColor, lastLeftColor,endColor);
            }
            else
            {
                AddQuad(lastLeft, lastRight, v3, v4);
                AddQuadColor(lastLeftColor, lastRightColor, c2, c3);
            }

            lastLeft = v3;
            lastRight = v4;
            lastLeftColor = c2;
            lastRightColor = c3;
        }
    }
    
    void TriangulateEdgeQuat(Vector3 beginLeft, Vector3 beginRight, Color beginColor, Vector3 endLeft, Vector3 endRight, Color endColor)
    {
        AddQuad(beginLeft, beginRight, endLeft, endRight);
        AddQuadColor(beginColor, endColor);
    }

    void TriangulateEdgeTerracesQuat(Vector3 beginLeft, Vector3 beginRight, Color beginColor, Vector3 endLeft, Vector3 endRight, Color endColor)
    {
        Vector3 lastLeft = beginLeft;
        Vector3 lastRight = beginRight;
        Color lastColor = color;
        for (int i = 1; i <= HexCellConf.terraceSteps; i++)
        {
            Vector3 v3 = HexCellConf.TerraceLerp(beginLeft, endLeft, i, HexCellConf.terracesPerSlope);
            Vector3 v4 = HexCellConf.TerraceLerp(beginRight, endRight, i, HexCellConf.terracesPerSlope);
            Color c2 = HexCellConf.TerraceLerpColor(beginColor, endColor, i, HexCellConf.terracesPerSlope);

            AddQuad(lastLeft, lastRight, v3, v4);
            AddQuadColor(lastColor, c2);

            lastLeft = v3;
            lastRight = v4;
            lastColor = c2;
        }
    }

    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    void AddTriangleColor(Color c1)
    {
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c1);
    }

    void AddTriangleColor(Color c1, Color c2, Color c3)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
    }

    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }

    void AddQuadColor(Color c1, Color c2)
    {
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c2);
    }

    void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void ChangeColor()
    {
        color = HexCellConf.color;
    }
}
