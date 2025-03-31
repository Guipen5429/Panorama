using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterMove : MonoBehaviour
{
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
    public string ignoredTag;


    void Start()
    {
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

        MapEvent mapMove = GameObject.Find("Map").GetComponent<MapEvent>();
        Transform pt = GameObject.Find("Player").transform;
        int evnt0 = mapMove.eventTime[0];
        int evnt1 = mapMove.eventTime[1];
        int evnt2 = mapMove.eventTime[2];

        LoopBuildings loopie = GameObject.Find("BackGround").GetComponent<LoopBuildings>();
        float f = loopie.preSum;
        float newPos = 0;

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

            newPos = pt.position.x;

            if (evnt0 == 0) { pt.position = new Vector3(newPos, pt.position.y, pt.position.z); }
            
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == ignoredTag)
        {
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
        }
    }

    void Commander()
    {
        com[0] = (Math.Abs(Input.GetAxis("Horizontal")) != 0 && !(Input.GetKey(KeyCode.Space))) ? true : false;
        com[1] = (Input.GetKey(KeyCode.Space) && !(Input.GetKey(KeyCode.C))) ? true : false;
        com[3] = (Input.GetKeyDown(KeyCode.X)) ? true : false;
        com[2] = (Input.GetKeyDown(KeyCode.C) && !(Input.GetKey(KeyCode.Space))) ? true : false;
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
    void FixedUpdate()
    {
        
    }
}