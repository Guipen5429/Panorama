using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public class PinMake : MonoBehaviour
{
    public GameObject Map;
    MapEvent mapEvent;
    PathMake pathMake;
    RouteMake routeMake;
    public GameObject BackGround;
    LoopBuildings loopBuildings;

    public GameObject pPrefab;
    public GameObject lPrefab;

    public bool callP;
    public int evntP;
    bool rcv;
    bool rcv2;

    public GameObject[] pins;
    int[] pathX;
    int[] pathY;
    int mapL;

    void Start()
    {
        mapEvent = Map.GetComponent<MapEvent>();
        pathMake = Map.GetComponent<PathMake>();
        routeMake = Map.GetComponent<RouteMake>();
        loopBuildings = BackGround.GetComponent<LoopBuildings>();

        mapL = loopBuildings.mapL;

        callP = false;
        rcv = false;
        rcv2 = true;
    }
    void Update()
    {
        int evnt0 = mapEvent.eventTime[0];
        int evnt1 = mapEvent.eventTime[1];
        bool go = mapEvent.go;

        if (evnt0 == 1 || evnt0 == 3 || evnt0 == 6 || evnt0 == 8) { rcv = false; rcv2 = true; callP = false; } //default
        if (evnt0 == 2 || evnt0 == 5 && rcv2) { rcv = true; }

        //��, �� ����
        if (evnt0 == 2 && !callP && go && rcv) 
        {
            pathX = pathMake.pathX;
            pathY = pathMake.pathY;

            CreatePins();

            switch (evnt1)
            {
                case 0: evntP = 3; break;
                case 1: evntP = 6; break;
                default: break;
            }
            callP = true;
            rcv = false;
            rcv2 = false;
        }

        //��, ��, ��ũ ����
        if (evnt0 == 5 && !callP && go && rcv)
        {
            Transform gridPins = GameObject.Find("GridPins").transform;
            foreach (Transform childTransform in gridPins)
            {
                GameObject.Destroy(childTransform.gameObject);
            } //�� ����
            Transform gridMark = GameObject.Find("GridMark").transform;
            foreach (Transform childTransform in gridMark)
            {
                GameObject.Destroy(childTransform.gameObject);
            } //��ũ ����
            Transform gridLines = GameObject.Find("GridLines").transform;
            foreach (Transform childTransform in gridLines)
            {
                GameObject.Destroy(childTransform.gameObject);
            } // �� ����

            switch (evnt1)
            {
                case 0: evntP = 1; break;
                case 1: evntP = 1; break;
                case 2: evntP = 8; break;
                default: break;
            }
            callP = true;
            rcv = false;
            rcv2 = false;
        }
    }
    void CreatePins()
    {
        int[,] pinState = routeMake.pinState;
        int[,] pathDir = routeMake.pathDir;
        int[] GStt = routeMake.GStt;

        int evnt1 = mapEvent.eventTime[1];

        Transform gridPins = GameObject.Find("GridPins").transform;
        pins = new GameObject[mapL * mapL + 1];

        Transform gridLines = GameObject.Find("GridLines").transform;
        GameObject[] lines = new GameObject[pathX.Length];

        Sprite[] pinSprites = Resources.LoadAll<Sprite>("Images/Pins");
        Sprite[] TileSprites = Resources.LoadAll<Sprite>("Images/Maptiles");


        int x;
        int y;
        for (int i = 1; i <= (mapL * mapL); i++) //Pin ����
        {
            x = (i - 1) % mapL;
            y = (i - 1) / mapL;
            pins[i] = Instantiate(pPrefab, new Vector3(x * 1.25f, y * 1.25f, -1), Quaternion.identity); //�� ����   

            pins[i].transform.SetParent(gridPins, false); //���� Grid_Pins�� �ڽ����� ����
            pins[i].name = "Pin " + x + "," + y; ; ; //�� �̸� ����

            SpriteRenderer pinImage = pins[i].GetComponent<SpriteRenderer>(); //�̹��� �ҷ�����
            Transform pinDir = pins[i].GetComponent<Transform>();
            pinDir.Rotate(0, 0, TransDir(pathDir[x, y])); //�� ���� ����

            pins[i].AddComponent<PinAct>();

            //�� �̹���
            if (evnt1 == 0)
            {
                switch (pinState[x, y])
                {
                    case 0: pinImage.sprite = pinSprites[0]; break;
                    case 1: case 6: case 7: pinImage.sprite = pinSprites[1]; break;
                    case 2: case 3: pinImage.sprite = pinSprites[3]; break;
                    case 4: case 5: pinImage.sprite = pinSprites[0]; break;
                    default: break;
                }
            }
            else if (evnt1 == 1)
            {
                switch (pinState[x, y])
                {
                    case 0: pinImage.sprite = pinSprites[0]; break;
                    case 1: case 7: pinImage.sprite = pinSprites[6]; break;
                    case 2: GImage(2); break;
                    case 3: GImage(3); break;
                    case 4: pinImage.sprite = pinSprites[GStt[2] == 0 ? 15 : 0]; break;
                    case 5: pinImage.sprite = pinSprites[GStt[2] == 1 ? 15 : 0]; break;
                    case 6: pinImage.sprite = pinSprites[12]; break;
                    default: break;
                }
            }

            void GImage(int i)
            {
                int j = (i == 2 ? 0 : 1);
                switch (GStt[j])
                {
                    case 0: pinImage.sprite = pinSprites[GStt[2] == j ? 8 : 3]; break;
                    case 1: pinImage.sprite = pinSprites[GStt[2] == j ? 10 : 13]; break;
                    case 2: pinImage.sprite = pinSprites[GStt[2] == j ? 5 : 13]; break;
                }
            }
        }

        for (int i = 1; i < pathX.Length; i++) //�� ����
        {
            Transform pinTrans = pins[pathPin(i)].GetComponent<Transform>();
            Transform prePinTrans = pins[pathPin(i - 1)].GetComponent<Transform>();

            lines[i] = Instantiate(lPrefab,
                new Vector3(Average(pinTrans.position.x, prePinTrans.position.x), Average(pinTrans.position.y, prePinTrans.position.y), -2),
                Quaternion.identity); //�� ����, ��ġ ����

            lines[i].transform.SetParent(gridLines, true); //�� �θ� ����
            lines[i].name = "Line " + i; //�� �̸� ����

            SpriteRenderer lineImage = lines[i].GetComponent<SpriteRenderer>(); //�� ���� ����
            lineImage.sprite = TileSprites[21]; //�� �̹��� ����

            Transform lineDir = lines[i].GetComponent<Transform>(); //�� ���� ����
            lineDir.Rotate(0, 0, (pathX[i] == pathX[i - 1]) ? 90 : 0);

            float Average(float a, float b)
            {
                return (a + b) / 2;
            }

            int pathPin(int i)
            {
                return pathY[i] * mapL + pathX[i] + 1;
            }
        }
    }
    int TransDir(int i)
    {
        return i switch
        {
            0 => 90,
            1 => 270,
            2 => 0,
            3 => 180,
            _ => 0,
        };
    }
}