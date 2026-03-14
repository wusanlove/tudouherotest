using UnityEngine;

/// <summary>
/// 敌人基类：移动、攻击、技能、受伤、死亡。
/// 死亡时通过 EventCenter 广播经验/金币变化，不直接调用 GamePanel。
/// </summary>
public class EnemyBase : MonoBehaviour
{
    private float attackTimer = 0;
    private bool  isContact   = false;
    private bool  isCooling   = false;

    [SerializeField]
    public EnemyData enemyData;

    protected float skillTimer = 0;
    protected bool  skilling   = false;

    public void Awake() { }

    public void Start()
    {
        // 按名称在 ConfigService 缓存中找到对应数据
        foreach (var data in ConfigService.Instance.Enemies)
        {
            if (data.name == gameObject.name.Replace("(Clone)", "").Trim())
            {
                Init(data);
                break;
            }
        }
    }

    public void Init(EnemyData data)
    {
        // 深拷贝，避免修改原始数据影响其他敌人实例
        this.enemyData = data.Clone();
    }

    public void Update()
    {
        if (GameManager.Instance.isDead) return;

        Move();

        if (isContact && !isCooling)
            Attack();

        if (isCooling)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0) { attackTimer = 0; isCooling = false; }
        }

        UpdateSkill();
    }

    public void SetElite()
    {
        enemyData.hp     *= 2;
        enemyData.damage *= 2;
        GetComponent<SpriteRenderer>().color = new Color(255/255f, 113/255f, 113/255f);
    }

    private void UpdateSkill()
    {
        if (enemyData.skillTime < 0) return;

        if (skillTimer <= 0)
        {
            float dis = Vector2.Distance(transform.position, Player.Instance.transform.position);
            if (dis <= enemyData.range)
            {
                Vector2 dir = (Player.Instance.transform.position - transform.position).normalized;
                LaunchSkill(dir);
                skillTimer = enemyData.skillTime;
            }
        }
        else
        {
            skillTimer -= Time.deltaTime;
            if (skillTimer < 0) skillTimer = 0;
        }
    }

    public virtual void LaunchSkill(Vector2 dir) { }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isContact = true;
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isContact = false;
    }

    public void Move()
    {
        if (skilling) return;
        Vector2 dir = (Player.Instance.transform.position - transform.position).normalized;
        transform.Translate(dir * enemyData.speed * Time.deltaTime);
        TurnAround();
    }

    public void TurnAround()
    {
        float dx = Player.Instance.transform.position.x - transform.position.x;
        transform.localScale = new Vector3(
            dx >= 0.1f ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x),
            transform.localScale.y, transform.localScale.z);
    }

    public void Attack()
    {
        if (isCooling) return;
        Player.Instance.Injured(enemyData.damage);
        isCooling   = true;
        attackTimer = enemyData.attackTime;
    }

    public void Injured(float attack)
    {
        if (enemyData.hp - attack <= 0)
        {
            enemyData.hp = 0;
            Dead();
        }
        else
        {
            enemyData.hp -= attack;
        }
    }

    public void Dead()
    {
        // 广播经验/金币变化，GamePanel 通过事件订阅刷新 UI
        GameManager.Instance.exp += enemyData.provideExp * GameManager.Instance.propData.expMuti;
        EventCenter.Instance.EventTrigger(E_EventType.GamePlay_ExpChanged, GameManager.Instance.exp);

        Instantiate(GameManager.Instance.moeny_prefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}