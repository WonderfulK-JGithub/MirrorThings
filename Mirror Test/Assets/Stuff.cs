using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Quiz
{
    public string name;

    public List<Question> questions;

    public Quiz()
    {
        questions = new List<Question>();

        name = "MyQuiz";
    }
}

[System.Serializable]
public class Question
{
    public string question;

    public List<string> answers;
    public List<bool> corrections;

    public float time;

    public int points;

    public QuizImage image;

    public Question()
    {
        answers = new List<string>();
        corrections = new List<bool>();

        time = 10f;

        points = 1000;

        answers.Add(null);
        answers.Add(null);
        answers.Add(null);
        answers.Add(null);

        corrections.Add(false);
        corrections.Add(false);
        corrections.Add(false);
        corrections.Add(false);
    }
}

[System.Serializable]
public class QuizImage
{
    public byte[] imageData;

    public int width;
    public int height;

    public TextureFormat format;
}
