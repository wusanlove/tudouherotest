using UnityEngine;

public class Enemy5 : EnemyBase
{

    public override void LaunchSkill(Vector2 dir)
    {
        // 1. 获取半径：利用 Bounds 的中心到角的距离（斜边）
        float radius = 0.5f; // 给个默认保底值，防止没获取到组件
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // bounds.extents 是半长宽高 (x, y, z)
            // magnitude 就是向量长度，即中心点到包围盒角的距离
            radius = sr.bounds.extents.magnitude;
        }
        

        int bulletCount = 16;
        float angleStep = 360f / bulletCount; // 结果是 22.5 度

        for (int i = 0; i < bulletCount; i++)
        {
            // 2. 计算当前这颗子弹的角度
            float currentAngle = i * angleStep;

            // 3. 将角度转换为方向向量 (三角函数需要弧度)
            // x = cos(θ), y = sin(θ)
            float angleRad = currentAngle * Mathf.Deg2Rad; // 角度转弧度
            Vector2 bulletDir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

            // 4. 计算生成位置： 原点 + (方向 * 半径)
            // 这里的 transform.position 就是你说的 enemy5center
            Vector3 spawnPos = transform.position + (Vector3)(bulletDir * radius);

            // 5. 生成子弹
            if (GameManager.Instance.enemyBullet_prefab != null)
            {
                GameObject bulletObj = Instantiate(GameManager.Instance.enemyBullet_prefab, spawnPos, Quaternion.identity);
                bulletObj.GetComponent<Bullet>().dir= bulletDir;
               
            }
        }
    }
}