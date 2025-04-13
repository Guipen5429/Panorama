using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundScale : MonoBehaviour
{
    public GameObject BackGround;
    LoopBuildings loopBuildings;

    void Start()
    {
        loopBuildings = BackGround.GetComponent<LoopBuildings>();
    }

    void Update()
    {
        float b = loopBuildings.sum;

        this.transform.localScale =
            new Vector2(b + 50, 3); //길이
        this.transform.localPosition =
            new Vector3(b / 2, -9, 2); //원점 위치
    }
}
