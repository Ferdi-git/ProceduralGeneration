using UnityEngine;

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
    [Range(1f, 10f)][SerializeField] float heightMultiplier = 4f;
    [Range(1, 6)][SerializeField] int octaves = 3;
    [Range(0f, 1f)][SerializeField] float persistence = 0.5f;
    [Range(1f, 4f)][SerializeField] float lacunarity = 2f;

    [SerializeField] int seed = 0;


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
        for (int i = 0, z = 0; z <= zSize; z++)
            for (int x = 0; x <= xSize; x++, i++)
                vertices[i] = CalculatePos(x, z, falloff);

        triangles = new int[xSize * zSize * 6];
        for (int t = 0, v = 0, z = 0; z < zSize; z++, v++)
            for (int x = 0; x < xSize; x++, v++, t += 6)
            {
                triangles[t] = v; triangles[t + 1] = v + xSize + 1;
                triangles[t + 2] = v + 1; triangles[t + 3] = v + 1;
                triangles[t + 4] = v + xSize + 1; triangles[t + 5] = v + xSize + 2;
            }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
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