using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

    public int GStt2;
    public int[] pathX;
    public int[] pathY;
    public int[] bPathX;
    public int[] bPathY;
    List<int> pathXList;
    List<int> pathYList;
    int[] pathLc;
    int mapL;

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

        mapL = loopBuildings.mapL;

        pathX = new int[] { 1, 2, 3 };
        pathY = new int[] { 1, 1, 1 };
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
        if (evnt0 == 6 || evnt0 == 8 && rcv2) { rcv = true; }

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
            //PathExpn();
            switch (evnt1)
            {
                case 0: case 1: evntPin = 7; break;
                case 2: evntPin = 9; break;
            }
            pinCall = true;
            rcv = false;
            rcv2 = false;
        }

        if (evnt0 == 3) { GStt2 = 0; }
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

    void PathExpn()
    {
        List<int> bPathXList = new List<int>(pathX);
        List<int> bPathYList = new List<int>(pathY);
        bPathX = pathX;
        bPathY = pathY;
        pi = 0;

        if (bPathX[0] == bPathX[1])
        {
            if (bPathY[0] < bPathY[1])
            {
                for (int i = bPathY[0] - 1; i >= 0; i--)
                {
                    bPathXList.Insert(0, bPathX[0]);
                    bPathYList.Insert(0, i);
                    pi++;
                }
            }
            else
            {
                for (int i = bPathY[0] + 1; i < mapL; i++)
                {
                    bPathXList.Insert(0, bPathX[0]);
                    bPathYList.Insert(0, i);
                    pi++;
                }
            }
        }
        else
        {
            if (bPathX[0] < bPathX[1])
            {
                for (int i = bPathX[0] - 1; i >= 0; --i)
                {
                    bPathXList.Insert(0, i);
                    bPathYList.Insert(0, bPathY[0]);
                    pi++;
                }
            }
            else
            {
                for (int i = bPathX[0] + 1; i < mapL; ++i)
                {
                    bPathXList.Insert(0, i);
                    bPathYList.Insert(0, bPathY[0]);
                    pi++;
                }
            }
        }
        if (bPathX[^1] == bPathX[^2])
        {
            if (bPathY[^1] < bPathY[^2])
            {
                for (int i = bPathY[^1] - 1; i >= 0; i--)
                {
                    bPathXList.Add(bPathX[^1]);
                    bPathYList.Add(i);
                }
            }
            else
            {
                for (int i = bPathY[^1] + 1; i < mapL; i++)
                {
                    bPathXList.Add(bPathX[^1]);
                    bPathYList.Add(i);
                }
            }
        }
        else
        {
            if (bPathX[^1] < bPathX[^2])
            {
                for (int i = bPathX[^1] - 1; i >= 0; --i)
                {
                    bPathXList.Add(i);
                    bPathYList.Add(bPathY[^1]);
                }
            }
            else
            {
                for (int i = bPathX[^1] + 1; i < mapL; ++i)
                {
                    bPathXList.Add(i);
                    bPathYList.Add(bPathY[^1]);
                }
            }
        }
        bPathX = bPathXList.ToArray();
        bPathY = bPathYList.ToArray();
    }
}
