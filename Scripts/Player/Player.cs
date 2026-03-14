using System.Collections;
using UnityEngine;

/// <summary>
/// 玩家控制器：只管移动、受伤、死亡逻辑，
/// 状态变化通过 EventCenter 广播，GamePanel 订阅后自行刷新 UI，两者解耦。
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
        playerVisualTransform.GetComponent<SpriteRenderer>().sprite
            = Resources.Load<Sprite>(GameManager.Instance.currentRoleData.avatar);
        GameManager.Instance.weaponsPos = weaponsPos;

        if (GameManager.Instance.waveCount == 1)
            GameManager.Instance.InitProp();
    }

    private void Start()
    {
        // 解锁条件：公牛（最大生命值达到 50）
        if (GameManager.Instance.propData.maxHp >= 50)
        {
            if (!SaveService.Instance.IsRoleUnlocked("公牛"))
            {
                SaveService.Instance.UnlockRole("公牛");
                Debug.Log("公牛解锁");
                SyncRoleUnlockToConfig("公牛");
            }
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

        float gain = GameManager.Instance.propData.revive;
        if (GameManager.Instance.currentRoleData.name == "公牛") gain *= 2f;

        GameManager.Instance.hp = Mathf.Clamp(
            GameManager.Instance.hp + gain, 0, GameManager.Instance.propData.maxHp);

        // 广播 HP 变化，GamePanel 订阅后刷新
        EventCenter.Instance.EventTrigger(E_EventType.GamePlay_HpChanged, GameManager.Instance.hp);
    }

    private void EatMoney()
    {
        Collider2D[] inRange = Physics2D.OverlapCircleAll(
            transform.position,
            0.5f * GameManager.Instance.propData.pickRange,
            LayerMask.GetMask("Item"));

        for (int i = 0; i < inRange.Length; i++)
        {
            Destroy(inRange[i].gameObject);
            GameManager.Instance.money += 1;
            EventCenter.Instance.EventTrigger(E_EventType.GamePlay_MoneyChanged, GameManager.Instance.money);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Money"))
        {
            GameManager.Instance.money += 1;
            EventCenter.Instance.EventTrigger(E_EventType.GamePlay_MoneyChanged, GameManager.Instance.money);
            Destroy(other.gameObject);
        }
    }

    private void Move()
    {
        float moveH = Input.GetAxis("Horizontal");
        float moveV = Input.GetAxis("Vertical");
        Vector2 movement = new Vector2(moveH, moveV).normalized;
        transform.Translate(movement * GameManager.Instance.speed
                            * GameManager.Instance.propData.speedPer * Time.deltaTime);

        playerAnimator.SetBool("isMove", movement.magnitude != 0);
        TurnAround(moveH);
    }

    private void TurnAround(float moveH)
    {
        SpriteRenderer sr = playerVisualTransform.GetComponent<SpriteRenderer>();
        if (moveH > 0)       sr.flipX = false;
        else if (moveH < 0)  sr.flipX = true;
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
            Instantiate(GameManager.Instance.hurtMusic);
        }

        EventCenter.Instance.EventTrigger(E_EventType.GamePlay_HpChanged, GameManager.Instance.hp);
    }

    public void Dead()
    {
        GameManager.Instance.isDead = true;
        StartCoroutine(Die());
    }

    IEnumerator Die()
    {
        yield return new WaitForSeconds(2f);
        LevelControl.Instance.BadGame();
    }

    // ── 工具：同步解锁状态到 ConfigService 缓存的 RoleData ─
    private void SyncRoleUnlockToConfig(string roleName)
    {
        var roles = ConfigService.Instance.Roles;
        for (int i = 0; i < roles.Count; i++)
            if (roles[i].name == roleName) { roles[i].unlock = 1; break; }
    }
}