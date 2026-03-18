using UnityEngine;

public class WeaponLong: WeaponBase
{
    public override void Fire()
    {
        if (isCooling)
        {
            return; 
        }
        
        //获取方向
        Vector2 dir = (enemy.position - transform.position).normalized;
        
        //音效（AudioId 枚举驱动，无需关心音频名称）
        EventCenter.Instance.EventTrigger(E_EventType.Audio_PlaySfx, AudioId.SFX_Shoot);
        
        //创造子弹
        GameObject bullet = GenerateBullet(dir);
        
        //设置子弹头方向
        SetZ(bullet);
        
        //判断是否暴击
        bool isCritical = CriticalHits();
        //设置伤害
        bullet.GetComponent<Bullet>().isCritical = isCritical;
         if (isCritical)
         {
             bullet.GetComponent<Bullet>().damage = data.damage * data.critical_strikes_multiple; 
            
         }
         else
         {
          //  设置伤害
            bullet.GetComponent<Bullet>().damage = data.damage; 
         }
        
        
        bullet.GetComponent<Bullet>().speed = 15f;  //子弹初速度
        
    
        isCooling = true; 
    
    }
    private void SetZ(GameObject bullet)
    {
        bullet.transform.eulerAngles = new Vector3(bullet.transform.eulerAngles.x, bullet.transform.eulerAngles.y
            , transform.eulerAngles.z - originZ);
    }
    public virtual GameObject GenerateBullet(Vector2 dir)
    {
        return null;
    }
}