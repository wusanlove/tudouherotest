using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class WeaponBase : MonoBehaviour
{
    public WeaponData data;  //武器数据
  
   
    public bool isAttack = false; //是否可以攻击, 必须在范围内检测到敌人
    public bool isCooling = false; //攻击冷却 
    public bool isAiming = true; //是否自动瞄准 
    protected float attackTimer = 0 ; //攻击计时器
    protected Transform enemy; // 要攻击的敌人
    
    protected float originZ; //原始z轴旋转
    protected float moveSpeed; //近战武器移动速度
        
    
    public void Awake()
    {
        originZ = transform.eulerAngles.z;
    }

    public void Start()
    {
        //暴击
        data.critical_strikes_probability *= GameManager.Instance.propData.critical_strikes_probability;
        
        //近战修改 伤害 范围 冷却时间
        if (data.isLong == 0)
        {
            data.range *= GameManager.Instance.propData.short_range;
            data.damage *= GameManager.Instance.propData.short_damage* data.grade;
            data.cooling /= GameManager.Instance.propData.short_attackSpeed; 
        }else if (data.isLong == 1)
        {
            data.range *= GameManager.Instance.propData.long_range;
            data.damage *= GameManager.Instance.propData.long_damage* data.grade;
            data.cooling /= GameManager.Instance.propData.long_attackSpeed; 
        }

    }

    public void Update()
    {   
        //TODO：用事件管理系统监听游戏状态
        if (GameManager.Instance.isDead)
            return;
        // 自动瞄准
        if (isAiming)
        {
            Aiming();  
        }
         
        
        
        // 判断攻击
        if (isAttack && !isCooling)
        {
            Fire();
        }
     

        //攻击计时器冷却
        if (isCooling)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer >= data.cooling)
            {
                attackTimer = 0;
                isCooling = false;
            }
            
        }


    }
    
    
    private void Aiming()
    {
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(
            transform.position, data.range,
            LayerMask.GetMask("Enemy"));
        
        //当前有敌人
        if (enemiesInRange.Length > 0)
        {
            isAttack = true;
            Collider2D nearestEnemy = enemiesInRange
                .OrderBy(enemy => Vector2.Distance(transform.position, enemy.transform.position))
                .First();

            enemy = nearestEnemy.transform;

            Vector2 enemyPos = enemy.position;
            Vector2 direction =  enemyPos - (Vector2)transform.position;
            float angleDegrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            transform.eulerAngles =
                new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, angleDegrees + originZ);
            

        }
        else
        {
            isAttack = false;
            enemy = null;
            transform.eulerAngles =  new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, originZ);
        }
        
    }

    public virtual void Fire()
    {
    
    }
    
    
    //计算是否暴击
     public bool CriticalHits()
     {
         float randomValue = Random.Range(0,1f);  
         return randomValue < data.critical_strikes_probability;
    
     } 
     
     
     
}