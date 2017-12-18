using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexWater : MonoBehaviour
{
    Mesh hexMesh;
    MeshCollider meshCollider;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Color> colors = new List<Color>();

    HexCell hexCell;
    [SerializeField]
    public bool isDirty;

    void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        meshCollider = GetComponent<MeshCollider>();
        hexMesh.name = "Hex Water";
        hexCell = this.transform.parent.GetComponent<HexCell>();
    }

    // Use this for initialization
    void Start()
    {
        SetDirty();
    }

    // Update is called once per frame
    void Update()
    {
        Render();
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void Render()
    {
        if(this.isDirty)
        {
            hexMesh.Clear();
            vertices.Clear();
            triangles.Clear();
            colors.Clear();

            if(hexCell.IsUnderWater())
            {
                CaculateGrahpic();
            }

            hexMesh.SetVertices(vertices);
            hexMesh.SetTriangles(triangles, 0);
            hexMesh.SetColors(colors);
            hexMesh.RecalculateNormals();

            meshCollider.sharedMesh = hexMesh;
        }
    }

    void CaculateGrahpic()
    {
        this.isDirty = false;
        for (int i = 0; i < 6; i++)
        {
            Triangulate((HexDirection)i);
        }
    }

    void Triangulate(HexDirection direction)
    {
        Vector3 center = Vector3.zero + new Vector3(0, hexCell.WaterLevel, 0);

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

        c1 = HexCellConf.color1;
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
            v3.y = v4.y = neigbor_middle.WaterLevel * HexCellConf.elevationStep;
            
            Color c2 = HexCellConf.color2;

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
            v5.y = neigbor_next.WaterLevel * HexCellConf.elevationStep;
            
            Color c3 = HexCellConf.color3;
            if (hexCell.GetEdgeType(direction) == HexEdgeType.Slope)
            {
                TriangulateCornerTerracesCliff(v5, c3, neigbor_next, v2, c1, hexCell, v4, c2, neigbor_middle);
            }
            else if(hexCell.GetEdgeType(direction.Next()) == HexEdgeType.Slope)
            {
                TriangulateCornerTerracesCliff(v4, c2, neigbor_middle, v5, c3, neigbor_next, v2, c1, hexCell);
            }
            else if(neigbor_next.GetEdgeType(direction.Previous()) == HexEdgeType.Slope)
            {
                TriangulateCornerTerracesCliff(v2, c1, hexCell, v4, c2, neigbor_middle, v5, c3, neigbor_next);
            }
            else
            {
                AddTriangle(v4, v5, v2);
                AddTriangleColor(c2, c3, c1);
            }
        }
    }

    void TriangulateCornerTerracesCliff(Vector3 first, Color firstColor, HexCell firstCell, Vector3 second, Color secondColor, HexCell secondCell, Vector3 third, Color thirdColor, HexCell thirdCell)
    {
        int second_first = Mathf.Abs(secondCell.Elevation - firstCell.Elevation);
        int third_first = Mathf.Abs(thirdCell.Elevation - firstCell.Elevation);

        if (firstCell.Elevation != secondCell.Elevation && secondCell.Elevation != thirdCell.Elevation && thirdCell.Elevation != firstCell.Elevation)
        {
            if (HexCellConf.GetEdgeType(firstCell.Elevation, secondCell.Elevation) == HexEdgeType.Slope || HexCellConf.GetEdgeType(firstCell.Elevation, thirdCell.Elevation) == HexEdgeType.Slope)
            {
                if (second_first > third_first)
                {
                    float b = (1f - 1f / second_first);
                    Vector3 boundary = Vector3.Lerp(first, second, b);
                    Color boundaryColor = Color.Lerp(firstColor, secondColor, b);

                    Vector3 lastPos = first;
                    Color lastColor = firstColor;
                    for (int i = 1; i <= HexCellConf.terraceSteps; i++)
                    {
                        Vector3 v3 = HexCellConf.TerraceLerp(first, third, i, HexCellConf.terracesPerSlope);
                        Color c3 = HexCellConf.TerraceLerpColor(firstColor, thirdColor, i, HexCellConf.terracesPerSlope);

                        AddTriangle(lastPos, boundary, v3);
                        AddTriangleColor(lastColor, boundaryColor, c3);

                        lastPos = v3;
                        lastColor = c3;
                    }

                    lastPos = third;
                    lastColor = thirdColor;
                    for (int i = 1; i <= HexCellConf.terraceSteps; i++)
                    {
                        Vector3 v3 = HexCellConf.TerraceLerp(third, second, i, HexCellConf.terracesPerSlope);
                        Color c3 = HexCellConf.TerraceLerpColor(thirdColor, secondColor, i, HexCellConf.terracesPerSlope);

                        AddTriangle(boundary, v3, lastPos);
                        AddTriangleColor(boundaryColor, c3, lastColor);

                        lastPos = v3;
                        lastColor = c3;
                    }
                }
                else
                {
                    float b = (1f - 1f / third_first);
                    Vector3 boundary = Vector3.Lerp(first, third, b);
                    Color boundaryColor = Color.Lerp(firstColor, thirdColor, b);

                    Vector3 lastPos = first;
                    Color lastColor = firstColor;
                    for (int i = 1; i <= HexCellConf.terraceSteps; i++)
                    {
                        Vector3 v3 = HexCellConf.TerraceLerp(first, second, i, HexCellConf.terracesPerSlope);
                        Color c3 = HexCellConf.TerraceLerpColor(firstColor, secondColor, i, HexCellConf.terracesPerSlope);

                        AddTriangle(boundary, lastPos, v3);
                        AddTriangleColor(boundaryColor, lastColor, c3);

                        lastPos = v3;
                        lastColor = c3;
                    }

                    lastPos = second;
                    lastColor = secondColor;
                    for (int i = 1; i <= HexCellConf.terraceSteps; i++)
                    {
                        Vector3 v3 = HexCellConf.TerraceLerp(second, third, i, HexCellConf.terracesPerSlope);
                        Color c3 = HexCellConf.TerraceLerpColor(secondColor, thirdColor, i, HexCellConf.terracesPerSlope);

                        AddTriangle(boundary, lastPos, v3);
                        AddTriangleColor(boundaryColor, lastColor, c3);

                        lastPos = v3;
                        lastColor = c3;
                    }
                }
            }
            else
            {
                if (second_first > third_first)
                {
                    float b = (1f - 1f / second_first);
                    Vector3 boundary = Vector3.Lerp(first, second, b);
                    Color boundaryColor = Color.Lerp(firstColor, secondColor, b);

                    AddTriangle(first, boundary, third);
                    AddTriangleColor(firstColor, boundaryColor, thirdColor);

                    Vector3 lastPos = third;
                    Color lastColor = thirdColor;
                    for (int i = 1; i <= HexCellConf.terraceSteps; i++)
                    {
                        Vector3 v3 = HexCellConf.TerraceLerp(third, second, i, HexCellConf.terracesPerSlope);
                        Color c3 = HexCellConf.TerraceLerpColor(thirdColor, secondColor, i, HexCellConf.terracesPerSlope);

                        AddTriangle(boundary, v3, lastPos);
                        AddTriangleColor(boundaryColor, c3, lastColor);

                        lastPos = v3;
                        lastColor = c3;
                    }
                }
                else
                {
                    float b = (1f - 1f / third_first);
                    Vector3 boundary = Vector3.Lerp(first, third, b);
                    Color boundaryColor = Color.Lerp(firstColor, thirdColor, b);

                    AddTriangle(first, second, boundary);
                    AddTriangleColor(firstColor, secondColor, boundaryColor);

                    Vector3 lastPos = second;
                    Color lastColor = secondColor;
                    for (int i = 1; i <= HexCellConf.terraceSteps; i++)
                    {
                        Vector3 v3 = HexCellConf.TerraceLerp(second, third, i, HexCellConf.terracesPerSlope);
                        Color c3 = HexCellConf.TerraceLerpColor(secondColor, thirdColor, i, HexCellConf.terracesPerSlope);

                        AddTriangle(boundary, lastPos, v3);
                        AddTriangleColor(boundaryColor, lastColor, c3);

                        lastPos = v3;
                        lastColor = c3;
                    }
                }
            }
        }
        else
        {
            if ((secondCell.Elevation > thirdCell.Elevation && secondCell.Elevation > firstCell.Elevation) || (secondCell.Elevation < thirdCell.Elevation && secondCell.Elevation < firstCell.Elevation))
            {
                TriangulateEdgeTerracesTriangle(third, thirdColor, thirdCell, first, firstColor, firstCell, second, secondColor, secondCell);
            }
            else if ((thirdCell.Elevation > secondCell.Elevation && thirdCell.Elevation > firstCell.Elevation) || (thirdCell.Elevation < secondCell.Elevation && thirdCell.Elevation < firstCell.Elevation))
            {
                TriangulateEdgeTerracesTriangle(first, firstColor, firstCell, second, secondColor, secondCell, third, thirdColor, thirdCell);
            }
            else
            {
                TriangulateEdgeTerracesTriangle(second, secondColor, secondCell, third, thirdColor, thirdCell, first, firstColor, firstCell);
            }            
        }
    }

    void TriangulateEdgeTerracesTriangle(Vector3 beginLeft, Color beginLeftColor, HexCell leftCell, Vector3 beginRight, Color beginRightColor, HexCell rightCell, Vector3 end, Color endColor, HexCell endCell)
    {
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
                AddTriangle(lastLeft, lastRight, end);
                AddTriangleColor(lastLeftColor, lastRightColor, endColor);
            }
            else
            {
                AddQuad(lastRight, lastLeft, v4, v3);
                AddQuadColor(lastRightColor, lastLeftColor, c3, c2);
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
        Color lastColor = beginColor;
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
    public void SetDirty()
    {
        this.isDirty = true;
    }
}
