using UnityEngine;
using UnityEditor;
[CustomEditor (typeof(HexGenerator))]
public class HexGeneratorEditor : Editor
{
    public override void OnInspectorGUI(){
        DrawDefaultInspector();
        HexGenerator script = (HexGenerator) target;

        if(GUILayout.Button("Make Hex")){
            script.GenerateHex();
        }
        if(GUILayout.Button("Calculate Verts")){
            HexGenerator.CalculateVertices(script.defaultHexHeight);
        }
    }
}
