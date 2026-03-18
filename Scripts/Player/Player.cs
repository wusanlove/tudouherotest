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
        if (GameManager.Instance.currentWave == 1)
            GameManager.Instance.InitProp();
    }

    private void Start()
    {
        
        if (GameManager.Instance.propData.maxHp >= 50)
        {
            if (PlayerPrefs.GetInt("公牛") == 0) //TODO:解锁条件可不可以放在一起
            {
                Debug.Log("公牛解锁");
                PlayerPrefs.SetInt("公牛", 1);

                for (int i = 0; i < GameManager.Instance.roleDatas.Count; i++)
                {
                    if (GameManager.Instance.roleDatas[i].name == "公牛")
                    {
                        GameManager.Instance.roleDatas[i].unlock = 1;
                    }
                }
            }
        }
    }

    private void Update()
    {
        //TODO：用事件管理系统监听游戏状态
        if (GameManager.Instance.isDead)
            return;
        Move();
        Revive(); //生命再生
        EatMoney(); //吃金币
        
    }
    private void Revive()
    {
        reviveTimer += Time.deltaTime;
        if (reviveTimer >= 1f)
        {
            //不扣血
            if (GameManager.Instance.propData.revive <= 0)
            {
                return;
            }
            
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
        }
       
        
        reviveTimer = 0;
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
                GamePanel.Instance.RenewMoney();
                

                //文字
                // Number number = Instantiate(GameManager.Instance.number_prefab).GetComponent<Number>();
                // number.text.text = "+1";
                // number.text.color = new Color(86 / 255f, 185/255f, 86/255f);
                // number.transform.position = transform.position+new Vector3(0,0.7f,0);
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Money"))
        {
            //拾取金币
            GameManager.Instance.money += 1;
            GamePanel.Instance.RenewMoney();
            Destroy(other.gameObject);
        }
    }

    void Move()
    {
        //TODO: 使用输入系统获取输入,并用Dotween移动
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
            Dead();
        }
        else
        {
            GameManager.Instance.hp -= attack;

            //文字
            // Number number = Instantiate(GameManager.Instance.number_prefab).GetComponent<Number>();
            // number.text.text = attack.ToString();
            // number.text.color = new Color(255 / 255f, 0, 0);
            // number.transform.position = transform.position+new Vector3(0,0.7f,0);

            
            //音效（AudioId 枚举驱动，无需关心音频名称）
            EventCenter.Instance.EventTrigger(E_EventType.Audio_PlaySfx, AudioId.SFX_PlayerHurt);
        }
        
        //更新血条
        GamePanel.Instance.RenewHp();
    }

    public void Dead()
    {
        //死亡音效，一段时间后弹出失败面板
        GameManager.Instance.isDead = true;
        StartCoroutine(Die());
    }

    IEnumerator Die()
    {
        //播放死亡动画 音效
        //TODO: 播放死亡动画 音效
        
        
        //等待几秒
        yield return new WaitForSeconds(2f);
        //弹出失败面板
        LevelControl.Instance.BadGame();
    }
}