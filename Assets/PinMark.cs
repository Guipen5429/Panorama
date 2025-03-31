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
using UnityEngine.UIElements;

public class PinMark : MonoBehaviour
{
    public int[,] pinState = new int[5, 5]; //핀의 상태
    public int[] ipathX; //경로 x좌표
    public int[] ipathY; //경로 y좌표
    public int[] pathX; //
    public int[] pathY; //
    public int[,] pathDir = new int[5, 5];
    public bool rcv;
    public bool rcv2;

    public int count;
    int[] fDir;

    public bool callP;
    public int evntP;

    private void Awake()
    {
        ipathX = new int[] { 1, 1, 2, 2, 3, 3 };
        ipathY = new int[] { 3, 4, 4, 3, 3, 2 };
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
        RouteMake route = GameObject.Find("Map").GetComponent<RouteMake>();
        bool rCall = route.callR;
        LoopBuildings build = GameObject.Find("BackGround").GetComponent<LoopBuildings>();
        bool bCall = build.callB;
        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>(); //Map 객체의 MapEvent 스크립트를 mapMove란 이름으로 가져오기
        int evnt0 = mapMove.eventTime[0]; //MapEvent(mapMove)의 eventTime 배열 중 0번째를 인용
        int evnt1 = mapMove.eventTime[1];
        int evnt2 = mapMove.eventTime[2];
        bool go = mapMove.go;

        if (evnt0 != 2 && evnt0 != 4 && evnt0 != 5) { rcv = false; rcv2 = true; callP = false; } //default
        if (evnt0 == 2 || evnt0 == 4 || evnt0 == 5 && rcv2) { rcv = true; }

        if (evnt0 == 2 && !callP && go && rcv) //핀, 선 생성
        {
            CreatePins();
            /*if (count != 1)
            {
                Debug.Log("----------------------------------------------");
                CreateMark();
            }*/
            ChngState();

            switch (evnt1)
            {
                case 0: evntP = 3; break;
                case 1: evntP = 6; break;
                case 2: evntP = 9; break;
                default: break;
            }
            callP = true;
            rcv = false;
            rcv2 = false;
        }

        if (evnt0 == 4 && !callP && go && rcv)
        {
            switch (evnt2)
            {
                case 0: evntP = 9; break;
                case 1: evntP = 5; break;
                default: break;
            }
            callP = true;
            rcv = false;
            rcv2 = false;
        }

        if (evnt0 == 5 && !callP && go && rcv) //핀, 선 삭제
        {
            Transform gridPins = GameObject.Find("GridPins").transform;
            foreach (Transform childTransform in gridPins)
            {
                GameObject.Destroy(childTransform.gameObject);
            } //핀 삭제
            Transform gridLines = GameObject.Find("GridLines").transform;
            foreach (Transform childTransform in gridLines)
            {
                GameObject.Destroy(childTransform.gameObject);
            } // 선 삭제

            evntP = 2;
            callP = true;
            rcv = false;
            rcv2 = false;
        }
    }

    public void CreatePins()
    {
        Transform gridPins = GameObject.Find("GridPins").transform;
        GameObject[,] position = new GameObject[5, 5];
        GameObject[] pins = new GameObject[26];
        GameObject pPrefab = Resources.Load<GameObject>("Prefabs/Pin");
        Sprite[] pinSprites = Resources.LoadAll<Sprite>("Prefabs/Pins 1");
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

            if (evnt0 == 3)
            {
                if (pinState[x, y] != 0)
                {
                    if (x == pathX[0] && y == pathY[0] || x == pathX[pathX.Length - 1] && y == pathY[pathY.Length - 1])
                    {
                        pinImage.sprite = pinSprites[3];
                        pinState[x, y] = 2;
                    }
                    else { pinImage.sprite = pinSprites[4]; pinState[x, y] = 1; }

                    pinDir.Rotate(0, 0, TransDir());
                }
                else { pinImage.sprite = pinSprites[0]; }
            }
            else if (evnt0 == 6)
            {
                if (pinState[x, y] == 1)
                { pinImage.sprite = pinSprites[11]; pinDir.Rotate(0, 0, TransDir()); }
                else if (pinState[x, y] == 2)
                { pinImage.sprite = pinSprites[8]; pinDir.Rotate(0, 0, TransDir()); }
                    else if (pinState[x, y] == 3)
                    {
                    pinImage.sprite = (count == 1) ? pinSprites[12] : pinSprites[12];
                    pinDir.Rotate(0, 0, TransDir());
                }
                else if (pinState[x, y] == 4)
                {
                    pinImage.sprite = (count == 1) ? pinSprites[10] : pinSprites[10];
                    pinDir.Rotate(0, 0, TransDir());
                }
                else { pinImage.sprite = pinSprites[0]; }
            }

            int TransDir()
            {
                return pathDir[x, y] switch
                {
                    0 => 270,
                    1 => 90,
                    2 => 180,
                    3 => 0,
                    _ => 0,
                };
            }
        }

        for (int i = 1; i < pathX.Length; ++i) //선 생성
        {
            Transform pinTrans = pins[pathPin(i + 1)].GetComponent<Transform>();
            Transform prePinTrans = pins[pathPin(i)].GetComponent<Transform>();

            lines[i] = Instantiate(sPrefab,
                new Vector3(Average(pinTrans.position.x, prePinTrans.position.x), Average(pinTrans.position.y, prePinTrans.position.y), -1),
                Quaternion.identity); //선 위치 지정
            lines[i].transform.SetParent(gridLines, true); //선 부모 지정
            lines[i].name = "Line " + i; //선 이름 지정

            SpriteRenderer lineImage = lines[i].GetComponent<SpriteRenderer>(); //선 색깔 지정
            if ((pinState[pathX[i], pathY[i]] == 3 && pinState[pathX[i + 1], pathY[i + 1]] == 3) ||
                (pinState[pathX[i], pathY[i]] == 4 && pinState[pathX[i + 1], pathY[i + 1]] == 3) ||
                (pinState[pathX[i], pathY[i]] == 3 && pinState[pathX[i + 1], pathY[i + 1]] == 4))
            { lineImage.sprite = TileSprites[14]; }
            else
            { lineImage.sprite = TileSprites[12]; }

            Transform lineDir = lines[i].GetComponent<Transform>(); //선 방향 지정
            lineDir.Rotate(0, 0, (pathX[i - 1] == pathX[i]) ? 90 : 0);

            float Average(float a, float b)
            {
                return (a + b) / 2;
            }

            int pathPin(int i)
            {
                return ipathY[i - 1] * 5 + ipathX[i - 1];
            }
        }
    }

    public void ChngState()
    {
        LoopBuildings loopBuildings = GameObject.Find("BackGround").GetComponent<LoopBuildings>();
        int[] iDir = loopBuildings.cameraDir;

        for (int i = 0; i < pathX.Length; i++)
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

    /*public void CreateMark() //플레이어 위치표지
    {
        Transform gridPins = GameObject.Find("GridPins").transform; //부모 오브젝트 Transform
        //오브젝트 생성
        GameObject mPrefab = Resources.Load<GameObject>("Prefabs/Mark");
        GameObject playerMark = Instantiate(mPrefab, new Vector3(1, 1), Quaternion.identity);

        playerMark.name = "PlayerMark";
        RectTransform MarkPos = playerMark.AddComponent<RectTransform>();
        MarkPos.localScale = new Vector3(1, 1);

        //이미지 부여
        SpriteRenderer MarkImage = playerMark.AddComponent<SpriteRenderer>();
        Sprite[] bluepin = Resources.LoadAll<Sprite>("Prefabs/Pins");
        Sprite[] bluepin3 = Resources.LoadAll<Sprite>("Prefabs/Pins3");

        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        int evnt1 = mapMove.eventTime[1];
        int evnt2 = mapMove.eventTime[2];
        int evnt3 = mapMove.eventTime[3];

        MarkImage.sprite = (evnt1 == 3) ? bluepin3[5] : bluepin[1];

        //위치 변환 준비
        LoopBuildings loopBuildings = GameObject.Find("BackGround").GetComponent<LoopBuildings>();
        Transform playerPos = GameObject.Find("Player").transform;
        int[] route = loopBuildings.route;
        float[] left = loopBuildings.leftIns;
        float[] right = loopBuildings.rightIns;
        int[] fcameraDir;
        float mSum = 0;
        float rPos = playerPos.position.x;

        float markX = (pathY[0] - 1.5f) * 3.8f;
        float markY = (1.5f - pathX[0]) * 3.8f;
        Debug.Log($"마크 초기위치 : {markX}, {markY}");
        float lastCorner = 0;

        //페이크
        int[] froute = new int[1];
        fakeDir();
        float fPos = fakePos(rPos);
        float Pos = (modifyTime) ? fPos : rPos;
        int markDir = fcameraDir[0];

        //위치 변환
        for (int i = 1; i < route.Length; i++)
        {
            if (i - 1 == 0 && route[i - 1] == 0 && mSum + right[route[0]] <= Pos) //0 예외
            {   
                mSum += right[route[0]];
                lastCorner += right[route[0]];
                lastPos(markDir, 0);
            }
            if (mSum + left[route[i]] + right[route[i]] <= Pos) //지난 구간들
            {
                mSum += left[route[i]] + right[route[i]];
                lastCorner += left[route[i]] + right[route[i]];
                if (route[i] != 5 && route[i] != 12) { lastPos(markDir, route[i]); }
                markDir = TransDir(markDir, route[i]);
                if (route[i] != 6 && route[i] != 11) { lastPos(markDir, route[i]); }
            }
            else //현재 구간
            {
                switch (route[i])
                {
                    case 0:
                        {
                            MoveMark(left[route[i]], markDir);
                            if (i != route.Length - 1)
                            { MoveMark(right[route[i]], markDir);}
                            else
                            {
                                MarkPos.position = new Vector3(markX, markY, -2);
                                MoveMark(7.5f, markDir);
                            }
                            break;
                        }
                    case 1:
                        {
                            MoveMark(6.6f, markDir); MoveMark(5.2f, markDir);
                            markDir = TransDir(markDir, route[i]);
                            MoveMark(5.2f, markDir); MoveMark(6.6f, markDir);
                            break;
                        }
                    case 2:
                        {
                            MoveMark(16.5f, markDir);
                            markDir = TransDir(markDir, route[i]);
                            markTrack(markDir);
                            MarkPos.position = new Vector3(markX, markY, -2); mSum += 3.8f;
                            MoveMark(16.5f, markDir);
                            break;
                        }
                    case 3:
                        {
                            if (i != route.Length - 1)
                            { MoveMark(left[route[i]] + right[route[i]], markDir); }
                            else
                            {
                                MarkPos.position = new Vector3(markX, markY, -2);
                                MoveMark(15f, markDir);
                            }
                            break;
                        }
                    case 5:
                        {
                            markTrack(markDir);
                            MarkPos.position = new Vector3(markX, markY, -2); mSum += 3.8f;
                            markDir = TransDir(markDir, route[i]);
                            MoveMark(16.5f, markDir);
                            break;
                        }
                    case 6:
                        {
                            MoveMark(16.5f, markDir);
                            markDir = TransDir(markDir, route[i]);
                            markTrack(markDir);
                            MarkPos.position = new Vector3(markX, markY, -2);
                            break;
                        }
                    case 7:
                        {
                            markTrack(markDir);
                            markDir = TransDir(markDir, route[i]);
                            MarkPos.position = new Vector3(markX, markY, -2);   
                            break;
                        }
                    case 9:
                        {
                            markTrack(markDir);
                            MarkPos.position = new Vector3(markX, markY, -2);
                            break;
                        }
                    case 11:
                        {
                            MoveMark(left[route[i]], markDir);
                            MarkPos.position = new Vector3(markX, markY, -2);
                            MoveMark(right[route[i]], markDir);
                            break;
                        }
                    default:
                        {
                            MoveMark(left[route[i]] + right[route[i]], markDir);
                            break;
                        }
                }
                break;
            }
        }

        void fakeDir()
        {
            fcameraDir = new int[pathX.Length + 1];
            int[] fpath = new int[pathX.Length]; ;
            int[,] map = loopBuildings.map;

            for (int i = 0; i < pathX.Length; i++)
            {
                fpath[i] = map[pathX[i], pathY[i]];
            }

            for (int i = 0; i < pathX.Length; i++)
            {
                if (i == pathX.Length - 1)
                {
                    fcameraDir[0] = fcameraDir[1];
                    fcameraDir[i + 1] = fcameraDir[i];
                }
                else
                {
                    if (pathX[i] != pathX[i + 1])
                    {
                        fcameraDir[i + 1] = (pathX[i] - pathX[i + 1] == 1) ? 1 : 0;
                    }
                    else if (pathY[i] != pathY[i + 1])
                    {
                        fcameraDir[i + 1] = (pathY[i] - pathY[i + 1] == 1) ? 2 : 3;
                    }
                }
            }
            fDir = fcameraDir;
            string fcamString = string.Join(", ", fcameraDir);
            Debug.Log("가짜 카메라방향 : " + fcamString);

            int[] fdirChange = new int[fcameraDir.Length - 1];
            for (int i = 0; i < fdirChange.Length; i++)
            {
                switch (fpath[i])
                {
                    case 1:
                    case 2:
                    case 6:
                        {
                            fdirChange[i] = TransDir(fcameraDir[i], 0) * 4 + TransDir(fcameraDir[i + 1], 0);
                            break;
                        }
                    case 3:
                    case 4:
                    case 5:
                        {
                            fdirChange[i] = TransDir(fcameraDir[i], 6 - fpath[i]) * 4 + TransDir(fcameraDir[i + 1], 6 - fpath[i]);
                            break;
                        }
                    case 7:
                    case 8:
                    case 9:
                        {
                            fdirChange[i] = TransDir(fcameraDir[i], 10 - fpath[i]) * 4 + TransDir(fcameraDir[i + 1], 10 - fpath[i]);
                            break;
                        }
                }
            }

            int TransDir(int i, int j)
            {
                int k = 0;
                while (k < j)
                {
                    switch (i)
                    {
                        case 1: i = 3; break;
                        case 3: i = 0; break;
                        case 0: i = 2; break;
                        case 2: i = 1; break;
                    }
                    k++;
                }
                return i;
            }

            int q = 0;
            int pp = 0;
            int dir = 0;
            while (q < fpath.Length)
            {
                switch (fpath[q])
                {
                    case 1:
                        switch (fdirChange[dir])
                        {
                            case 0: case 5: case 10: case 15: RouteFrame(4, 3); break;
                            case 2:
                            case 7:
                            case 9:
                            case 12:
                                RouteTemp(pp++, 4);
                                RouteTemp(pp++, 7);
                                RouteTemp(pp++, 3);
                                break;
                            case 3: case 6: case 8: case 13: RouteTemp(pp++, 1); break;
                        }
                        break;
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        switch (fdirChange[dir])
                        {
                            case 0: RouteFrame(4, 9); break;
                            case 2: RouteFrame(4, 5); break;
                            case 5: RouteFrame(10, 3); break;
                            case 9: RouteFrame(6, 3); break;
                            case 15: RouteFrame(4, 3); break;
                            case 3: case 13: RouteTemp(pp++, 1); break;
                            case 10: RouteTemp(pp++, 0); break;
                        }
                        break;
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                        switch (fdirChange[dir])
                        {
                            case 0: RouteFrame(10, 3); break;
                            case 10: RouteFrame(4, 9); break;
                            case 5: RouteTemp(pp++, 11); break;
                            case 7: RouteTemp(pp++, 2); break;
                            case 8: RouteTemp(pp++, 1); break;
                            case 15: RouteTemp(pp++, 12); break;
                        }

                        void RouteFrame(int a, int b)
                        {
                            RouteTemp(pp++, a);
                            RouteTemp(pp++, b);
                        }

                        break;
                }
                dir++;
                q++;
            }

            void RouteTemp(int pp, int b)
            {
                int[] temp = froute;
                froute = new int[++pp];
                for (int i = 0; i < temp.Length; i++)
                {
                    froute[i] = temp[i];
                }
                froute[--pp] = b;
            }


        }

        float fakePos(float p)
        {
            int blocks = froute.Length;
            float fsum = 0;
            for (int i = 0; i < blocks; i++)
            {
                if (i == 0) //경로 시작
                {
                    fsum -= (froute[0] == 0) ? 0 : right[froute[i]];
                }
                else
                {
                    fsum += left[froute[i]];
                }
                fsum += right[froute[i]];
            }

            string routeString = string.Join(", ", froute);
            Debug.Log($"가짜 루트 : {routeString}");
            return fsum - p;
        }


        void MoveMark(float l, int d)
        {

            if (Pos <= 0)
            {
                MarkPos.position = new Vector3((pathY[0] - 1.5f) * 3.8f, (1.5f - pathX[0]) * 3.8f, - 2); //초기 위치
                markTrack(d);
            }
            else if (mSum <= Pos && Pos < mSum + l)
            {
                float w = l;
                switch (l) //이동할 거리
                {
                    case 5.2f: case 6.6f: { w = l * 2; break; } //0.95
                    case 16.5f: case 13.2f: case 13.7f: case 20.3f: { w = l; break; } //1.9
                    case 33f: { w = l / 2; break; } //3.8
                }
                if (l % 7.5f != 0)
                {
                    switch (d) //이동
                    {
                        case 0:
                            {
                                MarkPos.position = new Vector3(
                                    markX, markY -= (Pos - mSum) / w * 1.9f, -2);
                                break;
                            }
                        case 1:
                            {
                                MarkPos.position = new Vector3(
                                    markX, markY += (Pos - mSum) / w * 1.9f, -2);
                                break;
                            }
                        case 2:
                            {
                                MarkPos.position = new Vector3(
                                    markX -= (Pos - mSum) / w * 1.9f, markY, -2);
                                break;
                            }
                        case 3:
                            {
                                MarkPos.position = new Vector3(
                                    markX += (Pos - mSum) / w * 1.9f, markY, -2);
                                break;
                            }
                    }
                }
                markTrack(d);
            }
            else
            {
                float ww = (6.6f < l) ? 1.9f : 0.95f;
                switch (d)
                {
                    case 0: { markY -= ww; break; }
                    case 1: { markY += ww; break; }
                    case 2: { markX -= ww; break; }
                    case 3: { markX += ww; break; }
                }
            }

            //Debug.Log(mSum);
            mSum += l;
        }

        void markTrack(int d)
        {
            float lastX = markX;
            float lastY = markY;
            float prex = (1.5f - lastY / 3.8f);
            float prey = (1.5f + lastX / 3.8f);
            int x = Convert.ToInt16(Math.Round(prex));
            int y = Convert.ToInt16(Math.Round(prey));
            float deltax = prex - x;
            float deltay = prey - y;
            bool vh = (d == 0 || d == 1) ? false : true;

            Transform mkDir = playerMark.GetComponent<Transform>();

            //Debug.Log($"{prex}, {prey} / {x}, {y} / {deltax}, {deltay}");

            switch (vh)
            {
                case false:
                    {
                        if (deltax > 0)
                        {
                            if (deltax <= 0.15f)
                            {
                                fpinState[x, y] = (fpinState[x, y] == 2) ? 4 : 3;
                            }
                            else
                            {
                                fpinState[x, y] = (fpinState[x, y] == 2) ? 4 : 3;
                                fpinState[x + 1, y] = (fpinState[x + 1, y] == 2) ? 4 : 3;
                                mkDir.Rotate(0, 0, RotateDir((pathDir[x, y] == 0) ? x : x + 1, y));
                            }
                        }
                        else
                        {
                            if (deltax >= -0.15f)
                            {
                                fpinState[x, y] = (fpinState[x, y] == 2) ? 4 : 3;
                            }
                            else
                            {
                                fpinState[x, y] = (fpinState[x, y] == 2) ? 4 : 3;
                                fpinState[x - 1, y] = (fpinState[x - 1, y] == 2) ? 4 : 3;
                                mkDir.Rotate(0, 0, RotateDir((pathDir[x, y] == 1) ? x : x - 1, y));
                            }
                        }
                        break;
                    }
                case true:
                    {
                        if (deltay > 0)
                        {
                            if (deltay <= 0.15f)
                            {
                                fpinState[x, y] = (fpinState[x, y] == 2) ? 4 : 3;
                            }
                            else
                            {
                                fpinState[x, y] = (fpinState[x, y] == 2) ? 4 : 3;
                                fpinState[x, y + 1] = (fpinState[x, y + 1] == 2) ? 4 : 3;
                                mkDir.Rotate(0, 0, RotateDir(x, (pathDir[x, y] == 3) ? y : y + 1));
                            }
                        }
                        else
                        {
                            if (deltay >= -0.15f)
                            {
                                fpinState[x, y] = (fpinState[x, y] == 2) ? 4 : 3;
                            }
                            else
                            {
                                fpinState[x, y] = (fpinState[x, y] == 2) ? 4 : 3;
                                fpinState[x, y - 1] = (fpinState[x, y - 1] == 2) ? 4 : 3;
                                mkDir.Rotate(0, 0, RotateDir(x, (pathDir[x, y] == 2) ? y : y - 1));
                            }
                        }
                        break;
                    }
            }

            int RotateDir(int x, int y)
            {
                switch (pathDir[x, y])
                {
                    case 0: return 270;
                    case 1: return 90;
                    case 2: return 180;
                    case 3: return 0;
                    default: return 0;
                }
            }
        }

        void lastPos(int d, int i)
        {
            float w;
            switch (i)
            {
                case 0: case 1: case 2: case 5: case 6: { w = 1.9f; break; }
                case 7: { w = 0; break; }
                default: { w = 0.95f; break; }
            }
            switch (d)
            {
                case 0: { markY -= w; break; }
                case 1: { markY += w; break; }
                case 2: { markX -= w; break; }
                case 3: { markX += w; break; }
            }
        }

        playerMark.transform.SetParent(gridPins, false); //GridPins의 자식화

        int TransDir(int i, int j)
        {
            int[] trans = { 2, 3, 1, 0 };
            switch (j)
            {
                case 1: i = trans[trans[trans[i]]]; break; //turn left
                case 2: case 5: case 6: case 7: i = trans[i]; break; //turn right
                default: break;
            }
            return i;
        }
    }*/
}
