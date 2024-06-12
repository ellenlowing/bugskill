using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayFlyDetection : BasePowerUpBehavior
{

    [Range(0, 100f)] public float m_MaxDistance;
    [Range(0, 10f)] public float ScaleFactor = 0.2f;
    [Range(0, 10f)] public float TimeBtwChecks = 1.0f;
    [SerializeField] private GameObject DeadFly;
    [SerializeField] private SettingSO settings;
    float m_Speed;
    private bool m_HitDetect;

    public Vector3 bounds = new Vector3(0.1f, 0.1f, 0.1f);
    RaycastHit m_Hit;

    public bool isStreaming = false;
    private float initialTime = 0.0f;


    public void CheckSteamOut()
    {
        isStreaming = true;
    }

    public override void UpdateIdleState()
    {
        if (isStreaming)
        {
            if (initialTime > TimeBtwChecks)
            {
                initialTime = 0;
                isStreaming = false;

            }
            initialTime += Time.deltaTime;

        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (isStreaming)
        {
            if (other.CompareTag("Fly"))
            {
                Instantiate(DeadFly, other.transform.position, Quaternion.Euler(Random.Range(0,360), Random.Range(0, 360), Random.Range(0, 360)));
                Destroy(other.gameObject);
                settings.score += 15;
            }
        }
     
    }


 

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;


        if (m_HitDetect)
        {

            Gizmos.DrawRay(transform.position, transform.forward * m_Hit.distance);
            Gizmos.DrawWireCube(transform.position + transform.forward * m_Hit.distance, transform.localScale);
        }
        else
        {
            Gizmos.DrawRay(transform.position, transform.forward * m_MaxDistance);
            Gizmos.DrawWireCube(transform.position + transform.forward * m_MaxDistance, transform.localScale);
        }
    }
}

