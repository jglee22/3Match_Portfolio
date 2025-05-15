using DG.Tweening;
using UnityEngine;

public class SpecialBlockEffect : MonoBehaviour
{
    void Start()
    {
        // 밝기 반복 반짝임
        GetComponent<SpriteRenderer>().DOFade(0.5f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
}
