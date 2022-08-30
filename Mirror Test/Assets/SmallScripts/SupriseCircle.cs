using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupriseCircle : MonoBehaviour
{
    [SerializeField] BezierSpline[] splines;
    [SerializeField] float time;
    [SerializeField] float slowDown;
    [SerializeField] RectTransform getRekt;
    [SerializeField] float expandSpeed;
    [SerializeField] Animator[] numbers;
    [SerializeField] GameObject[] theOutcasts;

    float timer;
    BezierSpline currentSpline;

    int o;
    
    public bool expand;
    bool wait;

    private void Update()
    {
        if (expand)
        {
            getRekt.sizeDelta += expandSpeed * Time.deltaTime * Vector2.one;
            if (getRekt.sizeDelta.magnitude >= 1400f) transform.parent.gameObject.SetActive(false);
            return;
        }

        if (currentSpline == null || wait) return;

        timer += Time.deltaTime * currentSpline.GetAdapdiveSpeed(timer / time, slowDown, 0.1f);

        timer = Mathf.Clamp(timer, 0, time);

        Vector3 _pos = currentSpline.GetPoint(timer / time);

        transform.position = _pos;

        if(timer == time)
        {
            numbers[o].SetTrigger("AmongUs");
            o++;
            wait = true;
            if(o == 3)
            {
                Invoke(nameof(EXPAND), 1f);
            }
            else
            {
                Invoke(nameof(Continue), 1f);
            }
        }
    }

    public void StartMoving()
    {
        timer = 0;
        time = 3;
        currentSpline = splines[o];
    }

    void Continue()
    {
        timer = 0;
        time = 3;
        currentSpline = splines[o];

        wait = false;
    }
    void EXPAND()
    {
        expand = true;
        foreach (var item in theOutcasts)
        {
            item.SetActive(true);
        }
    }
}
