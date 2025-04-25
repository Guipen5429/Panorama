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
    StateMake stateMake;
    public GameObject BackGround;
    LoopBuildings loopBuildings;
    public GameObject Player;
    CharacterMove characterMove;

    public int[] pathX;
    public int[] pathY;
    public int[] bPathX;
    public int[] bPathY;
    List<int> pathXList;
    List<int> pathYList;
    int mapL;
    float leftBound; //���� ���� ���
    float rightBound; //���� ���� ���

    public int pi;
    public int pt;

    public bool pathCall;
    public int evntPath;
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
        stateMake = Map.GetComponent<StateMake>();
        loopBuildings = BackGround.GetComponent<LoopBuildings>();
        characterMove = Player.GetComponent<CharacterMove>();

        mapL = loopBuildings.mapL;

        pathX = new int[] { 0, 1, 2 };
        pathY = new int[] { 0, 0, 0 };

        pathCall = false;
        evntPath = 0;
        rcv = false;
        rcv2 = true;
    }

    // Update is called once per frame
    void Update()
    {
        int evnt0 = mapEvent.eventTime[0];
        int evnt1 = mapEvent.eventTime[1];
        bool go = mapEvent.go;
        if (evnt0 == 7 || evnt0 == 8 || evnt0 == 9) { rcv = false; rcv2 = true; pathCall = false; } //default
        if (evnt0 == 0 || evnt0 == 8 && rcv2) { rcv = true; }

        if (evnt0 == 8 && !pathCall && go && rcv)
        {
            if (evnt1 == 1)
            {
                pathX = stateMake.pathX;
                pathY = stateMake.pathY;
            }
            BuildPathExpn();
            /*pi = 0;
            bPathX = pathX;
            bPathY = pathY;*/
            switch (evnt1)
            {
                case 0: case 1: /*BuildPathExpn();*/ evntPath = 7; break;
                case 2: evntPath = 9; break;
                case 3: evntPath = 7; break;
            }
            pathCall = true;
            rcv = false;
            rcv2 = false;
        }
        if (evnt0 == 9) { BoundMake(); }
        if (evnt0 == 0 && !pathCall && go && rcv) { PathExpn(); }
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
                PsthExpn(0, true); //��
            }
            else
            {
                PsthExpn(1, true); //��
            }
        }
        else
        {
            if (bPathX[0] > bPathX[1])
            {
                PsthExpn(2, true); //��
            }
            else
            {
                PsthExpn(3, true); //��
            }
        }
        if (bPathX[^1] == bPathX[^2])
        {
            if (bPathY[^1] > bPathY[^2])
            {
                PsthExpn(0, false); //��
            }
            else
            {
                PsthExpn(1, false); //��
            }
        }
        else
        {
            if (bPathX[^1] > bPathX[^2])
            {
                PsthExpn(2, false); //��
            }
            else
            {
                PsthExpn(3, false); //��
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
        leftBound = pi == 0 ? 0 : bRoutePoint[pi - 1] + BoundMake(pathPoint[pi - 1]); //���� ���� ���
        rightBound = bRoutePoint[pi + pathX.Length - 1] + BoundMake(pathPoint[pi + pathX.Length - 1]); //���� ���� ���
        //Debug.Log("���� ���� ���: " + leftBound);
        //Debug.Log("���� ���� ���: " + rightBound);

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
        float[] bRoutePoint = loopBuildings.routePoint;
        float pX = Player.transform.position.x;
        pathXList = new List<int>(pathX);
        pathYList = new List<int>(pathY);

        if (pX < leftBound && pi != 0)
        {
            pathXList.Insert(0, bPathX[pi - 1]);
            pathYList.Insert(0, bPathY[pi - 1]);
            Debug.Log("�·� ����");
            evntPath = 2;
            pathCall = true;
            rcv = false;
            rcv2 = false;
        }
        else if (pX > rightBound && rightBound != bRoutePoint[^1])
        {
            pathXList.Add(bPathX[pi + pathX.Length]);
            pathYList.Add(bPathY[pi + pathY.Length]);
            Debug.Log("��� ����");
            evntPath = 2;
            pathCall = true;
            rcv = false;
            rcv2 = false;
        }
        pathX = pathXList.ToArray();
        pathY = pathYList.ToArray();
    }
}