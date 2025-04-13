using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class CharacterMove : MonoBehaviour
{
    public GameObject Map;
    MapEvent mapEvent;
    MarkMake markMake;
    public GameObject BackGround;
    LoopBuildings loopBuildings;

    public float moveSpeed; //�̵� �ӷ�
    public float jumpPower; //���� ũ��

    public Transform pos; //�� Ȯ�� �ڽ� ������Ʈ
    public LayerMask isLayer; //��
    private float groundCheckRadius = 0.1f; //�� Ȯ�� ����
    private bool isGround; //�� Ȯ��

    private float dir = 0; //�ٶ󺸴� ���� �ʱ�ȭ
    private bool isChop = false; //��� �ʱ�ȭ
    private bool[] com = new bool[4];

    Rigidbody2D rb;
    SpriteRenderer rd;
    Animator anim;

    public float radius = 50f;
    public float angle = 90f;

    void Start()
    {
        mapEvent = Map.GetComponent<MapEvent>();
        markMake = Map.GetComponent<MarkMake>();
        loopBuildings = BackGround.GetComponent<LoopBuildings>();

        rb = GetComponent<Rigidbody2D>(); //�̵�
        rd = GetComponent<SpriteRenderer>(); //����
        anim = GetComponent<Animator>(); //�ִϸ��̼�
    }

    void Update()
    {
        isGround = Physics2D.OverlapCircle(pos.position, groundCheckRadius, isLayer); //�� Ȯ��

        //�ִϸ��̼� ��ȯ
        anim.SetBool("IsJump", !isGround || com[1]);
        anim.SetBool("IsChop", (com[2] && isGround));
        anim.SetBool("IsSlay", com[3]);
        dir = Input.GetAxisRaw("Horizontal"); //���� ����

        Transform pt = GameObject.Find("Player").transform;
        int evnt0 = mapEvent.eventTime[0];

        if (evnt0 == 0)
        {
            Commander();

            if (!isChop) //�̵�, ����, ������ȯ
            {
                Move();
                Jump();

                if (dir == 1) //���⿡ ���� ��ȯ
                {
                    rd.flipX = false;
                }
                else if (dir == -1)
                {
                    rd.flipX = true;
                }
            }

            pt.position = new Vector3(pt.position.x, pt.position.y, pt.position.z);
        }
        if (evnt0 == 9)
        {
            //pt.position = new Vector3(markMake.Pos, pt.position.y, pt.position.z);
            pt.position = new Vector3(InverseTransform(), pt.position.y, pt.position.z);
        }
    }

    void Commander()
    {
        com[0] = (Math.Abs(Input.GetAxis("Horizontal")) != 0 && !(Input.GetKey(KeyCode.Space))) ? true : false;
        com[1] = (Input.GetKey(KeyCode.Space) && !(Input.GetKey(KeyCode.C))) ? true : false;
        com[2] = Input.GetKey(KeyCode.LeftShift) && (Input.GetMouseButtonDown(0) && !(Input.GetKey(KeyCode.Space))) ? true : false;
        com[3] = (!Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0)) ? true : false;
    }
    void Move()
    {
        float v = Input.GetAxis("Horizontal"); //�̵� Ű ����
        rb.velocity = new Vector2(v * moveSpeed, rb.velocity.y); //�̵�
        anim.SetBool("IsRun", Math.Abs(Input.GetAxis("Horizontal")) > 0.2);
    }
    void Jump()
    {
        if (isGround && com[1]) //���� Ű ����
        {
            rb.velocity = Vector2.up * jumpPower; //���� ����
            anim.SetBool("IsJump", true); //���� �ִϸ��̼�
        }
    }
    public void AttackStart() //��� ����
    {
        isChop = true;
    }
    public void AttackEnd()
    {
        isChop = false;
    }
    float InverseTransform()
    {
        float markLc = markMake.markLc;
        int p = (int)markLc;
        float subp = markLc - p; //����ġ
        if (subp <= 0.08)
        {
            markLc = p;
        }
        else if (subp >= 0.92f)
        {
            p++; markLc = p;
        }
        subp = markLc - p;

        float[] routePoint = loopBuildings.routePoint;
        int pPoint = loopBuildings.pathPoint[p];
        float[] left = loopBuildings.leftIns;
        float[] right = loopBuildings.rightIns;
        float bound;
        switch (pPoint)
        {
            case 0: case 1: case 2:
                bound = right[pPoint];
                break;
            case 4: case 10:
                bound = left[3] + right[3];
                break;
            case 5:
                bound = left[5] + right[5] - 1.9f;
                break;
            case 6:
                bound = left[3] + right[3] + 1.9f;
                break;
            case 7:
                bound = right[7] + left[3] + right[3];
                break;
            case 12:
                bound = right[pPoint];
                break;
            default:
                bound = 0;
                break;
        }
        float pos = 0; //���� ��ġ
        if (markLc <= routePoint.Length - 1 && markLc > 0)
        {
            if (subp <= 0.5f && (pPoint != 10 || pPoint != 12))
            {
                pos = routePoint[p] + (bound * (subp * 2));
            }
            else
            {
                pos = routePoint[p] + bound + ((routePoint[p + 1] - routePoint[p] - bound) * ((subp - 0.5f) * 2));
            }
        }

        return pos;
    }
}