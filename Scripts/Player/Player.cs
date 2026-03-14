using System.Collections;
using UnityEngine;

/// <summary>
/// Player — 移动、生命再生、吃金币、受伤、死亡。
///
/// 改进：
///   · 改变 HP / 金币 / 经验后发送 EventCenter 事件，GamePanel 订阅刷新，
///     不再直接调用 GamePanel.Instance.RenewXxx()。
///   · 解耦：Player 不持有对 GamePanel 的硬引用。
/// </summary>
class Player : BaseMgrMono<Player>
{
    public Transform playerVisualTransform;
    public Transform weaponsPos;
    [SerializeField] Animator playerAnimator;
    private float reviveTimer = 0;

    public override void Awake()
    {
        base.Awake();
        if (GameManager.Instance.currentRoleData != null)
            playerVisualTransform.GetComponent<SpriteRenderer>().sprite =
                Resources.Load<Sprite>(GameManager.Instance.currentRoleData.avatar);

        GameManager.Instance.weaponsPos = weaponsPos;
        if (GameManager.Instance.waveCount == 1)
            GameManager.Instance.InitProp();
    }

    private void Start()
    {
        // 解锁"公牛"：maxHp >= 50 且未解锁
        if (GameManager.Instance.propData.maxHp >= 50 && PlayerPrefs.GetInt("公牛") == 0)
        {
            PlayerPrefs.SetInt("公牛", 1);
            var bulls = GameManager.Instance.roleDatas;
            if (bulls != null)
                foreach (var r in bulls)
                    if (r.name == "公牛") r.unlock = 1;
        }
    }

    private void Update()
    {
        if (GameManager.Instance.isDead) return;
        Move();
        Revive();
        EatMoney();
    }

    private void Revive()
    {
        reviveTimer += Time.deltaTime;
        if (reviveTimer < 1f) return;
        reviveTimer = 0;

        if (GameManager.Instance.propData.revive <= 0) return;

        float heal = GameManager.Instance.propData.revive;
        // 公牛生命再生翻倍
        if (GameManager.Instance.currentRoleData?.name == "公牛") heal *= 2;

        GameManager.Instance.hp = Mathf.Clamp(
            GameManager.Instance.hp + heal, 0, GameManager.Instance.propData.maxHp);

        // 通知 GamePanel 刷新（事件驱动，无直接引用）
        EventCenter.Instance.EventTrigger(E_EventType.Game_PlayerHpChanged, GameManager.Instance.hp);
    }

    private void EatMoney()
    {
        Collider2D[] items = Physics2D.OverlapCircleAll(
            transform.position, 0.5f * GameManager.Instance.propData.pickRange,
            LayerMask.GetMask("Item"));

        if (items.Length == 0) return;

        foreach (var c in items)
        {
            Destroy(c.gameObject);
            GameManager.Instance.money += 1;
        }
        EventCenter.Instance.EventTrigger(E_EventType.Game_PlayerMoneyChanged, GameManager.Instance.money);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Money")) return;
        GameManager.Instance.money += 1;
        EventCenter.Instance.EventTrigger(E_EventType.Game_PlayerMoneyChanged, GameManager.Instance.money);
        Destroy(other.gameObject);
    }

    private void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector2 dir = new Vector2(h, v).normalized;
        transform.Translate(dir * GameManager.Instance.speed
            * GameManager.Instance.propData.speedPer * Time.deltaTime);
        playerAnimator.SetBool("isMove", dir.magnitude > 0);
        TurnAround(h);
    }

    private void TurnAround(float h)
    {
        SpriteRenderer sr = playerVisualTransform.GetComponent<SpriteRenderer>();
        if (h > 0)       sr.flipX = false;
        else if (h < 0)  sr.flipX = true;
    }

    public void Injured(float attack)
    {
        if (GameManager.Instance.isDead) return;

        if (GameManager.Instance.hp - attack <= 0)
        {
            GameManager.Instance.hp = 0;
            EventCenter.Instance.EventTrigger(E_EventType.Game_PlayerHpChanged, 0f);
            Dead();
        }
        else
        {
            GameManager.Instance.hp -= attack;
            Instantiate(GameManager.Instance.hurtMusic);
            EventCenter.Instance.EventTrigger(E_EventType.Game_PlayerHpChanged, GameManager.Instance.hp);
        }
    }

    public void Dead()
    {
        GameManager.Instance.isDead = true;
        EventCenter.Instance.EventTrigger(E_EventType.Game_PlayerDead);
        StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        yield return new WaitForSeconds(2f);
        LevelControl.Instance.BadGame();
    }
}