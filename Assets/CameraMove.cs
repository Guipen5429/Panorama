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
    }
}
