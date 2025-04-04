using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class PathMake : MonoBehaviour
{
    public int[] GStt;
    public int[] pathX;
    public int[] pathY;
    public bool pinCall;
    List<int> pathXList;
    List<int> pathYList;

    public 

    // Start is called before the first frame update
    void Start()
    {
        pathX = new int[] { 0, 1, 2, 3, 4 };
        pathY = new int[] { 4, 4, 4, 4, 4 };
        GStt = new int[3] {0, 0, 0};
    }

    // Update is called once per frame
    void Update()
    {
        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        int evnt0 = mapMove.eventTime[0];
        int evnt1 = mapMove.eventTime[1];
        
        if (evnt0 == 7) { pinCall = false; }
        if (evnt0 == 6)
        {
            PinMark pinMark = GameObject.Find("Map").GetComponent<PinMark>();
            GameObject[] pins = pinMark.pins;
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

        if (evnt0 == 3) { GStt[2] = 0; }
    }

    public void CheckState(int x, int y)
    {
        RouteMake routeMake = GameObject.Find("Map").GetComponent<RouteMake>();
        int[,] pinState = routeMake.pinState;

        switch (pinState[x, y])
        {
            case 1: Ctrc(); pinCall = true; break;
            case 2: if (GStt[2] == 1) { GStt[2] = 0; } pinCall = true; break;
            case 3: if (GStt[2] == 0) { GStt[2] = 1; } pinCall = true; break;
            case 4: if (GStt[2] == 0) { Expn(x, y, 0); } break;
            case 5: if (GStt[2] == 1) { Expn(x, y, pathX.Length - 1); } break;
        }
    }

    void Expn(int x, int y, int t)
    {
        int lc = 5 * y + x + 1;
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

    void Ctrc()
    {

    }
}
