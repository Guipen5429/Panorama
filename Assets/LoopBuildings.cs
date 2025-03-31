using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

public class LoopBuildings : MonoBehaviour
{
    public int[] route = { 0 }; //경로 배열, 5칸에서 늘릴려면 x와 t좌표의 끝(맵의 한계)를 지정하고 만들어야
    public GameObject[] prefab; //프리팹 번호 지정
    public float sum = 0; //시작 지점으로부터의 길이
    public float initial = 0;

    Transform leftWall;
    Transform rightWall;
    Transform iCamera;

    public int[,] map;
    public float[] leftIns;
    public float[] rightIns;

    public int[] cameraDir; //East = 0, West = 1, South = 2, North = 3
    public int[] dirChange;

    public bool callB;
    public bool rcv;
    public bool rcv2;

    public float preSum = 0;

    void Start()
    {
        map = new int[,] { { 6, 4, 4, 4, 7 }, { 3, 1, 1, 1, 5 }, { 3, 1, 1, 1, 5 }, { 3, 1, 1, 1, 5 }, { 9, 2, 2, 2, 8 } }; //지형 정보 (0~9)
        leftIns = new float[] { 16.5f, 11.7f, 18.3f, 6.6f, 9.9f, 3.8f, 9.9f, 1.8f, 6.6f, 6.6f, 0.9f, 16.5f, 7.5f }; //왼쪽 거리
        rightIns = new float[] { 16.5f, 11.9f, 18.5f, 9.9f, 6.6f, 9.9f, 10.4f, 2.0f, 6.6f, 0.9f, 6.6f, 7.5f, 16.5f }; //오른쪽 거리
        callB = false;
        rcv = false;
        rcv2 = true;
    }

    void Update()
    {
        RouteMake route = GameObject.Find("Map").GetComponent<RouteMake>();
        bool rCall = route.callR;
        PinMark pin = GameObject.Find("Map").GetComponent<PinMark>();
        bool pCall = pin.callP;
        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        int evnt0 = mapMove.eventTime[0];
        int evnt1 = mapMove.eventTime[1];
        int evnt2 = mapMove.eventTime[2];
        bool go = mapMove.go;

        if (evnt0 != 9) { rcv = false; rcv2 = true; callB = false; } //default
        if (evnt0 == 9 && rcv2) { rcv = true; }

        if (evnt0 == 9 && !callB && go && rcv)
        {
            Transform inses = GameObject.Find("Inses").transform;
            foreach (Transform childTransform in inses)
            {
                GameObject.Destroy(childTransform.gameObject);
            }
            sum = 0;

            //path, dirChange, route
            CreateRoute();

            string routeString = string.Join(", ", route);
            Debug.Log("최종배경 : " + routeString);

            //배경 복제
            FrameRoute();

            callB = true;
            rcv = false;
            rcv2 = false;
        }
    }

    void FrameRoute()
    {
        int blocks = route.Length;
        leftWall = GameObject.Find("LeftWall").transform;
        rightWall = GameObject.Find("RightWall").transform;
        iCamera = GameObject.Find("Main Camera").transform;

        Transform inses = GameObject.Find("Inses").transform;
        GameObject[] buildings = new GameObject[blocks];

        for (int i = 0; i < blocks; i++)
        {
            if (i == 0) //경로 시작
            {
                sum -= (route[0] == 0 || route[0] == 10 || route[0] == 12) ? 0 : rightIns[route[i]];
                initial = sum - leftIns[route[i]];
                leftWall.position = new Vector3(initial + 3.4f, 0); //왼쪽 벽
                iCamera.position = new Vector3(initial + 19.7f, 0, -30); //초기 카메라 위치
            }
            else
            {
                sum += leftIns[route[i]];
            }

            buildings[i] = Instantiate(prefab[route[i]], new Vector3(transform.position.x + sum,
            transform.position.y, transform.position.z), Quaternion.identity, transform); //복제
            buildings[i].transform.SetParent(inses, false);

            sum += rightIns[route[i]];

            if (i == blocks - 1) //경로 끝
            {
                rightWall.position = new Vector3(sum - 3.4f, 0); //오른쪽 벽
                preSum = sum;
                Debug.Log(preSum);
            }
        }
    }

    void CreateRoute()
    {
        PinMark pinMark = GameObject.Find("Map").GetComponent<PinMark>();
        int[] x = pinMark.ipathX;
        int[] y = pinMark.ipathY;
        int[] path = new int[x.Length];
        cameraDir = new int[x.Length + 1];

        //path(경로) 생성
        for (int i = 0; i < x.Length; i++)
        {
            path[i] = map[x[i], y[i]];
        }

        //cameraDir(카메라방향) 생성
        for (int i = 0; i < x.Length; i++)
        {
            if (i == x.Length - 1)
            {
                cameraDir[0] = cameraDir[1];
                cameraDir[i + 1] = cameraDir[i];
            }
            else
            {
                if (x[i] != x[i + 1])
                {
                    cameraDir[i + 1] = (x[i] - x[i + 1] == 1) ? 1 : 0;
                }
                else if (y[i] != y[i + 1])
                {
                    cameraDir[i + 1] = (y[i] - y[i + 1] == 1) ? 2 : 3;
                }
            }
        }
        string camString = string.Join(", ", cameraDir);
        /*Debug.Log("카메라방향 : " + camString);*/

        //dirChange(방향변환) 생성
        dirChange = new int[cameraDir.Length - 1];
        for (int i = 0; i < dirChange.Length; i++)
        {
            switch (path[i])
            {
                case 1: case 2: case 6:
                    {
                        dirChange[i] = TransDir(cameraDir[i], 0) * 4 + TransDir(cameraDir[i + 1], 0);
                        break;
                    }
                case 3: case 4: case 5:
                    {
                        dirChange[i] = TransDir(cameraDir[i], 6 - path[i]) * 4 + TransDir(cameraDir[i + 1], 6 - path[i]);
                        break;
                    }
                case 7: case 8: case 9:
                    {
                        dirChange[i] = TransDir(cameraDir[i], 10 - path[i]) * 4 + TransDir(cameraDir[i + 1], 10 - path[i]);
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

        //route(경로배경) 생성
        int p = 0;
        int pp = 0;
        int dir = 0;
        route = new int[1];
        while (p < path.Length)
        {
            switch (path[p])
            {
                case 1:
                    switch (dirChange[dir])
                    {
                        case 0: case 5: case 10: case 15: RouteFrame(4, 3); break;
                        case 2: case 7: case 9: case 12:
                            RouteTemp(pp++, 4);
                            RouteTemp(pp++, 7);
                            RouteTemp(pp++, 3);
                            break;
                        case 3: case 6: case 8: case 13: RouteTemp(pp++, 1); break;
                    }
                    break;
                case 2: case 3: case 4: case 5:
                    switch (dirChange[dir])
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
                case 6: case 7: case 8: case 9:
                    switch (dirChange[dir])
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
            p++;
        }

        void RouteTemp(int pp, int b)
        {
            int[] temp = route;
            route = new int[++pp];
            for (int i = 0; i < temp.Length; i++)
            {
                route[i] = temp[i];
            }
            route[--pp] = b;
            /*string routeString = string.Join(", ", route);
            Debug.Log(routeString);*/
        }
    }
}
