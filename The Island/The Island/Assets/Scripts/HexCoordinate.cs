using UnityEngine;


public class HexCoordinate : MonoBehaviour
{
    [Header("Do not alter these values manually")]
    public int x;
    public int y;
    public int z;

    public void setCoordinates(int _x, int _y, int _z){
        x = _x;
        y = _y;
        z = _z;
    }
}
