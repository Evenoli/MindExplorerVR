using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : MonoBehaviour {

    public enum BUTTONFUNCTION { QUERY, MESSAGE, MODEL, BOX, HELP, OPTIONS };

    public GameObject m_BasicBacking;
    public GameObject m_HighlightBacking;
    public BUTTONFUNCTION m_function;
    public MenuManager m_MenuManager;

    // Other menu buttons that are deactivated when this one is activated
    public MenuButton[] m_ButtonGroup;

    public void Start()
    {
        // Set default selection
        if (m_function == BUTTONFUNCTION.QUERY || m_function == BUTTONFUNCTION.MODEL)
            Activate();
    }

    public void Activate()
    {
        m_HighlightBacking.SetActive(true);
        m_BasicBacking.SetActive(false);

        if(m_ButtonGroup.Length > 0)
        {
            foreach (MenuButton mb in m_ButtonGroup)
                mb.Deactivate();
        }
    }

    public void Deactivate()
    {
        m_HighlightBacking.SetActive(false);
        m_BasicBacking.SetActive(true);
    }

    private void Function()
    {
        if(m_function == BUTTONFUNCTION.OPTIONS)
        {
            //Options menu
        }
        else if (m_function == BUTTONFUNCTION.HELP)
        {
            // Help menu
        }
        else
        {
            m_MenuManager.ChangeMode(m_function);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Finger")
            Function();
    }
}
