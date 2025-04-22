using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public class PathMake : MonoBehaviour
{
    public GameObject Map;
    MapEvent mapEvent;
    PinMake pinMake;
    MarkMake markMake;
    RouteMake routeMake;
    public GameObject BackGround;
    LoopBuildings loopBuildings;
    public GameObject Player;
    CharacterMove characterMove;

    public int GStt2;
    public int[] pathX;
    public int[] pathY;
    public int[] bPathX;
    public int[] bPathY;
    List<int> pathXList;
    List<int> pathYList;
    int[] pathLc;
    int mapL;
    float leftBound; //좌측 연장 경계
    float rightBound; //우측 연장 경계

    public int pi;
    public int pt;

    public bool pinCall;
    public int evntPin;
    public bool call2;
    bool rcv;
    bool rcv2;

    // Start is called before the first frame update
    void Start()
    {
        mapEvent = Map.GetComponent<MapEvent>();
        routeMake = Map.GetComponent<RouteMake>();
        markMake = Map.GetComponent<MarkMake>(); ;
        pinMake = Map.GetComponent<PinMake>();
        loopBuildings = BackGround.GetComponent<LoopBuildings>();
        characterMove = Player.GetComponent<CharacterMove>();

        mapL = loopBuildings.mapL;

        pathX = new int[] { 0, 1, 2 };
        pathY = new int[] { 0, 0, 0 };
        GStt2 = 0;

        pinCall = false;
        evntPin = 0;
        rcv = false;
        rcv2 = true;
    }

    // Update is called once per frame
    void Update()
    {
        int evnt0 = mapEvent.eventTime[0];
        int evnt1 = mapEvent.eventTime[1];
        bool go = mapEvent.go;
        if (evnt0 == 7 || evnt0 == 8 || evnt0 == 9) { rcv = false; rcv2 = true; pinCall = false; } //default
        if (evnt0 == 0 || evnt0 == 6 || evnt0 == 8 && rcv2) { rcv = true; }

        if (evnt0 == 4) { call2 = false; }
        if (evnt0 == 6)
        {
            pathLc = new int[pathX.Length];
            for (int i = 0; i < pathX.Length - 1; i++)
            {
                pathLc[i] = (mapL * pathY[i]) + pathX[i] + 1;
            }
            call2 = false;
            GameObject[] pins = pinMake.pins;
            bool[] callPin = new bool[mapL * mapL + 1];
            for (int i = 1; i < (mapL * mapL + 1); i++)
            {
                callPin[i] = pins[i].GetComponent<PinAct>().callPin;
                if (callPin[i])
                {
                    evntPin = 8;
                    pathXList = new List<int>(pathX);
                    pathYList = new List<int>(pathY);
                    CheckState((i - 1) % mapL, (i - 1) / mapL);
                    pathX = pathXList.ToArray();
                    pathY = pathYList.ToArray();
                    rcv = false;
                    rcv2 = false;
                    break;
                }
            }
        }
        if (evnt0 == 8 && !pinCall && go && rcv)
        {
            BuildPathExpn();
            /*pi = 0;
            bPathX = pathX;
            bPathY = pathY;*/
            switch (evnt1)
            {
                case 0: case 1: /*BuildPathExpn();*/ evntPin = 7; break;
                case 2: evntPin = 9; break;
                case 3: evntPin = 7; break;
            }
            pinCall = true;
            rcv = false;
            rcv2 = false;
        }

        if (evnt0 == 3) { GStt2 = 0; }
        if (evnt0 == 9) { BoundMake(); }
        if (evnt0 == 0 && !pinCall && go && rcv)
        {
            PathExpn();
        }

    }

    public void CheckState(int x, int y)
    {
        int[,] pinState = routeMake.pinState;
        int p = markMake.p;

        switch (pinState[x, y])
        {
            case 1: case 7: Ctrc(Array.IndexOf(pathLc, mapL * y + x + 1)); pinCall = true; break;
            case 2:
                switch (GStt2)
                {
                    case 0:
                        if (pinState[pathX[1], pathY[1]] == 6)
                        {
                            pathXList.RemoveAt(0);
                            pathYList.RemoveAt(0);
                            call2 = true;
                        } break;
                    case 1: GStt2 = 0; break;
                } pinCall = true; break;
            case 3:
                switch (GStt2)
                {
                    case 0: GStt2 = 1; break;
                    case 1:
                        if (pinState[pathX[^2], pathY[^2]] == 6)
                        {
                            pathXList.RemoveAt(pathXList.Count - 1);
                            pathYList.RemoveAt(pathYList.Count - 1);
                        } break;
                } pinCall = true; break;
            case 4: if (GStt2 == 0) { Expn(x, y, 0); call2 = true; } break;
            case 5: if (GStt2 == 1) { Expn(x, y, pathX.Length - 1); } break;
        }
    }

    void Expn(int x, int y, int t)
    {
        int tx = pathX[t];
        int ty = pathY[t];
        if (x == tx)
        {
            if (y < ty)
            {
                for (int i = ty - 1; i >= y; i--)
                {
                    AddInsert(tx, i);
                }
            }
            else
            {
                for (int i = ty + 1; i <= y; i++)
                {
                    AddInsert(tx, i);
                }
            }
        }
        else
        {
            if (x < tx)
            {
                for (int i = tx - 1; i >= x; --i)
                {
                    AddInsert(i, ty);
                }
            }
            else
            {
                for (int i = tx + 1; i <= x; ++i)
                {
                    AddInsert(i, ty);
                }
            }
        }

        void AddInsert(int x, int y)
        {
            if (t == 0)
            {
                pathXList.Insert(0, x);
                pathYList.Insert(0, y);
            }
            else
            {
                pathXList.Add(x);
                pathYList.Add(y);
            }
        }
        pinCall = true;
    }

    void Ctrc(int index)
    {
        int p = markMake.p;
        if (index <= p)
        {
            for (int i = 0; i < index; i++)
            {
                pathXList.RemoveAt(0);
                pathYList.RemoveAt(0);
            }
            GStt2 = 0;
            call2 = true;
        }
        else
        {
            for (int i = pathXList.Count - 1; i > index; i--)
            {
                pathXList.RemoveAt(pathXList.Count - 1);
                pathYList.RemoveAt(pathYList.Count - 1);
            }
            GStt2 = 1;
        }
    }

    void BuildPathExpn()
    {
        bPathX = pathX;
        bPathY = pathY;
        List<int> bPathXList = new(bPathX);
        List<int> bPathYList = new(bPathY);
        pi = 0;
        int[,] map = loopBuildings.map;

        if (bPathX[0] == bPathX[1])
        {
            if (bPathY[0] > bPathY[1])
            {
                PsthExpn(0, true); //상
            }
            else
            {
                PsthExpn(1, true); //하
            }
        }
        else
        {
            if (bPathX[0] > bPathX[1])
            {
                PsthExpn(2, true); //우
            }
            else
            {
                PsthExpn(3, true); //좌
            }
        }
        if (bPathX[^1] == bPathX[^2])
        {
            if (bPathY[^1] > bPathY[^2])
            {
                PsthExpn(0, false); //상
            }
            else
            {
                PsthExpn(1, false); //하
            }
        }
        else
        {
            if (bPathX[^1] > bPathX[^2])
            {
                PsthExpn(2, false); //우
            }
            else
            {
                PsthExpn(3, false); //좌
            }
        }

        void PsthExpn(int d, bool fb)
        {
            bool hb = true;
            bool b = true;
            int ini;
            int inini;
            switch (d)
            {
                case 0:
                    ini = fb ? 0 : bPathXList.Count - 1;
                    inini = map[bPathX[ini], bPathY[ini]];
                    if (map[bPathX[ini], bPathY[ini]] == 17 || map[bPathX[ini], bPathY[ini]] == 18)
                    {
                        PsthExpn(inini == 17 ? 2 : 3, fb);
                    }
                    for (int i = bPathY[ini] + 1; i < mapL; i++)
                    {
                        switch (map[bPathX[ini], i])
                        {
                            case 5: case 7: case 8: case 12: hb = false; break;
                            case 17: BuildAddInsert(ini, bPathX[ini], i);
                                PsthExpn(2, fb); break;
                            case 18: BuildAddInsert(ini, bPathX[ini], i);
                                PsthExpn(3, fb); break;
                            case 1: case 2: case 4: case 10: hb = false; b = false; break;
                            default: break;
                        }
                        if (hb) { break; }
                        else if (b) { BuildAddInsert(ini, bPathX[ini], i); break; }
                        else { BuildAddInsert(ini, bPathX[ini], i); hb = true; b = true; }
                    } break;
                case 1:
                    ini = fb ? 0 : bPathXList.Count - 1;
                    inini = map[bPathX[ini], bPathY[ini]];
                    if (map[bPathX[ini], bPathY[ini]] == 16 || map[bPathX[ini], bPathY[ini]] == 19)
                    {
                        PsthExpn(inini == 16 ? 2 : 3, fb);
                    }
                    for (int i = bPathY[ini] - 1; i >= 0; i--)
                    {
                        switch (map[bPathX[ini], i])
                        {
                            case 3: case 6: case 9: case 14: hb = false; break;
                            case 16: BuildAddInsert(ini, bPathX[ini], i);
                                PsthExpn(2, fb); break;
                            case 19: BuildAddInsert(ini, bPathX[ini], i);
                                PsthExpn(3, fb); break;
                            case 1: case 2: case 4: case 10: hb = false; b = false; break;
                            default: break;
                        }
                        if (hb) { break; }
                        else if (b) { BuildAddInsert(ini, bPathX[ini], i); break; }
                        else { BuildAddInsert(ini, bPathX[ini], i); hb = true; b = true; }
                    } break;
                case 2:
                    ini = fb ? 0 : bPathYList.Count - 1;
                    inini = map[bPathX[ini], bPathY[ini]];
                    if (map[bPathX[ini], bPathY[ini]] == 18 || map[bPathX[ini], bPathY[ini]] == 19)
                    {
                        PsthExpn(inini == 18 ? 1 : 0, fb);
                    }
                    for (int i = bPathX[ini] + 1; i < mapL; i++)
                    {
                        switch (map[i, bPathY[ini]])
                        {
                            case 2: case 8: case 9: case 15: hb = false; break;
                            case 18: BuildAddInsert(ini, i, bPathY[ini]);
                                PsthExpn(1, fb); break;
                            case 19: BuildAddInsert(ini, i, bPathY[ini]);
                                PsthExpn(0, fb); break;
                            case 1: case 3: case 5: case 11: hb = false; b = false; break;
                            default: break;
                        }
                        if (hb) { break; }
                        else if (b) { BuildAddInsert(ini, i, bPathY[ini]); break; }
                        else { BuildAddInsert(ini, i, bPathY[ini]); hb = true; b = true; }
                    } break;
                case 3:
                    ini = fb ? 0 : bPathYList.Count - 1;
                    inini = map[bPathX[ini], bPathY[ini]];
                    if (map[bPathX[ini], bPathY[ini]] == 16 || map[bPathX[ini], bPathY[ini]] == 17)
                    {
                        PsthExpn(inini == 16 ? 0 : 1, fb);
                    }
                    for (int i = bPathX[ini] - 1; i >= 0; i--)
                    {
                        switch (map[i, bPathY[ini]])
                        {
                            case 4: case 6: case 7: case 13: hb = false; break;
                            case 16: BuildAddInsert(ini, i, bPathY[ini]);
                                PsthExpn(0, fb); break;
                            case 17: BuildAddInsert(ini, i, bPathY[ini]);
                                PsthExpn(1, fb); break;
                            case 1: case 3: case 5: case 11: hb = false; b = false; break;
                            default: break;
                        }
                        if (hb) { break; }
                        else if (b) { BuildAddInsert(ini, i, bPathY[ini]); break; }
                        else { BuildAddInsert(ini, i, bPathY[ini]); hb = true; b = true; }
                    } break;
            }

            void BuildAddInsert(int ini, int x, int y)
            {
                if (ini == 0)
                {
                    bPathXList.Insert(0, x);
                    bPathYList.Insert(0, y);
                    bPathX = bPathXList.ToArray();
                    bPathY = bPathYList.ToArray();
                    pi++;
                }
                else
                {
                    bPathXList.Add(x);
                    bPathYList.Add(y);
                    bPathX = bPathXList.ToArray();
                    bPathY = bPathYList.ToArray();
                }
            }
        }
    }

    void BoundMake()
    {
        int[] pathPoint = loopBuildings.pathPoint;
        float[] bRoutePoint = loopBuildings.routePoint;
        float[] left = loopBuildings.leftIns;
        float[] right = loopBuildings.rightIns;
        leftBound = pi == 0 ? 0 : bRoutePoint[pi - 1] + BoundMake(pathPoint[pi - 1]); //좌측 연장 경계
        rightBound = bRoutePoint[pi + pathX.Length - 1] + BoundMake(pathPoint[pi + pathX.Length - 1]); //우측 연장 경계
        //Debug.Log("좌측 연장 경계: " + leftBound);
        //Debug.Log("우측 연장 경계: " + rightBound);

        float BoundMake(int p)
        {
            float b;
            switch (p)
            {
                case 0: case 1: case 2: case 13: case 14:
                    b = right[p];
                    break;
                case 4: case 10:
                    b = left[3] + right[3];
                    break;
                case 5:
                    b = left[5] + right[5] - 1.9f;
                    break;
                case 6:
                    b = left[3] + right[3] + 1.9f;
                    break;
                case 7:
                    b = right[7] + left[3] + right[3];
                    break;
                case 12:
                    b = right[p];
                    break;
                default:
                    b = 0;
                    break;
            }
            return b;
        }
    }

    void PathExpn()
    {
        float pX = Player.transform.position.x;
        pathXList = new List<int>(pathX);
        pathYList = new List<int>(pathY);

        if (pX < leftBound && pi != 0)
        {
            pathXList.Insert(0, bPathX[pi - 1]);
            pathYList.Insert(0, bPathY[pi - 1]);
            Debug.Log("좌로 연장");
            evntPin = 2;
            pinCall = true;
            rcv = false;
            rcv2 = false;
        }
        else if (pX > rightBound)
        {
            pathXList.Add(bPathX[pi + pathX.Length]);
            pathYList.Add(bPathY[pi + pathY.Length]);
            Debug.Log("우로 연장");
            evntPin = 2;
            pinCall = true;
            rcv = false;
            rcv2 = false;
        }
        pathX = pathXList.ToArray();
        pathY = pathYList.ToArray();
    }
}

/*class Terrain //실험
{
    enum PathSort
    {
        Nothing, Cross,
        Tup, Tright, Tdown, Tleft,
        Lup, Lright, Ldown, Lleft,
        Iup, Idown,
        iup, iright, idown, ileft,
        CurveLup, CurveLright, CurveLdown, CurveLleft,
    }

    int Tiling(bool[] boolArray)
    {
        string binaryString = string.Join("", Array.ConvertAll(boolArray, b => b ? "1" : "0"));
        return binaryString switch
        {
            "00000" => (int)PathSort.Nothing,
            "11110" => (int)PathSort.Cross,
            "10110" => (int)PathSort.Tup,
            "11100" => (int)PathSort.Tright,
            "01110" => (int)PathSort.Tdown,
            "11010" => (int)PathSort.Tleft,
            "10100" => (int)PathSort.Lup,
            "01100" => (int)PathSort.Lright,
            "01010" => (int)PathSort.Ldown,
            "10010" => (int)PathSort.Lleft,
            "11000" => (int)PathSort.Iup,
            "00110" => (int)PathSort.Idown,
            "10101" => (int)PathSort.CurveLup,
            "01101" => (int)PathSort.CurveLright,
            "01011" => (int)PathSort.CurveLdown,
            "10011" => (int)PathSort.CurveLleft,
        };
    }
}*/
