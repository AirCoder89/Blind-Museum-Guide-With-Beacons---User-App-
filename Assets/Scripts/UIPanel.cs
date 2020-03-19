using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UIPanel : MonoBehaviour
{
    public PanelName pName;

    [Header("Ease & Tween")] 
    [SerializeField] private float openSpeed;
    [SerializeField] private float closeSpeed;
    [SerializeField] private Ease ease;
    [SerializeField] private Vector2 scale;
    [SerializeField] private Vector2 position;
    
    private Action _tweenCallBack;
    private RectTransform _rt;
    
    public void Initialize()
    {
        _rt = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }
    
    public void Open(Action callback)
    {
        gameObject.SetActive(true);
        _tweenCallBack = callback;
        _rt.DOScale(scale, 0);
        _rt.DOScale(Vector3.one, openSpeed).SetEase(ease);
        _rt.DOAnchorPos(position, 0);
        _rt.DOAnchorPos(Vector2.zero, openSpeed).SetEase(ease).OnComplete(() =>
        {
            _tweenCallBack?.Invoke();
            _tweenCallBack = null;
        });
    }

    public void Close(Action callback)
    {
        _tweenCallBack = callback;
        _rt.DOScale(Vector3.one, 0);
        _rt.DOScale(scale, closeSpeed).SetEase(ease);
        _rt.DOAnchorPos(Vector2.zero, 0);
        _rt.DOAnchorPos(position, closeSpeed).SetEase(ease).OnComplete(() =>
        {
            gameObject.SetActive(false);
            _tweenCallBack?.Invoke();
            _tweenCallBack = null;
        });
    }

}
