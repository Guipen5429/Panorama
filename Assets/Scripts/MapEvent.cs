using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class MapEvent : MonoBehaviour
{
    public GameObject Map;
    PinMake pin;
    MarkMake mark;
    PathMake path;
    RouteMake route;
    public GameObject BackGround;
    LoopBuildings build;

    public Transform track; //Player(Tranform)
    public float distance;
    GameObject mapBase;
    public int[] eventTime;
    public bool rcv;
    public bool rcv2;
    public bool go;

    void Start()
    {
        path = Map.GetComponent<PathMake>();
        route = Map.GetComponent<RouteMake>();
        mark = Map.GetComponent<MarkMake>();
        pin = Map.GetComponent<PinMake>();
        build = BackGround.GetComponent<LoopBuildings>();

        mapBase = transform.Find("MapBase").gameObject;
        mapBase.SetActive(false);
        eventTime = new int[]{ 7, 0, 1 };
    }

    void Update()
    {
        bool pCall = pin.callP; int evntP = pin.evntP;
        bool mCall = mark.callM; int evntM = mark.evntM;
        bool rCall = route.callR; int evntR = route.evntR;
        bool bCall = build.callB; int evntB = build.evntB;
        bool pinCall = path.pinCall;

        //카메라를 추적
        transform.position = new Vector3(track.position.x, 0, -1);

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (eventTime[0] == 0) { eventTime[0] = 1; mapBase.SetActive(true); }
            else { eventTime[0] = 5; eventTime[1] = 2; }
        }

        if (!pCall && !mCall && !rCall && !bCall && !pinCall) { go = true; rcv = false; rcv2 = true; } //default
        if (pCall || mCall || rCall || bCall || pinCall && rcv2) { rcv = true; go = false; }

        if (rCall && rcv)
        {
            switch (evntR)
            {
                case 2: eventTime[0] = 2; break;
                case 3: eventTime[0] = 5; break;
                case 6: eventTime[0] = 5; eventTime[1] = 0; break;
            }
            rcv = false;
            rcv2 = false;
        }

        if (pCall && rcv)
        {
            switch (evntP)
            {
                case 1: eventTime[0] = 1; break;
                case 3: eventTime[0] = 3; eventTime[1] = 1; break;
                case 6: eventTime[0] = 6; break;
                case 8: mapBase.SetActive(false); eventTime[0] = 9; break;
            }
            rcv = false;
            rcv2 = false;
        }

        if (mCall && rcv)
        {
            switch (evntM)
            {
                case 4: eventTime[0] = 4; break;
            }
            rcv = false;
            rcv2 = false;
        }

        if (bCall && rcv)
        {
            switch (evntB)
            {
                case 0: eventTime[0] = 0; eventTime[1] = 0; eventTime[2] = 0; break;
                case 5: eventTime[0] = 5; break;
                case 9: eventTime[0] = 9; break;
            }
            rcv = false;
            rcv2 = false;
        }

        if (pinCall & rcv)
        {
            //Debug.Log("핀 클릭");

            eventTime[0] = 7; eventTime[2] = 1;
            rcv = false;
            rcv2 = false;
        }
    }
}
