using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StateMake : MonoBehaviour
{
    public GameObject Map;
    MapEvent mapEvent;
    PinMake pinMake;
    MarkMake markMake;
    RouteMake routeMake;
    PathMake pathMake;
    public GameObject BackGround;
    LoopBuildings loopBuildings;
    public GameObject Player;
    CharacterMove characterMove;

    public int GStt2;
    public int[] pathX;
    public int[] pathY;

    public int pi;
    public int pt;

    List<int> pathXList;
    List<int> pathYList;
    int[] pathLc;
    int mapL;

    bool[] callPin;
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
        pathMake = Map.GetComponent<PathMake>();
        loopBuildings = BackGround.GetComponent<LoopBuildings>();
        characterMove = Player.GetComponent<CharacterMove>();

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
        if (evnt0 == 8) { rcv = false; rcv2 = true; pinCall = false; } //default
        if (evnt0 == 0 || evnt0 == 6 && rcv2) { rcv = true; }

        if (evnt0 == 4)
        {
            call2 = false;
            mapL = loopBuildings.mapL;
            callPin = new bool[mapL * mapL + 1];
            pathLc = new int[pathX.Length];
            for (int i = 0; i < pathX.Length - 1; i++)
            {
                try { pathLc[i] = (mapL * pathY[i]) + pathX[i] + 1; }
                catch { Debug.Log(i); }
            }
        }
        if (evnt0 == 7)
        {
            pathX = pathMake.pathX;
            pathY = pathMake.pathY;
        }
        if (evnt0 == 6)
        {
            for (int i = 1; i < ((mapL * mapL) + 1); i++)
            {
                callPin[i] = pinMake.pins[i].GetComponent<PinAct>().callPin;
                if (Array.IndexOf(callPin, true) != -1 && go && rcv)
                {
                    evntPin = 8;
                    pathXList = new List<int>(pathX);
                    pathYList = new List<int>(pathY);
                    CheckState((Array.IndexOf(callPin, true) - 1) % mapL, (Array.IndexOf(callPin, true) - 1) / mapL);
                    pathX = pathXList.ToArray();
                    pathY = pathYList.ToArray();
                    rcv = false;
                    rcv2 = false;
                }
            }
        }
        if (evnt0 == 3) { GStt2 = 0; }
    }

    public void CheckState(int x, int y)
    {
        int[,] pinState = routeMake.pinState;
        int p = markMake.p;
        //Debug.Log(pinState[x,y]);

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
                        }
                        break;
                    case 1: GStt2 = 0; break;
                }
                pinCall = true; break;
            case 3:
                switch (GStt2)
                {
                    case 0: GStt2 = 1; break;
                    case 1:
                        if (pinState[pathX[^2], pathY[^2]] == 6)
                        {
                            pathXList.RemoveAt(pathXList.Count - 1);
                            pathYList.RemoveAt(pathYList.Count - 1);
                        }
                        break;
                }
                pinCall = true; break;
            case 4: if (GStt2 == 0) { Expn(x, y, 0); call2 = true; pinCall = true; } break;
            case 5: if (GStt2 == 1) { Expn(x, y, pathX.Length - 1); pinCall = true; } break;
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
