using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    [SerializeField] private Light sun;
    [SerializeField] private Light moon;

    [SerializeField] private TextMeshProUGUI timeDisplay;
    [SerializeField] private AnimationCurve animationCurve;

    [SerializeField] private Camera camera;

    [SerializeField] private float timeMultiplier;
    [SerializeField] private float startHour;

    [SerializeField] private float sunRiseHour;
    [SerializeField] private float sunSetHour;

    private DateTime currentTime;
    private TimeSpan sunRiseTime;
    private TimeSpan sunSetTime;

    [SerializeField] private Color ambiantLightDay;
    [SerializeField] private Color ambiantLightNight;

    [SerializeField] private Color skyBoxColorDay;
    [SerializeField] private Color skyBoxColorNight;

    [SerializeField] private float maxSunLightIntensity;
    [SerializeField] private float maxMoonLightIntensity;

    void Start()
    {
        currentTime = DateTime.Now.Date + TimeSpan.FromHours(startHour);

        sunRiseTime = TimeSpan.FromHours(sunRiseHour);
        sunSetTime = TimeSpan.FromHours(sunSetHour);
    }

    void Update()
    {
        UpdateTimeOfDay();
        RotateSunAndMoon();
        UpdateLightSettings();
    }

    private void RotateSunAndMoon()
    {
        float sunRotation;

        if (currentTime.TimeOfDay > sunRiseTime && currentTime.TimeOfDay < sunSetTime)
        {
            TimeSpan sunRiseToSunSetDuration = CalculateTimeDifference(sunRiseTime, sunSetTime);
            TimeSpan timeSinceSunRise = CalculateTimeDifference(sunRiseTime, currentTime.TimeOfDay);

            double percentage = timeSinceSunRise.TotalMinutes / sunRiseToSunSetDuration.TotalMinutes;
            sunRotation = Mathf.Lerp(0, 180, (float)percentage);
        }
        else
        {
            TimeSpan sunSetToSunRiseDuration = CalculateTimeDifference(sunSetTime, sunRiseTime);
            TimeSpan timeSinceSunSet = CalculateTimeDifference(sunSetTime, currentTime.TimeOfDay);

            double percentage = timeSinceSunSet.TotalMinutes / sunSetToSunRiseDuration.TotalMinutes;
            sunRotation = Mathf.Lerp(180, 360, (float)percentage);
        }
        sun.transform.rotation = Quaternion.AngleAxis(sunRotation, Vector3.up);
        moon.transform.rotation = Quaternion.AngleAxis(sunRotation + 180, Vector3.up);
    }

    private void UpdateLightSettings()
    {
        float dotProduct = Vector3.Dot(sun.transform.forward, Vector3.forward);
        sun.intensity = Mathf.Lerp(0, maxSunLightIntensity, animationCurve.Evaluate(dotProduct));
        moon.intensity = Mathf.Lerp(maxMoonLightIntensity, 0, animationCurve.Evaluate(dotProduct));

        RenderSettings.ambientLight = Color.Lerp(ambiantLightNight, ambiantLightDay, animationCurve.Evaluate(dotProduct));


        float dotProductPlayer = Vector3.Dot(sun.transform.forward, camera.transform.parent.transform.up);
        camera.backgroundColor = Color.Lerp(skyBoxColorDay, skyBoxColorNight, animationCurve.Evaluate(dotProductPlayer));
    }

    private void UpdateTimeOfDay()
    {
        currentTime = currentTime.AddSeconds(Time.deltaTime * timeMultiplier);

        if (timeDisplay != null ) 
        {
            timeDisplay.text = currentTime.ToString("HH:mm");
        }
    }

    private TimeSpan CalculateTimeDifference(TimeSpan from, TimeSpan to)
    {
        TimeSpan diff = to - from;

        return diff.TotalSeconds < 0 ? diff + TimeSpan.FromHours(24) : diff;
    }
}
