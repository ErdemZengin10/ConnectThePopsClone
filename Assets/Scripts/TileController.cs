
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Core.Easing;
using DG.Tweening.Plugins.Options;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileController : MonoBehaviour
{
    private int _number = 0;
    // Private variable to store the number of the tile. Initialized to 0.

    public int xIndex;
    // Public variable to store the x index of the tile in the grid.

    public int yIndex;
    // Public variable to store the y index of the tile in the grid.

    public GameObject selectIndicator;
    // Reference to the select indicator GameObject associated with the tile.

    [SerializeField]
    public Image tileImage;
    // Reference to the Image component used to display the tile's image.

    private Vector3 scaledValue;
    // Private variable to store the scaled value of the tile's transform.

    private Vector3 originalScale = new Vector3(1.5f, 1.5f, 1.5f);
    // The original scale of the tile, set to (1.5, 1.5, 1.5) as a Vector3.

    [SerializeField]
    private TextMeshProUGUI _textMeshPro;
    // Reference to the TextMeshProUGUI component used to display text on the tile.

    public int number => _number;
    // Public property that returns the value of _number. Provides read-only access to the number of the tile.
    private void Start()
    {
        scaledValue = originalScale * 1.1f;
    }

    public void Set(int number)
    {
        _number = number;
        // Set the number of the tile to the provided value

        _textMeshPro.text = NumberFormatter(_number);
        // Update the TextMeshProUGUI component with the formatted number text

        gameObject.name = NumberFormatter(_number);
        // Update the name of the GameObject to the formatted number text

        SetColor(number);
        // Set the color of the tile based on the number
    }
    public void LevelUp(int multiplyFactor)
    {
        _number *= multiplyFactor;
        // Multiply the current number by the provided multiplyFactor

        _textMeshPro.text = NumberFormatter(_number);
        // Update the TextMeshProUGUI component with the formatted number text

        SetColor(_number);
        // Set the color of the tile based on the updated number
    }
    public void SetCoordinates(int x, int y)
    {
        xIndex = x;
        // Set the x index of the tile to the provided value

        yIndex = y;
        // Set the y index of the tile to the provided value
    }
    private void SetColor(int number)
    {
        Color tileColor = Colors.colorsDict[number];
        // Retrieve the color associated with the number from the Colors.colorsDict dictionary

        tileImage.color = tileColor;
        // Set the color of the tile's image to the retrieved color
    }
    public void ScaleUpOnHover()
    {
        transform.DOScale(scaledValue, 0f);
        // Apply a scaling animation to the tile's transform using the DOTween library
        // The tile's scale will be set to the value of scaledValue with a duration of 0 seconds (instantaneous)
    }
    public void ScaleToOriginal()
    {
        transform.DOScale(originalScale, 0f);
        // Apply a scaling animation to the tile's transform to scale it back to the original scale
    }

    public TweenerCore<Vector3, Vector3, VectorOptions> MoveAndDestroy(TileController lastTile)
    {
        GameManager.Instance.isAnimating = true;
        // Set the isAnimating flag in the GameManager to true to indicate that an animation is in progress

        return transform.DOMove(lastTile.transform.position, 0.3f).OnComplete(() =>
        {
            GetComponentInParent<GamePiece>().BlastTile();
            // Call the BlastTile method on the GamePiece component that the tile belongs to

            GameManager.Instance.isAnimating = false;
            // Set the isAnimating flag in the GameManager to false to indicate that the animation is complete
        });
        // Move the tile's transform to the position of the lastTile with a duration of 0.3 seconds using DOTween.
        // When the animation completes, execute the provided lambda expression as the OnComplete callback.
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
