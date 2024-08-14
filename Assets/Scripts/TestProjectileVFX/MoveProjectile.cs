using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveProjectile : MonoBehaviour
{
    public float speed;
    public GameObject muzzlePrefab;
    public GameObject hitPrefab;

    void Start()
    {
        if (muzzlePrefab != null)
        {
            var muzzle = Instantiate(muzzlePrefab, transform.position, Quaternion.identity);
            muzzle.transform.forward = gameObject.transform.forward;

            var muzzlePs = muzzle.GetComponent<ParticleSystem>();
            if (muzzlePs != null)
            {
                Destroy(muzzle, muzzlePs.main.duration);
            }
            else
            {
                var psChild = muzzle.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(muzzle, psChild.main.duration);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision other)
    {
        ContactPoint contact = other.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point;

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
}
