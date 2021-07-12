using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneTest : MonoBehaviour
{
    //Here's how this code would go:
    //Microphone input turns into game input.
    //For the maingame, it would go like "If microphone input is done during right timing, 1 point is added"
    //For this test script, I want to do something like "The white box in SampleScene turns red when a microhpone input is done"

        //nvm, just start doing the karaoke part instead

    public float RmsValue;
    public float DbValue;
    public float PitchValue;
    public GameObject Square;

    private const int qSamples = 1024;
    private const float RefValue = 0.1f;
    private const float Threshold = 0.02f;

    float[] _samples;
    private float[] _spectrum;
    private float _fSample;

    void Start()
    {
        _samples = new float[qSamples];
        _spectrum = new float[qSamples];
        _fSample = AudioSettings.outputSampleRate;
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = Microphone.Start(null, true, 10, 44100);
        audioSource.Play();
    }

    void Update()
    {
        AnalyzeSound();
        ChangeColour();
    }

    void AnalyzeSound()
    {
        GetComponent<AudioSource>().GetOutputData(_samples, 0); // fill array with samples
        int i;
        float sum = 0;
        for (i = 0; i < qSamples; i++)
        {
            sum += _samples[i] * _samples[i]; // sum squared samples
        }
        RmsValue = Mathf.Sqrt(sum / qSamples); // rms = square root of average
        DbValue = 20 * Mathf.Log10(RmsValue / RefValue); // calculate dB
        if (DbValue < -160) DbValue = -160; // clamp it to -160dB min
                                            // get sound spectrum
        GetComponent<AudioSource>().GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);
        float maxV = 0;
        var maxN = 0;
        for (i = 0; i < qSamples; i++)
        { // find max 
            if (!(_spectrum[i] > maxV) || !(_spectrum[i] > Threshold))
                continue;

            maxV = _spectrum[i];
            maxN = i; // maxN is the index of max
        }
        float freqN = maxN; // pass the index to a float variable
        if (maxN > 0 && maxN < qSamples - 1)
        { // interpolate index using neighbours
            var dL = _spectrum[maxN - 1] / _spectrum[maxN];
            var dR = _spectrum[maxN + 1] / _spectrum[maxN];
            freqN += 0.5f * (dR * dR - dL * dL);
        }
        PitchValue = freqN * (_fSample / 2) / qSamples; // convert index to frequency
    }

    void ChangeColour()
    {
        if (DbValue < 50)
        {
            GameObject Square = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var spriteRenderer = Square.GetComponent<Renderer>();
            spriteRenderer.material.SetColor("_Color", Color.white);
        }
        else
        {
            GameObject Square = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var spriteRenderer = Square.GetComponent<Renderer>();
            spriteRenderer.material.SetColor("_Color", Color.white);
        }
    }
}

