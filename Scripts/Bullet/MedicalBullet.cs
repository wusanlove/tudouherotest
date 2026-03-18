using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicalBullet : Bullet
{
    public new void Awake()
    {
        base.Awake();

        tagName = "Enemy"; 
    }
    
    
}