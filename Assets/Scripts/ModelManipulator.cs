using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelManipulator : MonoBehaviour {

    public GameObject m_LeftController;
    private SteamVR_TrackedController m_LeftTrackedContr;

    public GameObject m_RightController;
    private SteamVR_TrackedController m_RightTrackedContr;

    public float m_scaleFactor;
    public float m_moveFactor;

    // Set 0 for no max scale
    public Vector3 m_maxScale;

    public bool m_AutoSetMoveScale;
    public bool m_allowRotation;
    public CortexDrawer m_drawer;
    // WHich control mode allows for this manipulation
    public ControlModeManager.CONTROL_MODE m_activeMode;
    public ControlModeManager.CONTROL_MODE m_secondActiveMode;
    public ControlModeManager.CONTROL_MODE m_thirdActiveMode;
    public ControlModeManager m_controlManager;

    private enum SIDE {LEFT, RIGHT };
    private bool m_MoveModeActive;
    private SIDE m_rotatingHand;
    private Quaternion m_startingModelRotation;
    private Quaternion m_startingControllerRotation;
    private Vector3 m_startingModelPos;
    private Vector3 m_startingControllerPos;

    private bool m_ScaleModeActive;
    private Vector3 m_startingScale;
    private float m_startingDist;



    // Use this for initialization
    void Start () {
        if (m_LeftController)
            m_LeftTrackedContr = m_LeftController.GetComponent<SteamVR_TrackedController>();
        else
            print("ERROR: Missing left controller reference in ModelManipulator!");

        if (m_RightController)
            m_RightTrackedContr = m_RightController.GetComponent<SteamVR_TrackedController>();
        else
            print("ERROR: Missing right controller reference in ModelManipulator!");

        if (!m_LeftTrackedContr || !m_RightTrackedContr)
            print("ERROR: Couldn't retrieve vrTracked controller components. Make sure they're attached to both controllers");

        //Set up controller action listeners
        if (m_LeftTrackedContr)
        {
            m_LeftTrackedContr.Gripped += new ClickedEventHandler(LeftGrip);
            m_LeftTrackedContr.Ungripped += new ClickedEventHandler(LeftUngrip);
        }

        if(m_RightTrackedContr)
        {
            m_RightTrackedContr.Gripped += new ClickedEventHandler(RightGrip);
            m_RightTrackedContr.Ungripped += new ClickedEventHandler(RightUngrip);
        }

    }

    private void LeftGrip(object sender, ClickedEventArgs e)
    {
        SetGrip(true, SIDE.LEFT);
    }

    private void LeftUngrip(object sender, ClickedEventArgs e)
    {
        SetGrip(false, SIDE.LEFT);
    }

    private void RightGrip(object sender, ClickedEventArgs e)
    {
        SetGrip(true, SIDE.RIGHT);
    }

    private void RightUngrip(object sender, ClickedEventArgs e)
    {
        SetGrip(false, SIDE.RIGHT);
    }

    private void SetGrip(bool gripped, SIDE side)
    {
        if (gripped)
        {
            //Other hand is already moving. switch to scale
            if (m_MoveModeActive)
            {
                InitScaleMode();
            }
            //This shouldn't happen...
            else if (m_ScaleModeActive)
            {
                print("Gripped while scaling...");
            }
            // Else must be first hand to grip. begin moving
            else
            {
                InitMoveMode(side);
            }
        }
        else
        {
            //hand released. Now rotating with other hand.
            if(m_ScaleModeActive)
            {
                InitMoveMode((SIDE)Math.Abs((int)side - 1));
            }
            // Stop rotating
            else if(m_MoveModeActive && m_rotatingHand==side)
            {
                m_MoveModeActive = false;
            }
        }
    }

    private void InitScaleMode()
    {
        m_MoveModeActive = false;
        m_ScaleModeActive = true;

        Vector3 leftPos = m_LeftController.transform.position;
        Vector3 rightPos = m_RightController.transform.position;
        m_startingDist = Vector3.Distance(leftPos, rightPos);
        m_startingScale = transform.localScale;
    }

    private void InitMoveMode(SIDE side)
    {
        m_MoveModeActive = true;
        m_ScaleModeActive = false;

        m_startingModelRotation = transform.rotation;
        m_startingModelPos = transform.position;
        m_rotatingHand = side;

        if (side == SIDE.LEFT)
        {
            m_startingControllerRotation = m_LeftController.transform.rotation;
            m_startingControllerPos = m_LeftController.transform.position;
        }
        else
        {
            m_startingControllerRotation = m_RightController.transform.rotation;
            m_startingControllerPos = m_RightController.transform.position;
        }
    }

    // Update is called once per frame
    void Update () {

        // If we're in wrong control mode, return
        if (m_controlManager.GetCurrentControlMode() != m_activeMode 
            && m_controlManager.GetCurrentControlMode() != m_secondActiveMode 
            && m_controlManager.GetCurrentControlMode() != m_thirdActiveMode)
            return;

        // We're in move/rotate mode
		if(m_MoveModeActive)
        {
            Quaternion curContrRot;
            Vector3 curContrPos;
            bool rotateEnabled = false;
            if (m_rotatingHand == SIDE.LEFT)
            {
                curContrRot = m_LeftController.transform.rotation;
                curContrPos = m_LeftController.transform.position;
                rotateEnabled = m_LeftTrackedContr.triggerPressed;
            }
            else
            {
                curContrRot = m_RightController.transform.rotation;
                curContrPos = m_RightController.transform.position;
                rotateEnabled = m_RightTrackedContr.triggerPressed;
            }

            Quaternion rotationDiff = Quaternion.Inverse(m_startingControllerRotation) * curContrRot;
            Vector3 posDiff = curContrPos - m_startingControllerPos;

            if (rotateEnabled && m_allowRotation)
            {
                //Vector3 eulerRot = (m_startingModelRotation * rotationDiff).eulerAngles;
                //Vector3 queryCenter = m_drawer.GetQueryCenter();
                //transform.RotateAround(queryCenter, new Vector3(1, 0, 0), eulerRot.x);
                //transform.RotateAround(queryCenter, new Vector3(0, 1, 0), eulerRot.y);
                //transform.RotateAround(queryCenter, new Vector3(0, 0, 1), eulerRot.z);
                transform.rotation = m_startingModelRotation * rotationDiff;
            }
            else
            {
                if(m_AutoSetMoveScale)
                {
                    float modelScale = transform.parent.localScale.x;
                    m_moveFactor = GetMoveScale(modelScale);

                }
                transform.position = m_startingModelPos + posDiff * m_moveFactor;

            }
            
        }

        // Scale mode
        else if(m_ScaleModeActive)
        {
            Vector3 curLeftPos = m_LeftController.transform.localPosition;
            Vector3 curRightPos = m_RightController.transform.localPosition;
            float curContrDist = Vector3.Distance(curLeftPos, curRightPos);

            float scale = (curContrDist / m_startingDist) * m_scaleFactor;
            if(m_maxScale != Vector3.zero)
            {
                transform.localScale = Vector3.Min(m_startingScale * scale, m_maxScale);
            }
            else
            {
                transform.localScale = m_startingScale * scale;
            }
            
        }
	}

    // Predicts a good move scale for QueryCube for different scales
    private float GetMoveScale(float modelScale)
    {
        return Mathf.Max(-1919.1919191919f * modelScale + 1134.338f, 10f);
    }
}
