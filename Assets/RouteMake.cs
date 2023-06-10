using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RouteMake : MonoBehaviour
{
    public bool routeDone = true;
    SpriteRenderer pinImage;
    GameObject veil;

    void Start()
    {
        pinImage = GetComponent<SpriteRenderer>();
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
        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        bool mapTime = mapMove.eventTime[0];
        bool pinTime = mapMove.eventTime[1];
        bool routeTime = mapMove.eventTime[2];

        if (Input.GetKeyDown(KeyCode.A))
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
