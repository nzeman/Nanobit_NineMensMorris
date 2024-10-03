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

    public List<ColorPair> allColors = new List<ColorPair>();

   public ColorPair GetColorById(string _colorId)
    {
        ColorPair cp = allColors.Find(x => x.colorId == _colorId);
        return cp;
    }

}

[System.Serializable]
public class ColorPair
{
    public string colorId;
    public Color color;
}
