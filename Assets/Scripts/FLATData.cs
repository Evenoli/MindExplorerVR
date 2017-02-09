using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Runtime.InteropServices;
using System;
using System.IO;
using UnityEditor;

[InitializeOnLoad]
public static class FLATData {

    public struct FlatRes
    {
        public float[] coords;
        public int numcoords;
    };

    public static string path;

    static FLATData() // static Constructor
    {
        var currentPath = Environment.GetEnvironmentVariable("PATH",
            EnvironmentVariableTarget.Process);
#if UNITY_EDITOR_32
    var dllPath = Application.dataPath + "/Plugins";
        + Path.DirectorySeparatorChar + "C:\\Users\\Oli\\Documents\\MindExplorerVR\\MindExplorerVR\\Assets\\"
        + Path.DirectorySeparatorChar + "Plugins";
       // + Path.DirectorySeparatorChar + "x32";
#elif UNITY_EDITOR_64
        var dllPath = Application.dataPath + "/Plugins";
        //+ Path.DirectorySeparatorChar + "C:\\Users\\Oli\\Documents\\MindExplorerVR\\MindExplorerVR\\Assets"
        //+ Path.DirectorySeparatorChar + "Plugins";
        //+ Path.DirectorySeparatorChar + "x64";
#else // Player
    var dllPath = Application.dataPath
        + Path.DirectorySeparatorChar + "Plugins";

#endif
        if (currentPath != null && currentPath.Contains(dllPath) == false)
            Environment.SetEnvironmentVariable("PATH", currentPath + Path.PathSeparator
                + dllPath, EnvironmentVariableTarget.Process);

        path = dllPath;

    }

    [DllImport("FLATDLL2")]
    private static extern int add(int a, int b);
    [DllImport("FLATDLL2")]
    private static extern bool PerformQuery(ref IntPtr ptrResVerts, ref int resVertsLen, 
        float p0, float p1, float p2, float p3, float p4, float p5);

    public static int TestAdd(int a, int b)
    {
        return add(a, b);
    }

    public static FlatRes Query(float p0, float p1, float p2, float p3, float p4, float p5)
    {
        IntPtr ptrResVerts = IntPtr.Zero;
        int resVertsLen = 0;

        bool success = PerformQuery(ref ptrResVerts, ref resVertsLen, p0, p1, p2, p3, p4, p5);

        FlatRes r = new FlatRes();
        r.coords = null;
        r.numcoords = -1;
        if(success)
        {
            r.numcoords = resVertsLen;
            r.coords = new float[resVertsLen];

            Marshal.Copy(ptrResVerts, r.coords, 0, resVertsLen);
        }

        return r; 
    }
}
