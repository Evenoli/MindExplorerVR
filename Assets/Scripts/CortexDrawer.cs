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
    public GameObject m_MeshParts;
    public Vector3 m_ModelScale;

    private List<GameObject> m_MeshModelParts;
    private const int MAX_VERTECIS_PER_MESH = 64998; // Multiple of 3 to keep triangles in order
    // Thread used for performing queries
    private Thread m_queryThread;
    // True if query has returned, and mesh hasn't been rendered yet.
    private bool m_renderDue;
    //Results of last query
    private FLATData.FlatRes m_latestCortexData;
    //Scale of most recent Query
    private Vector3 m_curModelSize;
    // Lower corner position of current query
    private Vector3 m_curBottomQueryCorner;
    // center position of current query
    private Vector3 m_QueryCenter;

    //Default model position anmd scale
    private Vector3 m_DefaultModelPos = new Vector3(0, 4, 0);
    private Vector3 m_DefaultModelScale = new Vector3(0.006f, 0.006f, 0.006f);

    // Use this for initialization
    void Start () {

        // Init mesh model list and add pre-loaded mesh parts to it
        m_MeshModelParts = new List<GameObject>();
        for(int c = 0; c < m_MeshParts.transform.childCount; c++)
        {
            m_MeshModelParts.Add(m_MeshParts.transform.GetChild(c).gameObject);
        }


        print("Initialising Flat manager...");
        bool InitSuccess = FLATData.InitFlat();
        if(!InitSuccess)
        {
            print("Flat Object failed to initialise!");
            return;
        } 
       
    }

    public void DrawNewQuery(float p0, float p1, float p2, float p3, float p4, float p5)
    {
        print("performing Query...");
        m_curModelSize = new Vector3(Math.Abs(p3 - p0), Math.Abs(p4 - p1), Math.Abs(p5 - p2));
        m_curBottomQueryCorner = new Vector3(p0, p1, p2);
        m_QueryCenter = (new Vector3(p3, p4, p5) + m_curBottomQueryCorner) / 2;
        m_queryThread = new Thread(() => QueryThread(p0, p1, p2, p3, p4, p5));
        m_queryThread.Start();
    }

    private void QueryThread(float p0, float p1, float p2, float p3, float p4, float p5)
    {
        m_latestCortexData = FLATData.Query(p0, p1, p2, p3, p4, p5);
        m_renderDue = true;
    }

    private void DrawModel(FLATData.FlatRes cortexData)
    {
        // Destroy old model
        if(m_MeshModelParts.Count > 0)
        {
            print("Destroying old mesh..");
            foreach (GameObject mesh in m_MeshModelParts)
            {
                Destroy(mesh);
            }
            m_MeshModelParts.Clear();
        }

        print("Query done. Building Mesh...");

        // Put data into vector3 form
        List<Vector3> cortexVertices = new List<Vector3>();
        for (int i = 0; i < cortexData.numcoords; i += 3)
        {
            //Vector3 v = transform.TransformPoint(new Vector3(cortexData.coords[i], cortexData.coords[i + 1], cortexData.coords[i + 2]));
            Vector3 v = new Vector3(cortexData.coords[i], cortexData.coords[i + 1], cortexData.coords[i + 2]) - m_QueryCenter;
            cortexVertices.Add(v);
        }

        int vertexCount = cortexVertices.Count;

        if (vertexCount > 0)
        {

            int numMeshesRequired = (vertexCount / MAX_VERTECIS_PER_MESH) + 1;

            for (int i = 0; i < numMeshesRequired; i++)
            {

                Mesh mesh = new Mesh();
                List<Vector2> UVs = new List<Vector2>();
                List<int> triangles = new List<int>();
                int startInd = i * MAX_VERTECIS_PER_MESH;
                int endInd = Math.Min(startInd + MAX_VERTECIS_PER_MESH, vertexCount);
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
                meshPart.transform.SetParent(m_MeshParts.transform);

                MeshFilter mf = meshPart.GetComponent<MeshFilter>();
                if (mf)
                {
                    mf.mesh = mesh;
                }

                m_MeshModelParts.Add(meshPart);
                meshPart.transform.localPosition = new Vector3(0, 0, 0);
                meshPart.transform.localScale = new Vector3(1, 1, 1);
            }
        }

        //Reset model position/rotation/scale
        transform.localScale = m_DefaultModelScale;
        transform.localPosition = m_DefaultModelPos;
        transform.localEulerAngles = new Vector3(0, 0, 0);

        print("Done");
    }

    // Update is called once per frame
    void Update () {
        if (m_renderDue)
        {
            DrawModel(m_latestCortexData);
            m_renderDue = false;
        }
    }
}
