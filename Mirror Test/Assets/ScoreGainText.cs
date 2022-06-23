using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreGainText : MonoBehaviour
{
    public TextMeshProUGUI text;

    [SerializeField] Vector3 startPos;
    [SerializeField] float floatSpeed;
    [SerializeField] float visableTime;

    private void Update()
    {
        transform.localPosition += Vector3.up * floatSpeed * Time.deltaTime;
    }

    private void OnEnable()
    {
        Invoke(nameof(Disable), visableTime);

        transform.localPosition = startPos;
    }

    void Disable()
    {
        gameObject.SetActive(false);
    }
}
