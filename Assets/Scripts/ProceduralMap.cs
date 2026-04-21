using UnityEngine;
using UnityEngine.UIElements;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMap : MonoBehaviour
{
    [SerializeField] int xSize = 20, zSize = 20;
    [Header("Falloff")]
    [Range(0f, 1f)][SerializeField] float falloffStart = 0.3f;
    [Range(0f, 1f)][SerializeField] float falloffEnd = 0.7f;


    [Header("Perlin Noise")]
    [Range(0.01f, 1f)][SerializeField] float noiseScale = 0.3f;
    [SerializeField] float heightMultiplier = 4f;
    [Range(1, 6)][SerializeField] int octaves = 3;
    [Range(0f, 1f)][SerializeField] float persistence = 0.5f;
    [Range(1f, 4f)][SerializeField] float lacunarity = 2f;

    [SerializeField] int seed = 0;
    Color[] colors; 
    public Gradient gradient;

    float minTerrainHeight;
    float maxTerrainHeight;

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    void OnEnable() => Regenerate();

    void OnValidate()
    {
        if (mesh == null) return;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () => { if (this) Regenerate(); };
#endif
    }

    void Regenerate()
    {
        if (!mesh)
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
        }

        float[,] falloff = FallOffGenerator.Generate(
            new Vector2Int(xSize + 1, zSize + 1), falloffStart, falloffEnd);

        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        maxTerrainHeight = vertices[0].y;
        minTerrainHeight = vertices[0].y;

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                vertices[i] = CalculatePos(x, z, falloff);
                if (vertices[i].y > maxTerrainHeight)
                    maxTerrainHeight = vertices[i].y;
                if (vertices[i].y < maxTerrainHeight)
                    minTerrainHeight = vertices[i].y;
            }

        }
        triangles = new int[xSize * zSize * 6];


        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z++)
        {
        
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris] = vert; 
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1; 
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1; 
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        colors = new Color[vertices.Length];

        for(int i = 0, z = 0; z<= zSize; z++)
        {
            for(int x = 0;x<= xSize; x++)
            {
                float height = Mathf.InverseLerp(minTerrainHeight,maxTerrainHeight, vertices[i].y);
                colors[i] = gradient.Evaluate(height);
                i++;
            }

        }


        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.colors = colors;
    }

    Vector3 CalculatePos(int x, int z, float[,] falloff)
    {
        System.Random rng = new System.Random(seed);
        float amplitude = 1f, frequency = 1f, y = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = rng.Next(-100000, 100000);
            float offsetZ = rng.Next(-100000, 100000);

            y += Mathf.PerlinNoise(x * noiseScale * frequency + offsetX,
                                   z * noiseScale * frequency + offsetZ) * amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return new Vector3(x, y * falloff[x, z] * heightMultiplier, z);
    }

    void OnDrawGizmos()
    {
        if (vertices == null) return;
        Gizmos.color = Color.yellow;
        foreach (var v in vertices)
            Gizmos.DrawSphere(transform.TransformPoint(v), 0.08f);
    }


}