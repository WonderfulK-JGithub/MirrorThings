using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGround2 : MonoBehaviour
{
    [SerializeField] Transform imageX;
    [SerializeField] Transform imageY;
    [SerializeField] float time;
    [SerializeField] float interval;

    float timer;

    int mode;

    private void Awake()
    {
        NewStuff();
    }

    void Update()
    {
        switch (mode)
        {
            case 0:
                imageX.localPosition = new Vector3(Mathf.Lerp(450f, -450f, timer / time),imageX.localPosition.y,0f);
                timer -= Time.deltaTime;
                if (timer <= 0) NewStuff();
                break;
            case 1:
                imageX.localPosition = new Vector3(Mathf.Lerp(-450f, 450f, timer / time), imageX.localPosition.y, 0f);
                timer -= Time.deltaTime;
                if (timer <= 0) NewStuff();
                break;
            case 2:
                imageY.localPosition = new Vector3(imageY.localPosition.x, Mathf.Lerp(310f, -310f, timer / time), 0f);
                timer -= Time.deltaTime;
                if (timer <= 0) NewStuff();
                break;
            case 3:
                imageY.localPosition = new Vector3(imageY.localPosition.x, Mathf.Lerp(310f, -310f, timer / time), 0f);
                timer -= Time.deltaTime;
                if (timer <= 0) NewStuff();
                break;
        }
    }

    void NewStuff()
    {
        mode = -1;
        timer = time;
        Invoke(nameof(IDK), Random.Range(0f, interval));
    }

    void IDK()
    {
        mode = Random.Range(0, 4);
    }
}
