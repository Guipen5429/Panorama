using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using UnityEngine.Timeline;

public class MarkMake : MonoBehaviour
{
    public GameObject Map;
    MapEvent mapEvent;
    PathMake pathMake;
    RouteMake routeMake;
    public GameObject BackGround;
    LoopBuildings loopBuildings;

    public GameObject mPrefab;

    public bool callM;
    public int evntM;
    bool rcv;
    bool rcv2;

    bool pre;
    int[] pathX;
    int[] pathY;
    public int p;
    public float markLc; //��ũ�� ��ġ
    public float Pos;
    float markX; //��ũ x��ǥ
    float markY; //��ũ y��ǥ

    public float prePos;

    void Start()
    {
        mapEvent = Map.GetComponent<MapEvent>();
        pathMake = Map.GetComponent<PathMake>();
        routeMake = Map.GetComponent<RouteMake>();
        loopBuildings = BackGround.GetComponent<LoopBuildings>();

        callM = false;
        rcv = false;
        rcv2 = true;
    }

    void Update()
    {
        int evnt0 = mapEvent.eventTime[0]; //MapEvent(mapEvent)�� eventTime �迭 �� 0��°�� �ο�
        int evnt1 = mapEvent.eventTime[1];
        bool go = mapEvent.go;

        if (evnt0 == 4) { rcv = false; rcv2 = true; callM = false; } //default
        if (evnt0 == 1 && rcv2) { rcv = true; }

        if (evnt0 == 9) { pre = true; }

        if (evnt0 == 1 && !callM && go && rcv) //��ũ ����
        {
            pathX = pathMake.pathX;
            pathY = pathMake.pathY;

            Transform playerPos = GameObject.Find("Player").transform;
            if (pre) { prePos = playerPos.position.x; pre = false; }
            CreateMarkLc();
            CreateMark();
            prePos = Pos;

            if ((markLc - p <= 0.08 || markLc - p >= 0.92) && evnt1 == 1) //��ũ ���� ����
            {
                Transform gridPins = GameObject.Find("GridPins").transform;
                GameObject.Destroy(GameObject.Find("PlayerMark"));
            }

            evntM = 4;
            callM = true;
            rcv = false;
            rcv2 = false;
        }
    }
    void CreateMarkLc() //��ũ ��ġ
    {
        int pi = pathMake.pi; //��ũ ������
        int[] path = loopBuildings.path;
        int[] routeDir = loopBuildings.routeDir;
        float[] routePoint = loopBuildings.routePoint; //��� �� ����Ʈ �Ÿ�
        float[] left = loopBuildings.leftIns;
        float[] right = loopBuildings.rightIns;
        float preSum = loopBuildings.preSum;

        Pos = preSum == 0 ? prePos : prePos + routePoint[^1] - preSum;

        //�ֱ� ����Ʈ Ư��
        for (int i = 0; i < path.Length; i++)
        {
            if (i == 0 && Pos < routePoint[i])
            {
                p = i;
                break;
            }
            else if (Pos < routePoint[i])
            {
                p = i - 1;
                break;
            }
            else if (i == path.Length - 1 && Pos >= routePoint[i])
            {
                p = i;
                break;
            }
        }

        float pointLc = routePoint[p]; //�ֱ� ����Ʈ �Ÿ�
        markX = pathX[p] * 1.25f;
        markY = pathY[p] * 1.25f;
        int pPoint = loopBuildings.pathPoint[p]; //��� �� ����Ʈ
        float bound; //���

        //��� Ư��
        switch (pPoint)
        {
            case 0: case 1: case 2: case 13: case 14:
                bound = pointLc + right[pPoint];
                break;
            case 4: case 10:
                bound = pointLc + left[3] + right[3];
                break;
            case 5:
                bound = pointLc + left[5] + right[5] - 1.9f;
                break;
            case 6:
                bound = pointLc + left[3] + right[3] + 1.9f;
                break;
            case 7:
                bound = pointLc + right[7] + left[3] + right[3];
                break;
            case 12:
                bound = pointLc + right[pPoint];
                break;
            default:
                bound = pointLc;
                break;
        }

        //����ġ Ư��
        float lB = Pos - pointLc; //���� ��� �Ÿ�
        float rB = Pos - bound; //���� ��� �Ÿ�
        float ww = bound - pointLc; //��� �Ÿ�
        if (Pos <= routePoint[(^1)] && Pos > 0)
        {
            if (rB < 0 && (pPoint != 10 || pPoint != 12))
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
                markLc = p + lB / l / 2;
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
                markLc = p + 0.5f + rB / l / 2;
            }
        }
    }
    void CreateMark() //�÷��̾� ��ġǥ��
    {
        int[,] pathDir = routeMake.pathDir;

        //������Ʈ ����
        GameObject playerMark = Instantiate(mPrefab, new Vector3(markX, markY, -2), Quaternion.identity);
        Transform gridPins = GameObject.Find("GridMark").transform; //�θ� ������Ʈ Transform
        playerMark.transform.SetParent(gridPins, false);
        playerMark.name = "PlayerMark";

        //�̹���, ���� �ο�
        SpriteRenderer markImage = playerMark.GetComponent<SpriteRenderer>();
        Sprite[] pinSprites = Resources.LoadAll<Sprite>("Images/Pins");
        markImage.sprite = pinSprites[12];
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
