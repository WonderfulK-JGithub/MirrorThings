using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioVisualTest : MonoBehaviour
{
    [SerializeField] RectTransform[] staplar;
    [SerializeField] float heightMultiplier;
    [SerializeField] int numberOfSamples;
    [SerializeField] FFTWindow fftWindow;
    [SerializeField] float lerpTime;
    [SerializeField] int frameInterval;

    int frameCount;
    float[] spectrum;

    AudioSource source;

    private void Awake()
    {
        spectrum = new float[numberOfSamples];
        staplar = new RectTransform[transform.childCount];
        int i = 0;
        foreach (var item in transform.GetComponentsInChildren<RectTransform>())
        {
            staplar[i] = item;
            i++;
        }


        source = GetComponent<AudioSource>();
    }

    void Update()
    {

        if(frameCount == 0)
        {
            //source.GetSpectrumData(spectrum, 0, fftWindow);
            AudioListener.GetSpectrumData(spectrum, 0, fftWindow);
        }
        frameCount++;
        if (frameCount >= frameInterval)
        {
            frameCount = 0;
        }
        

        for (int i = 0; i < staplar.Length; i++)
        {
            RectTransform _stapel = staplar[i];

            float _intesity = spectrum[i] * heightMultiplier;

            float _height = Mathf.Lerp(_stapel.sizeDelta.y, _intesity, lerpTime * Time.deltaTime );

            _stapel.sizeDelta = new Vector2(_stapel.sizeDelta.x, _height);
        }
    }
}
