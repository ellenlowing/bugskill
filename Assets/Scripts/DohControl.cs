using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DohControl : MonoBehaviour
{
    public List<SpringJoint> SpringJoints = new List<SpringJoint>();
    public float SpringForce;
    public float Damper;

    void Start()
    {

    }

    void Update()
    {

    }

    void OnValidate()
    {
        foreach (var joint in SpringJoints)
        {
            joint.spring = SpringForce;
            joint.damper = Damper;
        }
    }
}
