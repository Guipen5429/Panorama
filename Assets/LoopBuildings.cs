using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class LoopBuildings : MonoBehaviour
{
    public int[] route = { 0 }; //��� �迭
    public GameObject[] prefab; //������ ��ȣ ����
    public float sum = 0; //����
    public float initial = 0;

    Transform leftWall;
    Transform rightWall;
    Transform iCamera;

    public int[,] map;
    public float[] leftIns;
    public float[] rightIns;

    public int[] cameraDir;
    //East = 0, West = 1, South = 2, North = 3
    public int[] dirChange;

    public bool buildTime = true;
    bool o = false;
    bool w = false;

    public float preSum = 0;

    void Start()
    {
        map = new int[,] { { 6, 4, 4, 7 }, { 3, 1, 1, 5 }, { 3, 1, 1, 5 }, { 2, 2, 2, 8 } };
        leftIns = new float[] { 16.5f, 11.7f, 18.3f, 6.6f, 9.9f, 10.4f, 9.9f, 1.8f, 6.6f, 6.6f, 0.9f, 16.5f, 7.5f };
        rightIns = new float[] { 16.5f, 11.9f, 18.5f, 9.9f, 6.6f, 9.9f, 10.4f, 2.0f, 6.6f, 0.9f, 6.6f, 7.5f, 16.5f };
    }

    void Update()
    {
        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        bool mapTime = mapMove.eventTime[0];
        bool pinTime = mapMove.eventTime[1];
        bool routeTime = mapMove.eventTime[2];
        bool modifyTime = mapMove.eventTime[3];

        if (mapTime && !routeTime)
        {
            if (!o && w)
            {
                o = true;
                w = false;
            }
        }
        if (o)
        {
            buildTime = true;
            o = false;
        }
        if (routeTime) { w = true; }

        if (modifyTime)
        {
            //path, dirChange, route
            CreateRoute();
        }
        
        if (buildTime)
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
            Debug.Log("������� : " + routeString);

            //��� ����
            FrameRoute();
            buildTime = false;
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
            if (i == 0) //��� ����
            {
                sum -= (route[0] == 0) ? 0 : rightIns[route[i]];
                initial = sum - leftIns[route[i]];
                leftWall.position = new Vector3(initial + 3.4f, 0); //���� ��
                iCamera.position = new Vector3(initial + 19.7f, 0, -30); //�ʱ� ī�޶� ��ġ
            }
            else
            {
                sum += leftIns[route[i]];
            }

            buildings[i] = Instantiate(prefab[route[i]], new Vector3(transform.position.x + sum,
            transform.position.y, transform.position.z), Quaternion.identity, transform); //����
            buildings[i].transform.SetParent(inses, false);

            sum += rightIns[route[i]];

            if (i == blocks - 1) //��� ��
            {
                rightWall.position = new Vector3(sum - 3.4f, 0); //������ ��
                preSum = sum;
            }
        }
    }

    void CreateRoute()
    {
        PinMark pinMark = GameObject.Find("Map").GetComponent<PinMark>();
        int[] x = pinMark.pathX;
        int[] y = pinMark.pathY;
        int[] path = new int[x.Length];
        cameraDir = new int[x.Length + 1];

        //path(���) ����
        for (int i = 0; i < x.Length; i++)
        {
            path[i] = map[x[i], y[i]];
        }

        //cameraDir(ī�޶����) ����
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
        /*Debug.Log("ī�޶���� : " + camString);*/

        //dirChange(���⺯ȯ) ����
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

        //route(��ι��) ����
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
