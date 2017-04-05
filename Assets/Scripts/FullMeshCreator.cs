﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FullMeshCreator : MonoBehaviour {

    public GameObject m_MeshPartPrefab;
    public GameObject m_FullModelPrefab;
    private const int MAX_VERTECIS_PER_MESH = 64998;

    private float minX;
    private float minZ;
    private float maxX;
    private float maxZ;

    // Use this for initialization
    void Start () {

        print("Building Prefab Model...");

        FLATData.InitFlat();
        FLATData.FlatRes cortexData = FLATData.Query(0f, 0f, 0f, 3000f, 870f, 1500f);
        //FLATData.FlatRes cortexData = FLATData.Query(0f, 0f, 0f, 300f, 100f, 200f);
        GameObject fullModel = new GameObject();

        minX = minZ = Mathf.Infinity;
        maxX = maxZ = 0;
         

        // Put data into vector3 form
        List<Vector3> cortexVertices = new List<Vector3>();
        for (int i = 0; i < cortexData.numcoords; i += 3)
        {
            //Vector3 v = transform.TransformPoint(new Vector3(cortexData.coords[i], cortexData.coords[i + 1], cortexData.coords[i + 2]));
            Vector3 v = new Vector3(cortexData.coords[i], cortexData.coords[i + 1], cortexData.coords[i + 2]);
            if (v.x < minX)
                minX = v.x;
            if (v.z < minZ)
                minZ = v.z;
            if (v.x > maxX)
                maxX = v.x;
            if (v.z > maxZ)
                maxZ = v.z;

            cortexVertices.Add(v);
        }

        int vertexCount = cortexVertices.Count;
        
        int counter = 0;
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
                counter += endInd - startInd;
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

                meshPart.transform.SetParent(fullModel.transform);

                UnityEditor.AssetDatabase.CreateAsset(mesh, "Assets/FullCortexModel/Small/MeshPart_small_" + i.ToString() + ".asset");
                

            }
            UnityEditor.AssetDatabase.SaveAssets();
            PrefabUtility.ReplacePrefab(fullModel, m_FullModelPrefab, ReplacePrefabOptions.ConnectToPrefab);
            print("Done");

            print("vertex count: " + vertexCount.ToString());
            //print("counter: " + counter.ToString());
            
            //print("Min x: " + minX.ToString());
            //print("Min z: " + minZ.ToString());
            //print("Max x: " + maxX.ToString());
            //print("Max z: " + maxZ.ToString());
        }


    }

    // Update is called once per frame
    void Update () {
		
	}
}
