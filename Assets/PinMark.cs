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

public class PinMark : MonoBehaviour
{
    public bool callP;
    public int evntP;
    public bool rcv;
    public bool rcv2;
    int[] pathX;
    int[] pathY;
    int[] GStt;

    public GameObject[] pins;

    void Start()
    {
        callP = false;
        evntP = 0;
        rcv = false;
        rcv2 = true;
    }

    // Update is called once per frame
    void Update()
    {
        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>(); //Map ��ü�� MapEvent ��ũ��Ʈ�� mapMove�� �̸����� ��������
        int evnt0 = mapMove.eventTime[0]; //MapEvent(mapMove)�� eventTime �迭 �� 0��°�� �ο�
        int evnt1 = mapMove.eventTime[1];
        bool go = mapMove.go;

        if (evnt0 == 2 || evnt0 == 3 || evnt0 == 6 || evnt0 == 9) { rcv = false; rcv2 = true; callP = false; } //default
        if (evnt0 == 2 || evnt0 == 5 && rcv2) { rcv = true; }

        //��, ��, ��ũ ����
        if (evnt0 == 2 && !callP && go && rcv) 
        {
            PathMake pathMake = GameObject.Find("Map").GetComponent<PathMake>();
            pathX = pathMake.pathX;
            pathY = pathMake.pathY;
            GStt = pathMake.GStt;

            CreatePins();
            CreateMark();

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
            } //��, ��ũ ����
            Transform gridLines = GameObject.Find("GridLines").transform;
            foreach (Transform childTransform in gridLines)
            {
                GameObject.Destroy(childTransform.gameObject);
            } // �� ����

            switch (evnt1)
            {
                case 0: evntP = 2; break;
                case 1: evntP = 2; break;
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
        RouteMake routeMake = GameObject.Find("Map").GetComponent<RouteMake>();
        int[,] pinState = routeMake.pinState;
        int[,] pathDir = routeMake.pathDir;

        Transform gridPins = GameObject.Find("GridPins").transform;
        GameObject[,] position = new GameObject[5, 5];
        pins = new GameObject[26];
        GameObject pPrefab = Resources.Load<GameObject>("Prefabs/Pin");
        Sprite[] pinSprites = Resources.LoadAll<Sprite>("Prefabs/Pins");
        Sprite[] TileSprites = Resources.LoadAll<Sprite>("Prefabs/Maptiles");

        Transform gridLines = GameObject.Find("GridLines").transform;
        GameObject[] lines = new GameObject[pathX.Length];
        GameObject sPrefab = Resources.Load<GameObject>("Prefabs/Line");

        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        int evnt0 = mapMove.eventTime[0];
        int evnt1 = mapMove.eventTime[1];

        int x;
        int y;
        for (int i = 1; i <= 25; i++) //Pin ����
        {
            x = (i - 1) % 5;
            y = (i - 1) / 5;
            pins[i] = Instantiate(pPrefab, new Vector3(x * 1.25f, y * 1.25f, -1), Quaternion.identity); //�� ����   

            pins[i].transform.SetParent(gridPins, false); //���� Grid_Pins�� �ڽ����� ����
            pins[i].name = "Pin " + x + "," + y; ; ; //�� �̸� ����
            position[x, y] = pins[i]; //�� ��ġ ����

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
                    case 1: pinImage.sprite = pinSprites[1]; break;
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
                    case 1: pinImage.sprite = pinSprites[6]; break;
                    case 2: GImage(2); break;
                    case 3: GImage(3); break;
                    case 4: pinImage.sprite = pinSprites[GStt[2] == 0 ? 15 : 0]; break;
                    case 5: pinImage.sprite = pinSprites[GStt[2] == 1 ? 15 : 0]; break;
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

            lines[i] = Instantiate(sPrefab,
                new Vector3(Average(pinTrans.position.x, prePinTrans.position.x), Average(pinTrans.position.y, prePinTrans.position.y), -1),
                Quaternion.identity); //�� ����, ��ġ ����
            lines[i].transform.SetParent(gridLines, true); //�� �θ� ����
            lines[i].name = "Line " + i; //�� �̸� ����

            SpriteRenderer lineImage = lines[i].GetComponent<SpriteRenderer>(); //�� ���� ����
            lineImage.sprite = TileSprites[12];

            Transform lineDir = lines[i].GetComponent<Transform>(); //�� ���� ����
            lineDir.Rotate(0, 0, (pathX[i] == pathX[i - 1]) ? 90 : 0);

            float Average(float a, float b)
            {
                return (a + b) / 2;
            }

            int pathPin(int i)
            {
                return pathY[i] * 5 + pathX[i] + 1;
            }
        }
    }
    void CreateMark() //�÷��̾� ��ġǥ��
    {
        RouteMake routeMake = GameObject.Find("Map").GetComponent<RouteMake>();
        int[,] pathDir = routeMake.pathDir;

        LoopBuildings loopBuildings = GameObject.Find("BackGround").GetComponent<LoopBuildings>();
        Transform playerPos = GameObject.Find("Player").transform;
        int[] route = loopBuildings.route;
        int[] path = loopBuildings.path;
        int[] routeDir = loopBuildings.routeDir;
        float[] left = loopBuildings.leftIns;
        float[] right = loopBuildings.rightIns;

        float Pos = playerPos.position.x; //�÷��̾ �̵��� �Ÿ�
        float mSum = 0;
        float[] routePoint = new float[path.Length]; //��� �� ����Ʈ �Ÿ�
        int[] pathPoint = new int[path.Length]; //��� �� ����Ʈ
        float markLc = 0; //�ֱ� ����Ʈ �Ÿ�
        float bound = 0; //���
        int p = 0; //����Ʈ
        float markX = 0; //��ũ x��ǥ
        float markY = 0; //��ũ y��ǥ

        //����Ʈ ����
        int t = 0;
        for (int i = 0; i < route.Length; i++)
        {
            if (i == 0) //��� ����
            {
                routePoint[t] = 0;
                mSum += right[route[i]]; pathPoint[t] = route[i];
                t++;
            }
            else
            {
                switch (route[i])
                {
                    case 0: case 1: case 2:
                        mSum += left[route[i]];
                        routePoint[t] = mSum; pathPoint[t] = route[i];
                        mSum += right[route[i]];
                        t++; break;
                    case 4:
                        mSum += left[route[i]] + right[route[i]];
                        routePoint[t] = mSum; pathPoint[t] = route[i];
                        t++; break;
                    case 6:
                        mSum += left[route[i]] + right[route[i]] - 1.9f;
                        routePoint[t] = mSum; mSum += 1.9f; pathPoint[t] = route[i];
                        t++; break;
                    case 3:
                        mSum += left[route[i]] + right[route[i]];
                        break;
                    case 5:
                        mSum += 1.9f;
                        routePoint[--t] = mSum; pathPoint[t] = route[i];
                        mSum += left[route[i]] + right[route[i]] - 1.9f;
                        t++;  break;
                    case 7:
                        mSum += left[route[i]];
                        routePoint[--t] = mSum; pathPoint[t] = route[i];
                        mSum += right[route[i]];
                        t++; break;
                    case 9: case 11:
                        mSum += left[route[i]] + right[route[i]];
                        routePoint[t] = mSum; pathPoint[t] = route[i];
                        t++; break;
                    default:
                        break;
                }
            }

            if (t == path.Length)
            {
                break;
            }
        }

        //�ֱ� ����Ʈ Ư��
        for (int i = 0; i < path.Length; i++)
        {
            if (i == 0 && Pos < routePoint[i])
            {
                markLc = routePoint[i];
                markX = pathX[i] * 1.25f;
                markY = pathY[i] * 1.25f;
                p = i;
                break;
            }
            else if (Pos < routePoint[i])
            {
                markLc = routePoint[i - 1];
                markX = pathX[i - 1] * 1.25f;
                markY = pathY[i - 1] * 1.25f;
                p = i - 1;
                break;
            }
            else if (i == path.Length - 1 && Pos >= routePoint[i])
            {
                markLc = routePoint[i];
                markX = pathX[i] * 1.25f;
                markY = pathY[i] * 1.25f;
                p = i;
                break;
            }
        }

        //��� Ư��
        switch (pathPoint[p])
        {
            case 0: case 1: case 2:
                bound = markLc + right[pathPoint[p]];
                break;
            case 4: case 10:
                bound = markLc + left[3] + right[3];
                break;
            case 5:
                bound = markLc + left[5] + right[5] - 1.9f;
                break;
            case 6:
                bound = markLc + left[3] + right[3] + 1.9f;
                break;
            case 7:
                bound = markLc + right[7] + left[3] + right[3];
                break;
            case 12:
                bound = markLc + right[pathPoint[p]];
                break;
            default:
                bound = markLc;
                break;
        }

        //����ġ Ư��
        float lB = Pos - markLc; //���� �Ÿ�
        float rB = Pos - bound; //���� �Ÿ�
        float ww = bound - markLc; //��� �Ÿ�
        if (Pos <= routePoint[routePoint.Length - 1] && Pos > 0)
        {
            if (rB < 0 && (pathPoint[p] != 10 || pathPoint[p] != 12))
            {
                MoveMark(ww, routeDir[p]);
            }
            else
            {
                MoveMark(routePoint[p + 1] - bound, routeDir[p]);
            }
        }

        void MoveMark(float l, int d)
        {
            if (rB < 0)
            {
                switch (d) //�̵�
                {
                    case 0: { markY += lB / l * 0.625f; break; }
                    case 1: { markY -= lB / l * 0.625f; break; }
                    case 2: { markX += lB / l * 0.625f; break; }
                    case 3: { markX -= lB / l * 0.625f; break; }
                }
            }
            else
            {
                switch (d) //�̵�
                {
                    case 0: { markY += 0.625f + (rB / l * 0.625f); break; }
                    case 1: { markY -= 0.625f + (rB / l * 0.625f); break; }
                    case 2: { markX += 0.625f + (rB / l * 0.625f); break; }
                    case 3: { markX -= 0.625f + (rB / l * 0.625f); break; }
                }
            }
        }

        //������Ʈ ����
        GameObject mPrefab = Resources.Load<GameObject>("Prefabs/Mark");
        GameObject playerMark = Instantiate(mPrefab, new Vector3(markX, markY, -2), Quaternion.identity);
        Transform gridPins = GameObject.Find("GridPins").transform; //�θ� ������Ʈ Transform
        playerMark.transform.SetParent(gridPins, false);
        playerMark.name = "PlayerMark";

        //�̹���, ���� �ο�
        SpriteRenderer markImage = playerMark.GetComponent<SpriteRenderer>();
        Sprite[] pinImage = Resources.LoadAll<Sprite>("Prefabs/Pins");
        markImage.sprite = pinImage[12];
        Transform markTrans = playerMark.GetComponent<Transform>();
        markTrans.Rotate(0, 0, TransDir(pathDir[pathX[p], pathY[p]]));

        playerMark.AddComponent<MarkAct>();
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