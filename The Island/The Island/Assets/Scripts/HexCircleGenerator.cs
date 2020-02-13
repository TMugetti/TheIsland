using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCircleGenerator : MonoBehaviour
{
    /*
    According to https://docs.unity3d.com/ScriptReference/Mesh-indexFormat.html, the vertex limit is  65535, 
    if we want to be able to build for mobile.
    knowing:
    About Arithmetic progression
    A given radius r
    That there will be 6 vertices per hexagon, if r is equal or greater than 8
    The ammount of hexes a = 1 + 6*r(r+1)/2 , since it can be describes as the center hexagon, plus r concentric rings of radiuses ( 1 -> r) * 6 hexagons
    And the amount of verts = a * 6

    meaning that our radius limit can be deduced as follows:
    65535 = a * 6
    65535 = (1 + 6*r(r+1)/2) * 6
    65535 / 6 = 1 + 6*r(r+1)/2
    10922.5 - 1 = 6*r(r+1)/2
    10921.5 / 6 = r(r+1)/2
    1820.25 * 2 = r(r+1)
    3640.5 = r(r+1)
    3640.5 = r*r + r
    0 = 1*r*r + 1*r - 3640.5                    from here we apply baskara
    r1 = -1 * ((1 + sqrt(1 - 4 * 1 * (-3640.5)))/2*1)
    r2 = -1 * ((1 - sqrt(1 - 4 * 1 * (-3640.5)))/2*1)
    r1 = -1 * ((1 + sqrt(1 + 14562))/2)
    r2 = -1 * ((1 - sqrt(1 + 14562))/2)
    r1 = -1 * ((1 + sqrt(14563))/2)
    r2 = -1 * ((1 - sqrt(14563))/2)       {sqrt(14563) = 120.67725552066553357095917599082}
    r1 = -1 * ((1 + 120.68/2)
    r2 = -1 * ((1 - 120.68)/2)
    r1 = -1 * ((121.68/2)
    r2 = -1 * ((-119.68)/2)
    r1 = -1 * 60.84
    r2 = -1 * (-59.84)
    r1 = -60.84
    r2 = 59.84

    Since we cannot have negative, nor non-integer, radius, this means our radius limit is 59.
*/
    [SerializeField] GameObject sampleHex;
    [Range(8,59)][SerializeField] int radius;
    [Range(0f,1f)][SerializeField] float minHeightClamp;
    [Range(0f,1f)][SerializeField] float maxHeightClamp;
    [SerializeField] float heightScale;
    [SerializeField] float perlinScale;
    [SerializeField] float coneRadius;
    [Range(0f,1f)][SerializeField] float coneToPerlinRatio;
    [SerializeField] Material[] materials;

    private float hexHeight;
    private float hexWidth;

    private GameObject[,,] hexes;
    private GameObject hexGroup;


    void OnValidate(){
        hexes = new GameObject[(radius * 2) + 1, (radius * 2) + 1,(radius * 2) + 1];
        if(sampleHex){
            Vector3 HexSize = sampleHex.GetComponent<MeshFilter>().sharedMesh.bounds.size;
            hexHeight = HexSize.z;
            hexWidth = HexSize.x;
        }
    }

    public float widthDistance(){
        return 0.75f * hexWidth;
    }

    public float heightDistance(){
        return 0.5f * hexHeight;
    }

    public void GenerateTerrain(){
        if(sampleHex){
            Vector3 position;
            hexGroup = new GameObject("Hexes");

            for(int x = radius * -1; x < radius + 1; x++){
                for(int y = radius * -1; y < radius + 1; y++){
                    int z = 0 - x - y;
                    if(z >= radius * -1 && z <= radius ){
                        position = transform.position;
                        position.z += z * hexHeight + x * 0.5f * hexHeight ;
                        position.x += x * 0.75f * hexWidth;
                        GameObject go = Instantiate(sampleHex, position, Quaternion.identity, hexGroup.transform) as GameObject;
                        go.isStatic = true;
                        hexes[radius + x, radius + y, radius + z] = go;
                        HexCoordinate HC = go.AddComponent<HexCoordinate>();
                        HC.setCoordinates(x,y,z);
                    }
                }
            }
        }
    }

    public void ClearHexes(){
        foreach(GameObject go in hexes){
            DestroyImmediate(go);
        }
        hexes = new GameObject[(radius * 2) + 1, (radius * 2) + 1, (radius * 2) + 1];
        DestroyImmediate(hexGroup);
    }

    public void ApplyPerlin(){
        if(hexes.Length > 0){
            Vector3 position;
            float[,] perlinMap = NoiseMapGenerator.GetPerlinMap((radius*2 + 1), (radius*2) + 1, perlinScale);
            float newY;
            for(int x = radius * -1; x < radius +1; x++){
                for (int y = radius * -1; y < radius +1; y++){
                    int z = 0 - x - y;
                    if (z >= radius * -1 && z <= radius){
                        position = hexes[radius + x, radius + y, radius + z].transform.position;
                        position.y = transform.position.y;
                        newY = perlinMap[radius + x, radius + y] ;
                        newY = MapHeightTolerance(newY) * hexHeight * heightScale;
                        position.y += newY;
                        hexes[radius + x, radius + y, radius + z].transform.position = position;
                    }
                }
            }
        }
    }

    public void ApplyConeMap(){
        if(hexes.Length > 0){
            Vector3 position;
            float[,] coneMap = NoiseMapGenerator.GetConeMap((radius*2 + 1), (radius*2) + 1, coneRadius);
            float newY;
            for(int x = radius * -1; x < radius +1; x++){
                for (int y = radius * -1; y < radius +1; y++){
                    int z = 0 - x - y;
                    if (z >= radius * -1 && z <= radius){
                        position = hexes[radius + x, radius + y, radius + z].transform.position;
                        position.y = transform.position.y;
                        newY = coneMap[radius + x, radius + y];
                        newY = MapHeightTolerance(newY)* hexHeight * heightScale;
                        position.y += newY;
                        hexes[radius + x, radius + y, radius + z].transform.position = position;
                    }
                }
            }
        }
    }


    public void ApplyConeAndPerlin(){
        if(hexes.Length > 0){
            Vector3 position;
            float[,] map = NoiseMapGenerator.GetConeMapWithPerlin((radius*2 + 1), (radius*2) + 1, coneRadius, perlinScale, coneToPerlinRatio);
            float newY;
            for(int x = radius * -1; x < radius +1; x++){
                for (int y = radius * -1; y < radius +1; y++){
                    int z = 0 - x - y;
                    if (z >= radius * -1 && z <= radius){
                        position = hexes[radius + x, radius + y, radius + z].transform.position;
                        position.y = transform.position.y;
                        newY = map[radius + x, radius + y];
                        newY = MapHeightTolerance(newY)* hexHeight * heightScale;
                        position.y += newY;
                        hexes[radius + x, radius + y, radius + z].transform.position = position;
                    }
                }
            }
        }
    }

    private float MapHeightTolerance(float number){
        if(number <= maxHeightClamp && number >= minHeightClamp){
            return number;
        } else if(number < minHeightClamp){
            number = 0f;
        } else if (number > maxHeightClamp){
            number = 1f;
        }
        return number;
    }

    public void TestMeshCombiner(){
        HexGenerator.TESTJoinHexagonsInCircle(hexes, materials);
        ClearHexes();
    }
}
