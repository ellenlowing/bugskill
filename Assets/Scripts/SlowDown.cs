using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class SlowDown : MonoBehaviour
{
    public float SlowDownTime = 8f;
    public float SlowDownSpeed = 0.5f;
    public float InitialTime = 0.0f;
    public bool slowDownNow = false;
    public FlyMovement flyMovements;
    public Material[] CircularEyeMaterial;
    public Material[] DefaultMaterial;
    public List<MeshRenderer> EyeMeshRenderers;
    

    private float currentSpeed = 0.0f;
    

    private void Start()
    {
       flyMovements =  GetComponentInParent<FlyMovement>();
       Assert.IsNotNull(flyMovements, "Flymovements not assigned");
       Assert.IsNotNull(EyeMeshRenderers, "Fly Mesh not Assigned");
    }

    // changes the materials of the eye mesh
    public void ChangeEyeType(bool circular)
    {
        foreach(MeshRenderer eye in EyeMeshRenderers)
        {
             eye.materials = (circular == true) ? CircularEyeMaterial : DefaultMaterial;
            //eye.material = CircularEyeMaterial;
        }
    }

    public void SlowDownFly()
    {
        slowDownNow = true;
        currentSpeed = flyMovements.speed;
        flyMovements.speed = SlowDownSpeed;
        ChangeEyeType(true);
    }

    public void Update()
    {
        if (slowDownNow)
        {
            if(InitialTime > SlowDownTime)
            {
                InitialTime = 0f;
                slowDownNow = false;
                flyMovements.speed = currentSpeed;
                ChangeEyeType(false);
            }
            InitialTime += Time.deltaTime;
        }

    }
}
