using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HintObject : MonoBehaviour
{
    private int _number;
    // Private variable to store the number.

    [SerializeField]
    private Image hintImage;
    // Reference to the Image component used to display the hint image.

    [SerializeField]
    private TextMeshProUGUI hintText;
    // Reference to the TextMeshProUGUI component used to display the hint text.

    private void Start()
    {
        gameObject.SetActive(false);
        // Deactivate the game object that this script is attached to.
        // This ensures that it is initially hidden.
    }

    public void SetColorAndNumber(int number)
    {
        hintImage.color = Colors.colorsDict[number];
        // Set the color of the hint image based on the number using the Colors.colorsDict dictionary.

        hintText.text = NumberFormatter(number);
        // Set the text of the hint text using the formatted number based on the number.
    }
    private string NumberFormatter(int numberToFormat)
    {
        string formattedNumber;

        if (_number >= 1000)
        {
            int thousands = numberToFormat / 1000;
            formattedNumber = thousands.ToString() + "K";
            // Format the number as thousands and append "K" to the formatted string
        }
        else
        {
            formattedNumber = numberToFormat.ToString();
            // Format the number as a regular string
        }

        return formattedNumber;
        // Return the formatted number string
    }
}
