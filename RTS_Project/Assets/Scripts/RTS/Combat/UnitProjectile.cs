using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float destroyAfterSeconds = 5f;
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private int damage = 20;

    private void Start()
    {
        rb.velocity = transform.forward * projectileSpeed;
    }

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }

    [Server]
    private void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out NetworkIdentity networkIdentity))
        {
            if (networkIdentity.connectionToClient == connectionToClient) return;
        }

        if(other.TryGetComponent(out Health health))
        {
            health.TakeDamage(damage);
        }

        DestroySelf();
    }
}
