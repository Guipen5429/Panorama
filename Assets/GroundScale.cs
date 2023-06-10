using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundScale : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        float b = GameObject.Find("BackGround").GetComponent<LoopBuildings>().sum;

        this.transform.localScale =
            new Vector2(b + 50, 3); //길이
        this.transform.localPosition =
            new Vector3(b / 2, -9, 2); //원점 위치
    }
}
