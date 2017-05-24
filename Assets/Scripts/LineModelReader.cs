﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;



public class LineModelReader : MonoBehaviour {

    public GameObject m_LineSegPrefab;
    public GameObject m_LineModelObj;
    public GameObject m_lineModelPrefab;

    public Material m_meshMat;

    private string m_lineModelFile = "\\Data\\neocortex_s.txt";
    private string m_adjModelFile = "\\Data\\neocortex_s_adj.txt";

    private const int MAX_VERTICES_PER_MESH = 64998;

    private int numVerts; 
    // List to hold all vertices from file
    private List<Vector3> lines;
    // Lists to hold adjacency info from file
    private List<List<int>> adj;
    // List to hold all mesh objects
    private List<Mesh> meshes;
    // List to hold all mesh part game objects
    private List<GameObject> meshParts;
    // List to track which vertices are contained in which mesh (meshVerts[meshInd][VertInd])
    private List<List<int>> meshVerts;

    // Mesh info of last run mesh build
    private MeshInfo m_meshInfo;
    private bool m_buildRun;

    // Use this for initialization
    void Awake () {
        ReadLineModelData();
        m_buildRun = false;
    }

    public List<Vector3> GetVertices()
    {
        return lines;
    }

    public List<List<int>> GetAdjData()
    {
        return adj;
    }

    // May return uninitialised variable if BuildLineMeshes hasn't been run yet
    public MeshInfo GetMeshInfo()
    {
        if(m_buildRun)
            return m_meshInfo;
        else
        {
            MeshInfo m = new MeshInfo();
            m.numVerts = -1;
            m.numMeshes = -1;
            m.meshVerts = new List<List<int>>();
            return m;
        }
    }

    public void ReadLineModelData()
    {
        string pathBase = Application.dataPath;
        string path = pathBase + m_lineModelFile;

        // Should contain 1 line
        string fileTxt = File.ReadAllLines(path)[0];
        string[] points = fileTxt.Split(' ');
        lines = new List<Vector3>();
        bool fail = false;

        for (int i = 0; i < points.Length - 2; i += 3)
        {

            float x;
            if (!float.TryParse(points[i], out x))
                fail = true;

            float y;
            if (!float.TryParse(points[i + 1], out y))
                fail = true;
            float z;
            if (!float.TryParse(points[i + 2], out z))
                fail = true;

            Vector3 vec = new Vector3(x, y, z);
            lines.Add(CoordinateConvertion.ModelToFlat(vec));

        }

        if (fail)
            print("Parse error!");

        path = pathBase + m_adjModelFile;

        // Should contain 1 line
        fileTxt = File.ReadAllLines(path)[0];
        string[] adjlists = fileTxt.Split(';');
        adj = new List<List<int>>();
        for (int i = 0; i < adjlists.Length; i++)
        {
            string[] vals = adjlists[i].Split(' ');
            List<int> valList = new List<int>();
            for (int j = 0; j < vals.Length; j++)
            {
                int v = 0;

                if (vals[j] == "" || vals[j] == " ")
                    continue;

                if (!int.TryParse(vals[j], out v))
                {
                    print("Error: " + vals[j]);
                    return;
                }
                valList.Add(v);
            }
            adj.Add(valList);
        }
    }

    public void BuildLineMeshes()
    {
        numVerts = lines.Count;

        meshes = new List<Mesh>();
        meshParts = new List<GameObject>();

        //float lineWidth = 0.002f;
        float lineWidth = 2.5f;

        meshVerts = new List<List<int>>();

        int numMeshesRequired = (numVerts / ((MAX_VERTICES_PER_MESH / 2) - 1)) + 1;

        // Per mesh m
        for (int m = 0; m < numMeshesRequired; m++)
        {
            // Create mesh and vertecis, uv and triangles lists
            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> UVs = new List<Vector2>();
            List<int> triangles = new List<int>();

            // Clear any existing mesh objects
            for(int i=0; i < m_LineModelObj.transform.childCount; i++)
            {
                Destroy(m_LineModelObj.transform.GetChild(i).gameObject);
            }

            // Create list of vert ids that will be in current mesh
            List<int> vs = new List<int>();

            int vertCounter = 0;

            // Calculate start and end vert indexes for current mesh
            int startInd = m * ((MAX_VERTICES_PER_MESH / 2) - 1);
            int endInd = Mathf.Min(startInd + ((MAX_VERTICES_PER_MESH / 2) - 1), numVerts);

            // For each vert in this mesh
            for (int i = startInd; i < endInd - 1; i += 2)
            {
                // add vert indexes to current vs list
                vs.Add(i);
                vs.Add(i + 1);

                // Get start and end points of line
                Vector3 end = lines[i];
                Vector3 start = lines[i + 1];

                // calculate normal and 'side' direction of line segment
                Vector3 normal = Vector3.Cross(start, end);
                Vector3 side = Vector3.Cross(normal, end - start);
                side.Normalize();

                // Create vectors for each corner of the line segment
                Vector3 a = start + side * (lineWidth / 2);
                Vector3 b = start + side * (lineWidth / -2);
                Vector3 c = end + side * (lineWidth / 2);
                Vector3 d = end + side * (lineWidth / -2);

                // add vectors to vertices list
                vertices.Add(a);
                vertices.Add(b);
                vertices.Add(c);
                vertices.Add(d);

                // Add uvs for these 4 vectors

                UVs.Add(new Vector2(vertices[vertCounter].x, vertices[vertCounter].z));
                UVs.Add(new Vector2(vertices[vertCounter + 1].x, vertices[vertCounter + 1].z));
                UVs.Add(new Vector2(vertices[vertCounter + 2].x, vertices[vertCounter + 2].z));
                UVs.Add(new Vector2(vertices[vertCounter + 3].x, vertices[vertCounter + 3].z));

                // Add triangles for these 4 vectors
                triangles.Add(vertCounter);
                triangles.Add(vertCounter + 2);
                triangles.Add(vertCounter + 1);

                triangles.Add(vertCounter + 1);
                triangles.Add(vertCounter + 2);
                triangles.Add(vertCounter + 3);

                // Increment vert counter
                vertCounter += 4;
            }

            meshVerts.Add(vs);

            // Add arrays to mesh object
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = UVs.ToArray();
            mesh.RecalculateNormals();


            UnityEditor.AssetDatabase.CreateAsset(mesh, "Assets/FullCortexModel/Line/MeshPart__" + m + ".asset");

            // Create lineSeg object
            GameObject lineSeg = Instantiate(m_LineSegPrefab);
            lineSeg.transform.SetParent(m_LineModelObj.transform);
            // Reset lineSeg object's transform
            lineSeg.transform.localScale = new Vector3(1, 1, 1);
            lineSeg.transform.localPosition = Vector3.zero;
            // Set mesh
            lineSeg.GetComponent<MeshFilter>().mesh = mesh;

            // Set data in VertexData script on lineSeg
            VertexManager vd = lineSeg.GetComponent<VertexManager>();
            if (vd)
            {
                vd.m_MeshID = m;
                vd.m_VertexIDs = vs;
            }

            meshes.Add(mesh);
            meshParts.Add(lineSeg);

        }

        PrefabUtility.ReplacePrefab(m_LineModelObj, m_lineModelPrefab, ReplacePrefabOptions.ConnectToPrefab);

        MeshInfo meshInf = new MeshInfo();
        meshInf.numVerts = numVerts;
        meshInf.numMeshes = meshes.Count;
        meshInf.meshVerts = meshVerts;

        m_meshInfo = meshInf;
        m_buildRun = true;

        MeshData.SaveMeshInfo(meshInf);
    }
	
}
