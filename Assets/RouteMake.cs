using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class RouteMake : MonoBehaviour
{
    SpriteRenderer pinImage;
    GameObject veil;

    public bool callR;
    public int evntR;
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

        /*routeDone = !routeDone;
        if (routeDone) { c.a = 1.0f; s.color = c; Invoke("Veiling", 0.5f);}
        pinImage.sprite = (routeDone) ? pinSprites1[3] : pinSprites2[3];*/
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
        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        int evnt0 = mapMove.eventTime[0];
        int evnt1 = mapMove.eventTime[1];
        bool go = mapMove.go;
        if (evnt0 == 5) { rcv = false; rcv2 = true; callR = false; } //default
        if (evnt0 != 0 && rcv2) { rcv = true; }

        /*if (evnt0 == 7 && !callR && go && rcv)
        {
            switch (evnt1)
            {
                case 0: evntR = 9; break;
                case 1: evntR = 5; break;
                default: break;
            }
            callR = true;
            rcv = false;
            rcv2 = false;
        }*/

        if (Input.GetKeyDown(KeyCode.R) && (evnt0 == 3 || evnt0 == 6))
        {
            Sprite[] pinSprites1 = Resources.LoadAll<Sprite>("Prefabs/Pins");
            Sprite[] pinSprites2 = Resources.LoadAll<Sprite>("Prefabs/Pins2");
            /*veil = transform.Find("Veil").gameObject;
            SpriteRenderer s = veil.GetComponent<SpriteRenderer>();
            Color c = s.color;

            if (evnt0 == 6) { c.a = 1.0f; s.color = c; Invoke("Veiling", 0.5f); }*/

            //Debug.Log("작동은 함!");

            switch (evnt0)
            {
                case 3: evntR = 3;  break;
                case 6: evntR = 6; break;
                default: break;
            }
            callR = true;
            rcv = false;
            rcv2 = false;
        }
    }
}
