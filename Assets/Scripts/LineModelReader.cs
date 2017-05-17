using System.Collections;
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
    private List<Position> lines;
    // Lists to hold adjacency info from file
    private List<List<int>> adj;
    // List to hold all mesh objects
    private List<Mesh> meshes;
    // List to hold all mesh part game objects
    private List<GameObject> meshParts;
    // List to track which vertices are contained in which mesh (meshVerts[meshInd][VertInd])
    private List<List<int>> meshVerts;

    // Use this for initialization
    void Awake () {
        MeshInfo mi = MeshData.LoadMeshInfo();
        numVerts = mi.numVerts;
        if (numVerts != -1)
        {
            print("Mesh Info found!");
            return;
        }

        string pathBase = Application.dataPath;
        string path = pathBase + m_lineModelFile;

        // Should contain 1 line
        string fileTxt = File.ReadAllLines(path)[0];
        string[] points = fileTxt.Split(' ');
        lines = new List<Position>();
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

            Position vec = new Position(x, y, z);
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

        numVerts = lines.Count;

        meshes = new List<Mesh>();
        meshParts = new List<GameObject>();

        //float lineWidth = 0.002f;
        float lineWidth = 2.5f;

        meshVerts = new List<List<int>>();

        int numMeshesRequired = (numVerts / ((MAX_VERTICES_PER_MESH/2 ) - 1) ) + 1;

        // Per mesh m
        for (int m = 0; m < numMeshesRequired; m++)
        {
            // Create mesh and vertecis, uv and triangles lists
            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> UVs = new List<Vector2>();
            List<int> triangles = new List<int>();

            // Create list of vert ids that will be in current mesh
            List<int> vs = new List<int>();

            int vertCounter = 0;

            // Calculate start and end vert indexes for current mesh
            int startInd = m * ((MAX_VERTICES_PER_MESH / 2) - 1);
            int endInd = Mathf.Min(startInd + ((MAX_VERTICES_PER_MESH / 2) - 1), numVerts);

            // For each vert in this mesh
            for (int i = startInd; i < endInd - 1; i+=2)
            {
                // add vert indexes to current vs list
                vs.Add(i);
                vs.Add(i + 1);

                // Get start and end points of line
                Vector3 end = CoordinateConvertion.ModelToFlat(lines[i].GetVector());
                Vector3 start = CoordinateConvertion.ModelToFlat(lines[i+1].GetVector());

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
                UVs.Add(new Vector2(vertices[vertCounter+1].x, vertices[vertCounter+1].z));
                UVs.Add(new Vector2(vertices[vertCounter+2].x, vertices[vertCounter+2].z));
                UVs.Add(new Vector2(vertices[vertCounter+3].x, vertices[vertCounter+3].z));

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
            VertexData vd = lineSeg.GetComponent<VertexData>();
            if(vd)
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

        MeshData.SaveMeshInfo(meshInf);
        /*
        // Sort points into line groups, make a mesh for each group
        List<List<Vector3>> groupedPoints = LineGrouper.GroupPoints(lines, meshMaxSize);

        int meshCounter = 0;

        // Build a mesh for each grouped line
        foreach(List<Vector3> line in groupedPoints)
        {
            Mesh m = new Mesh();
            List<Vector3> vertices = new List<Vector3>();

            // Add first point manually (edge case due to normal calculation)
            if (line.Count >= 2)
            {
                Vector3 start = CoordinateConvertion.ModelToFlat(line[0]);
                Vector3 next = CoordinateConvertion.ModelToFlat(line[1]);

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
                    Vector3 point = CoordinateConvertion.ModelToFlat(line[i]);

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

            m.triangles = triangles.ToArray();
            m.uv = UVs.ToArray();
            m.RecalculateNormals();

            meshes.Add(m);

            GameObject lineSeg = Instantiate(m_LineSegPrefab);
            lineSeg.transform.SetParent(m_LineModelObj.transform);
            lineSeg.transform.localScale = new Vector3(1, 1, 1);
            lineSeg.GetComponent<MeshFilter>().mesh = m;

            UnityEditor.AssetDatabase.CreateAsset(m, "Assets/FullCortexModel/Line2/MeshPart_line2_" + meshCounter.ToString() + ".asset");
            meshCounter++;
        }

        UnityEditor.AssetDatabase.SaveAssets();
        PrefabUtility.ReplacePrefab(m_LineModelObj, m_lineModelPrefab, ReplacePrefabOptions.ConnectToPrefab);

        print(meshes.Count);
       


        // t2
        //m_numLines = lines.Count / 2;
        //float lineWidth = 0.1f;
        //int meshMaxSize = 10;

        int vCount = 0;
        int meshCounter = 0;
        // iterate through all vertices
        while (vCount < lines.Count - 1)
        {
            Mesh m = new Mesh();
            List<Vector3> vertices = new List<Vector3>();

            Vector3 end = CoordinateConvertion.ModelToFlat(lines[vCount]);
            Vector3 start = CoordinateConvertion.ModelToFlat(lines[vCount + 1]);

            Vector3 normal = Vector3.Cross(start, end);
            Vector3 side = Vector3.Cross(normal, end - start);
            side.Normalize();

            Vector3 a = start + side * (lineWidth / 2);
            Vector3 b = start + side * (lineWidth / -2);

            vertices.Add(a);
            vertices.Add(b);

            Vector3 c = end + side * (lineWidth / 2);
            Vector3 d = end + side * (lineWidth / -2);

                
            vertices.Add(c);
            vertices.Add(d);

            vCount += 2;

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

            m.triangles = triangles.ToArray();
            m.uv = UVs.ToArray();
            m.RecalculateNormals();

            meshes.Add(m);
            UnityEditor.AssetDatabase.CreateAsset(m, "Assets/FullCortexModel/LineUngrouped/MeshPart_lineUG_" + meshCounter.ToString() + ".asset");
            meshCounter++;

            GameObject lineSeg = Instantiate(m_LineSegPrefab);
            lineSeg.transform.SetParent(m_LineModelObj.transform);
            lineSeg.GetComponent<MeshFilter>().mesh = m;
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
