using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboarding : MonoBehaviour
{
    private Transform camera;

    private void Start()
    {
        camera = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + camera.rotation * Vector3.forward, camera.rotation * Vector3.up);
    }
}
