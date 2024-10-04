using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colors : MonoBehaviour
{
    #region Singleton
    private static Colors _Instance;
    public static Colors Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindFirstObjectByType<Colors>();
            return _Instance;
        }
    }
    #endregion

    [Header("Colors used by players")]
    [SerializeField] private List<ColorPair> allColors = new List<ColorPair>();

    public ColorPair GetColorById(string _colorId)
    {
        ColorPair cp = allColors.Find(x => x.colorId == _colorId);
        return cp;
    }

}
/// <summary>
/// Color pairs, only the colorId is saved in PlayerPrefs
/// </summary>
[System.Serializable]
public class ColorPair
{
    public string colorId;
    public Color color;
}
