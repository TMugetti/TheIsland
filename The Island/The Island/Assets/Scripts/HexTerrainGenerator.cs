using UnityEngine;

public class HexTerrainGenerator : MonoBehaviour
{
    [SerializeField] GameObject sampleHex;
    [SerializeField] int gridHeight = 11;
    [SerializeField] int gridWidth = 11;
    [SerializeField] float perlinScale;
    [SerializeField] float coneRadius;
    [Range(0f,1f)] [SerializeField] float coneToPerlinRatio;
    
    private float hexHeight;
    private float hexWidth;

    private GameObject[,] hexes;
    private GameObject hexGroup;

    public void OnValidate(){
        //Hex grid sizes must be positive
        if (gridHeight < 1){ gridHeight = 1;}
        if (gridWidth < 1){ gridWidth = 1;}
        hexes  = new GameObject[gridHeight,gridWidth];

        if(sampleHex){
            Vector3 HexSize = sampleHex.GetComponent<MeshFilter>().sharedMesh.bounds.size;
            hexHeight = HexSize.z;
            hexWidth = HexSize.x;
        }
    }

    public void ClearHexes(){
        if(hexes != null){
            for (int height = 0; height < gridHeight; height++){
                for(int width = 0; width < gridWidth; width ++){
                    if(hexes[height,width]){
                        DestroyImmediate(hexes[height,width]);
                    }
                }
            }
        }
        if (hexGroup){
            DestroyImmediate(hexGroup);
        }
    }

    public void GenerateHexGrid(){
        if(sampleHex){
            Vector3 initialPosition = transform.position;
            Vector3 position;
            float positionZ;
            float offSet = 0;
            
            hexGroup = new GameObject("Hexes");
            for(int height = 0; height < gridHeight; height++){
                positionZ = height * hexHeight;

                for(int width = 0; width < gridWidth; width++){
                    if(width %2 != 0){
                        offSet = 0.5f * hexHeight;
                    } else {
                        offSet = 0;
                    }
                    position = initialPosition;
                    position.z = positionZ + offSet;
                    position.x = initialPosition.x + width * 0.75f * hexWidth;
                    GameObject go = Instantiate(sampleHex, position, Quaternion.identity, hexGroup.transform) as GameObject;
                    go.isStatic = true;
                    hexes[height,width] = go;

                }
            }
        }
    }

    public void JoinMeshes(){
        if(hexes.Length > 0){
            int gridSize = gridWidth * gridHeight;
            MeshFilter[] meshFilters = new MeshFilter[gridSize];
            CombineInstance[] combines = new CombineInstance[gridSize];

            int counter = 0;
            for(int height = 0; height < gridHeight ; height++){
                for(int width = 0; width < gridWidth; width++){
                    meshFilters[counter] = hexes[height,width].GetComponent<MeshFilter>();
                    counter++;
                }
            }

            for(int i = 0; i < gridSize; i++){
                combines[i].mesh = meshFilters[i].sharedMesh;
                combines[i].transform = meshFilters[i].transform.localToWorldMatrix;
            }

            GameObject terrain = new GameObject("Terrain");
            MeshFilter terrainMeshFilter = terrain.AddComponent<MeshFilter>();
            MeshRenderer terrainMeshRenderer = terrain.AddComponent<MeshRenderer>();
            terrainMeshFilter.sharedMesh = new Mesh();
            terrainMeshFilter.sharedMesh.CombineMeshes(combines);
            terrainMeshRenderer.sharedMaterial = sampleHex.GetComponent<MeshRenderer>().sharedMaterial;
            
            foreach(GameObject go in hexes){
                DestroyImmediate(go);
            }

            hexes = new GameObject[gridHeight, gridWidth];

            DestroyImmediate(hexGroup);
            hexGroup = terrain;
        }
    }

    public void ApplyPerlin(){
        if(hexes.Length > 0){
            Vector3 position;
            float[,] perlinMap = NoiseMapGenerator.GetPerlinMap(gridHeight,gridWidth, perlinScale);
            float newY;
            for(int height = 0; height < gridHeight; height++){
                for (int width = 0; width < gridWidth; width++){
                    position = hexes[height,width].transform.position;
                    /*position.y = transform.position.y;
                    float perlinX = (float)height / (float)gridHeight * perlinScale;
                    float perlinY = (float)width / (float)gridWidth * perlinScale;
                    newY = Mathf.PerlinNoise(perlinX ,perlinY) * hexHeight * 5;*/
                    newY = perlinMap[height, width] * hexHeight * 5;
                    newY = MapHeightTolerance(newY);
                    position.y += newY;
                    hexes[height,width].transform.position = position;
                }
            }
        }
    }

    public void ApplyConeMap(){
        if(hexes.Length > 0){
            Vector3 position;
            float[,] coneMap = NoiseMapGenerator.GetConeMap(gridHeight,gridWidth, coneRadius);
            float newY;
            for(int height = 0; height < gridHeight; height++){
                for (int width = 0; width < gridWidth; width++){
                    position = hexes[height,width].transform.position;
                    newY = coneMap[height, width] * hexHeight * 5;
                    //newY = MapHeightTolerance(newY) * hexHeight;
                    position.y += newY;
                    hexes[height,width].transform.position = position;
                }
            }
        }
    }

    public void ApplyConeAndPerlin(){
        if(hexes.Length > 0){
            Vector3 position;
            float[,] Map = NoiseMapGenerator.GetConeMapWithPerlin(gridHeight,gridWidth,coneRadius,perlinScale, coneToPerlinRatio);
            float newY;
            for(int height = 0; height < gridHeight; height++){
                for (int width = 0; width < gridWidth; width++){
                    position = hexes[height,width].transform.position;
                    newY = Map[height, width] * hexHeight * 5;
                    //newY = MapHeightTolerance(newY) * hexHeight;
                    position.y += newY;
                    hexes[height,width].transform.position = position;
                }
            }
        }
    }

    private float MapHeightTolerance(float number){
        float minClamp = 0.3f;
        float maxClamp = 0.6f;
        if(number <= maxClamp && number >= 0.3){
            return 0f;
        }
        if(number < minClamp){
            number *= number;
            number *= -1;
        }
        if (number > maxClamp){
            number *= number;
        }
        return number;
    }
}
