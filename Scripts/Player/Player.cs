using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
class Player : BaseMgrMono<Player>
{
   
    
    public Transform playerVisualTransform;
    public Transform weaponsPos;
    [SerializeField]Animator playerAnimator;
    private float reviveTimer=0;
    public override void Awake()
    {
        base.Awake();
        playerVisualTransform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(GameManager.Instance.currentRoleData.avatar);
        // 初始化代码
        GameManager.Instance.weaponsPos = weaponsPos;
        if(GameManager.Instance.waveCount==1)
            GameManager.Instance.InitProp();
    }

    private void Start()
    {
        // 通过进度服务检查并解锁"公牛"
        SaveProgressService.Instance.TryUnlockRole(
            "公牛",
            GameManager.Instance.propData.maxHp >= 50,
            GameManager.Instance.roleDatas);
    }

    private void Update()
    {
        if (GameManager.Instance.isDead)
            return;
        Move();
        Revive(); //生命再生
        EatMoney(); //吃金币
        
    }
    private void Revive()
    {
        reviveTimer += Time.deltaTime;
        if (reviveTimer < 1f) return;
        reviveTimer = 0;

        if (GameManager.Instance.propData.revive <= 0) return;

        GameManager.Instance.hp = Mathf.Clamp(
            GameManager.Instance.hp + GameManager.Instance.propData.revive
            ,0,  GameManager.Instance.propData.maxHp);
        
        //公牛生命再生翻倍 
        if (GameManager.Instance.currentRoleData.name == "公牛")
        {
            GameManager.Instance.hp = Mathf.Clamp(
                GameManager.Instance.hp + GameManager.Instance.propData.revive
                ,0,  GameManager.Instance.propData.maxHp);
        }

        // 通过事件通知 GamePanel 更新血条
        EventCenter.Instance.EventTrigger(E_EventType.Player_HpChanged, GameManager.Instance.hp);
    }

    private void EatMoney()
    {
        Collider2D[] moenyInRange = Physics2D.OverlapCircleAll(
            transform.position, 0.5f * GameManager.Instance.propData.pickRange , LayerMask.GetMask("Item")
        );

        if (moenyInRange.Length>0)
        {
            for (int i = 0; i < moenyInRange.Length; i++)
            {
                Destroy(moenyInRange[i].gameObject);

                GameManager.Instance.money += 1;
                // 通过事件通知 GamePanel 更新金币显示
                EventCenter.Instance.EventTrigger(E_EventType.Player_MoneyChanged, GameManager.Instance.money);
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Money"))
        {
            //拾取金币
            GameManager.Instance.money += 1;
            EventCenter.Instance.EventTrigger(E_EventType.Player_MoneyChanged, GameManager.Instance.money);
            Destroy(other.gameObject);
        }
    }

    void Move()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        movement.Normalize();
        transform.Translate(movement * GameManager.Instance.speed*GameManager.Instance.propData.speedPer * Time.deltaTime);
        if (movement.magnitude != 0 )
        {
            playerAnimator.SetBool("isMove", true);
        }
        else
        {
            playerAnimator.SetBool("isMove", false); 
        }

        TurnAround(moveHorizontal);
    }

    void TurnAround(float moveHorizontal)
    {
        SpriteRenderer sr = playerVisualTransform.GetComponent<SpriteRenderer>();
        if (moveHorizontal > 0)
            sr.flipX = false;
        else if (moveHorizontal < 0)
            sr.flipX = true;
    }

    public void Injured(float attack)
    {
        
        if (GameManager.Instance.isDead)
        {
            return;
        }
        
        //判断本次攻击是否会死亡
        if (GameManager.Instance.hp - attack <= 0 )
        {
            GameManager.Instance.hp = 0;
            // 通知血量变化后再调用 Dead
            EventCenter.Instance.EventTrigger(E_EventType.Player_HpChanged, GameManager.Instance.hp);
            Dead();
        }
        else
        {
            GameManager.Instance.hp -= attack;

            //音效 – 通过事件系统发送，不直接 Instantiate
            AudioService.Instance.PlayHurtSfx();
        }
        
        //通过事件通知 GamePanel 更新血条
        EventCenter.Instance.EventTrigger(E_EventType.Player_HpChanged, GameManager.Instance.hp);
    }

    public void Dead()
    {
        GameManager.Instance.isDead = true;
        // 广播玩家死亡事件
        EventCenter.Instance.EventTrigger(E_EventType.Player_Dead);
        StartCoroutine(Die());
    }

    IEnumerator Die()
    {
        yield return new WaitForSeconds(2f);
        // 通过事件通知，让 LevelControl 处理失败逻辑
        EventCenter.Instance.EventTrigger(E_EventType.Game_Lose);
    }
}