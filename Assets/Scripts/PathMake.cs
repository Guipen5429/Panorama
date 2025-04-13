using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

public class PathMake : MonoBehaviour
{
    public GameObject Map;
    MapEvent mapEvent;
    PinMake pinMake;
    MarkMake markMake;
    RouteMake routeMake;

    public int GStt2;
    public int[] pathX;
    public int[] pathY;
    public bool pinCall;
    List<int> pathXList;
    List<int> pathYList;
    int[] pathLc;

    public bool call2;

    // Start is called before the first frame update
    void Start()
    {
        pathX = new int[] { 0, 1, 2, 3, 4 };
        pathY = new int[] { 0, 0, 0, 0, 0 };
        GStt2 = 0;

        mapEvent = Map.GetComponent<MapEvent>();
        routeMake = Map.GetComponent<RouteMake>();
        markMake = Map.GetComponent<MarkMake>(); ;
        pinMake = Map.GetComponent<PinMake>();
    }

    // Update is called once per frame
    void Update()
    {
        int evnt0 = mapEvent.eventTime[0];
        int evnt1 = mapEvent.eventTime[1];

        if (evnt0 == 4) { call2 = false; }
        if (evnt0 == 7) { pinCall = false; }
        if (evnt0 == 6)
        {
            pathLc = new int[pathX.Length];
            for (int i = 0; i < pathX.Length - 1; i++)
            {
                pathLc[i] = (5 * pathY[i]) + pathX[i] + 1;
            }
            call2 = false;
            GameObject[] pins = pinMake.pins;
            bool[] callPin = new bool[26];
            for (int i = 1; i < 26; i++)
            {
                callPin[i] = pins[i].GetComponent<PinAct>().callPin;
                if (callPin[i])
                {

                    pathXList = new List<int>(pathX);
                    pathYList = new List<int>(pathY);
                    CheckState((i - 1) % 5, (i - 1) / 5);
                    pathX = pathXList.ToArray();
                    pathY = pathYList.ToArray();
                    break;
                }
            }
        }

        if (evnt0 == 3) { GStt2 = 0; }
    }

    public void CheckState(int x, int y)
    {
        int[,] pinState = routeMake.pinState;
        int p = markMake.p;

        switch (pinState[x, y])
        {
            case 1: case 7: Ctrc(Array.IndexOf(pathLc, 5 * y + x + 1)); pinCall = true; break;
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
}
