using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Runtime.InteropServices;
using System;
using System.IO;
using UnityEditor;

public class CortexDrawer : MonoBehaviour {


    private FLATData.FlatRes m_cortexData;
    private List<Vector3> m_cortexVertices;

    // Use this for initialization
    void Start () {

        print("Performing test query: \n");

        m_cortexData = FLATData.Query((float)-100, (float)-100, (float)-100, (float)100, (float)100, (float)100);
        print(m_cortexData.numcoords);

        print("Building Mesh...");
        m_cortexVertices = new List<Vector3>();
        for (int i = 0; i < m_cortexData.numcoords; i += 3)
        {
            Vector3 v = new Vector3(m_cortexData.coords[i], m_cortexData.coords[i + 1], m_cortexData.coords[i + 2]);
            m_cortexVertices.Add(v);
        }

        int vertexCount = m_cortexVertices.Count;

        

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
