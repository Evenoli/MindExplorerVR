using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class FullLineModelRenderer : MonoBehaviour {

    private bool m_messagingActive;

    public Color m_defaultVertexCol = new Color(0.0f, 0.0f, 1.0f, 1.0f);
    public Color m_activatedvertexCol = new Color(1.0f, 0.0f, 0.0f, 1.0f);

    private int m_numVerts;
    private int m_numMeshes;
    // List to track which vertices are contained in which mesh (meshVerts[meshInd][VertInd])
    private List<List<int>> m_meshVerts;
    // List to hold all vertices from file
    private List<Position> m_verts;
    // Lists to hold adjacency info from file
    private List<List<int>> m_adj;
    // List to hold state of each vertex for messaging
    private List<int> m_vertexState;
    // List of all mesh parts game objects
    private GameObject[] m_meshParts;

    // Load mesh/vertex data of line model
    void Start()
    {
        MeshInfo mi = MeshData.LoadMeshInfo();
        m_numVerts = mi.numVerts;
        m_numMeshes = mi.numMeshes;
        if(m_numVerts == -1)
        {
            print("Load error: No mesh Info found!");
            return;
        }
        m_meshVerts = mi.meshVerts;

        // Initialise all vertex states to -2 (unactivated)
        m_vertexState = new List<int>();
        for (int i = 0; i < m_numVerts; i++)
            m_vertexState.Add(-2);

        // Get each mesh part game object (should be children of this gameobject)
        // and add them to meshPart list accordingly based on meshID
        m_meshParts = new GameObject[m_numMeshes];
        foreach (VertexData vd in transform.GetComponentsInChildren<VertexData>())
        {
            int meshID = vd.m_MeshID;
            m_meshParts[meshID] = vd.gameObject;
        }

        print("Loaded data successfully!");
    }

    // Converts from vertex index in data file to vertex index used with meshes
    private int[] FileIndexToVertexIndex(int fileInd)
    {
        int[] res = new int[2];
        res[0] = fileInd * 2;
        res[1] = (fileInd * 2) + 1;
        return res;
    }
}
