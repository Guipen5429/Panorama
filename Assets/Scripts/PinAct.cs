using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Net.NetworkInformation;
using UnityEditor.PackageManager;
using UnityEngine;

public class PinAct : MonoBehaviour
{
    public GameObject Map;
    MapEvent mapEvent;
    PinMake pinMake;
    RouteMake routeMake;
    public GameObject BackGround;
    LoopBuildings loopBuildings;

    public int index;
    public bool callPin;

    // Start is called before the first frame update
    void Awake()
    {
        Map = GameObject.Find("Map");
        mapEvent = Map.GetComponent<MapEvent>();
        routeMake = Map.GetComponent<RouteMake>();
        pinMake = Map.GetComponent<PinMake>();
        BackGround = GameObject.Find("BackGround");
        loopBuildings = Map.GetComponent<LoopBuildings>();

        GameObject[] pins = pinMake.pins;

        index = Array.IndexOf(pins, gameObject);
        callPin = false;
    }

    private void Update()
    {
        int evnt0 = mapEvent.eventTime[0];
        if (evnt0 == 7) { callPin = false; }
    }

    private void OnMouseDown()
    {
        int evnt0 = mapEvent.eventTime[0];
        bool go = mapEvent.go;
        BackGround = GameObject.Find("BackGround");
        loopBuildings = BackGround.GetComponent<LoopBuildings>();
        int mapL = loopBuildings.mapL;

        int[,] pinState = routeMake.pinState;

        if (evnt0 == 6 && !callPin && go)
        {
            if (pinState[(index - 1) % mapL, (index - 1) / mapL] != 0)
            {
                callPin = true;
            }
        }
    }
}
