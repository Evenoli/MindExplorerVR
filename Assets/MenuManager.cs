using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour {

    public ControlModeManager m_ControlModeManager;

    // All menu buttons 
    public MenuButton LeftQuery;
    public MenuButton LeftMessage;
    public MenuButton LeftModel;
    public MenuButton LeftBox;

    public MenuButton RightQuery;
    public MenuButton RightMessage;
    public MenuButton RightModel;
    public MenuButton RightBox;

    

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
                return;
            case ControlModeManager.CONTROL_MODE.QUERY_BOX:
                LeftQuery.Activate();
                RightQuery.Activate();
                LeftBox.Activate();
                RightBox.Activate();
                return;
            case ControlModeManager.CONTROL_MODE.MESSAGE_MODEL:
                LeftModel.Activate();
                RightModel.Activate();
                LeftMessage.Activate();
                RightMessage.Activate();
                return;
            case ControlModeManager.CONTROL_MODE.MESSAGE_BOX:
                LeftMessage.Activate();
                RightMessage.Activate();
                LeftBox.Activate();
                RightBox.Activate();
                return;
        }
    }
}
