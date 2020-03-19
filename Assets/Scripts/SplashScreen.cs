using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] private Image logo1;
    [SerializeField] private Image logo2;
    [SerializeField] private float logoShowTime;
    [SerializeField] private float logoFadeSpeed;
    
    private void Start()
    {
        this.logo1.DOFade(0, 0);
        this.logo2.DOFade(0, 0);
        logo1.DOFade(1, logoFadeSpeed).OnComplete(() =>
        {
            logo1.transform.parent.gameObject.GetComponent<RectTransform>().DOScale(new Vector2(1.3f, 1.3f), logoFadeSpeed).SetDelay(logoShowTime);
            logo1.DOFade(0, logoFadeSpeed).SetDelay(logoShowTime).OnComplete(() =>
            {
                logo2.DOFade(1, logoFadeSpeed).OnComplete(() =>
                    {
                        logo2.transform.parent.gameObject.GetComponent<RectTransform>().DOScale(new Vector2(1.3f, 1.3f), logoFadeSpeed).SetDelay(logoShowTime);
                        logo2.DOFade(0, logoFadeSpeed).SetDelay(logoShowTime).OnComplete(() =>
                            {
                                SplashScreenComplete();
                            });
                    });
            });
        });
    }

    private void SplashScreenComplete()
    {
        gameObject.transform.SetAsFirstSibling();
        gameObject.SetActive(false);
    }
}
