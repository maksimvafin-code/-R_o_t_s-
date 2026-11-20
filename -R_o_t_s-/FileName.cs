using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridGenerator : MonoBehaviour
{
    public int xSize = 20;       // Количество треугольников по ширине
    public int zSize = 20;       // Количество рядов
    public float spacing = 1f;   // Длина стороны треугольника

    void Start()
    {
        GenerateEquilateralGrid();
    }

    void GenerateEquilateralGrid()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Equilateral Triangle Grid";
        GetComponent<MeshFilter>().mesh = mesh;

        float height = spacing * Mathf.Sqrt(3f) / 2f;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        Dictionary<string, int> vertexDict = new Dictionary<string, int>();

        int GetVertexIndex(float x, float z)
        {
            string key = $"{x:F3}_{z:F3}";
            if (!vertexDict.ContainsKey(key))
            {
                vertexDict[key] = vertices.Count;
                vertices.Add(new Vector3(x, 0, z));
            }
            return vertexDict[key];
        }

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                float xOffset = (z % 2 == 1) ? spacing / 2f : 0f;
                float x0 = x * spacing + xOffset;
                float x1 = x0 + spacing;
                float z0 = z * height;
                float z1 = z0 + height;

                int v0 = GetVertexIndex(x0, z0);
                int v1 = GetVertexIndex(x1, z0);
                int v2 = GetVertexIndex(x0 + spacing / 2f, z1);

                triangles.Add(v0);
                triangles.Add(v1);
                triangles.Add(v2);

                if (x < xSize - 1 || z % 2 == 1)
                {
                    int v3 = GetVertexIndex(x1 + spacing / 2f, z1);
                    triangles.Add(v1);
                    triangles.Add(v3);
                    triangles.Add(v2);
                }
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();

        // Создаем базовый материал, чтобы сетку было видно
        GetComponent<MeshRenderer>().material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    }
}