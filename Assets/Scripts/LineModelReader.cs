using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class LineModelReader : MonoBehaviour {

    public GameObject m_LineSegPrefab;
    public GameObject m_LineModelObj;

    public Material m_meshMat;

    private string m_lineModelFile = "\\Data\\neocortex_s.txt";
    private string m_adjModelFile = "\\Data\\neocortex_s_adj.txt";

    private List<Vector3> lines;
    private int m_numLines;
    private List<List<int>> adj;

    private List<Mesh> meshes;
    
    // Use this for initialization
    void Start () {
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
            if (!float.TryParse(points[i+1], out y))
                fail = true;
            float z;
            if (!float.TryParse(points[i+2], out z))
                fail = true;

            Vector3 vec = new Vector3(x, y, z);
            lines.Add(vec);

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

        print(lines.Count / 2);

        meshes = new List<Mesh>();

        float lineWidth = 0.1f;
        int meshMaxSize = 1000;

        // Sort points into line groups, make a mesh for each group
        List<List<Vector3>> groupedPoints = LineGrouper.GroupPoints(lines.GetRange(0, 20000), meshMaxSize);


        // Build a mesh for each grouped line
        foreach(List<Vector3> line in groupedPoints)
        {
            Mesh m = new Mesh();
            List<Vector3> vertices = new List<Vector3>();

            // Add first point manually (edge case due to normal calculation)
            if (line.Count >= 2)
            {
                Vector3 start = line[0] * 50;
                Vector3 next = line[1] * 50;

                Vector3 normal = Vector3.Cross(start, next);
                Vector3 side = Vector3.Cross(normal, next - start);
                side.Normalize();

                Vector3 a = start + side * (lineWidth / 2);
                Vector3 b = start + side * (lineWidth / -2);

                vertices.Add(a);
                vertices.Add(b);

                Vector3 prev = start;

                // Iterate through rest of points in line, adding to mesh.
                for (int i = 1; i < line.Count; i++)
                {
                    Vector3 point = line[i] * 50;

                    normal = Vector3.Cross(point, prev);
                    side = Vector3.Cross(normal, prev - point);
                    side.Normalize();
                    a = point + side * (lineWidth / 2);
                    b = point + side * (lineWidth / -2);

                    vertices.Add(a);
                    vertices.Add(b);

                    prev = point;
                }
            }

            // Calculate UVs and triangles
            List<Vector2> UVs = new List<Vector2>();
            List<int> triangles = new List<int>();

            for (int j = 0; j < vertices.Count; j++)
            {
                UVs.Add(new Vector2(vertices[j].x, vertices[j].z));

                
                if (j < vertices.Count - 3 && (j % 2 == 0))
                {
                    
                    triangles.Add(j);
                    triangles.Add(j + 2);
                    triangles.Add(j + 1);

                    triangles.Add(j + 1);
                    triangles.Add(j + 2);
                    triangles.Add(j + 3);
                }
                
            }


            m.vertices = vertices.ToArray();

            //int[] t = { 0, 1, 2, 3, 1, 0};

            m.triangles = triangles.ToArray();
            m.uv = UVs.ToArray();
            m.RecalculateNormals();

            meshes.Add(m);

            GameObject lineSeg = Instantiate(m_LineSegPrefab);
            lineSeg.transform.SetParent(m_LineModelObj.transform);
            lineSeg.GetComponent<MeshFilter>().mesh = m;
        }

        print(meshes.Count);

        /*
        // t2
        m_numLines = lines.Count / 2;
        float lineWidth = 0.1f;
        int meshMaxSize = 10;

        int vCount = 0;

        // iterate through all vertices
        while(vCount < lines.Count - 1)
        {
            //Create new mesh. We limit each mesh size, and make sure it's continuous
            int perMeshCounter = 0;
            Vector3 tail = Vector3.zero;

            Mesh m = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            while (perMeshCounter < meshMaxSize && vCount < lines.Count - 1)
            {
                Vector3 end = lines[vCount] * 5;
                Vector3 start = lines[vCount + 1] * 5;

                // If mesh not continuous, break and start new one
                //if(start != tail && tail != Vector3.zero)
                //    break;

                Vector3 normal = Vector3.Cross(start, end);
                Vector3 side = Vector3.Cross(normal, end - start);
                side.Normalize();
                // Only on first iteration
                if (tail == Vector3.zero)
                {
                    Vector3 a = start + side * (lineWidth / 2);
                    Vector3 b = start + side * (lineWidth / -2);

                    vertices.Add(a);
                    vertices.Add(b);
                }
                Vector3 c = end + side * (lineWidth / 2);
                Vector3 d = end + side * (lineWidth / -2);

                
                vertices.Add(c);
                vertices.Add(d);

                vCount += 2;
                perMeshCounter += 2;

                tail = end;
            }

            List<Vector2> UVs = new List<Vector2>();
            List<int> triangles = new List<int>();

            for (int j = 0; j < vertices.Count; j++)
            {
                UVs.Add(new Vector2(vertices[j].x, vertices[j].z));

                if (j < vertices.Count - 2 && (j % 3 == 0))
                {
                    triangles.Add(j);
                    triangles.Add(j + 1);
                    triangles.Add(j + 2);
                }
            }

            m.vertices = vertices.ToArray();

            m.triangles = triangles.ToArray();
            m.uv = UVs.ToArray();
            m.RecalculateNormals();

            meshes.Add(m);

            //GameObject lineSeg = Instantiate(m_LineSegPrefab);
            //lineSeg.transform.SetParent(m_LineModelObj.transform);
            //lineSeg.GetComponent<MeshFilter>().mesh = m;
        }

        print(meshes.Count);

    */



    }
	
	// Update is called once per frame
	void Update () {
		//foreach(Mesh m in meshes)
        //{
        //    Graphics.DrawMesh(m, new Vector3(0, 5, 0), Quaternion.identity, m_meshMat, 0);
        //}
	}
}
