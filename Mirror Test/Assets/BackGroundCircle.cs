using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackGroundCircle : MonoBehaviour
{
    [Header("pls")]
    [SerializeField] Vector3 a;
    [SerializeField] Vector3 b;

    [SerializeField] Color[] randomColors;

    [SerializeField] float maxX;
    [SerializeField] float maxY;

    [SerializeField] float minWidthHeight;

    [SerializeField] float travelSpeed;
    [SerializeField] float slowDown;

    float t;
    float adaptiveSpeed;
    float aproprietSpeed;

    Vector3 og;
    
    void Start()
    {
        float _rng1 = Random.Range(minWidthHeight, 100f);

        GetComponent<RectTransform>().sizeDelta = new Vector2(_rng1, _rng1);

        GetComponent<Image>().color = randomColors[Random.Range(0, randomColors.Length)];

        NewDestination();
    }

    // Update is called once per frame
    void Update()
    {

        float _dif = Mathf.Abs(t - 0.5f);

        float _a = (1 - slowDown) / 0.25f;

        adaptiveSpeed = 1 - _a * _dif * _dif;


        t += Time.deltaTime * travelSpeed * adaptiveSpeed;

        transform.localPosition = Vector3.Lerp(Vector3.Lerp(og, a, t), Vector3.Lerp(a, b, t), t);

        if(t >= 1f)
        {
            NewDestination();
        }
    }

    void NewDestination()
    {
        t = 0f;

        og = transform.localPosition;

        a = new Vector3(Random.Range(-maxX, maxX), Random.Range(-maxY, maxY), 0f);
        b = new Vector3(Random.Range(-maxY, maxY), Random.Range(-maxY, maxY), 0f);

        aproprietSpeed = 100f / Vector3.Distance(og, b) * Vector3.Distance(og, b);
    }
}
