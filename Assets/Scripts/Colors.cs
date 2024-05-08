
using System.Collections.Generic;
using UnityEngine;

public static class Colors
{
    public static Dictionary<int, Color> colorsDict = new Dictionary<int, Color>();
   

    static Colors()
    {
        colorsDict.Add(2, new Color(0.91f, 0.54f, 0.34f));
        colorsDict.Add(4, new Color(0.85f, 0.39f, 0.25f));
        colorsDict.Add(8, new Color(0.95f, 0.74f, 0.23f));
        colorsDict.Add(16, new Color(0.84f, 0.62f, 0.08f));
        colorsDict.Add(32, new Color(0.54f, 0.76f, 0.39f));
        colorsDict.Add(64, new Color(0.55f, 0.84f, 0.76f));
        colorsDict.Add(128, new Color(0.41f, 0.74f, 0.91f));
        colorsDict.Add(256, new Color(0.42f, 0.57f, 0.86f));
        colorsDict.Add(512, new Color(0.83f, 0.56f, 0.81f));
        colorsDict.Add(1024, new Color(0.6f, 0.42f, 0.80f));
        colorsDict.Add(2048, new Color(0.91f, 0.40f, 0.56f));

    }
}
