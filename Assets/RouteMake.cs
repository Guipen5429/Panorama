using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class RouteMake : MonoBehaviour
{
    public int[,] pinState = new int[5, 5]; //«…¿« ªÛ≈¬
    public int[,] pathDir = new int[5, 5];
    int[] pathX; //∞Ê∑Œ x¡¬«•
    int[] pathY; //∞Ê∑Œ y¡¬«•
    int[] GStt;

    public bool callR;
    public int evntR;
    public bool rcv;
    public bool rcv2;

    void Start()
    {
        callR = false;
        rcv = false;
        rcv2 = true;
    }

    private void Update()
    {
        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        int evnt0 = mapMove.eventTime[0];
        int evnt1 = mapMove.eventTime[1];
        bool go = mapMove.go;
        if (evnt0 == 5 || evnt0 == 9) { rcv = false; rcv2 = true; callR = false; } //default
        if (evnt0 == 3 || evnt0 == 6 || evnt0 == 4 && rcv2) { rcv = true; }

        //R≈∞ ¥≠∑∂¿ª ∂ß
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

        //«… ªÛ≈¬ πŸ≤Ÿ±‚
        if (evnt0 == 4 && !callR && go && rcv)
        {
            PathMake pathMake = GameObject.Find("Map").GetComponent<PathMake>();
            pathX = pathMake.pathX;
            pathY = pathMake.pathY;
            GStt = pathMake.GStt;

            RouteStt();
            WhiteExpn();

            switch (evnt1)
            {
                case 0: evntR = 9; break;
                case 1: evntR = 5; break;
                default: break;
            }
            callR = true;
            rcv = false;
            rcv2 = false;
        }
    }

    void RouteStt()
    {
        LoopBuildings loopBuildings = GameObject.Find("BackGround").GetComponent<LoopBuildings>();
        int[] iDir = loopBuildings.cameraDir;

        for (int i = 0; i < 5; i++) //«… ªÛ≈¬ √ ±‚»≠
        {
            for (int j = 0; j < 5; j++)
            {
                pinState[i, j] = 0;
            }
        }

        pinState[pathX[0], pathY[0]] = 2;
        pinState[pathX[^1], pathY[^1]] = 3;

        for (int i = 1; i < pathX.Length - 1; i++)
        {
            pinState[pathX[i], pathY[i]] = 1;
        }

        for (int i = 0; i < pathX.Length; i++) //∞Ê∑Œ «… º≥¡§
        {
            pathDir[pathX[i], pathY[i]] = iDir[i + 1];
        }
    }

    void WhiteExpn()
    {
        int i = GStt[2] == 0 ? 0 : pathX.Length - 1;
        int j = GStt[2] == 0 ? 4 : 5;

        int k = pathY[i] + 1; while (k < 5)
        {
            if (pinState[pathX[i], k] == 0)
            {
                pinState[pathX[i], k] = j; k++;
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
                pinState[pathX[i], k] = j; k--;
            }
            else
            {
                break;
            }
        }
        k = pathX[i] + 1; while (k < 5)
        {
            if (pinState[k, pathY[i]] == 0 || pinState[k, pathY[i]] == 4)
            {
                pinState[k, pathY[i]] = j; k++;
            }
            else
            {
                break;
            }
        }
        k = pathX[i] - 1; while (k >= 0)
        {
            if (pinState[k, pathY[i]] == 0 || pinState[k, pathY[i]] == 4)
            {
                pinState[k, pathY[i]] = j; k--;
            }
            else
            {
                break;
            }
        }
    }
}
