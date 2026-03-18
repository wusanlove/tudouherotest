using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : Bullet
{
    public new void Awake()
    {
        base.Awake();

        tagName = "Player"; 
    }
   
}