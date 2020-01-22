using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(HexTerrainGenerator))]
public class HexTerrainEditor : Editor
{
    public override void OnInspectorGUI(){
        DrawDefaultInspector();
        HexTerrainGenerator script = (HexTerrainGenerator)target;
        
        if(GUILayout.Button("Generate Hex Grid")){
            script.GenerateHexGrid();
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
        if(GUILayout.Button("Join Meshes")){
            script.JoinMeshes();
        }
    }
}
