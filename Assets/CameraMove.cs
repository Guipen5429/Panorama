using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform track; //플레이어 위치
    public float distance;

    void Start()
    {
    }

    void Update()
    {
        LoopBuildings loopie = GameObject.Find("BackGround").GetComponent<LoopBuildings>();
        float f = loopie.sum;
        float i = loopie.initial;

        if (i + 19.7f <= (track.position.x) && (track.position.x <= f - 19.7f))
        {
        transform.position = new Vector3(track.position.x, 0, -10f);  //플래이어를 추적
        }

        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        int evnt0 = mapMove.eventTime[0];
        int evnt1 = mapMove.eventTime[1];
        int evnt2 = mapMove.eventTime[2];

        if (evnt0 == 0) { transform.position = new Vector3(track.position.x, transform.position.y, transform.position.z); }
    }
}
