using UnityEngine;
public class NoiseMapGenerator
{
    public static float[,] GetPerlinMap(int sizeX, int sizeY, float scale){
        float[,] map = new float[sizeX, sizeY];
        for(int x = 0; x < sizeX; x++){
            for (int y = 0; y < sizeY; y++){
                map[x,y] = Mathf.PerlinNoise(
                    (float)x / (float)sizeX * scale,
                    (float)y / (float)sizeY * scale
                );
            }
        }
        return map;
    }

    public static float[,] GetConeMap(int sizeX, int sizeY, float coneRadius){
        float[,] map = new float[sizeX,sizeY];
        Vector2 coneCenter = new Vector2((float) sizeX * 0.5f, (float) sizeY * 0.5f);
        if(coneRadius <= 0){ coneRadius = 0.1f;}
        for (int x = 0; x < sizeX; x++){
            for (int y = 0; y < sizeY; y++){
                float h = Mathf.Sqrt((x - coneCenter.x) * (x - coneCenter.x) + (y - coneCenter.y) * (y - coneCenter.y));
                float value = 0f;
                if(h / coneRadius < 1){
                    value = 1 - h / coneRadius;
                }
                map[x,y] = value;
            }
        }
        return map;
    }

    public static float[,] GetConeMapWithPerlin(int sizeX, int sizeY, float coneRadius, float perlinScale, float ConeToPerlinRatio){
        float[,] map = new float[sizeX,sizeY];
        float[,] coneMAp = GetConeMap(sizeX,sizeY, coneRadius);
        float[,] perlinMap = GetPerlinMap(sizeX,sizeY,perlinScale);

        if(ConeToPerlinRatio > 1){ ConeToPerlinRatio = 1;}
        if(ConeToPerlinRatio < 0){ ConeToPerlinRatio = 0;}

        for(int x = 0; x < sizeX; x++){
            for (int y = 0; y < sizeY;y++){
                map[x,y] = coneMAp[x,y] * (1 - ConeToPerlinRatio) + perlinMap[x,y] * ConeToPerlinRatio;
            }
        }
        return map;
    }
}
