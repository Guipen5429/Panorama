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
    public int[,] pinState = new int[5, 5]; //���� ����
    public int[] pathX; //��� x��ǥ
    public int[] pathY; //��� y��ǥ
    public int[,] pathDir = new int[5, 5];
    public bool rcv;
    public bool rcv2;

    public int count;
    int[] fDir;

    public bool callP;
    public int evntP;

    private void Awake()
    {
        pathX = new int[] { 0, 0, 1, 1, 2, 2, 3, 3 };
        pathY = new int[] { 4, 3, 3, 4, 4, 3, 3, 4 };
        callP = false;
        evntP = 0;
        rcv = false;
        rcv2 = true;
    }

    // Start is called before the first frame update
    void Start()
    {

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

        if (evnt0 == 2 && !callP && go && rcv) //�� ����, ��, �� ����
        {
            ChngState();
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

        if (evnt0 == 5 && !callP && go && rcv) //��, ��, ��ũ ����
        {
            Transform gridPins = GameObject.Find("GridPins").transform;
            foreach (Transform childTransform in gridPins)
            {
                GameObject.Destroy(childTransform.gameObject);
            } //�� ����
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
    public void ChngState()
    {
        LoopBuildings loopBuildings = GameObject.Find("BackGround").GetComponent<LoopBuildings>();
        int[] iDir = loopBuildings.cameraDir;

        for (int i = 0; i < 5; i++) //�� ���� �ʱ�ȭ
        {
            for (int j = 0; j < 5; j++)
            {
                pinState[i, j] = 0;
            }
        }

        pinState[pathX[0], pathY[0]] = 2;
        pinState[pathX[^1], pathY[^1]] = 3;

        for (int i = 1; i < pathX.Length - 2; i++)
        {
            pinState[pathX[i], pathY[i]] = 1;
        }

        for (int i = 0; i < pathX.Length; i++) //��� �� ���� �ҷ�����
        {
            if (pinState[pathX[i], pathY[i]] == 0 || pinState[pathX[i], pathY[i]] == 1)
            { pinState[pathX[i], pathY[i]] = 1; }
            pathDir[pathX[i], pathY[i]] = iDir[i + 1];
        }

        count = 0;
        for (int i = 0; i < pathX.Length; i++)
        {
            if (pinState[pathX[i], pathY[i]] == 3 || pinState[pathX[i], pathY[i]] == 4)
            {
                count++;
            }
        }
    }

    public void CreatePins()
    {
        Transform gridPins = GameObject.Find("GridPins").transform;
        GameObject[,] position = new GameObject[5, 5];
        GameObject[] pins = new GameObject[26];
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

            //�� �̹���, ���� ����
            if (evnt1 == 0)
            {
                if (pinState[x, y] != 0)
                {
                    if (pinState[x, y] == 2 || pinState[x, y] == 3)
                    {
                        pinImage.sprite = pinSprites[3];
                    }
                    else { pinImage.sprite = pinSprites[4];}

                    pinDir.Rotate(0, 0, TransDir());
                }
                else { pinImage.sprite = pinSprites[0]; }
            }
            else if (evnt1 == 1)
            {
                if (pinState[x, y] != 0)
                {
                    if (pinState[x, y] == 2 || pinState[x, y] == 3)
                    {
                        pinImage.sprite = pinSprites[8];
                    }
                    else { pinImage.sprite = pinSprites[11]; }

                    pinDir.Rotate(0, 0, TransDir());
                }
                else { pinImage.sprite = pinSprites[0]; }
            }

            int TransDir()
            {
                return pathDir[x, y] switch
                {
                    0 => 90,
                    1 => 270,
                    2 => 0,
                    3 => 180,
                    _ => 0,
                };
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
            /*if ((pinState[pathX[i], pathY[i]] == 3 && pinState[pathX[i + 1], pathY[i + 1]] == 3) ||
                (pinState[pathX[i], pathY[i]] == 4 && pinState[pathX[i + 1], pathY[i + 1]] == 3) ||
                (pinState[pathX[i], pathY[i]] == 3 && pinState[pathX[i + 1], pathY[i + 1]] == 4))
            { lineImage.sprite = TileSprites[14]; }
            else
            { lineImage.sprite = TileSprites[12]; }*/

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

    public void CreateMark() //�÷��̾� ��ġǥ��
    {
        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        int evnt0 = mapMove.eventTime[0];
        int evnt1 = mapMove.eventTime[1];

        Transform gridPins = GameObject.Find("GridPins").transform; //�θ� ������Ʈ Transform
        
        //��ġ ��ȯ �غ�
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
                bound = markLc + left[3] + right[3] - 1.9f;
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

        //�߰� ��ġ Ư��
        float w = Pos - markLc; //�̵��� �ҰŸ�
        float ww = bound - markLc; //�� �ҰŸ�
        if (w <= ww)
        {
            MoveMark(ww, routeDir[p]);
        }
        else
        {
            if (pathPoint[p] != 9 || pathPoint[p] != 11)
            {
                MoveMark(routePoint[p + 1] - bound, routeDir[p]);
            }
            
        }

        void MoveMark(float l, int d)
        {
            if (w < ww)
            {
                switch (d) //�̵�
                {
                    case 0: { markY += w / l * 0.625f; break; }
                    case 1: { markY -= w / l * 0.625f; break; }
                    case 2: { markX += w / l * 0.625f; break; }
                    case 3: { markX -= w / l * 0.625f; break; }
                }
            }
            else
            {
                switch (d) //�̵�
                {
                    case 0: { markY += w / l * 0.625f; break; }
                    case 1: { markY -= w / l * 0.625f; break; }
                    case 2: { markX += w / l * 0.625f; break; }
                    case 3: { markX -= w / l * 0.625f; break; }
                }
            }
        }

        //������Ʈ ����
        GameObject mPrefab = Resources.Load<GameObject>("Prefabs/Mark");
        GameObject playerMark = Instantiate(mPrefab, new Vector3(markX, markY, -2), Quaternion.identity);
        playerMark.transform.SetParent(gridPins, false);

        playerMark.name = "PlayerMark";
        Transform MarkPos = playerMark.GetComponent<Transform>();

        //�̹��� �ο�
        SpriteRenderer MarkImage = playerMark.GetComponent<SpriteRenderer>();
        Sprite[] pinImage = Resources.LoadAll<Sprite>("Prefabs/Pins");
        MarkImage.sprite = pinImage[12];
    }
}