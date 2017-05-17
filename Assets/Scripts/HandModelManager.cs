using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandModelManager : MonoBehaviour {

    public GameObject m_defaultModel;
    public GameObject m_pointModel;
    public GameObject m_fistModel;

    private SteamVR_TrackedController m_controller;

	// Use this for initialization
	void Start () {
        m_defaultModel.SetActive(true);
        m_pointModel.SetActive(false);
        m_fistModel.SetActive(false);

        m_controller = gameObject.GetComponentInParent<SteamVR_TrackedController>();
    }
	
	// Update is called once per frame
	void Update () {
		if(m_controller.padPressed)
        {
            m_defaultModel.SetActive(false);
            m_pointModel.SetActive(true);
            m_fistModel.SetActive(false);
        }
        else if(m_controller.gripped)
        {
            m_defaultModel.SetActive(false);
            m_pointModel.SetActive(false);
            m_fistModel.SetActive(true);
        }
        else
        {
            m_defaultModel.SetActive(true);
            m_pointModel.SetActive(false);
            m_fistModel.SetActive(false);
        }
	}
}
