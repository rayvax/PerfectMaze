using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] private LineRenderer _outerWallsPrefab;     //a line for outer top and right walls
    [Space]
    [SerializeField] private Cell _cellPrefab;
    [Space]
    [SerializeField] private Vector2Int _startCellCoordinates;   //coordinates(from left-bottom corner) which the algorithm starts from
    [Space]
    [SerializeField] private SpriteRenderer _currentCellMarker;     //marker to show how maze is built
    [SerializeField] private SpriteRenderer _previousCellMarker;    //marker to show how maze is built
    [Space]
    [SerializeField] private Slider _widthSlider;
    [SerializeField] private Slider _heightSlider;
    [SerializeField] private Slider _stepTimeSlider;

    private Vector2Int _mazeSize;     //width and height of the maze

    private GameObject _currentMaze;
    private Vector3 _leftBottomCorner;
    private MarkedCell[,] _cells;

    private Tween _currentMove;
    private Tween _previousMove;
    private float _stepTime;          //time to move to next step

    private enum Direction
    {
        Left,
        Up,
        Right,
        Down
    }

    private void Awake()
    {
        _leftBottomCorner = transform.position + new Vector3(-_mazeSize.x / 2, -_mazeSize.y / 2, 0);
        _cells = new MarkedCell[_mazeSize.x, _mazeSize.y];
    }

    private void OnValidate()
    {
        if (_startCellCoordinates.x < 0)
            _startCellCoordinates.x = 0;

        if (_startCellCoordinates.y < 0)
            _startCellCoordinates.y = 0;

        if (_startCellCoordinates.x > _mazeSize.x)
            _startCellCoordinates.x = _mazeSize.x;

        if (_startCellCoordinates.y > _mazeSize.y)
            _startCellCoordinates.y = _mazeSize.y;
    }

    //draws a line, which is an outter wall on the up and on the right
    private void InitializeOuterWalls()
    {
        Vector3[] cornerPositions =
        {
            _leftBottomCorner + new Vector3(0,              _mazeSize.y,    0),
            _leftBottomCorner + new Vector3(_mazeSize.x,    _mazeSize.y,    0),
            _leftBottomCorner + new Vector3(_mazeSize.x,    0,              0)
        };

        var outerWalls = Instantiate(_outerWallsPrefab, transform.position, Quaternion.identity, _currentMaze.transform);
        outerWalls.positionCount = cornerPositions.Length;
        outerWalls.SetPositions(cornerPositions);
    }

    //places all the needed cells
    private void InitializeCells()
    {
        _cells = new MarkedCell[_mazeSize.x, _mazeSize.y];
        Vector3 cellPosition;
        for (int i = 0; i < _mazeSize.x; i++)
        {
            for (int j = 0; j < _mazeSize.y; j++)
            {
                cellPosition = _leftBottomCorner + new Vector3(i, j);
                var newCell = Instantiate(_cellPrefab, cellPosition, Quaternion.identity, _currentMaze.transform);
                _cells[i, j] = new MarkedCell(newCell, false);
            }
        }
    }

    private void GenerateMaze()
    {
        ReplaceMarkers(_startCellCoordinates, _startCellCoordinates);
        _cells[_startCellCoordinates.x, _startCellCoordinates.y].IsMarked = true;
        StartCoroutine(DigMazeCoroutine(_startCellCoordinates, _startCellCoordinates));
    }

    public void RegenerateMaze()
    {
        StopPreviousGeneration();

        _mazeSize = new Vector2Int((int)_widthSlider.value, (int)_heightSlider.value);
        _stepTime = _stepTimeSlider.value;

        if (_currentMaze != null)
            Destroy(_currentMaze);

        _currentMaze = new GameObject("Maze");

        _leftBottomCorner = transform.position + new Vector3(-_mazeSize.x / 2, -_mazeSize.y / 2, 0);
        InitializeOuterWalls();
        InitializeCells();
        GenerateMaze();
    }

    //If there was a maze before, stops it's algorithm
    private void StopPreviousGeneration()
    {
        StopAllCoroutines();

        if (_currentMove != null && _currentMove.IsPlaying())
            _currentMove.Kill();

        if (_previousMove != null && _previousMove.IsPlaying())
            _previousMove.Kill();
    }

    //recursive method which makes the maze perfect
    //The Algorithm: we get the position of cell
    //we break a wall in random direction of it
    //the wall doesn't break, if the cell on its side was already marked
    //the algorithm stops on the dead end
    private IEnumerator DigMazeCoroutine(Vector2Int currentCellCoord, Vector2Int previousCellCoord)
    {
        var randDirections = GetRandomDirectionsCombination();
        Vector2Int nextCellCoord;
        MarkedCell nextCell;

        for (int i = 0; i < randDirections.Length; i++)
        {
            nextCellCoord = currentCellCoord + GetDirectionVector(randDirections[i]);
            if (!IsCellCoordinateCorrect(nextCellCoord))
                continue;

            nextCell = _cells[nextCellCoord.x, nextCellCoord.y];
            if (nextCellCoord != previousCellCoord && !nextCell.IsMarked)
            {
                BreakWall(currentCellCoord, randDirections[i]);
                MoveMarker(currentCellCoord, nextCellCoord);
                yield return new WaitForSeconds(_stepTime);

                nextCell.IsMarked = true;
                yield return StartCoroutine(DigMazeCoroutine(nextCellCoord, currentCellCoord));
                ReplaceMarkers(currentCellCoord, previousCellCoord);
            }
        }
    }

    //moves markers without animation
    private void ReplaceMarkers(Vector2Int currentCellCoord, Vector2Int previousCellCoord)
    {
        _currentCellMarker.transform.position = _leftBottomCorner + (Vector3)(Vector2)currentCellCoord + new Vector3(0.5f, 0.5f, 0);
        _previousCellMarker.transform.position = _leftBottomCorner + (Vector3)(Vector2)previousCellCoord + new Vector3(0.5f, 0.5f, 0);
    }

    //moves markers using DOTween
    private void MoveMarker(Vector2Int currentCellCoord, Vector2Int nextCellCoord)
    {
        var currentPosition = _leftBottomCorner + (Vector3)(Vector2)nextCellCoord + new Vector3(0.5f, 0.5f, 0);
        _currentMove = _currentCellMarker.transform.DOMove(currentPosition, _stepTime);

        var previousPosition = _leftBottomCorner + (Vector3)(Vector2)currentCellCoord + new Vector3(0.5f, 0.5f, 0);
        _previousMove = _previousCellMarker.transform.DOMove(previousPosition, _stepTime);
    }

    //Disables a wall which is on the *direction* side from cellCoordinates
    private void BreakWall(Vector2Int cellCoordinate, Direction direction)
    {
        Cell cellToBreak;
        switch(direction)
        {
            case Direction.Up:
                cellToBreak = _cells[cellCoordinate.x, cellCoordinate.y + 1].Cell;
                cellToBreak.SetBottomWallActive(false);
                break;
            case Direction.Down:
                cellToBreak = _cells[cellCoordinate.x, cellCoordinate.y].Cell;
                cellToBreak.SetBottomWallActive(false);
                break;
            case Direction.Left:
                cellToBreak = _cells[cellCoordinate.x, cellCoordinate.y].Cell;
                cellToBreak.SetLeftWallActive(false);
                break;
            case Direction.Right:
                cellToBreak = _cells[cellCoordinate.x + 1, cellCoordinate.y].Cell;
                cellToBreak.SetLeftWallActive(false);
                break;
        }
    }

    //returns an array with random combination of the directions
    //(all 4 different directions are used)
    private Direction[] GetRandomDirectionsCombination()
    {
        Direction[] result = new Direction[4];

        var allDirections = GetAllDirections();

        int directionsCount = allDirections.Count;
        int randomIndex;
        for (int i = 0; i < directionsCount; i++)
        {
            randomIndex = Random.Range(0, allDirections.Count);
            result[i] = allDirections[randomIndex];
            allDirections.RemoveAt(randomIndex);
        }

        return result;
    }

    //returns the list with all the possible directions in it
    private List<Direction> GetAllDirections()
    {
        List<Direction> result = new List<Direction>()
        {
            Direction.Left,
            Direction.Right,
            Direction.Up,
            Direction.Down
        };

        return result;
    }

    //Converts Direction to Vector2Int equivalent
    private Vector2Int GetDirectionVector(Direction direction)
    {
        switch(direction)
        {
            case Direction.Up:
                return new Vector2Int(0, 1);
            case Direction.Down:
                return new Vector2Int(0, -1);
            case Direction.Left:
                return new Vector2Int(-1, 0);
            case Direction.Right:
                return new Vector2Int(1, 0);
        }

        return default;
    }

    //return if the coordinates in the maze
    private bool IsCellCoordinateCorrect(Vector2Int coordinate)
    {
        return coordinate.x < _mazeSize.x &&
               coordinate.y < _mazeSize.y &&
               coordinate.x >= 0 &&
               coordinate.y >= 0;
    }
}

//a cell with the flag
public class MarkedCell
{
    public Cell Cell;
    public bool IsMarked; //did the algorithm use the cell

    public MarkedCell(Cell cell, bool isMarked)
    {
        Cell = cell;
        IsMarked = isMarked;
    }
}
