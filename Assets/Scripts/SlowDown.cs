using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class SlowDown : MonoBehaviour
{
    public bool slowDownNow = false;
    public GameObject DeadFly;
    public SettingSO settings;
    public FlyMovement flyMovements;
    public List<MeshRenderer> NormalEyeMesh;
    public List<MeshRenderer> CircularEyeMesh;

    private float currentSpeed = 0.0f;
    private float InitialTime = 0.0f;

    private int totalCash = 0;

    private void Start()
    {
        flyMovements = GetComponentInParent<FlyMovement>();
        Assert.IsNotNull(flyMovements, "Flymovements not assigned");
        Assert.IsNotNull(NormalEyeMesh, "Fly Mesh not Assigned");
        Assert.IsNotNull(CircularEyeMesh, "Fly Circular Mesh not Assigned");
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

    public void DropFly (float DropTime)
    {
        Destroy(this, DropTime);
        StartCoroutine(DropAsap(DropTime));
    }

    IEnumerator DropAsap(float time)
    {
        yield return new WaitForSeconds(time);
        GameObject obj = Instantiate(DeadFly, transform.position, Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
        obj.transform.localScale = this.transform.localScale;


        if (transform.gameObject.transform.localScale == Vector3.one)
        {
            settings.Cash += (int)SCOREFACTOR.SLIM;
            totalCash += (int)SCOREFACTOR.SLIM;
        }
        else
        {
            settings.Cash += (int)SCOREFACTOR.FAT;
            totalCash += (int)SCOREFACTOR.FAT;
        }

        settings.Cash += (int)SCOREFACTOR.SWATTER;
        totalCash += (int)SCOREFACTOR.SWATTER;
        settings.numberOfKills += 1;
        UIManager.Instance.IncrementKill(transform.position, totalCash);
        totalCash = 0;
    } 


    public void SlowDownFly()
    {
        slowDownNow = true;
        currentSpeed = flyMovements.speed;
        flyMovements.speed = settings.flyIntelLevels[settings.waveIndex].flySlowDownSpeed;
        if (TryGetComponent<FlyAudioSource>(out var bz))
        {
            bz.DizzyClip();
        }
        ChangeEyeType(true);
    }

    public void Update()
    {
        if (slowDownNow)
        {
            if (InitialTime > settings.flyIntelLevels[settings.waveIndex].flySlowDownTime)
            {
                InitialTime = 0f;
                slowDownNow = false;
                flyMovements.speed = currentSpeed;
                if (TryGetComponent<FlyAudioSource>(out var bz))
                {
                    bz.MoveClip();
                }
                ChangeEyeType(false);
            }
            InitialTime += Time.deltaTime;
        }
    }
}
