using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCircleGenerator : MonoBehaviour
{
    [SerializeField] GameObject sampleHex;
    [SerializeField] int radius;
    [Range(0f,1f)][SerializeField] float minHeightClamp;
    [Range(0f,1f)][SerializeField] float maxHeightClamp;
    [SerializeField] float heightScale;
    [SerializeField] float perlinScale;
    [SerializeField] float coneRadius;
    [Range(0f,1f)][SerializeField] float coneToPerlinRatio;

    private float hexHeight;
    private float hexWidth;

    private GameObject[,,] hexes;
    private GameObject hexGroup;


    void OnValidate(){
        if(radius <= 0){
            radius = 1;
        } /*else if(radius %2 == 0){
            radius +=1;
        }*/
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
        HexGenerator.TESTJoinHexagonsInCircle(hexes);
        ClearHexes();
    }
}
