using System.Collections;
using UnityEngine;

/// <summary>
/// 玩家控制器。
///
/// 变更说明：
/// - 金币拾取和血量变化改为通过 EventCenter 广播，GamePanel 监听后刷新（解耦 Player ↔ GamePanel）。
/// - 公牛解锁逻辑委托 ProgressService，移除散落的 PlayerPrefs 直接写入。
/// - 受伤音效改为事件触发（AudioMgr 监听），移除 Instantiate(hurtMusic) 旧方式。
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
        playerVisualTransform.GetComponent<SpriteRenderer>().sprite =
            Resources.Load<Sprite>(GameManager.Instance.currentRoleData.avatar);
        GameManager.Instance.weaponsPos = weaponsPos;
        if (GameManager.Instance.waveCount == 1)
            GameManager.Instance.InitProp();
    }

    private void Start()
    {
        // 公牛解锁检测：委托 ProgressService 统一处理
        if (GameManager.Instance.propData.maxHp >= 50)
            ProgressService.Instance.UnlockRole("公牛");
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
        // 公牛角色生命再生翻倍
        if (GameManager.Instance.currentRoleData.name == "公牛") heal *= 2f;

        GameManager.Instance.hp = Mathf.Clamp(
            GameManager.Instance.hp + heal, 0, GameManager.Instance.propData.maxHp);

        // 通知 HUD 刷新血量
        EventCenter.Instance.EventTrigger(E_EventType.HUD_HpChanged);
    }

    private void EatMoney()
    {
        Collider2D[] inRange = Physics2D.OverlapCircleAll(
            transform.position,
            0.5f * GameManager.Instance.propData.pickRange,
            LayerMask.GetMask("Item")
        );

        foreach (Collider2D col in inRange)
        {
            Destroy(col.gameObject);
            GameManager.Instance.money += 1;
            EventCenter.Instance.EventTrigger(E_EventType.HUD_MoneyChanged);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Money"))
        {
            GameManager.Instance.money += 1;
            EventCenter.Instance.EventTrigger(E_EventType.HUD_MoneyChanged);
            Destroy(other.gameObject);
        }
    }

    private void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector2 movement = new Vector2(h, v).normalized;
        transform.Translate(movement * GameManager.Instance.speed
                                     * GameManager.Instance.propData.speedPer
                                     * Time.deltaTime);
        playerAnimator.SetBool("isMove", movement.magnitude > 0);
        TurnAround(h);
    }

    private void TurnAround(float horizontal)
    {
        var sr = playerVisualTransform.GetComponent<SpriteRenderer>();
        if      (horizontal > 0) sr.flipX = false;
        else if (horizontal < 0) sr.flipX = true;
    }

    public void Injured(float attack)
    {
        if (GameManager.Instance.isDead) return;

        if (GameManager.Instance.hp - attack <= 0)
        {
            GameManager.Instance.hp = 0;
            Dead();
        }
        else
        {
            GameManager.Instance.hp -= attack;
            // 受伤音效：通过 AudioMgr 事件触发（已配置 AudioRegistry key="hurt"）
            EventCenter.Instance.EventTrigger<AudioPlayRequest>(
                E_EventType.Audio_PlaySfx,
                new AudioPlayRequest { key = "hurt" }
            );
        }

        // 通知 HUD 刷新血量
        EventCenter.Instance.EventTrigger(E_EventType.HUD_HpChanged);
    }

    public void Dead()
    {
        GameManager.Instance.isDead = true;
        StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        yield return new WaitForSeconds(2f);
        LevelControl.Instance.BadGame();
    }
}