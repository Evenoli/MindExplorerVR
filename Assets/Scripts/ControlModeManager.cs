using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlModeManager : MonoBehaviour {

    public enum CONTROL_MODE { EXPLORE, QUERY };
    private CONTROL_MODE m_curControlMode;

    public GameObject m_LeftController;
    private SteamVR_TrackedController m_LeftTrackedContr;

    public GameObject m_RightController;
    private SteamVR_TrackedController m_RightTrackedContr;

    // Use this for initialization
    void Start () {
        // Default control mode
        m_curControlMode = CONTROL_MODE.EXPLORE;

        if (m_LeftController)
            m_LeftTrackedContr = m_LeftController.GetComponent<SteamVR_TrackedController>();
        else
            print("ERROR: Missing left controller reference in ControlModeManager!");

        if (m_RightController)
            m_RightTrackedContr = m_RightController.GetComponent<SteamVR_TrackedController>();
        else
            print("ERROR: Missing right controller reference in ControlModeManager!");

        if (!m_LeftTrackedContr || !m_RightTrackedContr)
            print("ERROR: Couldn't retrieve vrTracked controller components. Make sure they're attached to both controllers");

        //Set up controller action listeners
        if (m_LeftTrackedContr)
        {
            m_LeftTrackedContr.MenuButtonClicked += new ClickedEventHandler(MenuPressed);
        }

        if (m_RightTrackedContr)
        {
            m_RightTrackedContr.MenuButtonClicked += new ClickedEventHandler(MenuPressed);
        }
    }

    private void MenuPressed(object sender, ClickedEventArgs e)
    {
        ToggleMode();
    }

    private void ToggleMode()
    {
        m_curControlMode = (CONTROL_MODE)(((int)m_curControlMode + 1) % 2);
    }

    public CONTROL_MODE GetCurrentControlMode()
    {
        return m_curControlMode;
    }

    public void SetControlMode(CONTROL_MODE mode)
    {
        m_curControlMode = mode;
    }
       
}
