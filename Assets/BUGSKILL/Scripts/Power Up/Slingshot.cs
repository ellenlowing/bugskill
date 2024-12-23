using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using Oculus.Interaction.PoseDetection;

public class Slingshot : MonoBehaviour
{
    public GameObject BallPrefab;
    public SelectorUnityEventWrapper ScissorsPoseEvent;
    public Hand PrimaryHand;
    public GameObject StatusIndicator;
    public FingerSlingshotPowerUp CorePowerUp;

    private HandJointId _indexFingerJoint = HandJointId.HandIndexTip;
    private HandJointId _middleFingerJoint = HandJointId.HandMiddleTip;
    private GameObject _activeBall;
    private bool _isPoseActive = false;

    void Start()
    {
        ScissorsPoseEvent.WhenSelected.AddListener(OnScissorsPoseSelected);
        ScissorsPoseEvent.WhenUnselected.AddListener(OnScissorsPoseUnselected);
    }

    void Update()
    {
        if (_isPoseActive && _activeBall == null)
        {
            CreateNewBomb();
        }
    }

    public void OnScissorsPoseSelected()
    {
        _isPoseActive = true;
        Debug.Log("Scissors Pose Selected");
    }

    public void OnScissorsPoseUnselected()
    {
        _isPoseActive = false;
        Debug.Log("Scissors Pose Unselected");
    }

    public void CreateNewBomb()
    {
        // Get finger tip positions
        PrimaryHand.GetJointPose(_indexFingerJoint, out Pose indexFingerTipPose);
        PrimaryHand.GetJointPose(_middleFingerJoint, out Pose middleFingerTipPose);
        var averageFingerTipPosition = (indexFingerTipPose.position + middleFingerTipPose.position) / 2;

        _activeBall = Instantiate(BallPrefab, averageFingerTipPosition, Quaternion.identity);
        _activeBall.GetComponent<SlingshotBall>().PrimaryHand = PrimaryHand;

        if (CorePowerUp != null)
        {
            CorePowerUp.UsePowerCapacity();
        }
    }

}
