using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform track; //�÷��̾� ��ġ
    public float distance;

    void Start()
    {
    }

    void Update()
    {
        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        int evnt0 = mapMove.eventTime[0];
        int evnt1 = mapMove.eventTime[1];

        if (evnt0 == 0)
        {
            LoopBuildings loopie = GameObject.Find("BackGround").GetComponent<LoopBuildings>();
            float s = loopie.sum;
            float i = loopie.initial;
            int left = loopie.route[0];
            int right = loopie.route[^1];

            if (Length(left) <= (track.position.x) && (track.position.x <= s - Length(right)))
            {
                transform.position = new Vector3(track.position.x, 0, -50f);  //�÷��̾ ����
            }
            if (track.position.x < Length(left))
            {
                transform.position = new Vector3(Length(left), 0, -50f);  //���� ��
            }
            if (track.position.x > s - Length(right))
            {
                transform.position = new Vector3(s - Length(right), 0, -50f);  //������ ��
            }

            float Length(int i)
            {
                float length = 0;
                switch (i)
                {
                    case 9: length = 19.7f; break;
                    case 10: length = 12.2f; break;
                    case 11: length = 19.7f; break;
                    case 12: length = 12.2f; break;
                }
                return length;
            }
        }
    }
}
