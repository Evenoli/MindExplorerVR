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

    private enum SIDE {LEFT, RIGHT };
    private bool m_RotateModeActive;
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
            //Other hand is already rotating. switch to scale
            if (m_RotateModeActive)
            {
                InitScaleMode();
            }
            //This shouldn't happen...
            else if (m_ScaleModeActive)
            {
                print("Gripped while scaling...");
            }
            // Else must be first hand to grip. begin rotation
            else
            {
                InitRotateMode(side);
            }
        }
        else
        {
            //hand released. Now rotating with other hand.
            if(m_ScaleModeActive)
            {
                InitRotateMode((SIDE)Math.Abs((int)side - 1));
            }
            // Stop rotating
            else if(m_RotateModeActive && m_rotatingHand==side)
            {
                m_RotateModeActive = false;
            }
        }
    }

    private void InitScaleMode()
    {
        m_RotateModeActive = false;
        m_ScaleModeActive = true;

        Vector3 leftPos = m_LeftController.transform.localPosition;
        Vector3 rightPos = m_RightController.transform.localPosition;
        m_startingDist = Vector3.Distance(leftPos, rightPos);
        m_startingScale = transform.localScale;
    }

    private void InitRotateMode(SIDE side)
    {
        m_RotateModeActive = true;
        m_ScaleModeActive = false;

        m_startingModelRotation = transform.localRotation;
        m_startingModelPos = transform.localPosition;
        m_rotatingHand = side;

        if (side == SIDE.LEFT)
        {
            m_startingControllerRotation = m_LeftController.transform.localRotation;
            m_startingControllerPos = m_LeftController.transform.localPosition;
        }
        else
        {
            m_startingControllerRotation = m_RightController.transform.localRotation;
            m_startingControllerPos = m_RightController.transform.localPosition;
        }
    }

    // Update is called once per frame
    void Update () {
		if(m_RotateModeActive)
        {
            Quaternion curContrRot;
            Vector3 curContrPos;
            bool moveEnabled = false;
            if (m_rotatingHand == SIDE.LEFT)
            {
                curContrRot = m_LeftController.transform.localRotation;
                curContrPos = m_LeftController.transform.localPosition;
                moveEnabled = m_LeftTrackedContr.triggerPressed;
            }
            else
            {
                curContrRot = m_RightController.transform.localRotation;
                curContrPos = m_RightController.transform.localPosition;
                moveEnabled = m_RightTrackedContr.triggerPressed;
            }

            Quaternion rotationDiff = Quaternion.Inverse(m_startingControllerRotation) * curContrRot;
            Vector3 posDiff = curContrPos - m_startingControllerPos;

            if(moveEnabled)
                transform.localPosition = m_startingModelPos + posDiff * m_moveFactor;
            else
                transform.localRotation = m_startingModelRotation * rotationDiff;
        }
        else if(m_ScaleModeActive)
        {
            Vector3 curLeftPos = m_LeftController.transform.localPosition;
            Vector3 curRightPos = m_RightController.transform.localPosition;
            float curContrDist = Vector3.Distance(curLeftPos, curRightPos);

            float scale = (curContrDist / m_startingDist) * m_scaleFactor;
            transform.localScale = m_startingScale * scale;
        }
	}
}
