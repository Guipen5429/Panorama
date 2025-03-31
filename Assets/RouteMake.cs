using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RouteMake : MonoBehaviour
{
    public bool routeDone = true;
    SpriteRenderer pinImage;
    GameObject veil;

    public bool callR;
    public bool rcv;
    public bool rcv2;

    void Start()
    {
        pinImage = GetComponent<SpriteRenderer>();
        callR = false;
        rcv = false;
        rcv2 = true;
    }

    private void OnDisable()
    {
        routeDone = true;
        Sprite[] pinSprites1 = Resources.LoadAll<Sprite>("Prefabs/Pins");
        pinImage.sprite = pinSprites1[3];
    }

    private void OnMouseDown()
    {
        Sprite[] pinSprites1 = Resources.LoadAll<Sprite>("Prefabs/Pins");
        Sprite[] pinSprites2 = Resources.LoadAll<Sprite>("Prefabs/Pins2");
        veil = transform.Find("Veil").gameObject;
        SpriteRenderer s = veil.GetComponent<SpriteRenderer>();
        Color c = s.color;

        routeDone = !routeDone;
        if (routeDone) { c.a = 1.0f; s.color = c; Invoke("Veiling", 0.5f);}
        pinImage.sprite = (routeDone) ? pinSprites1[3] : pinSprites2[3];
    }

    void Veiling()
    {
        SpriteRenderer s = veil.GetComponent<SpriteRenderer>();
        Color c = s.color;

        /*c.a = 0f; s.color = c;*/

        while(c.a >= 0)
        {
            c.a -= Time.deltaTime;
            s.color = c;
        }
    }

    private void Update()
    {
        PinMark pin = GameObject.Find("Map").GetComponent<PinMark>();
        bool pCall = pin.callP;
        LoopBuildings build = GameObject.Find("BackGround").GetComponent<LoopBuildings>();
        bool bCall = build.callB;
        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        int evnt0 = mapMove.eventTime[0];
        int evnt1 = mapMove.eventTime[1];
        int evnt2 = mapMove.eventTime[2];
        bool go = mapMove.go;
        if (evnt0 != 7) { rcv = false; rcv2 = true; callR = false; } //default
        if (evnt0 == 7 && rcv2) { rcv = true; }

        if (evnt0 == 7 && !callR && go && rcv)
        {
            callR = true;
            rcv = false;
            rcv2 = false;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Sprite[] pinSprites1 = Resources.LoadAll<Sprite>("Prefabs/Pins");
            Sprite[] pinSprites2 = Resources.LoadAll<Sprite>("Prefabs/Pins2");
            veil = transform.Find("Veil").gameObject;
            SpriteRenderer s = veil.GetComponent<SpriteRenderer>();
            Color c = s.color;

            routeDone = !routeDone;
            if (routeDone) { c.a = 1.0f; s.color = c; Invoke("Veiling", 0.5f); }
            pinImage.sprite = (routeDone) ? pinSprites1[3] : pinSprites2[3];
        }
    }
}
