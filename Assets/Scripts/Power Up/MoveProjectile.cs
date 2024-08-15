using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveProjectile : MonoBehaviour
{
    public float speed;
    public GameObject hitPrefab;

    void Start()
    {
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision other)
    {
        ContactPoint contact = other.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point;

        if (other.gameObject.tag == "Fly")
        {
            UIManager.Instance.IncrementKill(transform.position, (int)SCOREFACTOR.FINGERGUN);
            Destroy(other.gameObject);
        }

        if (hitPrefab != null)
        {
            var hit = Instantiate(hitPrefab, pos, rot);
            var hitPs = hit.GetComponent<ParticleSystem>();
            if (hitPs != null)
            {
                Destroy(hit, hitPs.main.duration);
            }
            else
            {
                var psChild = hit.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(hit, psChild.main.duration);
            }
        }
        Destroy(gameObject);
    }

    void OnCollisionStay(Collision other)
    {
        Destroy(gameObject);
    }
}
