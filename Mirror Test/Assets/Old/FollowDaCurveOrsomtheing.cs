using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowDaCurveOrsomtheing : MonoBehaviour
{
    [SerializeField] BezierSpline spline;
    [SerializeField] float time;
    [SerializeField] float acceleration;
    [SerializeField] float slowDown;

    float timer;

    float averageLength;
    float speed = 1f;

    private void Awake()
    {
        averageLength = spline.GetTotalLength() / spline.CurveCount;
    }

    private void Update()
    {
        //speed = Mathf.MoveTowards(speed, averageLength / spline.GetLength(timer / time), acceleration * Time.deltaTime);

        timer += Time.deltaTime * speed * spline.GetAdapdiveSpeed(timer / time,slowDown,0.05f);

        timer = Mathf.Clamp(timer, 0, time);

        Vector3 _pos = spline.GetPoint(timer / time);

        transform.position = _pos;
    }

    [ContextMenu("a")]
    public void ABC()
    {
        timer = 0;
    }
}
