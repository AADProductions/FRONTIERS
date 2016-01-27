using UnityEngine;
using System;

/// Time iteration class.
///
/// Component of the sky dome parent game object.

public class TOD_Time : MonoBehaviour
{
    /// Day length inspector variable.
    /// Length of one day in minutes.
    public float DayLengthInMinutes = 30;

    /// Date progression inspector variable.
    /// Automatically updates Cycle.Day if enabled.
    public bool ProgressDate = true;

    /// Moon phase progression inspector variable.
    /// Automatically updates Moon.Phase if enabled.
    public bool ProgressMoonPhase = true;

    private TOD_Sky sky;

    protected void Start()
    {
        sky = GetComponent<TOD_Sky>();
    }

    protected void Update()
    {
        float oneDay = DayLengthInMinutes * 60;
        float oneHour = oneDay / 24;

		float hourIter = (float) Frontiers.WorldClock.ARTDeltaTime / oneHour;
		float moonIter = (float) Frontiers.WorldClock.ARTDeltaTime / (30*oneDay) * 2;

        sky.Cycle.Hour += hourIter;

        if (ProgressMoonPhase)
        {
            sky.Cycle.MoonPhase += moonIter;
            if (sky.Cycle.MoonPhase < -1) sky.Cycle.MoonPhase += 2;
            else if (sky.Cycle.MoonPhase > 1) sky.Cycle.MoonPhase -= 2;
        }

        if (sky.Cycle.Hour >= 24)
        {
            sky.Cycle.Hour = 0;

            if (ProgressDate)
            {
                int daysInMonth = DateTime.DaysInMonth(sky.Cycle.Year, sky.Cycle.Month);
                if (++sky.Cycle.Day > daysInMonth)
                {
                    sky.Cycle.Day = 1;
                    if (++sky.Cycle.Month > 12)
                    {
                        sky.Cycle.Month = 1;
                        sky.Cycle.Year++;
                    }
                }
            }
        }
    }
}
