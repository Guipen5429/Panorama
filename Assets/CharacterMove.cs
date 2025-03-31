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
    public float moveSpeed; //이동 속력
    public float jumpPower; //점프 크기

    public Transform pos; //땅 확인 자식 오브젝트
    public LayerMask isLayer; //땅
    private float groundCheckRadius = 0.1f; //땅 확인 범위
    private bool isGround; //땅 확인

    private float dir = 0; //바라보는 방향 초기화
    private bool isChop = false; //찍기 초기화
    private bool[] com = new bool[4];

    Rigidbody2D rb;
    SpriteRenderer rd;
    Animator anim;

    public float radius = 50f;
    public float angle = 90f;
    public string ignoredTag;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); //이동
        rd = GetComponent<SpriteRenderer>(); //방향
        anim = GetComponent<Animator>(); //애니메이션
    }

    void Update()
    {
        isGround = Physics2D.OverlapCircle(pos.position, groundCheckRadius, isLayer); //땅 확인

        //애니메이션 전환
        anim.SetBool("IsJump", !isGround || com[1]);
        anim.SetBool("IsChop", (com[2] && isGround));
        anim.SetBool("IsSlay", com[3]);
        dir = Input.GetAxisRaw("Horizontal"); //방향 지정

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

            if (!isChop) //이동, 점프, 방향전환
            {
                Move();
                Jump();

                if (dir == 1) //방향에 따라 전환
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
        float v = Input.GetAxis("Horizontal"); //이동 키 선택
        rb.velocity = new Vector2(v * moveSpeed, rb.velocity.y); //이동
        anim.SetBool("IsRun", Math.Abs(Input.GetAxis("Horizontal")) > 0.2);
    }
    void Jump()
    {
        if (isGround && com[1]) //점프 키 선택
        {
            rb.velocity = Vector2.up * jumpPower; //점프 동작
            anim.SetBool("IsJump", true); //점프 애니메이션
        }
    }
    public void AttackStart() //찍기 시작
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