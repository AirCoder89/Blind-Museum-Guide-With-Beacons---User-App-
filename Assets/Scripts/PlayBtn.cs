using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayBtn : MonoBehaviour
{
    [SerializeField] private Sprite playSp;
    [SerializeField] private Sprite stopSp;

    private bool _isStopped = true;

    public void Init()
    {
        GetComponent<Image>().sprite = stopSp;
    }
    
    public void Click()
    {
        GetComponent<Image>().sprite = _isStopped ? stopSp : playSp;
        _isStopped = !_isStopped;
    }
}
