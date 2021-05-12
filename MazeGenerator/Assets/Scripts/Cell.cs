using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private LineRenderer _leftWall;
    [SerializeField] private LineRenderer _bottomWall;

    public void SetLeftWallActive(bool value)
    {
        _leftWall.gameObject.SetActive(value);
    }

    public void SetBottomWallActive(bool value)
    {
        _bottomWall.gameObject.SetActive(value);
    }
}
