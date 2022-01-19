using System;
using UdonSharp;
using UnityEngine;

public class UdonSharpTest : UdonSharpBehaviour
{
    void Start()
    {
        Test();
    }

    void Update()
    {
        Test();
    }

    void Test()
    {
        var startTick = DateTime.Now.Ticks;
        var total = 0;
        for (var i = 0; i < 1000; i++)
        {
            total += i;
        }
        var endTick = DateTime.Now.Ticks;
        Debug.Log(endTick - startTick);
    }
}
