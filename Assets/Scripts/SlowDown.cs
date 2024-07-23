using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.Body.Input;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class SlowDown : MonoBehaviour
{
    public bool IsSlowed = false;
    public bool IsDying = false;
    public bool IsDead = false;
    public GameObject DeadFly;
    public SettingSO settings;
    public FlyMovement flyMovements;
    public List<MeshRenderer> NormalEyeMesh;
    public List<MeshRenderer> CircularEyeMesh;

    private float currentSpeed = 0.0f;
    private float initialTime = 0.0f;
    private int totalCash = 0;
    private Rigidbody rb;

    private void Start()
    {
        settings = GameManager.Instance.settings;
        flyMovements = GetComponentInParent<FlyMovement>();
        rb = GetComponentInParent<Rigidbody>();
        Assert.IsNotNull(flyMovements, "Flymovements not assigned");
        Assert.IsNotNull(NormalEyeMesh, "Fly Mesh not Assigned");
        Assert.IsNotNull(CircularEyeMesh, "Fly Circular Mesh not Assigned");
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            DropFly();
        }
#endif

        if (IsSlowed)
        {
            if (initialTime > settings.flyIntelLevels[settings.waveIndex].flySlowDownTime)
            {
                initialTime = 0f;
                IsSlowed = false;
                flyMovements.speed = currentSpeed;
                if (TryGetComponent<FlyAudioSource>(out var bz))
                {
                    bz.MoveClip();
                }
                ChangeEyeType(false);
            }
            initialTime += Time.deltaTime;
        }
    }

    // changes the materials of the eye mesh
    public void ChangeEyeType(bool circular)
    {
        CircularEyeMesh[0].enabled = circular;
        CircularEyeMesh[1].enabled = circular;
        NormalEyeMesh[0].enabled = !circular;
        NormalEyeMesh[1].enabled = !circular;

        CircularEyeMesh[0].transform.gameObject.GetComponent<EyeSpiral>().canRotate = circular;
        CircularEyeMesh[1].transform.gameObject.GetComponent<EyeSpiral>().canRotate = circular;
    }

    IEnumerator Die(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (gameObject != null)
        {
            KillFly();
        }
    }

    public void KillFly()
    {
        settings.Cash += (int)SCOREFACTOR.SPRAY;
        totalCash += (int)SCOREFACTOR.SPRAY;
        UIManager.Instance.IncrementKill(transform.position, totalCash);
        totalCash = 0;
        Destroy(gameObject);
    }

    public void DropFly()
    {
        if (!IsDying)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            flyMovements.enabled = false;
            IsDying = true;

            StartCoroutine(Die(5f));
        }
    }

    public void SlowDownFly()
    {
        IsSlowed = true;
        currentSpeed = flyMovements.speed;
        flyMovements.speed = settings.flyIntelLevels[settings.waveIndex].flySlowDownSpeed;
        if (TryGetComponent<FlyAudioSource>(out var bz))
        {
            bz.DizzyClip();
        }
        ChangeEyeType(true);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == 6)
        {
            foreach (ContactPoint contact in other.contacts)
            {
                if (!IsDead && IsDying && Math.Abs(Vector3.Dot(contact.normal, Vector3.up)) >= 0.9f)
                {
                    rb.isKinematic = true;
                    IsDead = true;
                    StartCoroutine(Die(0.5f));
                }
            }
        }
    }
}
