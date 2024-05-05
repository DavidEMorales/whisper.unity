using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AssignPrompt : MonoBehaviour
{
    public static string[] prompts =
    {
        "Homelessness",
        "Corruption",
        "Gun Control",
        "Climate Change",
        "Drugs",
        "Crime",
        "Terrorism",
        "The Economy",
        "Cybersecurity",
        "AI",
        "Foreign Policy",
        "Clean Energy",
        "Human Trafficking",
        "Illegal Immigration",
        "Police",
        "Healthcare",
        "Abortion",
        "Ukraine",
        "Israel",
        "Russia",
        "National Debt",
        "Inflation",
        "Lawfare",
        "Self Defense",
        "Jobs",
        "Racism",
        "Transgenderism",
        "Education",
        "China",
        "Taxes"
    };
    
    void Start()
    {
        var newPromptIndex = Random.Range(0, prompts.Length);
        var newPrompt = prompts[newPromptIndex];
        prompts = prompts.Where(prompt => prompt != newPrompt).ToArray();
        GetComponent<Text>().text = newPrompt;
    }
}
