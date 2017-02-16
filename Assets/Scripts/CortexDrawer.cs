using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Runtime.InteropServices;
using System;
using System.IO;
using UnityEditor;

public class CortexDrawer : MonoBehaviour {

    public GameObject m_MeshPartPrefab;
    private List<GameObject> m_MeshModelParts;
    private const int MAX_VERTECIS_PER_MESH = 64998; // Multiple of 3 to keep triangles in order

    // Use this for initialization
    void Start () {

        m_MeshModelParts = new List<GameObject>();


        print("Initialising Flat manager...");
        bool InitSuccess = FLATData.InitFlat();
        if(!InitSuccess)
        {
            print("Flat Object failed to initialise!");
            return;
        } 

        /*
        print("Performing test query... \n");

        FLATData.FlatRes cortexData = FLATData.Query((float)-100, (float)-100, (float)-100, (float)100, (float)100, (float)100);
        print(cortexData.numcoords);

        DrawModel(cortexData);
        */

       
    }

    public void DrawNewQuery(float p0, float p1, float p2, float p3, float p4, float p5)
    {
        FLATData.FlatRes cortexData = FLATData.Query(p0, p1, p2, p3, p4, p5);
        print(cortexData.numcoords);

        DrawModel(cortexData);
    }

    private void DrawModel(FLATData.FlatRes cortexData)
    {
        // Destroy old model
        if(m_MeshModelParts.Count > 0)
        {
            foreach (GameObject mesh in m_MeshModelParts)
            {
                Destroy(mesh);
            }
            m_MeshModelParts.Clear();
        }
            
        print("Building Mesh...");

        // Put data into vector3 form
        List<Vector3> cortexVertices = new List<Vector3>();
        for (int i = 0; i < cortexData.numcoords; i += 3)
        {
            Vector3 v = transform.TransformPoint(new Vector3(cortexData.coords[i], cortexData.coords[i + 1], cortexData.coords[i + 2]));
            cortexVertices.Add(v);
        }

        int vertexCount = cortexVertices.Count;

        int numMeshesRequired = ((vertexCount / 3) / MAX_VERTECIS_PER_MESH) + 1;

        for (int i = 0; i < numMeshesRequired; i++)
        {

            Mesh mesh = new Mesh();
            List<Vector2> UVs = new List<Vector2>();
            List<int> triangles = new List<int>();
            int startInd = i * MAX_VERTECIS_PER_MESH;
            int endInd = Math.Min(startInd + MAX_VERTECIS_PER_MESH, vertexCount - 1);
            Vector3[] vertices = cortexVertices.GetRange(startInd, endInd - startInd).ToArray();
            mesh.vertices = vertices;

            for (int j = 0; j < endInd - startInd; j++)
            {
                UVs.Add(new Vector2(vertices[j].x, vertices[j].z));

                if (j < endInd - startInd - 2 && (j % 3 == 0))
                {
                    triangles.Add(j);
                    triangles.Add(j + 1);
                    triangles.Add(j + 2);
                }
            }

            mesh.triangles = triangles.ToArray();
            mesh.uv = UVs.ToArray();
            mesh.RecalculateNormals();

            GameObject meshPart = Instantiate(m_MeshPartPrefab);
            meshPart.transform.SetParent(transform);

            MeshFilter mf = meshPart.GetComponent<MeshFilter>();
            if (mf)
            {
                mf.mesh = mesh;
            }

            m_MeshModelParts.Add(meshPart);
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
