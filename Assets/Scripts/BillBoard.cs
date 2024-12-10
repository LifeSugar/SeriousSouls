using System;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        // 让Canvas始终背向摄像机
        transform.LookAt(transform.position - mainCamera.transform.rotation * Vector3.forward,
            mainCamera.transform.rotation * Vector3.up);
    }
}