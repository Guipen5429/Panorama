using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class RouteMake : MonoBehaviour
{
    public GameObject Map;
    MapEvent mapEvent;
    MarkMake markMake;
    PathMake pathMake;
    StateMake stateMake;
    public GameObject BackGround;
    LoopBuildings loopBuildings;

    public Transform gridPins;
    public GameObject wPrefab;

    public int[,] pinState; //핀의 상태
    public int[,] pathDir;
    public int[] GStt;
    int[] pathX; //경로 x좌표
    int[] pathY; //경로 y좌표
    int mapL;

    public bool callR;
    public int evntR;
    bool rcv;
    bool rcv2;

    void Start()
    {
        mapEvent = Map.GetComponent<MapEvent>();
        pathMake = Map.GetComponent<PathMake>();
        markMake = Map.GetComponent<MarkMake>();
        stateMake = Map.GetComponent<StateMake>();
        loopBuildings = BackGround.GetComponent<LoopBuildings>();

        mapL = loopBuildings.mapL;
        pinState = new int[mapL, mapL];
        pathDir = new int[mapL, mapL];

        GStt = new int[] { 0, 0, 0 };
        callR = false;
        rcv = false;
        rcv2 = true;
    }

    private void Update()
    {
        int evnt0 = mapEvent.eventTime[0];
        bool go = mapEvent.go;
        if (evnt0 == 2 || evnt0 == 5) { rcv = false; rcv2 = true; callR = false; } //default
        if (evnt0 == 3 || evnt0 == 6 || evnt0 == 4 && rcv2) { rcv = true; }

        //R키 눌렀을 때
        if (Input.GetKeyDown(KeyCode.R) && (evnt0 == 3 || evnt0 == 6))
        {
            switch (evnt0)
            {
                case 3: evntR = 3; break;
                case 6: evntR = 6; break;
                default: break;
            }
            callR = true;
            rcv = false;
            rcv2 = false;
        }

        //핀 상태 바꾸기
        if (evnt0 == 4 && !callR && go && rcv)
        {
            pathX = pathMake.pathX;
            pathY = pathMake.pathY;
            GStt[2] = stateMake.GStt2;

            RouteStt();
            WhiteExpn();

            evntR = 2;
            callR = true;
            rcv = false;
            rcv2 = false;
        }
    }

    void RouteStt()
    {
        int[] iDir = loopBuildings.cameraDir;
        int p = markMake.p;
        float markLc = markMake.markLc;

        //핀 상태 초기화
        for (int i = 0; i < pinState.GetLength(0); i++)
        {
            for (int j = 0; j < pinState.GetLength(1); j++)
            {
                pinState[i, j] = 0;
                pathDir[i, j] = 0;
            }
        }
        GStt[0] = 0;
        GStt[1] = 0;

        //2, 3
        pinState[pathX[0], pathY[0]] = 2;
        pinState[pathX[^1], pathY[^1]] = 3;

        //1
        for (int i = 1; i < pathX.Length - 1; i++)
        {
            pinState[pathX[i], pathY[i]] = 1;
        }

        //6, 7
        float subp = markLc - p; //소위치
        if (subp <= 0.08)
        {
            switch (pinState[pathX[p], pathY[p]])
            {
                case 1:
                    pinState[pathX[p], pathY[p]] = 6; break;
                case 2:
                    GStt[0] = 1; break;
                case 3:
                    GStt[1] = 1; break;
            }
        }
        else if (subp >= 0.92)
        {
            switch (pinState[pathX[p + 1], pathY[p + 1]])
            {
                case 1:
                    pinState[pathX[p + 1], pathY[p + 1]] = 6; break;
                case 3:
                    GStt[1] = 1; break;
            }
        }
        else
        {
            switch (pinState[pathX[p], pathY[p]])
            {
                case 1:
                    pinState[pathX[p], pathY[p]] = 7; break;
                case 2:
                    GStt[0] = 2; break;
            }
            switch (pinState[pathX[p + 1], pathY[p + 1]])
            {
                case 1:
                    pinState[pathX[p + 1], pathY[p + 1]] = 7; break;
                case 3:
                    GStt[1] = 2; break;
            }
        }

        for (int i = 0; i < pathX.Length; i++) //경로 핀 방향 설정
            {
                pathDir[pathX[i], pathY[i]] = iDir[i + 1];
            }
    }

    void WhiteExpn()
    {
        int i = GStt[2] == 0 ? 0 : pathX.Length - 1;
        int ii = GStt[2] == 0 ? 1 : pathX.Length - 2;
        int j = GStt[2] == 0 ? 4 : 5;
        int[,] map = loopBuildings.map;
        bool hb = true;
        bool b = true;
        int evnt1 = mapEvent.eventTime[1];

        int k = pathY[i] + 1; while (k < mapL)
        {
            if (pinState[pathX[i], k] == 0)
            {
                switch (map[pathX[i], k])
                {
                    case 5: case 7: case 8: case 14: hb = false; break;
                    case 1: case 2: case 4: case 10: hb = false; b = false; break;
                }
                if (hb) { break; }
                else if (b)
                {
                    pinState[pathX[i], k] = j; hb = true; break;
                }
                else
                {
                    pinState[pathX[i], k] = j; k++; hb = true; b = true;
                }
            }
            else if (k == pathY[i] + 1 && pathY[ii] != k && evnt1 == 1)
            {
                CreateMoreWhite(0);
                break;
            }
            else
            {
                break;
            }
        }
        k = pathY[i] - 1; while (k >= 0)
        {
            if (pinState[pathX[i], k] == 0)
            {
                switch (map[pathX[i], k])
                {
                    case 3: case 6: case 9: case 12: hb = false; break;
                    case 1: case 2: case 4: case 10: hb = false; b = false; break;
                }
                if (hb) { break; }
                else if (b)
                {
                    pinState[pathX[i], k] = j; hb = true; break;
                }
                else
                {
                    pinState[pathX[i], k] = j; k--; hb = true; b = true;
                }
            }
            else if (k == pathY[i] - 1 && pathY[ii] != k && evnt1 == 1)
            {
                CreateMoreWhite(1);
                break;
            }
            else
            {
                break;
            }
        }
        k = pathX[i] + 1; while (k < mapL)
        {
            if (pinState[k, pathY[i]] == 0)
            {
                switch (map[k, pathY[i]])
                {
                    case 2: case 8: case 9: case 15: hb = false; break;
                    case 1: case 3: case 5: case 11: hb = false; b = false; break;
                }
                if (hb) { break; }
                else if (b)
                {
                    pinState[k, pathY[i]] = j; hb = true; break;
                }
                else
                {
                    pinState[k, pathY[i]] = j; k++; hb = true; b = true;
                }
            }
            else if (k == pathX[i] + 1 && pathX[ii] != k && evnt1 == 1)
            {
                CreateMoreWhite(2);
                break;
            }
            else
            {
                break;
            }
        }
        k = pathX[i] - 1; while (k >= 0)
        {
            if (pinState[k, pathY[i]] == 0)
            {
                switch (map[k, pathY[i]])
                {
                    case 4: case 6: case 7: case 13: hb = false; break;
                    case 1: case 3: case 5: case 11: hb = false; b = false; break;
                }
                if (hb) { break; }
                else if (b)
                {
                    pinState[k, pathY[i]] = j; hb = true; break;
                }
                else
                {
                    pinState[k, pathY[i]] = j; k--; hb = true; b = true;
                }
            }
            else if (k == pathX[i] - 1 && pathX[ii] != k && evnt1 == 1)
            {
                CreateMoreWhite(3);
                break;
            }
            else
            {
                break;
            }
        }
    }

    void CreateMoreWhite(int dir)
    {
        int i = GStt[2] == 0 ? 0 : pathX.Length - 1;
        int j = GStt[2] == 0 ? 4 : 5;
        float prex = pathX[i] * 1.25f;
        float prey = pathY[i] * 1.25f;
        float x = prex;
        float y = prey;
        switch (dir)
        {
            case 0: y = (pathY[i] + 1) * 1.25f; break;
            case 1: y = (pathY[i] - 1) * 1.25f; break;
            case 2: x = (pathX[i] + 1) * 1.25f; break;
            case 3: x = (pathX[i] - 1) * 1.25f; break;
        }

        GameObject[] wpins = new GameObject[8];
        Sprite[] pinSprites = Resources.LoadAll<Sprite>("Images/Pins");
        
        wpins[((dir + 1) * (GStt[2] == 0 ? 1 : 2)) - 1] = Instantiate(wPrefab, 
            new Vector3(Average(x, prex), Average(y, prey), -2),
            Quaternion.identity);
        wpins[((dir + 1) * (GStt[2] == 0 ? 1 : 2)) - 1].transform.SetParent(gridPins, false);
        wpins[((dir + 1) * (GStt[2] == 0 ? 1 : 2)) - 1].name = "WPin " + GStt[2] + "," + dir;
        wpins[((dir + 1) * (GStt[2] == 0 ? 1 : 2)) - 1].AddComponent<WhitePinAct>();

        SpriteRenderer wpinImage = wpins[((dir + 1) * (GStt[2] == 0 ? 1 : 2)) - 1].GetComponent<SpriteRenderer>();
        wpinImage.sprite = pinSprites[15];

        static float Average(float a, float b)
        {
            return (a + b) / 2;
        }
    }
}
