using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraficsSwitcher : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            QualitySettings.SetQualityLevel(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            QualitySettings.SetQualityLevel(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            QualitySettings.SetQualityLevel(0);
        }
    }
}
