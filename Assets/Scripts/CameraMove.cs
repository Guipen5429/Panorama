using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public GameObject Map;
    MapEvent mapEvent;
    public GameObject BackGround;
    LoopBuildings loopBuildings;

    public Transform track; //플레이어 위치
    public float distance;

    void Start()
    {
        mapEvent = Map.GetComponent<MapEvent>();
        loopBuildings = BackGround.GetComponent<LoopBuildings>();
    }

    void Update()
    {
        int evnt0 = mapEvent.eventTime[0];
        int evnt1 = mapEvent.eventTime[1];

        if (evnt0 == 0 || evnt1 == 3)
        {
            float s = loopBuildings.sum;
            float i = loopBuildings.initial;
            int left = loopBuildings.route[0];
            int right = loopBuildings.route[^1];

            if (Length(left) <= (track.position.x) && (track.position.x <= s - Length(right)))
            {
                transform.position = new Vector3(track.position.x, 0, -50f);  //플래이어를 추적
            }
            if (track.position.x < Length(left))
            {
                transform.position = new Vector3(Length(left), 0, -50f);  //왼쪽 끝
            }
            if (track.position.x > s - Length(right))
            {
                transform.position = new Vector3(s - Length(right), 0, -50f);  //오른쪽 끝
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
