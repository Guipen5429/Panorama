using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTerrain : MonoBehaviour
{
    public string mapChoice;
    public SpriteRenderer mapImage;
    public Sprite[] MapSprites;

    static readonly int[,] map9 = new int[,] {
        { 6, 4, 4, 4, 4, 4, 4, 4, 7 },
        { 3, 1, 1, 1, 1, 1, 1, 1, 5 },
        { 3, 1, 1, 1, 1, 1, 1, 1, 5 },
        { 3, 1, 1, 1, 1, 1, 1, 1, 5 },
        { 3, 1, 1, 1, 1, 1, 1, 1, 5 },
        { 3, 1, 1, 1, 1, 1, 1, 1, 5 },
        { 3, 1, 1, 1, 1, 1, 1, 1, 5 },
        { 3, 1, 1, 1, 1, 1, 1, 1, 5 },
        { 9, 2, 2, 2, 2, 2, 2, 2, 8 } }; //지형 정보 (0~9), 반시계로 한 번
    static readonly int[,] mapHylym = new int[,] {
         { 13, 00, 00, 00, 00, 00, 00, 00, 00 },
         { 11, 00, 00, 16, 04, 04, 04, 17, 00 },
         { 11, 00, 00, 03, 01, 01, 01, 05, 00 },
         { 09, 10, 10, 01, 01, 01, 01, 05, 00 },
         { 00, 00, 16, 01, 01, 01, 01, 05, 00 },
         { 00, 00, 03, 01, 01, 01, 01, 05, 00 },
         { 00, 00, 03, 01, 01, 01, 01, 05, 00 },
         { 00, 12, 01, 01, 01, 01, 02, 08, 00 },
         { 00, 00, 15, 19, 02, 18, 00, 00, 00 } };
    static readonly int[,] map1st = new int[,] {
         { 13, 00, 00, 00, 00, 00, 00, 00, 00 },
         { 11, 00, 00, 16, 10, 04, 04, 17, 00 },
         { 11, 00, 00, 19, 18, 19, 18, 15, 00 },
         { 09, 10, 10, 04, 10, 10, 10, 17, 00 },
         { 00, 00, 16, 01, 16, 17, 16, 05, 00 },
         { 00, 00, 03, 01, 05, 03, 05, 15, 00 },
         { 00, 00, 19, 02, 02, 18, 00, 00, 00 },
         { 00, 00, 00, 00, 00, 00, 00, 00, 00 },
         { 00, 00, 00, 00, 00, 00, 00, 00, 00 } }; //1층
    static readonly int[,] map5 = new int[,] {
        { 06, 04, 04, 04, 07 },
        { 03, 01, 01, 01, 05 },
        { 03, 01, 01, 01, 05 },
        { 03, 01, 01, 01, 05 },
        { 09, 02, 02, 02, 08 } }; //지형 정보 (0~9), 반시계로 한 번

    static Dictionary<string, int[,]> mapList = new Dictionary<string, int[,]>()
    {
        { "1st", map1st } ,
        { "5", map5 },
        { "9", map9 },
        { "Hylym", mapHylym }
    };

    public static string mapName = "5";
    public int[,] map;

    void Awake()
    {
        map = mapList[mapChoice ?? mapName];

        switch (mapChoice)
        {
            case "1st":
                mapImage.sprite = MapSprites[0]; ;
                break;
            case "5":
                mapImage.sprite = MapSprites[1]; ;
                break;
            case "9":
                mapImage.sprite = MapSprites[2]; ;
                break;
            case "Hylym":
                mapImage.sprite = MapSprites[3]; ;
                break;
        }
    }
}
