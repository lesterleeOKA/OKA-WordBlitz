using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class QuestionDataWrapper
{
    public QuestionList[] QuestionDataArray;
}

[Serializable]
public class QuestionData
{ 
    public List<QuestionList> Data;
}
[Serializable]
public class QuestionList
{
    public int id;
    public string QID;
    public string QuestionType;
    public string Question;
    public string[] Answers;
    public string CorrectAnswer;
    public string[] Media;
    public Texture texture;
    public AudioClip audioClip;
}

public enum QuestionType
{
    None = 0,
    Text = 1,
    Picture = 2,
    Audio = 3,
    FillInBlank = 4
}

[Serializable]
public class CurrentQuestion
{
    public int numberQuestion = 0;
    public QuestionType questiontype = QuestionType.None;
    public QuestionList qa = null;
    public string correctAnswer;
    public string[] answersChoics;
    public CanvasGroup[] questionBgs;
    private RawImage questionImage;
    public CanvasGroup audioPlayBtn = null;
    private AspectRatioFitter aspecRatioFitter = null;

    public void setNewQuestion(QuestionList qa = null, int totalQuestion = 0)
    {
        if (qa == null) return;
        this.qa = qa;
        TextMeshProUGUI questionText = null;
        switch (qa.QuestionType)
        {
            case "Picture":
                SetUI.SetGroup(this.questionBgs, 0, 0f);
                this.questionImage = this.questionBgs[0].GetComponentInChildren<RawImage>();
                this.questiontype = QuestionType.Picture;
                var qaImage = qa.texture;
                this.correctAnswer = qa.CorrectAnswer;
                this.answersChoics = qa.Answers;

                if (this.questionImage != null && qaImage != null)
                {
                    this.questionImage.enabled = true;
                    this.aspecRatioFitter = this.questionImage.GetComponent<AspectRatioFitter>();
                    this.questionImage.texture = qaImage;
                    var width = this.questionImage.GetComponent<RectTransform>().sizeDelta.x;
                    if (qaImage.width > qaImage.height)
                    {
                        this.questionImage.GetComponent<RectTransform>().sizeDelta = new Vector2(width, 350f);
                    }
                    else
                    {
                        this.questionImage.GetComponent<RectTransform>().sizeDelta = new Vector2(width, 430f);
                    }
                    this.aspecRatioFitter.aspectRatio = (float)qaImage.width / (float)qaImage.height;
                }
                break;
            case "Audio":
                SetUI.SetGroup(this.questionBgs, 1, 0f);
                this.audioPlayBtn = this.questionBgs[1].GetComponentInChildren<CanvasGroup>();
                if (this.audioPlayBtn != null)
                {
                    SetUI.Set(this.audioPlayBtn, true, 0f);
                }
                this.questiontype = QuestionType.Audio;
                this.correctAnswer = qa.CorrectAnswer;
                this.answersChoics = qa.Answers;
                this.playAudio();
                break;
            case "Text":
                SetUI.SetGroup(this.questionBgs, 2, 0f);
                this.questiontype = QuestionType.Text;
                questionText = this.questionBgs[2].GetComponentInChildren<TextMeshProUGUI>();
                if (questionText != null)
                {
                    questionText.text = qa.Question;
                }
                this.correctAnswer = qa.CorrectAnswer;
                this.answersChoics = qa.Answers;
                break;
            case "FillInBlank":
                SetUI.SetGroup(this.questionBgs, 3, 0f);
                questionText = this.questionBgs[3].GetComponentInChildren<TextMeshProUGUI>();
                if(questionText != null)
                {
                    questionText.text = qa.Question;
                }
                this.audioPlayBtn = this.questionBgs[3].GetComponentInChildren<CanvasGroup>();
                if(this.audioPlayBtn != null)
                {
                    SetUI.Set(this.audioPlayBtn, true, 0f);
                }
                this.questiontype = QuestionType.FillInBlank;
                this.correctAnswer = qa.CorrectAnswer;
                this.answersChoics = qa.Answers;
                this.playAudio();
                break;
        }

        if (LogController.Instance != null)
        {
            LogController.Instance.debug($"Get new {nameof(this.questiontype)} question");
        }

        if (this.numberQuestion < totalQuestion - 1)
            this.numberQuestion += 1;
        else
            this.numberQuestion = 0;
    }

    public void playAudio()
    {
        if(this.audioPlayBtn != null && this.qa.audioClip != null)
        {
            this.audioPlayBtn.GetComponentInChildren<AudioSource>().clip = this.qa.audioClip;
            this.audioPlayBtn.GetComponentInChildren<AudioSource>().Play();
        }
    }
}

public static class SortExtensions
{
    // Fisher-Yates shuffle algorithm
    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1); // Use Unity's Random class
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    public static void ShuffleArray<T>(T[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, array.Length);
            T temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }
}