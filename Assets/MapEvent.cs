using System.Collections;
using System.Collections.Generic;
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
    public bool[] eventTime;
    public bool o;
    public bool b;

    void Start()
    {
        mapBase = transform.Find("MapBase").gameObject;
        mapBase.SetActive(false);
        eventTime = new bool[]{ false, false, false, false };
        o = false;
        b = false;
    }

    void Update()
    {
        //카메라를 추적
        transform.position = new Vector3(track.position.x, 0, -1);

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            mapBase.SetActive(!mapBase.activeSelf);
            eventTime[0] = (mapBase.activeSelf);
            eventTime[1] = (mapBase.activeSelf);
            eventTime[2] = (mapBase.activeSelf);
        }

        if (eventTime[0])
        {
            PinMark pinMark = GameObject.Find("Map").GetComponent<PinMark>();
            bool pinDone = pinMark.pinsDone;
            bool w = pinMark.w;
            RouteMake routeMake = GameObject.Find("PathMake").GetComponent<RouteMake>();
            bool routeDone = routeMake.routeDone;

            eventTime[2] = !routeDone;
            if (o != routeDone) { b = true; o = routeDone; }
            if (b) { eventTime[1] = true; b = false; }

            if (w && pinDone && eventTime[1]) { eventTime[1] = false;}

            if (eventTime[2])
            {
                try
                {
                    MarkAct markAct = GameObject.Find("PlayerMark").GetComponent<MarkAct>();
                    bool modifyDone = markAct.modifyDone;
                    if (!modifyDone)
                    {
                        eventTime[3] = true;
                        o = !o;
                        eventTime[3] = false;
                    }
                }
                catch { }
            }
        }
    }
}
