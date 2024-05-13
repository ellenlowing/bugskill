using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class SlowDown : MonoBehaviour
{
    public bool slowDownNow = false;
    public SettingSO settings;
    public FlyMovement flyMovements;
    public List<MeshRenderer> NormalEyeMesh;
    public List<MeshRenderer> CircularEyeMesh;
    

    private float currentSpeed = 0.0f;
    private float InitialTime = 0.0f;

    private void Start()
    {
       flyMovements =  GetComponentInParent<FlyMovement>();
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

    
    public void SlowDownFly()
    {
        slowDownNow = true;
        currentSpeed = flyMovements.speed;
        flyMovements.speed = settings.flySlowDownSpeed;
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
            if(InitialTime > settings.flySlowDownTime)
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
