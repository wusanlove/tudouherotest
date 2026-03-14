using System.Collections;
using UnityEngine;

public class Enemy4: EnemyBase
{
    private float timer = 0; //冲锋时间, 0.6f

    public override void LaunchSkill(Vector2 dir)
    {
        StartCoroutine(Charge(dir));
    }

    IEnumerator Charge(Vector2 dir)
    {
        skilling = true;

        while (timer < 0.6f)
        {

            transform.position +=  (Vector3) dir * enemyData.speed * 3f * Time.deltaTime; 
            
            timer += Time.deltaTime;
            yield return null; 
        }



        skilling = false; 
    }
}