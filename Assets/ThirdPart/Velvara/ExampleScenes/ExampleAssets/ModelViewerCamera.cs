using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineFreeLook))]
public class ModelViewerCamera : MonoBehaviour
{
    private CinemachineFreeLook viewerCamera;

    private string XAxisName = "Mouse X";
    private string YAxisName = "Mouse Y";

    // Start is called before the first frame update
    void Start()
    {
        viewerCamera = GetComponent<CinemachineFreeLook>();
        viewerCamera.m_XAxis.m_InputAxisName = "";
        viewerCamera.m_YAxis.m_InputAxisName = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0)) 
        {
            viewerCamera.m_XAxis.m_InputAxisValue = Input.GetAxis(XAxisName);
            viewerCamera.m_YAxis.m_InputAxisValue = Input.GetAxis(YAxisName);
        }
        else
        {
            viewerCamera.m_XAxis.m_InputAxisValue = 0;
            viewerCamera.m_YAxis.m_InputAxisValue = 0;
        }
    }
}
