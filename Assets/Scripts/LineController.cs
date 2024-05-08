using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI.Extensions;

public  class LineController : MonoBehaviour 
{
    public UILineRenderer lineRenderer;
    #region SingletonMethod

    private static LineController instance;

    public static LineController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LineController>();

                if (instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    instance = singletonObject.AddComponent<LineController>();
                    singletonObject.name = "LineControllerSingleton";
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

    private void Start()
    {
       lineRenderer = GetComponent<UILineRenderer>();
        ClearPoints();
    }
    public  void AddPoint(float xValue,float yValue)
    {
        var point = new Vector2() { x = xValue, y = yValue };
        var pointlist = new List<Vector2>(lineRenderer.Points)
        {
            point
        };
        lineRenderer.Points = pointlist.ToArray();
    }
    public void ClearPoints()
    {
        lineRenderer.Points = null;
    }
    public void ChangeFirstDot(float xValue, float yValue)
    {
        lineRenderer.Points[0].y = yValue;
        lineRenderer.Points[0].x = xValue;
    }
    public void RemoveLastLine()
    {
        lineRenderer.Points = lineRenderer.Points[..^1];
    }
    public void SetColorForLine(int tileNumber)
    {
        Color tileColor = Colors.colorsDict[tileNumber];
        lineRenderer.color = tileColor;
    }
}

