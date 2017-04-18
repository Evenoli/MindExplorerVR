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
    public GameObject m_defaultMeshParts;
    public GameObject m_QueryBox;
    public Flash m_queryBoxFlash;
    public NewtonVR.NVRButton m_ResetButton;

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
    // Upper corner position of current query
    private Vector3 m_curUpperQueryCorner;
    // center position of current query
    private Vector3 m_QueryCenter;
    // Bool tracking if a query is currently in progress
    private bool m_queryInProgress;


    //Default model position and scale
    private Vector3 m_DefaultModelPos;
    private Vector3 m_DefaultModelScale;
    private Quaternion m_DefaultModelRotation;
    private float m_defaultNewQueryScale = 1000;


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

        m_queryInProgress = false;

        // Set default query center coords
        m_QueryCenter = new Vector3(875f, 435f, 875f);

        m_DefaultModelPos = transform.localPosition;
        m_DefaultModelScale = transform.localScale;
        m_DefaultModelRotation = transform.localRotation;
    }

    //Resets model to startup state
    public void Reset()
    {
        print("Reseting Model");
        transform.localPosition = m_DefaultModelPos;
        transform.localRotation = m_DefaultModelRotation;
        transform.localScale = m_DefaultModelScale;

        print("Destroying old mesh..");
        foreach (GameObject mesh in m_MeshModelParts)
        {
            Destroy(mesh);
        }
        m_MeshModelParts.Clear();

        int numChildren = m_defaultMeshParts.transform.childCount;

        for(int i = 0; i < numChildren; i++)
        {
            GameObject Prefabmesh = m_defaultMeshParts.transform.GetChild(i).gameObject;
            GameObject mesh = Instantiate(Prefabmesh);
            mesh.transform.SetParent(m_MeshParts.transform);
            mesh.transform.localScale = new Vector3(1, 1, 1);
            mesh.transform.localPosition = Vector3.zero;
            mesh.transform.localEulerAngles = Vector3.zero;
            m_MeshModelParts.Add(mesh);
        }

        // Set default query center coords
        m_QueryCenter = new Vector3(875f, 435f, 875f);
        m_MeshParts.transform.localPosition = -1 * m_QueryCenter;
        
    }

    public Vector3 GetQueryCenter()
    {
        return m_QueryCenter;
    }

    public void DrawNewQuery(float p0, float p1, float p2, float p3, float p4, float p5)
    {
        if (!m_queryInProgress)
        {
            print("performing Query...");
            m_queryBoxFlash.SetFlashActive(true);
            m_queryInProgress = true;
            m_curModelSize = new Vector3(Math.Abs(p3 - p0), Math.Abs(p4 - p1), Math.Abs(p5 - p2));
            m_curBottomQueryCorner = new Vector3(p0, p1, p2);
            m_curUpperQueryCorner = new Vector3(p3, p4, p5);
            print("Query lower: " + m_curBottomQueryCorner.ToString());
            print("Query Top: " + m_curUpperQueryCorner.ToString());
            m_QueryCenter = (m_curUpperQueryCorner + m_curBottomQueryCorner) / 2;
            m_queryThread = new Thread(() => QueryThread(p0, p1, p2, p3, p4, p5));
            m_queryThread.Start();
        }
        else
        {
            print("Query already in progress!!");
        }
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

        //Reset model position/rotation/scale
        transform.localPosition = m_DefaultModelPos;
        transform.localRotation = m_DefaultModelRotation;
        float newQueryBoxScale = m_QueryBox.transform.localScale.x; // x,y,z all same
        float newScale = m_defaultNewQueryScale / newQueryBoxScale;
        transform.localScale = newScale * m_DefaultModelScale;

        Vector3 min = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        Vector3 max = new Vector3(0, 0, 0);

        // Put data into vector3 form
        List<Vector3> cortexVertices = new List<Vector3>();
        for (int i = 0; i < cortexData.numcoords; i += 3)
        {
            Vector3 v = new Vector3(cortexData.coords[i], cortexData.coords[i + 1], cortexData.coords[i + 2]);
            min.x = Mathf.Min(v.x, min.x);
            min.y = Mathf.Min(v.y, min.y);
            min.z = Mathf.Min(v.z, min.z);

            max.x = Mathf.Max(v.x,  max.x);
            max.y = Mathf.Max(v.y, max.y);
            max.z = Mathf.Max(v.z, max.z);
            //Vector3 v = new Vector3(cortexData.coords[i], cortexData.coords[i + 1], cortexData.coords[i + 2]) * transform.localScale.x;
            cortexVertices.Add(v);
        }

        //print("Min: " + min.ToString());
        //print("Max: " + max.ToString());

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

        m_MeshParts.transform.localPosition = -1 * m_QueryCenter;
        m_QueryBox.transform.localPosition = Vector3.zero;
        ControlModeManager cmm = GetComponent<ControlModeManager>();
        if(cmm)
            cmm.SetControlMode(ControlModeManager.CONTROL_MODE.EXPLORE);

        m_queryInProgress = false;
        m_queryBoxFlash.SetFlashActive(false);
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
