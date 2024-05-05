using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonSitting : MonoBehaviour
{
    public bool isMaleOverride = false;
    public bool isFemaleOverride = false;

    public GameObject male;
    public GameObject female;

    public SkinnedMeshRenderer maleMR;
    public SkinnedMeshRenderer femaleMR;

    public Material[] potentialSkin;
    public Material[] potentialClothes;
    public Material[] potentialHair;
    public Material[] potentialPants;

    // Start is called before the first frame update
    void Start()
    {
        // Decide if male or female.
        var isMale = Random.Range(0f, 1f) < 0.5f;
        if (isMaleOverride) isMale = true;
        if (isFemaleOverride) isMale = false;

        // Pick a random skin material from the options and set all skin mesh renderers to use it.
        var skinOption = potentialSkin[Random.Range(0, potentialSkin.Length)];
        if (isMale)
        {
            var tempMaterials = maleMR.materials;
            tempMaterials[2] = skinOption;
            maleMR.materials = tempMaterials;
        }
        else
        {
            var tempMaterials = femaleMR.materials;
            tempMaterials[1] = skinOption;
            femaleMR.materials = tempMaterials;
        }
        
        // Pick a random clothes material from the options and set all clothes mesh renderers to use it.
        var clothesOption = potentialClothes[Random.Range(0, potentialClothes.Length)];
        if (isMale)
        {
            var tempMaterials = maleMR.materials;
            tempMaterials[1] = clothesOption;
            maleMR.materials = tempMaterials;
        }
        else
        {
            var tempMaterials = femaleMR.materials;
            tempMaterials[0] = clothesOption;
            femaleMR.materials = tempMaterials;
        }
        
        // Pick a random hair material from the options and set all hair mesh renderers to use it.
        var hairOption = potentialHair[Random.Range(0, potentialHair.Length)];
        if (isMale)
        {
            var tempMaterials = maleMR.materials;
            tempMaterials[3] = hairOption;
            maleMR.materials = tempMaterials;
        }
        else
        {
            var tempMaterials = femaleMR.materials;
            tempMaterials[2] = hairOption;
            femaleMR.materials = tempMaterials;
        }
        
        // Pick a random pants material from the options and set all pants mesh renderers to use it.
        var pantsOption = potentialPants[Random.Range(0, potentialPants.Length)];
        if (isMale)
        {
            var tempMaterials = maleMR.materials;
            tempMaterials[0] = pantsOption;
            maleMR.materials = tempMaterials;
        }
        else
        {
            var tempMaterials = femaleMR.materials;
            tempMaterials[3] = pantsOption;
            femaleMR.materials = tempMaterials;
        }
        
        // Set the male active if male. Else, set female active.
        if (isMale)
        {
            if (male) male.SetActive(true);
            if (female) female.SetActive(false);
        }
        else
        {
            if (male) male.SetActive(false);
            if (female) female.SetActive(true);
        }
    }
}
