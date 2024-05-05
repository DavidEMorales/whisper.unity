using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Samples;

public class PlayerAnimatesDuringSpeech : MonoBehaviour
{
    public GameObject male;
    public GameObject female;

    public Animator maleAnimator;
    public Animator femaleAnimator;
    private Animator applicableAnimator;

    public int totalSpeechTimer = 60;
    public int currentSpeechTimer = 60;
    GameObject duringSpeech;

    public int totalThinkingTimer = 60;
    public int currentThinkingTimer = 60;
    GameObject beforeSpeech;

    bool gaveSpeech = false;

    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.Instance.IsMale)
        {
            male.SetActive(true);
            female.SetActive(false);
            applicableAnimator = maleAnimator;
        }
        else
        {
            male.SetActive(false);
            female.SetActive(true);
            applicableAnimator = femaleAnimator;
        }

        beforeSpeech = GameObject.Find("BeforeSpeech");
        duringSpeech = GameObject.Find("DuringSpeech");

        beforeSpeech.SetActive(true);
        duringSpeech.SetActive(false);
        
        StartCoroutine(CountdownThinking());
    }

    // Update is called once per frame
    void Update()
    {
        if (!gaveSpeech && applicableAnimator.GetCurrentAnimatorStateInfo(0).IsName("GivingSpeech"))
        {
            gaveSpeech = true;
            // Start Whisper if not started.
            if (!SubtitlesDemo.Instance.microphoneRecord.IsRecording)
            {
                beforeSpeech.SetActive(false);
                duringSpeech.SetActive(true);

                SubtitlesDemo.Instance.OnButtonPressed();
                StartCoroutine(CountdownSpeech());
            }
        }
    }

    IEnumerator CountdownThinking()
    {
        var bannedwordsText = GameObject.Find("Banned Words").GetComponent<Text>();
        if (SubtitlesDemo.Instance.BannedWords.Count > 0)
        {
            bannedwordsText.text = "";
            foreach (string bannedWord in SubtitlesDemo.Instance.BannedWords)
            {
                bannedwordsText.text += bannedWord + ", ";
            }
            bannedwordsText.text = bannedwordsText.text.Substring(0, bannedwordsText.text.Length - 2);
        }

        currentThinkingTimer = totalThinkingTimer;

        var thinkingTimerText = GameObject.Find("Think Timer Text").GetComponent<Text>();
        thinkingTimerText.text = "" + currentThinkingTimer;

        while (currentThinkingTimer > 0)
        {
            yield return new WaitForSeconds(1);
            currentThinkingTimer--;
            thinkingTimerText.text = "" + currentThinkingTimer;
        }

        applicableAnimator.SetTrigger("WalkToStage");

        GameObject.Find("BeforeSpeech").SetActive(false);
    }

    IEnumerator CountdownSpeech()
    {
        currentSpeechTimer = totalSpeechTimer;

        var speechTimerText = GameObject.Find("Speech Timer Text").GetComponent<Text>();
        speechTimerText.text = "" + currentSpeechTimer;

        while (currentSpeechTimer > 0)
        {
            yield return new WaitForSeconds(1);
            currentSpeechTimer--;
            speechTimerText.text = "" + currentSpeechTimer;
        }

        applicableAnimator.SetTrigger("DoneSpeaking");
        SubtitlesDemo.Instance.OnButtonPressed();
        GameManager.Instance.FinishSpeech();
    }

    public void StartGivingSpeech()
    {
        currentThinkingTimer = 0;
    }

    public void StopGivingSpeech()
    {
        currentSpeechTimer = 0;
    }
}
