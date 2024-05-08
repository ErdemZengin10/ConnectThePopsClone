using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Prefabs
    public GameObject tilePlacePrefab; // The prefab for the tile placeholder
    public GameObject tilePrefab; // The prefab for the tile

    // Game state
    public bool isPressing; // Flag indicating if the player is pressing
    public bool isAnimating; // Flag indicating if the game is currently animating

    // Tile grid
    public GamePiece[,] tiles; // 2D array to hold the game pieces
    public List<GamePiece> emptyTiles; // List to store empty game pieces
    public List<GamePiece> fullTiles; // List to store non-empty game pieces
    public List<GamePiece> tileToRemove; // List to store tiles to be removed

    // Grid size
    public int tileRowSize; // Number of rows in the grid
    public int tileColumnSize; // Number of columns in the grid

    // Parent canvas
    public Transform parentCanvas; // Reference to the parent canvas for tile placement

    // Position and spacing
    public float _xPos = -45; // Initial x position of the tiles
    public float _yPos; // Initial y position of the tiles
    [SerializeField]
    private float spaceFactor = 45; // Spacing factor between tiles

    // Selected tile and matching
    public TileController firstClickedTile; // Reference to the first clicked tile
    public List<TileController> selectedTileControllers; // List of selected tile controllers
    public bool isMatchFound = false; // Flag indicating if a match is found
    public Vector3 punchVector = new Vector3(0.1f, 0.1f, 0.1f); // Punch vector for tile animation
    public GameObject hintObject; // Hint object for gameplay

    #region SingletonMethod

    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();

                if (instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    instance = singletonObject.AddComponent<GameManager>();
                    singletonObject.name = "GameManagerSingleton";
                    DontDestroyOnLoad(singletonObject);
                }
            }

            return instance;
        }
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion


    void Start()
    {
        // Initialize the tiles array
        tiles = new GamePiece[tileRowSize, tileColumnSize];

        // Generate the tile grid
        for (int x = 0; x < tileRowSize; x++)
        {
            for (int y = 0; y < tileColumnSize; y++)
            {
                // Calculate the position for the current tile
                Vector2 position = new Vector2(_xPos, _yPos);

                // Instantiate the tile placeholder
                GameObject gamePiece = Instantiate(tilePlacePrefab, parentCanvas);

                // Set the local position and coordinates for the game piece
                gamePiece.transform.localPosition = position;
                gamePiece.GetComponent<GamePiece>().SetXandY(x, y);

                // Store the game piece in the tiles array
                tiles[x, y] = gamePiece.GetComponent<GamePiece>();

                // Update the y position for the next tile
                _yPos += spaceFactor;
            }

            // Update the x position and reset the y position for the next row
            _xPos += spaceFactor;
            _yPos = 0;
        }

        // Start the game
        GameStart();
    }

    private void GameStart()
    {
        // Iterate over each row and column of the tile grid
        for (int i = 0; i < tileRowSize; i++)
        {
            for (int j = 0; j < tileColumnSize; j++)
            {
                // Generate the first game area
                GenerateTileFirst(i, j);
            }
        }
    }



    public void ShiftTiles()
    {
        // Iterate over each row of the tile grid
        for (int x = 0; x < tileRowSize; x++)
        {
            int counter = 0; // Counter for tracking empty spaces

            // Iterate over each column of the tile grid
            for (int y = 0; y < tileColumnSize; y++)
            {
                GamePiece gamePiece = tiles[x, y].GetComponent<GamePiece>();

                // If the current tile doesn't have a tile, increase the empty space counter
                if (!gamePiece.hasTile)
                {
                    counter++;
                }
                // If the current tile has a tile and there are empty spaces before it
                else if (gamePiece.hasTile && counter > 0)
                {
                    if (y - counter < 0) continue; // Skip if the tile would be shifted beyond the grid

                    // Pull the tile from the current position
                    var pulledTile = tiles[x, y].PullTile();

                    // Place the pulled tile in the position with the shifted index
                    tiles[x, y - counter].PlaceTile(pulledTile);

                    // Update the coordinates of the pulled tile
                    pulledTile.SetCoordinates(x, y - counter);

                    // Animate the movement of the pulled tile to the new position
                    pulledTile.transform.DOMove(tiles[x, y - counter].transform.position, 0.3F).OnComplete(() =>
                    {
                        // Apply a punch scale effect to the pulled tile
                        pulledTile.transform.DOPunchScale(punchVector, 0.1f, 1, 1);
                    });
                }
            }
        }
    }
    public void FindNullTiles()
    {
        // Iterate over each game piece in the tiles array
        foreach (var gamePiece in tiles)
        {
            if (gamePiece.tile == null)
            {
                // If the game piece does not have a tile, add it to the emptyTiles list
                emptyTiles.Add(gamePiece);
            }
            else
            {
                // If the game piece has a tile, add it to the fullTiles list
                fullTiles.Add(gamePiece);
            }
        }
    }

    public void FillNullTiles(List<GamePiece> emptyPieces)
    {
        if (!isMatchFound)
        {
            // If no match is found, handle the deadlock situation
            DeadLockSaver();
        }

        // Iterate over each game piece in the emptyPieces list
        foreach (var gamePiece in emptyPieces)
        {
            // Generate a new tile for the empty game piece at its x and y indices
            GenerateTile(gamePiece.xIndex, gamePiece.yIndex);
        }

        // Clear the emptyTiles and fullTiles lists
        emptyTiles.Clear();
        fullTiles.Clear();
    }
    public void GenerateTile(int xCoordinate, int yCoordinate)
    {
        // Instantiate a new tile object using the tilePrefab
        var tempObject = Instantiate(tilePrefab, tiles[xCoordinate, yCoordinate].transform);

        // Set the initial scale of the tile to zero
        tempObject.transform.localScale = Vector3.zero;

        // Animate the scaling of the tile to its target scale
        tempObject.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.3f);

        // Place the newly generated tile in the tile grid at the specified coordinates
        tiles[xCoordinate, yCoordinate].PlaceTile(tempObject.GetComponent<TileController>());

        // Set the value of the tile to a power of 2 within the range of 1 to 6 (inclusive)
        tempObject.GetComponent<TileController>().Set((int)Mathf.Pow(2, Random.Range(1, 7)));

        // Set the coordinates of the tile within the tile grid
        tempObject.GetComponent<TileController>().SetCoordinates(xCoordinate, yCoordinate);
    }
    public void DeadLockPrevention()
    {
        isMatchFound = false;

        // Iterate over each pair of full tiles
        for (int i = 0; i < fullTiles.Count - 1; i++)
        {
            for (int j = i + 1; j < fullTiles.Count; j++)
            {
                // Check if the numbers of the tiles match and their positions are adjacent
                if (CheckMatchingTiles(fullTiles[i], fullTiles[j]))
                {
                    // If a match is found, set the isMatchFound flag to true and break out of the loop
                    isMatchFound = true;
                    break;
                }
            }
        }
    }
    public void DeadLockSaver()
    {
        foreach (var fullTile in fullTiles)
        {
            // Check if there is an empty tile below the current full tile
            if (fullTile.tile.yIndex + 1 < tileRowSize && !tiles[fullTile.xIndex, fullTile.yIndex + 1].hasTile)
            {
                // Instantiate a new tile object at the empty position
                var tempObject = Instantiate(tilePrefab, tiles[fullTile.xIndex, fullTile.yIndex + 1].transform);

                // Place the new tile in the tile grid at the empty position
                tiles[fullTile.xIndex, fullTile.yIndex + 1].PlaceTile(tempObject.GetComponent<TileController>());

                // Set the value of the new tile to match the value of the current full tile
                tempObject.GetComponent<TileController>().Set(fullTile.tile.number);

                // Set the coordinates of the new tile within the tile grid
                tempObject.GetComponent<TileController>().SetCoordinates(fullTile.xIndex, fullTile.yIndex + 1);

                // Remove the empty position from the emptyTiles list
                RemoveFromEmptyLine(fullTile.xIndex, fullTile.yIndex + 1);

                // Exit the loop after generating a new tile to break the deadlock
                break;
            }
        }

        // Reset the isMatchFound flag to false
        isMatchFound = false;
    }

    public void RemoveFromEmptyLine(int xCoordinate, int yCoordinate)
    {
        foreach (var emptyTile in emptyTiles)
        {
            // Remove the empty tile from the emptyTiles list
            if (emptyTile.tile.xIndex == xCoordinate && emptyTile.tile.yIndex == yCoordinate)
            {
                emptyTiles.Remove(emptyTile);
                break;
            }
        }
    }

    public void GenerateTileFirst(int xCoordinate, int yCoordinate)
    {
        // Instantiate a new tile object using the tilePrefab
        var tempObject = Instantiate(tilePrefab, tiles[xCoordinate, yCoordinate].transform);

        // Place the newly generated tile in the tile grid at the specified coordinates
        tiles[xCoordinate, yCoordinate].PlaceTile(tempObject.GetComponent<TileController>());

        // Set the value of the tile to a power of 2 within the range of 1 to 6 (inclusive)
        tempObject.GetComponent<TileController>().Set((int)Mathf.Pow(2, Random.Range(1, 7)));

        // Set the coordinates of the tile within the tile grid
        tempObject.GetComponent<TileController>().SetCoordinates(xCoordinate, yCoordinate);
    }

    private bool CheckMatchingTiles(GamePiece tileInfo1, GamePiece tileInfo2)
    {
        return tileInfo1.tile.number == tileInfo2.tile.number &&
            Mathf.Abs(tileInfo1.tile.xIndex - tileInfo2.tile.xIndex) <= 1 &&
            Mathf.Abs(tileInfo1.tile.yIndex - tileInfo2.tile.yIndex) <= 1;
    }
}
