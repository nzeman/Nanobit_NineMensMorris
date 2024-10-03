using DG.Tweening;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public BoardPosition boardPosition;

    [Header("Sprite Renderers")]
    public SpriteRenderer mainSprite;
    public SpriteRenderer selectedSprite;
    public SpriteRenderer mildOutline;
    public SpriteRenderer deleteSprite;

    private bool isScalingUp = false;  

    public void OutlinePiece(bool on)
    {
        if (boardPosition == null) return;
        if (on)
        {
            selectedSprite.enabled = true;
        }
        else
        {
            selectedSprite.enabled = false; 
        }
    }

    public void ScaleUp(bool scale)
    {
        if (boardPosition == null) return;

        if (scale && !isScalingUp)  
        {
            isScalingUp = true;
            transform.DOScale(1.25f, 1f).SetLoops(-1, LoopType.Yoyo)
                .SetId(transform.GetInstanceID())
                .SetEase(Ease.InOutExpo)
                .OnKill(() => isScalingUp = false);
        }
        else if (!scale)
        {
            ResetVisual();  
        }
    }

    public void ResetVisual()
    {
        if (boardPosition == null) return;
        transform.localScale = Vector3.one;  
        deleteSprite.gameObject.SetActive(false);
        selectedSprite.enabled = false;
        DOTween.Kill(transform.GetInstanceID(), true); 
    }


    internal void Color(ColorPair colorPair)
    {
        mainSprite.color = colorPair.color;
    }
}
