using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour {

    public ControlModeManager m_ControlModeManager;
    public FullLineModelRenderer m_FullLineModelRenderer;

    // All menu buttons 
    public MenuButton LeftQuery;
    public MenuButton LeftMessage;
    public MenuButton LeftModel;
    public MenuButton LeftBox;

    public MenuButton RightQuery;
    public MenuButton RightMessage;
    public MenuButton RightModel;
    public MenuButton RightBox;

    public SliderButton LeftMessageLength;
    public SliderButton RightMessageLength;
    public SliderButton LeftMessageSpeed;
    public SliderButton RightMessageSpeed;

    public void SliderFunction(SliderButton.SLIDERFUNCTION func, float val)
    {
        switch (func)
        {
            case SliderButton.SLIDERFUNCTION.MESSAGELENGTH:
                // val is between -5 and 5. (where -5 is max) We want a value between 2 and 12
                int lenVal = (int) -val + 7;
                m_FullLineModelRenderer.m_steps = lenVal;
                return;
            case SliderButton.SLIDERFUNCTION.MESSAGESPEED:
                // Here we want a delay value between 1 and 11 (1 is fastest, 11 is slowest)
                int speedVal = (int)val + 6;
                m_FullLineModelRenderer.m_iterationDelay = speedVal;
                return;
        }
    }

    public void ChangeMode(MenuButton.BUTTONFUNCTION func)
    {
        ControlModeManager.CONTROL_MODE curMode = m_ControlModeManager.GetCurrentControlMode();
        switch (func)
        {
            case MenuButton.BUTTONFUNCTION.QUERY:
                if (curMode == ControlModeManager.CONTROL_MODE.MESSAGE_MODEL || curMode == ControlModeManager.CONTROL_MODE.QUERY_MODEL)
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.QUERY_MODEL);
                else
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.QUERY_BOX);
                return;

            case MenuButton.BUTTONFUNCTION.MESSAGE:
                if (curMode == ControlModeManager.CONTROL_MODE.MESSAGE_MODEL || curMode == ControlModeManager.CONTROL_MODE.QUERY_MODEL)
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.MESSAGE_MODEL);
                else
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.MESSAGE_BOX);
                return;

            case MenuButton.BUTTONFUNCTION.MODEL:
                if (curMode == ControlModeManager.CONTROL_MODE.MESSAGE_BOX || curMode == ControlModeManager.CONTROL_MODE.MESSAGE_MODEL)
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.MESSAGE_MODEL);
                else
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.QUERY_MODEL);
                return;

            case MenuButton.BUTTONFUNCTION.BOX:
                if (curMode == ControlModeManager.CONTROL_MODE.MESSAGE_BOX || curMode == ControlModeManager.CONTROL_MODE.MESSAGE_MODEL)
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.MESSAGE_BOX);
                else
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.QUERY_BOX);
                return;
        }
    }

    private void Update()
    {
        ControlModeManager.CONTROL_MODE curMode = m_ControlModeManager.GetCurrentControlMode();
        switch(curMode)
        {
            case ControlModeManager.CONTROL_MODE.QUERY_MODEL:
                LeftQuery.Activate();
                RightQuery.Activate();
                LeftModel.Activate();
                RightModel.Activate();
                break;
            case ControlModeManager.CONTROL_MODE.QUERY_BOX:
                LeftQuery.Activate();
                RightQuery.Activate();
                LeftBox.Activate();
                RightBox.Activate();
                break;
            case ControlModeManager.CONTROL_MODE.MESSAGE_MODEL:
                LeftModel.Activate();
                RightModel.Activate();
                LeftMessage.Activate();
                RightMessage.Activate();
                break;
            case ControlModeManager.CONTROL_MODE.MESSAGE_BOX:
                LeftMessage.Activate();
                RightMessage.Activate();
                LeftBox.Activate();
                RightBox.Activate();
                break;
        }

        int messageLen = m_FullLineModelRenderer.m_steps - 7;
        LeftMessageLength.SetPosition((float)-messageLen);
        RightMessageLength.SetPosition((float)-messageLen);

        int messageSpeed = m_FullLineModelRenderer.m_iterationDelay - 6;
        LeftMessageSpeed.SetPosition((float)messageSpeed);
        RightMessageSpeed.SetPosition((float)messageSpeed);
    }
}
