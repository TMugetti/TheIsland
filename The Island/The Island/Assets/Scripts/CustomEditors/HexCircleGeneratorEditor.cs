using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor (typeof(HexCircleGenerator))]
public class HexCircleGeneratorEditor : Editor
{
    public override void OnInspectorGUI(){
        DrawDefaultInspector();
        HexCircleGenerator script = (HexCircleGenerator)target;
        
        if(GUILayout.Button("Generate Hex Circle")){
            script.GenerateTerrain();
        }
        if(GUILayout.Button("Clear Hexes")){
            script.ClearHexes();
        }
        if(GUILayout.Button("Apply Perlin")){
            script.ApplyPerlin();
        }
         if(GUILayout.Button("Apply Cone")){
            script.ApplyConeMap();
        }
        if(GUILayout.Button("Apply Cone And Perlin")){
            script.ApplyConeAndPerlin();
        }
        if(GUILayout.Button("Test mesh combiner")){
            script.TestMeshCombiner();
        }
    }
}
