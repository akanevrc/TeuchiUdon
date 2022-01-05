using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UdonSharpTest : UdonSharpBehaviour
{
    public int _start = 100;

    void Start()
    {
        var arr = new int[10];
        foreach (var i in arr)
        {
            Debug.Log(i);
        }
    }
}
