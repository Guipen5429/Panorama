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
        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>(); //Map 객체의 MapEvent 스크립트를 mapMove란 이름으로 가져오기
        int evnt0 = mapMove.eventTime[0]; //MapEvent(mapMove)의 eventTime 배열 중 0번째를 인용
        int evnt1 = mapMove.eventTime[1];
        bool go = mapMove.go;

        if (evnt0 == 2 || evnt0 == 3 || evnt0 == 6 || evnt0 == 9) { rcv = false; rcv2 = true; callP = false; } //default
        if (evnt0 == 2 || evnt0 == 5 && rcv2) { rcv = true; }

        //핀, 선, 마크 생성
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

        //핀, 선, 마크 삭제
        if (evnt0 == 5 && !callP && go && rcv)
        {
            Transform gridPins = GameObject.Find("GridPins").transform;
            foreach (Transform childTransform in gridPins)
            {
                GameObject.Destroy(childTransform.gameObject);
            } //핀, 마크 삭제
            Transform gridLines = GameObject.Find("GridLines").transform;
            foreach (Transform childTransform in gridLines)
            {
                GameObject.Destroy(childTransform.gameObject);
            } // 선 삭제

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
        for (int i = 1; i <= 25; i++) //Pin 생성
        {
            x = (i - 1) % 5;
            y = (i - 1) / 5;
            pins[i] = Instantiate(pPrefab, new Vector3(x * 1.25f, y * 1.25f, -1), Quaternion.identity); //핀 생성   

            pins[i].transform.SetParent(gridPins, false); //핀을 Grid_Pins의 자식으로 지정
            pins[i].name = "Pin " + x + "," + y; ; ; //핀 이름 지정
            position[x, y] = pins[i]; //핀 위치 지정

            SpriteRenderer pinImage = pins[i].GetComponent<SpriteRenderer>(); //이미지 불러오기
            Transform pinDir = pins[i].GetComponent<Transform>();
            pinDir.Rotate(0, 0, TransDir(pathDir[x, y])); //핀 방향 지정

            pins[i].AddComponent<PinAct>();

            //핀 이미지
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

        for (int i = 1; i < pathX.Length; i++) //선 생성
        {
            Transform pinTrans = pins[pathPin(i)].GetComponent<Transform>();
            Transform prePinTrans = pins[pathPin(i - 1)].GetComponent<Transform>();

            lines[i] = Instantiate(sPrefab,
                new Vector3(Average(pinTrans.position.x, prePinTrans.position.x), Average(pinTrans.position.y, prePinTrans.position.y), -1),
                Quaternion.identity); //선 생성, 위치 지정
            lines[i].transform.SetParent(gridLines, true); //선 부모 지정
            lines[i].name = "Line " + i; //선 이름 지정

            SpriteRenderer lineImage = lines[i].GetComponent<SpriteRenderer>(); //선 색깔 지정
            lineImage.sprite = TileSprites[12];

            Transform lineDir = lines[i].GetComponent<Transform>(); //선 방향 지정
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
    void CreateMark() //플레이어 위치표지
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

        float Pos = playerPos.position.x; //플레이어가 이동한 거리
        float mSum = 0;
        float[] routePoint = new float[path.Length]; //경로 상 포인트 거리
        int[] pathPoint = new int[path.Length]; //경로 상 포인트
        float markLc = 0; //최기 포인트 거리
        float bound = 0; //경계
        int p = 0; //포인트
        float markX = 0; //마크 x좌표
        float markY = 0; //마크 y좌표

        //포인트 지정
        int t = 0;
        for (int i = 0; i < route.Length; i++)
        {
            if (i == 0) //경로 시작
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

        //최기 포인트 특정
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

        //경계 특정
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

        //소위치 특정
        float lB = Pos - markLc; //좌측 거리
        float rB = Pos - bound; //우측 거리
        float ww = bound - markLc; //경계 거리
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
                switch (d) //이동
                {
                    case 0: { markY += lB / l * 0.625f; break; }
                    case 1: { markY -= lB / l * 0.625f; break; }
                    case 2: { markX += lB / l * 0.625f; break; }
                    case 3: { markX -= lB / l * 0.625f; break; }
                }
            }
            else
            {
                switch (d) //이동
                {
                    case 0: { markY += 0.625f + (rB / l * 0.625f); break; }
                    case 1: { markY -= 0.625f + (rB / l * 0.625f); break; }
                    case 2: { markX += 0.625f + (rB / l * 0.625f); break; }
                    case 3: { markX -= 0.625f + (rB / l * 0.625f); break; }
                }
            }
        }

        //오브젝트 생성
        GameObject mPrefab = Resources.Load<GameObject>("Prefabs/Mark");
        GameObject playerMark = Instantiate(mPrefab, new Vector3(markX, markY, -2), Quaternion.identity);
        Transform gridPins = GameObject.Find("GridPins").transform; //부모 오브젝트 Transform
        playerMark.transform.SetParent(gridPins, false);
        playerMark.name = "PlayerMark";

        //이미지, 방향 부여
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