using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkAct : MonoBehaviour
{
    public int[] x;
    public int[] y;
    public bool modifyDone = true;

    private void Start()
    {
        PinMark pinMark = GameObject.Find("Map").GetComponent<PinMark>();
        x = (int[])pinMark.pathX.Clone();
        y = (int[])pinMark.pathY.Clone();
    }

    void Update()
    {
        PinMark pinMark = GameObject.Find("Map").GetComponent<PinMark>();
        bool w = pinMark.w;
        LoopBuildings loopie = GameObject.Find("BackGround").GetComponent<LoopBuildings>();
        bool buildTime = loopie.buildTime;
        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        bool pinTime = mapMove.eventTime[1];
        bool routeTime = mapMove.eventTime[2];

        if (w)
        { modifyDone = true; }

        if (!routeTime)
        {
            x = (int[])pinMark.pathX.Clone();
            y = (int[])pinMark.pathY.Clone();
        }
    }

    private void OnMouseDown()
    {
        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        bool mapTime = mapMove.eventTime[0];
        bool pinTime = mapMove.eventTime[1];
        bool routeTime = mapMove.eventTime[2];
        bool modifyTime = mapMove.eventTime[3];

        if (routeTime)
        {
            int[] tempx = new int[x.Length];
            int[] tempy = new int[y.Length];

            for (int i = 0; i < x.Length; i++)
            { tempx[x.Length - 1 - i] = x[i]; }
            for (int i = 0; i < y.Length; i++)
            { tempy[y.Length - 1 - i] = y[i]; }

            x = tempx;
            y = tempy;

            modifyDone = false;
        }
    }
}
