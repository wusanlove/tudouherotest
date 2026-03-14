using System;

[Serializable]
public class EnemyData
{
    public int id;
    public string name;  //名称 
    public float hp;  //血量
    public float damage;  //伤害
    public float speed;  //速度
    public float attackTime;  //攻击间隔
    public float provideExp;  //提供经验值
    public float skillTime; //技能冷却时间
    public float range; //技能范围

    // 深拷贝/克隆方法
    public EnemyData Clone()
    {
        // 使用 C# 原生的 MemberwiseClone 方法
        return (EnemyData)this.MemberwiseClone();
    }
}