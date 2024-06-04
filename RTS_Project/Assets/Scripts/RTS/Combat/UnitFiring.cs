using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitFiring : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Targeter targeter;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform muzzle;

    [Header("Stats")]
    [SerializeField] private float fireRange = 10f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float rotationSpeed = 20f;

    private float lastTimeFired;

    [ServerCallback]
    private void Update()
    {
        if (!CanFire()) return;

        Quaternion targetRotation = Quaternion.LookRotation(targeter.Target.transform.position - transform.position);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if(Time.time > (1 / fireRate) + lastTimeFired)
        {
            Quaternion projectileRotation = Quaternion.LookRotation(targeter.Target.AimPoint.position - muzzle.position);
            GameObject projectileInstance = Instantiate(projectilePrefab, muzzle.position, projectileRotation);

            NetworkServer.Spawn(projectileInstance, connectionToClient);

            lastTimeFired = Time.time;
        }
    }

    [Server]
    private bool CanFire()
    {
        if (targeter.Target == null) return false;
        return (targeter.Target.transform.position - transform.position).sqrMagnitude <= fireRange * fireRange;
    }
}
