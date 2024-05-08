using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class GamePiece : MonoBehaviour
{
    public LineController lineController => LineController.Instance;
    // The LineController instance used for managing the game lines.

    public bool isDestroy;
    // A flag indicating whether the object is flagged for destruction.

    public int xIndex;
    // The x-index of the tile within the game grid.

    public int yIndex;
    // The y-index of the tile within the game grid.

    private GameManager gameManager => GameManager.Instance;
    // The GameManager instance used for managing the game.

    public TileController tile { private set; get; }
    // The TileController component associated with the tile.

    public bool hasTile => tile != null;
    // Indicates whether the tile has a TileController component, i.e., whether it is filled.

    [SerializeField]
    private float tileMoveTime = 10f;
    // The time it takes for the tile to move from one position to another.

    public void PlaceTile(TileController tileController)
    {
        tile = tileController;
        // Assign the given tileController to the tile variable, indicating that the tile is filled.

        tileController.transform.SetParent(transform, true);
        // Set the parent of the tileController's transform to this object's transform.
        // This ensures that the tile appears as a child of this object in the game hierarchy.
    }

    public TileController PullTile()
    {
        var tempTile = tile;
        // Store a reference to the current tile in a temporary variable.

        tile = null;
        // Clear the tile variable, indicating that the tile is now empty.

        tempTile.transform.SetParent(transform.parent, true);
        // Set the parent of the tempTile's transform to the parent of this object's transform.
        // This removes the tile from being a child of this object and places it at the same level in the hierarchy.

        return tempTile;
        // Return the stored reference to the tile.
    }

    public void BlastTile()
    {
        Destroy(tile.gameObject);
        // Destroy the game object associated with the tile, removing it from the game.

        tile = null;
        // Clear the tile variable, indicating that the tile is now empty.
    }

    public void SetXandY(int x, int y)
    {
        xIndex = x;
        // Set the x-index of the tile to the given value.

        yIndex = y;
        // Set the y-index of the tile to the given value.
    }

    public void OnPressed()
    {
        if (!gameManager.isAnimating)
        {
            gameManager.isPressing = true;
            // Set the isPressing flag in the GameManager to indicate that a tile is being pressed.

            gameManager.firstClickedTile = tile;
            // Set the firstClickedTile in the GameManager to the current tile.

            lineController.ChangeFirstDot(gameManager.firstClickedTile.gameObject.transform.parent.GetComponent<RectTransform>().localPosition.x, gameManager.firstClickedTile.gameObject.transform.parent.GetComponent<RectTransform>().localPosition.y);
            // Update the position of the first dot in the LineController based on the position of the firstClickedTile.

            lineController.SetColorForLine(gameManager.firstClickedTile.number);
            // Set the color of the line in the LineController based on the number of the firstClickedTile.

            gameManager.selectedTileControllers.Add(tile);
            // Add the current tile to the list of selectedTileControllers in the GameManager.

            tile.ScaleUpOnHover();
            // Scale up the current tile to give a visual feedback on hover.

            SetHintObject(gameManager.firstClickedTile.number, true);
            // Set the hint object based on the number of the firstClickedTile, indicating a valid move.
        }
    }

    public void OnReleased()
    {
        OnReleasedAsync();
    }
    public async void OnReleasedAsync()
    {
        if (gameManager.selectedTileControllers.Count > 0)
        {
            // Get the last selected tile from the list
            TileController lastTile = gameManager.selectedTileControllers.Last();

            // Get the number of tiles in the selectedTileControllers list
            int tileListCount = gameManager.selectedTileControllers.Count;

            gameManager.isPressing = false;
            // Reset the isPressing flag in the GameManager

            lastTile.ScaleToOriginal();
            // Scale the last selected tile back to its original size

            gameManager.hintObject.SetActive(false);
            // Deactivate the hint object in the GameManager

            if (tileListCount > 1)
            {
                TweenerCore<Vector3, Vector3, VectorOptions> tweener = null;
                // Declare a tweener variable to hold the reference to the movement tween

                for (int i = 0; i < tileListCount - 1; i++)
                {
                    // Move and destroy each selected tile leading up to the last tile
                    tweener = gameManager.selectedTileControllers[i].MoveAndDestroy(lastTile);
                }

                lineController.ClearPoints();
                // Clear the points in the LineController

                await tweener.AsyncWaitForCompletion();
                // Wait for the movement tween to complete asynchronously

                lastTile.LevelUp(MultiplyReturner(tileListCount));
                // Level up the last tile based on the number of selected tiles

                gameManager.ShiftTiles();
                // Shift the tiles in the game grid

                gameManager.FindNullTiles();
                // Find and update the list of null tiles in the game grid

                gameManager.DeadLockPrevention();
                // Check for deadlock situations in the game grid and take necessary actions

                gameManager.FillNullTiles(gameManager.emptyTiles);
                // Fill the null tiles in the game grid with new tiles
            }

            gameManager.selectedTileControllers.Clear();
            // Clear the list of selected tiles in the GameManager
        }
    }
    public void OnHoverEnter()
    {
        if (gameManager.firstClickedTile != null && gameManager.isPressing)
        {
            // Check if there is a first clicked tile and tile pressing is ongoing

            if (IsNeighbour(tile))
            {
                // Check if the current tile has the same number as the first clicked tile and is adjacent to the last selected tile

                if (!gameManager.selectedTileControllers.Contains(tile))
                {
                    // If the current tile is not already selected, add it to the selectedTileControllers list

                    gameManager.selectedTileControllers.Add(tile);

                    tile.ScaleUpOnHover();
                    // Scale up the current tile to provide visual feedback

                    SetHintObject(gameManager.firstClickedTile.number, false);
                    // Set the hint object based on the number of the first clicked tile, indicating a valid move

                    lineController.AddPoint(gameManager.selectedTileControllers[gameManager.selectedTileControllers.Count - 1].gameObject.transform.parent.GetComponent<RectTransform>().localPosition.x, gameManager.selectedTileControllers[gameManager.selectedTileControllers.Count - 1].gameObject.transform.parent.GetComponent<RectTransform>().localPosition.y);
                    // Add a point to the LineController based on the position of the current tile
                }
                else
                {
                    // If the current tile is already selected

                    if (gameManager.selectedTileControllers.Count > 1 && tile == gameManager.selectedTileControllers[gameManager.selectedTileControllers.Count - 2])
                    {
                        // Check if the current tile is the second-to-last selected tile

                        gameManager.selectedTileControllers[gameManager.selectedTileControllers.Count - 1].ScaleToOriginal();
                        // Scale down the current tile to its original size

                        gameManager.selectedTileControllers.RemoveAt(gameManager.selectedTileControllers.Count - 1);
                        // Remove the current tile from the selectedTileControllers list

                        SetHintObject(gameManager.firstClickedTile.number, false);
                        // Set the hint object based on the number of the first clicked tile, indicating a valid move

                        lineController.RemoveLastLine();
                        // Remove the last line segment from the LineController
                    }
                }
            }
        }
    }
    
    private int MultiplyReturner(int count)
    {
        return (int)Math.Pow(2, Math.Floor(Math.Log(count, 2)));
        //MultiplyReturner method takes a count as input and returns the largest power of 2 less than or equal to the count
    }
    private void SetHintObject(int number, bool isFirstTile = false)
    {
        gameManager.hintObject.SetActive(true);
        // Activate the hint object in the GameManager

        if (!isFirstTile)
        {
            // If it's not the first tile, modify the number based on the count of selected tiles

            var modifiedNumber = MultiplyReturner(gameManager.selectedTileControllers.Count) * number;
            // Multiply the number by the result of MultiplyReturner method and assign it to modifiedNumber

            gameManager.hintObject.GetComponent<HintObject>().SetColorAndNumber(modifiedNumber);
            // Set the color and number of the hint object based on the modifiedNumber
        }
        else
        {
            // If it's the first tile, set the color and number of the hint object based on the number of the first tile

            gameManager.hintObject.GetComponent<HintObject>().SetColorAndNumber(number);
        }
    }

    private bool IsNeighbour(TileController tile)
    {
        return tile.number == gameManager.firstClickedTile.number &&
            Mathf.Abs(tile.xIndex - gameManager.selectedTileControllers.Last().xIndex) <= 1 &&
            Mathf.Abs(tile.yIndex - gameManager.selectedTileControllers.Last().yIndex) <= 1;
    }
}

