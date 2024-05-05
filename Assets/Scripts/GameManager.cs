using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Whisper.Samples;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject tvScreen;
    public bool IsMale;

    float TotalSupport = 0;

    int currentLevel = 0;
    int[] LEVEL_CENSORED_WORD_CAPS    = { 1, 1, 2, 2, 3, 4 };
    int[] LEVEL_SUPPORT_IF_SUCCESSFUL = { 5, 10, 15, 20, 25, 30 };
    int MINIMUM_WORD_COUNT = 50;

    int totalCensors = 0;
    int totalWords = 0;

    string[] goodQuotes = 
    {
        "That speech was great!",
        "What wise words...",
        "That was so true!",
        "Ain't that the truth!",
        "What a masterpiece!",
        "Wow, I love that!",
        "So true!",
        "I 100% agree!"
    };

    string[] badQuotes =
    {
        "What did that mean?",
        "I don't get it!",
        "What an idiot...",
        "Terrible candidate...",
        "Terrible... Just stop...",
        "Just stop...",
        "I lost some brain cells..."
    };

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartGame(bool isMale)
    {
        IsMale = isMale;
        SceneManager.LoadScene("GivingASpeechScene", LoadSceneMode.Single);
    }

    public async void FinishSpeech()
    {
        SceneManager.LoadScene("TVScene", LoadSceneMode.Single);

        await Task.Delay(1000);

        var quoteText = GameObject.Find("Quote").GetComponent<Text>();

        await SubtitlesDemo.Instance.ProcessAudio();

        await Task.Delay(3000);

        var totalWordCount = SubtitlesDemo.Instance.GetPreviousTotalWordCount();
        totalWords += totalWordCount;
        var censoredWordCount = SubtitlesDemo.Instance.GetPreviousCensoredWordCount();
        totalCensors += censoredWordCount;
        var addedSupport = 0;

        if (totalWordCount < MINIMUM_WORD_COUNT || censoredWordCount > LEVEL_CENSORED_WORD_CAPS[currentLevel])
        {
            quoteText.text = badQuotes[Random.Range(0, badQuotes.Length)];
            var wordsOverCount = Mathf.Max(0, censoredWordCount - LEVEL_CENSORED_WORD_CAPS[currentLevel]);
            addedSupport = LEVEL_SUPPORT_IF_SUCCESSFUL[currentLevel];
            addedSupport = Mathf.Max(0, addedSupport - currentLevel * wordsOverCount);
            addedSupport = Mathf.Max(0, addedSupport - Mathf.Max(0, (MINIMUM_WORD_COUNT - totalWordCount)));
        }
        else
        {
            quoteText.text = goodQuotes[Random.Range(0, goodQuotes.Length)];
            addedSupport = LEVEL_SUPPORT_IF_SUCCESSFUL[currentLevel];
        }

        TotalSupport += addedSupport;

        var newBannedWordsCount = (10 - currentLevel) + 3;
        var newBannedWords = SubtitlesDemo.Instance.BanSomeWords(newBannedWordsCount);
        
        var levelEndResultsText = GameObject.Find("Level End Text").GetComponent<Text>();

        await Task.Delay(3000);
        levelEndResultsText.text = "You swayed " + addedSupport + "% of the population.\n";
        await Task.Delay(3000);
        levelEndResultsText.text += "Your total support is now " + TotalSupport + "% of the population.\n";
        await Task.Delay(3000);
        levelEndResultsText.text += "You said " + censoredWordCount + " harmful words.\n";
        await Task.Delay(3000);

        currentLevel++;

        if (TotalSupport > 50)
        {
            GameOver(true);
        }
        else if (currentLevel <= 6)
        {
            levelEndResultsText.text += "But, we found some more words you used were harmful.\n";
            await Task.Delay(3000);
            levelEndResultsText.text += "Those were:\n";
            await Task.Delay(3000);
            levelEndResultsText.text = "";
            for (int i = 0; i < newBannedWords.Count; i++)
            {
                levelEndResultsText.text += newBannedWords[i] + "\n";
                await Task.Delay(1000 - i * 50);
            }
            await Task.Delay(1000);
            levelEndResultsText.text = "Thank you for helping us keep the country safe.";
            await Task.Delay(3000);

            StartGame(IsMale);
        }
        else
        {
            GameOver(false);
        }
    }

    async void GameOver(bool wonTheGame)
    {
        SceneManager.LoadScene("GameOverScene", LoadSceneMode.Single);

        await Task.Delay(1000);

        var gameOverText = GameObject.Find("Game Over Text").GetComponent<Text>();
        var infoText = GameObject.Find("Info Text").GetComponent<Text>();
        gameOverText.text = "";
        infoText.text = "";

        if (wonTheGame)
        {
            gameOverText.text = "Victory!";
            await Task.Delay(2000);
            infoText.text = "You achieved a total of " + TotalSupport + "% support, meaning you won the election.\n";
        }
        else
        {
            gameOverText.text = "Defeat!";
            await Task.Delay(2000);
            infoText.text = "You only achieved a total of " + TotalSupport + "% support, meaning you did not win the election.\n";
        }

        await Task.Delay(2000);
        infoText.text += "You gave " + currentLevel + " speeches.\n";
        await Task.Delay(2000);
        infoText.text += "You had " + SubtitlesDemo.Instance.BannedWords.Count + " words banned.\n";
        await Task.Delay(2000);
        infoText.text += "You were censored " + totalCensors + " times.\n";
        await Task.Delay(2000);
        infoText.text += "You said a total of " + totalWords + " words.\n\n";
        await Task.Delay(2000);

        infoText.text += "Censorship does not protect people.\n";
        await Task.Delay(2000);
        infoText.text += "It only leads to people not being able to say what they mean.\n";
        await Task.Delay(2000);
        infoText.text += "Its existence leads to a stifled sharing of ideas.\n";
        await Task.Delay(2000);
        infoText.text += "Constant fear means conversations are less likely to happen,\n";
        await Task.Delay(2000);
        infoText.text += "and when they do happen, they are less productive.\n";
        await Task.Delay(2000);
        infoText.text += "Do not be censored.";
        await Task.Delay(3000);
        Application.Quit();
    }

}
