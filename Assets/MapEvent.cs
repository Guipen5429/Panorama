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
    public Transform track; //Player(Tranform)
    public float distance;
    GameObject mapBase;
    public int[] eventTime;
    public bool rcv;
    public bool rcv2;
    public bool go;

    void Start()
    {
        mapBase = transform.Find("MapBase").gameObject;
        mapBase.SetActive(false);
        eventTime = new int[]{ 7, 0, 0 };
    }

    void Update()
    {
        PinMark pin = GameObject.Find("Map").GetComponent<PinMark>();
        bool pCall = pin.callP;
        int evntP = pin.evntP;
        RouteMake route = GameObject.Find("Map").GetComponent<RouteMake>();
        bool rCall = route.callR;
        LoopBuildings build = GameObject.Find("BackGround").GetComponent<LoopBuildings>();
        bool bCall = build.callB;

        //카메라를 추적
        transform.position = new Vector3(track.position.x, 0, -1);

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (eventTime[0] == 0) { eventTime[0] = 2; mapBase.SetActive(true); }
            else { eventTime[0] = 8; eventTime[1] = 5; eventTime[1] = 2; mapBase.SetActive(false); }
        }

        if (!pCall && !rCall && !bCall) { go = true; rcv = false; rcv2 = true; } //default
        if (pCall || rCall || bCall && rcv2) { rcv = true; go = false; }

        if (rCall && rcv)
        {
            eventTime[0] = 4;
            rcv = false;
            rcv2 = false;
        }

        if (pCall && rcv)
        {
            switch (evntP)
            {
                case 2: eventTime[0] = 2; break;
                case 3: eventTime[0] = 3; eventTime[1] = 1; break;
                case 5: eventTime[0] = 5; break;
                case 6: eventTime[0] = 6; break;
                case 9: eventTime[0] = 9; break;
            }
            rcv = false;
            rcv2 = false;
        }

        if (bCall && rcv)
        {
            eventTime[0] = 0;
            eventTime[1] = 0;
            eventTime[2] = 0;
            rcv = false;
            rcv2 = false;
        }
    }
}
