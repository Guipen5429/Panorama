using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Net.NetworkInformation;
using UnityEditor.PackageManager;
using UnityEngine;

public class PinAct : MonoBehaviour
{
    public int index;
    public bool callPin;

    // Start is called before the first frame update
    void Awake()
    {
        PinMark pinMark = GameObject.Find("Map").GetComponent<PinMark>();
        GameObject[] pins = pinMark.pins;

        index = Array.IndexOf(pins, gameObject);
        callPin = false;
    }

    private void Update()
    {
        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        int evnt0 = mapMove.eventTime[0];
        if (evnt0 == 7) { callPin = false; }
    }

    private void OnMouseDown()
    {
        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        int evnt0 = mapMove.eventTime[0];
        bool go = mapMove.go;

        RouteMake routeMake = GameObject.Find("Map").GetComponent<RouteMake>();
        int[,] pinState = routeMake.pinState;


        if (evnt0 == 6 && !callPin && go)
        {
            if (pinState[(index - 1) % 5, (index - 1) / 5] != 0)
            {
                callPin = true;
            }
        }

        Debug.Log(pinState[(index - 1) % 5, (index - 1) / 5]);
    }
}
