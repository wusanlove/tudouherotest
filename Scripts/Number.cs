using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Number : MonoBehaviour
{
    public TMP_Text text;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 1f);
    }

    // Update is called once per frame
    void Update()
    {

    }
}