using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Display : MonoBehaviour {

    public GameObject m_QueryMode;
    public GameObject m_ExploreMode;
    public GameObject m_MessageMode;
    public GameObject m_QueryLoading;

	public void SetExploreMode()
    {
        m_QueryMode.SetActive(false);
        m_MessageMode.SetActive(false);
        m_ExploreMode.SetActive(true);
    }

    public void SetQueryMode()
    {
        m_QueryMode.SetActive(true);
        m_MessageMode.SetActive(false);
        m_ExploreMode.SetActive(false);
    }

    public void SetMessageMode()
    {
        m_MessageMode.SetActive(true);
        m_ExploreMode.SetActive(false);
        m_QueryMode.SetActive(false);
    }

    public void ShowQueryLoading(bool show)
    {
        m_QueryLoading.SetActive(show);
    }
}
