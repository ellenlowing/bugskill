using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class FroggyController : MonoBehaviour
{
    public Transform FrogTongueTransform;
    public bool ClawActive = false;
    public float GrabSpeed = 2;
    public float ReturnSpeed = 1;
    public float MaxScaleY = 5f;
    public XRNode DeviceNode;

    private Vector3 _originalFrogTongueScale;
    private InputDevice _xrController;
    private bool triggerFired = false;
    private float triggerValueThreshold = 0.2f;

    void Start()
    {
        _originalFrogTongueScale = FrogTongueTransform.localScale;
        _xrController = InputDevices.GetDeviceAtXRNode(DeviceNode);
    }

    void Update()
    {
        // Get controller trigger value
        if (!_xrController.isValid)
        {
            _xrController = InputDevices.GetDeviceAtXRNode(DeviceNode);
        }

        _xrController.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue);

        if (!triggerFired && triggerValue > triggerValueThreshold)
        {
            triggerFired = true;
            TriggerPress();
        }
        else if (triggerFired && triggerValue <= 0.0f)
        {
            triggerFired = false;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerPress();
        }
    }

    void TriggerPress()
    {
        if (!ClawActive)
        {
            StartCoroutine(AnimateFrogTongueScale(_originalFrogTongueScale, new Vector3(1f, MaxScaleY, 1f), GrabSpeed, ReturnSpeed));

        }
    }

    IEnumerator AnimateFrogTongueScale(Vector3 inScale, Vector3 outScale, float grabSpeed, float returnSpeed)
    {
        // PlaySound("Reload");
        float t = 0;
        ClawActive = true;
        while (t <= 1)
        {
            FrogTongueTransform.localScale = Vector3.Lerp(inScale, outScale, t);
            t += Time.deltaTime * grabSpeed;
            yield return null;
        }

        // if (returnSpeed == FastClawReturnAnimationSpeed)
        // {
        // PlaySound("GrabSuccess");
        // }
        // else
        // {
        // PlaySound("GrabFailure");
        // }
        // yield return new WaitForSeconds(0.7f);
        // StopSound();

        t = 0;
        while (Vector3.Distance(FrogTongueTransform.localScale, inScale) > 0.001f)
        {
            FrogTongueTransform.localScale = Vector3.Lerp(outScale, inScale, t);
            t += Time.deltaTime * returnSpeed;
            yield return null;
        }
        ClawActive = false;
        FrogTongueTransform.localScale = inScale;
    }

}
