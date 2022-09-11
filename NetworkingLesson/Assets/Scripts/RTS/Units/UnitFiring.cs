using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] private float fireRange = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float rotationSpeed = 20f;

    private float lastFireTime;

    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        if (target == null) return;
        if (!CanFireAtTarget()) return;

        //Giriamo la unit in modo che guardi al bersaglio
        Quaternion targetRotation =
            Quaternion.LookRotation(target.transform.position - transform.position);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        //Sparo il proiettile, in base al mio firerate
        if(Time.time > (1 / fireRate) + lastFireTime)
        {
            //Ruoto il proiettile verso l'empty del target che fa da bersaglio
            Quaternion projectileRotation = Quaternion.LookRotation(
                target.GetAimAtPoint().position - projectileSpawnPoint.position);

            //Istanzio il proiettile
            GameObject projectileInstance = Instantiate(
                projectilePrefab, projectileSpawnPoint.position, projectileRotation);
            //Spawno sul server
            NetworkServer.Spawn(projectileInstance, connectionToClient);

            //aggiorno il time per calcolare il rateo di fuoco
            lastFireTime = Time.time;
        }
    }

    /// <summary>
    /// Controllo se sono in condizione di sparare
    /// </summary>
    /// <returns></returns>
    [Server]
    private bool CanFireAtTarget()
    {
        return (targeter.GetTarget().transform.position - transform.position).sqrMagnitude <= fireRange * fireRange;
    }
}
