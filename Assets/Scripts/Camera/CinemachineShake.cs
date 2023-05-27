using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineShake : MonoBehaviour
{    
    public static CinemachineShake Instance { get; private set; }

    [Range(0f, 3f)]
    public float shakeMulti = 1;
    public CinemachineVirtualCamera virtualCamera;
    float shakeTimer;

    float startIntensity;
    float startTime;

    private void Awake()
    {
        Instance = this;
    }

    public void ShakeCamera(float intensity, float time)
    {   
        intensity *= shakeMulti;

        // Shake Camera
        CinemachineBasicMultiChannelPerlin perlin =
            virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        perlin.m_AmplitudeGain = intensity;
        startIntensity = intensity;

        shakeTimer = time;
        startTime = time;
    }

    private void Update()
    {
        if(shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            CinemachineBasicMultiChannelPerlin perlin =
            virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            perlin.m_AmplitudeGain = Mathf.Lerp(0, startIntensity, shakeTimer / startTime);
        }
        
    }
}
