using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition current;

    public static Action OnTransitionExit;

    public static string startAnim;

    Animator anim;

    int sceneIndex;

    private void Awake()
    {
        current = this;
        anim = GetComponent<Animator>();

        if (startAnim != null) anim.Play(startAnim);
    }

    public void Exit()
    {
        anim.Play("Transition_Exit");
    }
    public void Exit(string _exitAnim)
    {
        anim.Play(_exitAnim);
    }
    public void ExitToScene(int _sceneIndex, string _exitAnim = "Transition_Exit")
    {
        sceneIndex = _sceneIndex;
        OnTransitionExit += LoadScene;
        anim.Play(_exitAnim);
    }

    public void ReEnter(string _exitAnim = "Transition_Enter")
    {
        anim.Play(_exitAnim);
    }

    void LoadScene()
    {
        OnTransitionExit -= LoadScene;
        SceneManager.LoadScene(sceneIndex);
    }

    void TransitionExit()
    {
        OnTransitionExit?.Invoke();
        startAnim = "Transition_Enter";
    }

    void SecondExit()
    {
        OnTransitionExit?.Invoke();
        startAnim = "Transition2_Enter";
    }
}
