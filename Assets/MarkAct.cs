using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkAct : MonoBehaviour
{
    public int[] x;
    public int[] y;
    public bool modifyDone;

    private void Start()
    {

    }

    void Update()
    {
        /*MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        int evnt0 = mapMove.eventTime[0];
        if (!mapTime)
        {
            modifyDone = true;
        }*/
    }

    private void OnMouseDown()
    {
        /*MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        bool mapTime = mapMove.eventTime[0];
        bool pinTime = mapMove.eventTime[1];
        bool routeTime = mapMove.eventTime[2];
        bool modifyTime = mapMove.eventTime[3];
        PinMark pinMark = GameObject.Find("Map").GetComponent<PinMark>();

        if (routeTime)
        {
            x = (int[])pinMark.pathX;
            y = (int[])pinMark.pathY;

            for (int i = 0; i < x.Length; i++)
            { tempx[x.Length - 1 - i] = x[i]; }
            for (int i = 0; i < y.Length; i++)
            { tempy[y.Length - 1 - i] = y[i]; }

            Array.Reverse(x);
            Array.Reverse(y);

            modifyDone = false;
        }*/
    }
}
