using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HexGenerator : MonoBehaviour
{
    private static List<Vector3> hexVerts = new List<Vector3>();
    public float defaultHexHeight = 1f;
    public bool drawGizmos = true;
    
    /*
    A hex can be considered as a group of six equilateral triangles, of side length = 1/2 of the width of the hex.
    At the same time these triangles can be cut in two, getting a rect triangle with a hypotenuse of 1/2w,
    a side of 1/4w, and a side of 1/2 height of the hex.
    Knowing this, and h, we can solve for w as follows, by applying the pythagorean theorem:

    (w/2)^2 = (1/2h)^2 + (1/4w)^2
    (w/2)^2 - (1/4w)^2 = (1/2h)^2
    w*w * 1/4 - w*w *1/16 = h*h*1/4
    w*w - w*w*1/4 = h*h
    w*w - h*h = w*w*1/4
    w*w*4 - h*h*4 = w*w
    w*w*4 - w*w = h*h*4
    w*w*3 = h*h*4
    3(w*w) = 4(h*h)
    w^2 = 4h*h / 3
    w = sqrt(4/3*h*h)
    */
    public static void CalculateVertices(float hexHeight){
        float width = Mathf.Sqrt(4f/3f * hexHeight * hexHeight);
        float sideLenght= width * 0.5f;
        float halfLenght = sideLenght * 0.5f;
        float halfHeight = hexHeight * 0.5f;

        Vector3 one = Vector3.zero;
        one.x += sideLenght * -1f;

        Vector3 two = Vector3.zero;
        two.x += halfLenght * -1f;
        two.z += halfHeight * -1f;

        Vector3 three = Vector3.zero;
        three.x += halfLenght * -1f;
        three.z += halfHeight;

        Vector3 four = Vector3.zero;
        four.x += halfLenght;
        four.z += halfHeight * -1f;

        Vector3 five = Vector3.zero;
        five.x += halfLenght;
        five.z += halfHeight;

        Vector3 six = Vector4.zero;
        six.x += sideLenght;


        hexVerts.Clear();
        hexVerts.Add(one);
        hexVerts.Add(two);
        hexVerts.Add(three);
        hexVerts.Add(four);
        hexVerts.Add(five);
        hexVerts.Add(six);

    }

    void OnDrawGizmos(){
        if(drawGizmos && hexVerts.Count >= 6){
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(hexVerts[0],hexVerts[1]);
            Gizmos.DrawLine(hexVerts[1],hexVerts[2]);
            Gizmos.DrawLine(hexVerts[2],hexVerts[0]);
            Gizmos.DrawLine(hexVerts[3],hexVerts[1]);
            Gizmos.DrawLine(hexVerts[2],hexVerts[3]);
            Gizmos.DrawLine(hexVerts[3],hexVerts[4]);
            Gizmos.DrawLine(hexVerts[4],hexVerts[2]);
            Gizmos.DrawLine(hexVerts[4],hexVerts[5]);
            Gizmos.DrawLine(hexVerts[5],hexVerts[3]);

            Handles.Label(hexVerts[0],"0");
            Handles.Label(hexVerts[1],"1");
            Handles.Label(hexVerts[2],"2");
            Handles.Label(hexVerts[3],"3");
            Handles.Label(hexVerts[4],"4");
            Handles.Label(hexVerts[5],"5");
        }
    }

    public void GenerateHex(){
        MakeHex(defaultHexHeight);
    }

    public static GameObject MakeHex(float hexHeight){
        CalculateVertices(hexHeight);
        GameObject go = new GameObject("Hex");
        MeshFilter mf = go.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();

        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        foreach(Vector3 point in hexVerts){
            uvs.Add(new Vector2(point.x,point.z));
        }
        
        //first tri
        tris.Add(2);
        tris.Add(1);  //needs to be clock-wise to be face-up
        tris.Add(0);

        //Second tri
        tris.Add(1);
        tris.Add(2);
        tris.Add(3);

        //third tri
        tris.Add(4);
        tris.Add(3);  //same as first tri
        tris.Add(2);

        //fourth tri
        tris.Add(3);
        tris.Add(4);
        tris.Add(5);

        mesh.Clear();

        mesh.vertices = hexVerts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mf.sharedMesh = mesh;

        return go;
    }

    /*
    To join circular hex grids, we'll define the upper, upper left, and left, as "female", with the other three sides as "male". 
    This defines which side recieves a mesh conection, and which begins one.

    "Female" sides:
    0-1 or bottom left
    0-2 or top left
    2-4 or top

    "Male" sides:
    1-3 or bottom
    3-5 or bottom right
    4-5 or top right

    resulting possible connections:
    bottom to top
    bottom right to top left
    top right to bottom left

    First, since we won't create any new vertices, we join all of them in a list. And do the same with the triangles.
    Then, we create triangles between "male" sides that are next to "female" sides of nearby hexes.
    */
    public static void TESTJoinHexagonsInCircle(GameObject[,,] hexesGOs){
        int radius = (hexesGOs.GetLength(0) -1) /2;
        Debug.Log("Radius: " + radius);


        //Step 1: Find and store all vertices and triangle indexes from the hex meshes
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2Int> hexEnumerator = new List<Vector2Int>();
        Mesh hexMesh;

        int hexCounter = 0;
        GameObject currentHex;
        for(int x = radius * -1; x < radius +1; x++){
            for(int y = radius * -1; y < radius +1; y++){
                int z = 0 - x - y;
                if (z >= radius * -1 && z <= radius){
                    currentHex = hexesGOs[radius + x, radius + y, radius + z];
                    if(currentHex){
                        hexMesh = currentHex.GetComponent<MeshFilter>().sharedMesh;
                        foreach(Vector3 v in hexMesh.vertices){
                            vertices.Add(v + currentHex.transform.position);
                        }
                        foreach(int i in hexMesh.triangles){
                            triangles.Add(i + (hexCounter * 6));
                        }
                        hexEnumerator.Add(new Vector2Int(x,z));
                        hexCounter++;
                    } else {
                        Debug.LogWarning("Non existant hex at " + x + " " + y + " " + z);
                    }
                }
            }
        }

        Debug.Log(
            "Hexes: " + hexCounter + 
            " Expected verts: " + (hexCounter * 6) + 
            " Verts in list: " + vertices.Count +
            " Expected tris: " + (hexCounter * 4) + 
            " Tris in list: " + (triangles.Count/3)
        );

        //Step 2: Create triangles from "male" faces to adjacent "female" faces
        int preCombiningVertCount = triangles.Count;

        hexCounter = 0;
        int receivingHexIndex;
        Vector2Int hexBelow = new Vector2Int();
        Vector2Int hexTopRight = new Vector2Int();
        Vector2Int hexBottomRight = new Vector2Int();
        Vector2Int hexAbove = new Vector2Int();
        Vector2Int hexBottomLeft = new Vector2Int();
        Vector2Int hexTopLeft = new Vector2Int();

        for (int x = radius * -1; x < radius +1; x++){
            for (int y = radius * -1; y < radius +1; y++){
                int z = 0 - x - y;
                if(z >= radius * -1 && z <= radius){
                    //1 increment in x moves top right
                    //1 increment in z moves up
                    hexBelow.x = x;           hexBelow.y = z - 1;
                    hexTopRight.x = x + 1;    hexTopRight.y = z;
                    hexBottomRight.x = x + 1; hexBottomRight.y = z - 1; 
                    hexAbove.x = x;           hexAbove.y = z + 1;
                    hexBottomLeft.x = x - 1;  hexBottomLeft.y = z; 
                    hexTopLeft.x = x - 1;     hexTopLeft.y = z + 1;

                    if(hexEnumerator.Contains(hexBelow)){
                        receivingHexIndex = hexEnumerator.IndexOf(hexBelow);
                        triangles.Add(1 + hexCounter * 6);
                        triangles.Add(3 + hexCounter * 6);
                        triangles.Add(2 + receivingHexIndex * 6);

                        triangles.Add(3 + hexCounter * 6);
                        triangles.Add(4 + receivingHexIndex * 6);
                        triangles.Add(2 + receivingHexIndex * 6);
                    } else {
                        int index1 = vertices.Count;
                        Vector3 index1Vert = vertices[1 + hexCounter * 6];
                        index1Vert.y = 0;
                        vertices.Add(index1Vert);

                        int index3 = vertices.Count;
                        Vector3 index3Vert = vertices[3 + hexCounter * 6];
                        index3Vert.y = 0;
                        vertices.Add(index3Vert);

                        triangles.Add(index1);
                        triangles.Add(1 + hexCounter * 6);
                        triangles.Add(3 + hexCounter * 6);

                        triangles.Add(3 + hexCounter * 6);
                        triangles.Add(index3);
                        triangles.Add(index1);

                    }
                    if(hexEnumerator.Contains(hexTopRight)){
                        receivingHexIndex = hexEnumerator.IndexOf(hexTopRight);
                        triangles.Add(5 + hexCounter * 6);
                        triangles.Add(4 + hexCounter * 6);
                        triangles.Add(0 + receivingHexIndex * 6);

                        triangles.Add(0 + receivingHexIndex * 6);
                        triangles.Add(1 + receivingHexIndex * 6);
                        triangles.Add(5 + hexCounter * 6);
                    } else {
                        int index4 = vertices.Count;
                        Vector3 index4Vert = vertices[4 + hexCounter * 6];
                        index4Vert.y = 0;
                        vertices.Add(index4Vert);

                        int index5 = vertices.Count;
                        Vector3 index5Vert = vertices[5 + hexCounter * 6];
                        index5Vert.y = 0;
                        vertices.Add(index5Vert);

                        triangles.Add(index4);
                        triangles.Add(5 + hexCounter * 6);
                        triangles.Add(4 + hexCounter * 6);

                        triangles.Add(5 + hexCounter * 6);
                        triangles.Add(index4);
                        triangles.Add(index5);

                    }
                    if(hexEnumerator.Contains(hexBottomRight)){
                        receivingHexIndex = hexEnumerator.IndexOf(hexBottomRight);
                        triangles.Add(3 + hexCounter * 6);
                        triangles.Add(5 + hexCounter * 6);
                        triangles.Add(2 + receivingHexIndex * 6);

                        triangles.Add(3 + hexCounter * 6);
                        triangles.Add(2 + receivingHexIndex * 6);
                        triangles.Add(0 + receivingHexIndex * 6);
                    } else {
                        int index5 = vertices.Count;
                        Vector3 index5Vert = vertices[5 + hexCounter * 6];
                        index5Vert.y = 0;
                        vertices.Add(index5Vert);

                        int index3 = vertices.Count;
                        Vector3 index3Vert = vertices[3 + hexCounter * 6];
                        index3Vert.y = 0;
                        vertices.Add(index3Vert);

                        triangles.Add(3 + hexCounter * 6);
                        triangles.Add(5 + hexCounter * 6);
                        triangles.Add(index5);

                        triangles.Add(index5);
                        triangles.Add(index3);
                        triangles.Add(3 + hexCounter * 6);
                    }
                    if(!hexEnumerator.Contains(hexAbove)){
                        int index2 = vertices.Count;
                        Vector3 index2Vert = vertices[2 + hexCounter * 6];
                        index2Vert.y = 0;
                        vertices.Add(index2Vert);

                        int index4 = vertices.Count;
                        Vector3 index4Vert = vertices[4 + hexCounter * 6];
                        index4Vert.y = 0;
                        vertices.Add(index4Vert);

                        triangles.Add(4 + hexCounter * 6);
                        triangles.Add(2 + hexCounter * 6);
                        triangles.Add(index2);

                        triangles.Add(index2);
                        triangles.Add(index4);
                        triangles.Add(4 + hexCounter * 6);
                    }
                    if(!hexEnumerator.Contains(hexBottomLeft)){
                        int index0 = vertices.Count;
                        Vector3 index0Vert = vertices[0 + hexCounter * 6];
                        index0Vert.y = 0;
                        vertices.Add(index0Vert);

                        int index1 = vertices.Count;
                        Vector3 index1Vert = vertices[1 + hexCounter * 6];
                        index1Vert.y = 0;
                        vertices.Add(index1Vert);

                        triangles.Add(0 + hexCounter * 6);
                        triangles.Add(1 + hexCounter * 6);
                        triangles.Add(index1);

                        triangles.Add(index1);
                        triangles.Add(index0);
                        triangles.Add(0 + hexCounter * 6);
                    }
                    if(!hexEnumerator.Contains(hexTopLeft)){
                        int index2 = vertices.Count;
                        Vector3 index2Vert = vertices[2 + hexCounter * 6];
                        index2Vert.y = 0;
                        vertices.Add(index2Vert);

                        int index0 = vertices.Count;
                        Vector3 index0Vert = vertices[0 + hexCounter * 6];
                        index0Vert.y = 0;
                        vertices.Add(index0Vert);

                        triangles.Add(2 + hexCounter * 6);
                        triangles.Add(0 + hexCounter * 6);
                        triangles.Add(index0);

                        triangles.Add(index0);
                        triangles.Add(index2);
                        triangles.Add(2 + hexCounter * 6);
                    }
                    hexCounter++;
                }
            }
        }

        Mesh resultingMesh = new Mesh();
        resultingMesh.Clear();

        resultingMesh.vertices = vertices.ToArray();
        resultingMesh.triangles = triangles.ToArray();

        List<Vector2> uvs = new List<Vector2>();
        foreach(Vector3 point in vertices){
            uvs.Add(new Vector2(point.x,point.z));
        }

        resultingMesh.uv = uvs.ToArray();

        resultingMesh.RecalculateBounds();
        resultingMesh.RecalculateNormals();

        GameObject resultingGO = new GameObject("HexGrid");
        MeshFilter mf = resultingGO.AddComponent<MeshFilter>();
        MeshRenderer mr = resultingGO.AddComponent<MeshRenderer>();

        mf.sharedMesh = resultingMesh;
        Debug.Log("Final vertex count: " + vertices.Count);
        Debug.Log("Vertices per hex " + vertices.Count / hexCounter);
    }
}