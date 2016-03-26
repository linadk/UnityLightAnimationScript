/// <summary>
/// Gives a suite of options for variances in light color and intensity over time.
/// Credit to  Harry1960 and the posters in http://forum.unity3d.com/threads/flickering-light.4988/ for the bulk of the code.
/// I added in intermitent animation options to help achieve a more realistic light flicker.
/// </summary>

using UnityEngine;
using System.Collections;

public enum enColorchannels
{
    all = 0,
    red = 1,
    blue = 2,
    green = 3
}
public enum enWaveFunctions
{
    sinus = 0,
    triangle = 1,
    square = 2,
    sawtooth = 3,
    inverted_saw = 4,
    noise = 5
}

public class LightColorAnimation : MonoBehaviour
{

    public enColorchannels colorChannel = enColorchannels.all;
    public enWaveFunctions waveFunction = enWaveFunctions.sinus;
    public float offset = 0.0f; // constant offset
    public float amplitude = 1.0f; // amplitude of the wave
    public float phase = 0.0f; // start point inside on wave cycle
    public float frequency = 0.5f; // cycle frequency per second
    public bool affectsIntensity = true;

    // Set variables regarding intermittency of animation
    public bool intermittent = false;
    public float smallestNonwaveInterval = 1.0f;
    public float largestNonwaveInterval = 5.0f;
    public float smallestWaveInterval = 0.1f;
    public float largestWaveInterval = 1.0f;

    // Keep a copy of the original values
    private Color originalColor;
    private float originalIntensity;
    private float timeSinceLastInterval = 0.0f;
    private float nextIntervalTime = 0.0f;
    private bool isInInterval = false;


    // Use this for initialization
    void Start()
    {
        originalColor = GetComponent<Light>().color;
        originalIntensity = GetComponent<Light>().intensity;
    }

    // Update is called once per frame
    void Update()
    {
        HandleIntermittent();

        if (!isInInterval && intermittent)
        {
            return;
        }

        Light light = GetComponent<Light>();
        if (affectsIntensity)
            light.intensity = originalIntensity * EvalWave();

        Color o = originalColor;
        Color c = GetComponent<Light>().color;

        if (colorChannel == enColorchannels.all)
            light.color = originalColor * EvalWave();
        else
        if (colorChannel == enColorchannels.red)
            light.color = new Color(o.r * EvalWave(), c.g, c.b, c.a);
        else
        if (colorChannel == enColorchannels.green)
            light.color = new Color(c.r, o.g * EvalWave(), c.b, c.a);
        else // blue       
            light.color = new Color(c.r, c.g, o.b * EvalWave(), c.a);


    }

    private float EvalWave()
    {
        float x = (Time.time + phase) * frequency;
        float y;
        x = x - Mathf.Floor(x); // normalized value (0..1)
        if (waveFunction == enWaveFunctions.sinus)
        {
            y = Mathf.Sin(x * 2f * Mathf.PI);
        }
        else if (waveFunction == enWaveFunctions.triangle)
        {
            if (x < 0.5f)
                y = 4.0f * x - 1.0f;
            else
                y = -4.0f * x + 3.0f;
        }
        else if (waveFunction == enWaveFunctions.square)
        {
            if (x < 0.5f)
                y = 1.0f;
            else
                y = -1.0f;
        }
        else if (waveFunction == enWaveFunctions.sawtooth)
        {
            y = x;
        }
        else if (waveFunction == enWaveFunctions.inverted_saw)
        {
            y = 1.0f - x;
        }
        else if (waveFunction == enWaveFunctions.noise)
        {
            y = 1f - (Random.value * 2f);
        }
        else
        {
            y = 1.0f;

        }
        return (y * amplitude) + offset;

    }

    void HandleIntermittent()
    {
        if (!intermittent){ return; }

        timeSinceLastInterval += Time.deltaTime;

        // If we aren't in a wave interval, determine when we should be and watch for it
        if (!isInInterval)
        {
            if (timeSinceLastInterval > nextIntervalTime)
            {

                isInInterval = true;
                timeSinceLastInterval = 0;
                nextIntervalTime = Random.Range(smallestWaveInterval, largestWaveInterval);
            }
        }
       
        // If we are in a wave interval, determine when we should not be and wait for it
        if (isInInterval)
        {

            if (timeSinceLastInterval > nextIntervalTime)
            {
                isInInterval = false;
                timeSinceLastInterval = 0;
                nextIntervalTime = Random.Range(smallestNonwaveInterval, largestNonwaveInterval);
                GetComponent<Light>().color = originalColor;
                GetComponent<Light>().intensity = originalIntensity;
                return;
            }

        }
    }

}
