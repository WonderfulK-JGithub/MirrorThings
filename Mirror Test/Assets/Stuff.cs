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

    public QuestionType type;

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
public class WriteQuestion : Question
{
    public List<string> acceptedAnswers;

    public bool correctCaps;

    public WriteQuestion() : base()
    {
        time = 10f;

        points = 1000;

        type = QuestionType.Write;

        acceptedAnswers = new List<string>();
        acceptedAnswers.Add(null);
    }
}
[System.Serializable]
public class AudioQuestion : Question
{
    public QuizAudio audio;
    public AudioQuestion() : base()
    {

        
        time = 10f;

        points = 1000;

        type = QuestionType.Audio;
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
[System.Serializable]
public class QuizAudio
{
    public float[] audioData;

    public int samples;
    public int channels;
    public int frequency;
}

public interface IClickable
{
    void Click();
}

public interface IHoverable
{
    void StartHover();

    void EndHover();
}

public enum QuestionType
{
    Normal,
    Write,
    Audio,
}