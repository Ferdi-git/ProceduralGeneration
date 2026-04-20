using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ProceduralMap : MonoBehaviour
{
    Mesh mesh;

    Vector3[] verticles;
    int[] triangles;

    [SerializeField] int xSize=20;
    [SerializeField] int zSize=20;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateMesh();

    }

    private void Update()
    {
        UpdateMesh();
    }

    private void CreateMesh()
    {
        verticles = new Vector3[(xSize + 1) * (zSize +1)];
        

        for (int i = 0, z = 0; z <= zSize ; z++)
        {

            for (int x = 0; x <= xSize ; x++)
            {
                float y = Mathf.PerlinNoise(x* 0.3f,z *0.3f) *2f;
                verticles[i] = new Vector3(x, y, z);
                i++;
            }
        }

        triangles = new int[xSize * zSize * 6];



        int vert = 0; 
        int tris = 0;


        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
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
    }


    private void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = verticles;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }


    private void OnDrawGizmos()
    {
        
        if(verticles == null) return;

        for(int i = 0; i < verticles.Length; i++)
        {
            Gizmos.DrawSphere(verticles[i], .1f);
        }


    }


}
