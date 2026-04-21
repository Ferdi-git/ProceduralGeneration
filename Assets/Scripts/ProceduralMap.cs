using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMap : MonoBehaviour
{
    [Header("Map Size")]
    [SerializeField] int xSize = 20;
    [SerializeField] int zSize = 20;

    [Header("Falloff")]
    [Range(0f, 1f)][SerializeField] float falloffStart = 0.3f;
    [Range(0f, 1f)][SerializeField] float falloffEnd = 0.7f;

    [Header("Height")]
    [SerializeField] float heightMultiplier = 4f;
    [SerializeField] AnimationCurve heightCurve;       

    [Header("Noise")]
    [Range(0.01f, 1f)][SerializeField] float noiseScale = 0.3f;
    [Range(1, 6)][SerializeField] int octaves = 3;
    [Range(0f, 1f)][SerializeField] float persistence = 0.5f;  
    [Range(1f, 4f)][SerializeField] float lacunarity = 2f;   
    [SerializeField] int seed = 0;

    [Header("Color")]
    [SerializeField] Gradient gradient; 

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    Color[] colors;

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
        if (!mesh) { mesh = new Mesh(); GetComponent<MeshFilter>().mesh = mesh; }

        float[,] falloff = FallOffGenerator.Generate(new Vector2Int(xSize + 1, zSize + 1), falloffStart, falloffEnd);

        BuildVertices(falloff);
        BuildTriangles();
        BuildColors();

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
    }

    void BuildVertices(float[,] falloff)
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z = 0; z <= zSize; z++)
            for (int x = 0; x <= xSize; x++, i++)
                vertices[i] = SampleVertex(x, z, falloff);
    }
    Vector3 SampleVertex(int x, int z, float[,] falloff)
    {
        System.Random rng = new System.Random(seed);

        float amplitude = 1f, frequency = 1f, height = 0f;
        float maxPossible = 0f, amp = 1f;

        for (int i = 0; i < octaves; i++) { maxPossible += amp; amp *= persistence; }

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = x * noiseScale * frequency + rng.Next(-100000, 100000);
            float sampleZ = z * noiseScale * frequency + rng.Next(-100000, 100000);
            height += Mathf.PerlinNoise(sampleX, sampleZ) * amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        float normalized = height / maxPossible;          
        height = heightCurve.Evaluate(normalized);       
        height = height * falloff[x, z] * heightMultiplier;
        return new Vector3(x, height, z);

    }

    void BuildTriangles()
    {
        triangles = new int[xSize * zSize * 6];
        int vert = 0, tris = 0;

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
                vert++; tris += 6;
            }
            vert++;
        }
    }

    void BuildColors()
    {
        float min = float.MaxValue, max = float.MinValue;
        foreach (var v in vertices)
        {
            if (v.y < min) min = v.y;
            if (v.y > max) max = v.y;
        }

        colors = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            float t = Mathf.InverseLerp(min, max, vertices[i].y);
            colors[i] = gradient.Evaluate(t);
        }
    }

    void OnDrawGizmos()
    {
        if (vertices == null) return;
        Gizmos.color = Color.yellow;
        foreach (var v in vertices)
            Gizmos.DrawSphere(transform.TransformPoint(v), 0.08f);
    }
}