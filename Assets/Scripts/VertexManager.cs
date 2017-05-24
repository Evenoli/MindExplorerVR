using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script used to hold information about attached 
// mesh and the vertices it contains. Mostly for debugging

public class VertexManager : MonoBehaviour {
    public int m_MeshID;
    public List<int> m_VertexIDs;

    private Mesh m_mesh;

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if(!mf)
        {
            print("Missing mesh filter on cortex mesh part!!!");
            return;
        }
        m_mesh = mf.mesh;
    }
    
    public void SetMeshCols(List<Color> cols)
    {
        GetComponent<MeshFilter>().mesh.SetColors(cols);
    }
}
