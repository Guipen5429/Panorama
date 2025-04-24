using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
using static UnityEngine.EventSystems.EventTrigger;

public class LoopBuildings : MonoBehaviour
{
    public GameObject MapManager;
    MapTerrain mapTerrain;
    public int[,] map;
    public int mapL;

    public GameObject Map;
    MapEvent mapEvent;
    PathMake pathMake;

    public int[] route = { 0 }; //경로 배열, 5칸에서 늘릴려면 x와 t좌표의 끝(맵의 한계)를 지정하고 만들어야 함
    public GameObject[] prefab; //프리팹 번호 지정
    public float sum = 0; //시작 지점으로부터의 길이
    public float initial = 0;
    public float sub = 0;

    public Transform leftWall;
    public Transform rightWall;
    public Transform iCamera;
    public Transform inses;

    public readonly float[] leftIns = new float[] { 16.5f, 11.8f, 11.8f, 6.6f, 9.9f, 10.4f, 9.9f, 1.9f, 6.6f, 6.6f, -5.7f, 16.5f, 7.5f, 11.8f, 11.8f }; //왼쪽 거리;
    public readonly float[] rightIns = new float[] { 16.5f, 11.8f, 11.8f, 9.9f, 6.6f, 9.9f, 10.4f, 1.9f, 6.6f, 0.9f, 0f, 7.5f, 16.5f, 11.8f, 11.8f }; //오른쪽 거리;

    int[] x;
    int[] y;

    public int[] cameraDir; //East = 0, West = 1, South = 2, North = 3
    public int[] routeDir; //방향을 부여한 지형 정보, 0 ~ 15
    public int[] pathDir; //경로 지형 방향 변환, 0 ~ 15
    public int[] path;
    public float[] routePoint = new float[] { 0 };
    public int[] pathPoint;
    public float preSum;
    public float markLc;

    public int[] mPath;
    public int[] mRouteDir;
    public float[] mRoutePoint;

    public bool callB;
    public int evntB;
    bool rcv;
    bool rcv2;

    enum Dir
    {
        East = 0,
        West = 1,
        South = 2,
        North = 3
    }

    void Start()
    {
        mapEvent = Map.GetComponent<MapEvent>();
        pathMake = Map.GetComponent<PathMake>();

        mapTerrain = MapManager.GetComponent<MapTerrain>();
        map = mapTerrain.map;
        mapL = map.GetLength(0);

        callB = false;
        rcv = false;
        rcv2 = true;
    }

    void Update()
    {
        int evnt0 = mapEvent.eventTime[0];
        int evnt1 = mapEvent.eventTime[1];
        int evnt2 = mapEvent.eventTime[2];
        bool go = mapEvent.go;

        if (evnt0 == 0 || evnt0 == 5 || evnt0 == 9) { rcv = false; rcv2 = true; callB = false; } //default
        if (evnt0 == 7 || evnt0 == 9 && rcv2) { rcv = true; }

        //경로 바꾸기
        if (evnt0 == 7 && !callB && go && rcv)
        {
            x = pathMake.bPathX;
            y = pathMake.bPathY;
            bool call2 = pathMake.call2;

            CreateRoute();
            MapRoute();
            preSum = call2 ? routePoint[^1] : 0;
            LocateMark();

            string routeString = string.Join(", ", route);
            //Debug.Log("최종배경 : " + routeString);

            switch (evnt1)
            {
                case 0: case 3: evntB = 9; break;
                case 1: evntB = 5; break;
            }
            callB = true;
            rcv = false;
            rcv2 = false;
        }

        if (evnt0 == 6)
        {
            preSum = routePoint[^1];
        }

        //배경 바꾸기
        if (evnt0 == 9 && !callB && go && rcv)
        {
            if (evnt2 == 1 && evnt1 != 3)
            {
                Transform inses = GameObject.Find("Inses").transform;
                foreach (Transform childTransform in inses)
                {
                    GameObject.Destroy(childTransform.gameObject);
                }
                preSum = routePoint[^1];
                sum = 0;

                FrameRoute();
            }

            evntB = 0;
            callB = true;
            rcv = false;
            rcv2 = false;
        }
    }
    void CreateRoute()
    {
        path = new int[x.Length]; //경로 지형 정보
        cameraDir = new int[x.Length + 1];

        //경로 지형 정보 생성
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
                    cameraDir[i + 1] = (x[i] - x[i + 1] == 1) ? (int) Dir.West : (int)Dir.East;
                }
                else if (y[i] != y[i + 1])
                {
                    cameraDir[i + 1] = (y[i] - y[i + 1] == 1) ? (int)Dir.South : (int)Dir.North;
                }
            }
        }

        //경로 지형 방향 변환 생성
        pathDir = new int[cameraDir.Length - 1];
        for (int i = 0; i < pathDir.Length; i++)
        {
            switch (path[i])
            {
                case 1: case 2: case 6: case 10: case 12: case 16://기본형
                    {
                        pathDir[i] = ClockWise(cameraDir[i], 0) * 4 + ClockWise(cameraDir[i + 1], 0);
                        break;
                    }
                case 3: case 4: case 5: //ㅓ자 변형
                    {
                        pathDir[i] = ClockWise(cameraDir[i], 6 - path[i]) * 4 + ClockWise(cameraDir[i + 1], 6 - path[i]);
                        break;
                    }
                case 7: case 8: case 9: //ㄴ자 변형
                    {
                        pathDir[i] = ClockWise(cameraDir[i], 10 - path[i]) * 4 + ClockWise(cameraDir[i + 1], 10 - path[i]);
                        break;
                    }
                case 11: //ㅣ자 변형
                    {
                        pathDir[i] = ClockWise(cameraDir[i], 1) * 4 + ClockWise(cameraDir[i + 1], 1);
                        break;
                    }
                case 13: case 14: case 15: //작은 ㅣ자 변형
                    {
                        pathDir[i] = ClockWise(cameraDir[i], 16 - path[i]) * 4 + ClockWise(cameraDir[i + 1], 16 - path[i]);
                        break;
                    }
                case 17: case 18: case 19: //곡선 ㄴ자 변형
                    {
                        pathDir[i] = ClockWise(cameraDir[i], 20 - path[i]) * 4 + ClockWise(cameraDir[i + 1], 20 - path[i]);
                        break;
                    }
            }
        }

        //방향을 부여한 지형 정보
        routeDir = new int[cameraDir.Length - 1];
        for (int i = 1; i < cameraDir.Length; i++)
        {
            routeDir[i - 1] = ClockWise(cameraDir[i], 1);
        }

        int ClockWise(int i, int j)
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

        //route 생성
        int p = 0;
        int pp = 0;
        int dir = 0;
        route = new int[1];
        while (p < path.Length)
        {
            switch (path[p])
            {
                case 1:
                    switch (pathDir[dir])
                    {
                        case 0: case 5: case 10: case 15: RouteFrame(4, 3); break;
                        case 2: case 7:  case 9: case 12:
                            RouteTemp(pp++, 4);
                            RouteTemp(pp++, 7);
                            RouteTemp(pp++, 3);
                            break;
                        case 3: case 6: case 8: case 13: RouteTemp(pp++, 1); break;
                    }
                    break;
                case 2: case 3: case 4: case 5:
                    switch (pathDir[dir])
                    {
                        case 0: RouteFrame(4, 9); break;
                        case 2: RouteFrame(4, 5); break;
                        case 3: case 13: RouteTemp(pp++, 1); break;
                        case 5: RouteFrame(10, 3); break;
                        case 9: RouteFrame(6, 3); break;
                        case 10: RouteTemp(pp++, 0); break;
                        case 15: RouteFrame(4, 3); break;
                    }
                    break;
                case 6: case 7: case 8: case 9:
                    switch (pathDir[dir])
                    {
                        case 0: RouteFrame(10, 3); break;
                        case 5: RouteTemp(pp++, 11); break;
                        case 7: RouteTemp(pp++, 2); break;
                        case 8: RouteTemp(pp++, 1); break;
                        case 10: RouteFrame(4, 9); break;
                        case 15: RouteTemp(pp++, 12); break;
                    }
                    break;
                case 10: case 11:
                    switch (pathDir[dir])
                    {
                        case 10: case 15: RouteTemp(pp++, 0); break;
                    }
                    break;
                case 12: case 13: case 14: case 15:
                    switch (pathDir[dir])
                    {
                        case 10: RouteTemp(pp++, 11); break;
                        case 15: RouteTemp(pp++, 12); break;
                    }
                    break;
                case 16: case 17: case 18: case 19:
                    switch (pathDir[dir])
                    {
                        case 0: RouteFrame(10, 3); break;
                        case 5: RouteTemp(pp++, 11); break;
                        case 7: RouteTemp(pp++, 14); break;
                        case 8: RouteTemp(pp++, 13); break;
                        case 10: RouteFrame(4, 9); break;
                        case 15: RouteTemp(pp++, 12); break;
                    }
                    break;
            }
            dir++;
            p++;
        }

        void RouteFrame(int a, int b)
        {
            RouteTemp(pp++, a);
            RouteTemp(pp++, b);
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
        }
    }

    void MapRoute() //맵 시각화 전용 배열 생성
    {
        int pi = pathMake.pi;
    }

    void LocateMark()
    {
        routePoint = new float[path.Length];
        pathPoint = new int[path.Length];
        float mSum = 0;
        int t = 0;
        for (int i = 0; i < route.Length; i++)
        {
            if (i == 0) //경로 시작
            {
                routePoint[t] = 0;
                mSum += rightIns[route[i]]; pathPoint[t] = route[i];
                t++;
            }
            else
            {
                switch (route[i])
                {
                    case 0: case 1: case 2: case 13: case 14:
                        mSum += leftIns[route[i]];
                        routePoint[t] = mSum; pathPoint[t] = route[i];
                        mSum += rightIns[route[i]];
                        t++; break;
                    case 4: case 9:
                        mSum += leftIns[route[i]] + rightIns[route[i]];
                        routePoint[t] = mSum; pathPoint[t] = route[i];
                        t++; break;
                    case 6:
                        mSum += leftIns[route[i]] + rightIns[route[i]] - 1.9f;
                        routePoint[t] = mSum; mSum += 1.9f; pathPoint[t] = route[i];
                        t++; break;
                    case 3:
                        mSum += leftIns[route[i]] + rightIns[route[i]];
                        break;
                    case 5:
                        mSum += 1.9f;
                        routePoint[--t] = mSum; pathPoint[t] = route[i];
                        mSum += leftIns[route[i]] + rightIns[route[i]] - 1.9f;
                        t++; break;
                    case 7:
                        mSum += leftIns[route[i]];
                        routePoint[--t] = mSum; pathPoint[t] = route[i];
                        mSum += rightIns[route[i]];
                        t++; break;
                    case 11:
                        mSum += leftIns[route[i]];
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
    }

    void FrameRoute() //배경 생성
    {
        GameObject[] buildings = new GameObject[route.Length];

        for (int i = 0; i < route.Length; i++)
        {
            if (i == 0) //경로 시작
            {
                sum -= (route[0] == 0 || route[0] == 12) ? 0 : rightIns[route[i]];
                initial = (route[0] == 10) ? -7.5f : sum - leftIns[route[i]];
                leftWall.position = new Vector3(initial + 3.4f, 0); //왼쪽 벽
                iCamera.position = new Vector3(initial + 19.7f, 0, -30); //초기 카메라 위치
            }
            else
            {
                sum += leftIns[route[i]];
            }

            buildings[i] = Instantiate(prefab[route[i]], new Vector3(transform.position.x + sum,
            transform.position.y, transform.position.z), Quaternion.identity, transform); //배경 복제
            buildings[i].transform.SetParent(inses, false);

            sum += rightIns[route[i]];

            if (i == route.Length - 1) //경로 끝
            {
                rightWall.position = new Vector3(sum - 3.4f, 0); //오른쪽 벽
            }
        }
    }
}