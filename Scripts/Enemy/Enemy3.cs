using UnityEngine;

public class Enemy3 : EnemyBase
{
    
    public override void LaunchSkill(Vector2 dir)
    {
        GameObject go = Instantiate(GameManager.Instance.enemyBullet_prefab, transform.position, Quaternion.identity);
        go.GetComponent<EnemyBullet>().dir = dir;
    }
}