using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSize : MonoBehaviour
{
    public GameObject BackGround;
    LoopBuildings loopBuildings;
    public Transform Basetrans;
    public Transform pin;
    public Transform mark;
    public Transform line;

    private void OnEnable()
    {
        loopBuildings = BackGround.GetComponent<LoopBuildings>();
        int mapL = loopBuildings.mapL;
        Basetrans.localScale = new Vector3((mapL == 5 ? 1 : 0.55f), (mapL == 5 ? 1 : 0.55f), 1);
        pin.localPosition = new Vector3((mapL == 5 ? 0 : -2.5f), (mapL == 5 ? 0 : -2.5f), -1);
        mark.localPosition = new Vector3((mapL == 5 ? 0 : -2.5f), (mapL == 5 ? 0 : -2.5f), -1);
        line.localPosition = new Vector3((mapL == 5 ? 0 : -2.5f), (mapL == 5 ? 0 : -2.5f), -1);
    }
}
