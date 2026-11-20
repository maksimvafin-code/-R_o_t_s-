using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridGenerator : MonoBehaviour
{
    public int xSize = 20;         // Количество плиток по ширине
    public int zSize = 20;         // Количество плиток по длине
    public float spacing = 1f;     // Расстояние между центрами
    public float tileSize = 0.85f; // Размер самой плитки (зазор = 0.15)

    void Awake() // Awake срабатывает раньше Start, чтобы сетка успела создаться
    {
        GenerateTiles();
    }

    void GenerateTiles()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Separated Tiles";
        GetComponent<MeshFilter>().mesh = mesh;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Центрируем поле, чтобы оно было ровно по центру (0,0,0)
        float offsetX = (xSize - 1) * spacing / 2f;
        float offsetZ = (zSize - 1) * spacing / 2f;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                float cx = x * spacing - offsetX;
                float cz = z * spacing - offsetZ;
                float half = tileSize / 2f;

                int startIndex = vertices.Count;

                // 4 угла плитки
                vertices.Add(new Vector3(cx - half, 0, cz - half)); // 0
                vertices.Add(new Vector3(cx + half, 0, cz - half)); // 1
                vertices.Add(new Vector3(cx + half, 0, cz + half)); // 2
                vertices.Add(new Vector3(cx - half, 0, cz + half)); // 3

                // Треугольники (смотрят вверх)
                triangles.Add(startIndex + 0); triangles.Add(startIndex + 3); triangles.Add(startIndex + 1);
                triangles.Add(startIndex + 1); triangles.Add(startIndex + 3); triangles.Add(startIndex + 2);
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
    }
}