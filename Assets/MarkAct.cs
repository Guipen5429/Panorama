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
        LoopBuildings loopie = GameObject.Find("BackGround").GetComponent<LoopBuildings>();
        bool buildTime = loopie.buildTime;
        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        bool pinTime = mapMove.eventTime[1];
        bool routeTime = mapMove.eventTime[2];

        if (pinTime)
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
            Array.Reverse(x);
            Array.Reverse(y);
            modifyDone = false;
        }
    }
}
