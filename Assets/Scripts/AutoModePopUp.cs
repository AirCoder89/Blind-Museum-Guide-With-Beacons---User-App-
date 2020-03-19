using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;

public class AutoModePopUp : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Ease ease;
    [SerializeField] private Vector2 openPos;
    [SerializeField] private Vector2 closedPos;
    [Header("UI")]
    [SerializeField] private Image bg;
    [SerializeField] private RectTransform panel;
    [SerializeField] private RtlText titleTxt;
    [SerializeField] private Image beaconImg;
    [SerializeField] private Button playAudioBtn;
    [SerializeField] private Button closeBtn;
    
    
    private BeaconData _target;
    private bool _isOpen;
    
    public void Open(BeaconData beacon,Action onClose)
    {
        if(beacon == null) return;
        if(beacon == _target) return;
        _target = beacon;
        
        gameObject.SetActive(true);
        closeBtn.gameObject.SetActive(false);
        bg.enabled = true;
        titleTxt.text = _target.title;
        beaconImg.sprite = _target.sprite;
        
        _target.onPause = false;
        AudioManager.Instance.Play(_target.audioClip);
        
        playAudioBtn.onClick.AddListener(() =>
        {
            if (_target.onPause)
            {
                _target.onPause = false;
                AudioManager.Instance.Play(_target.audioClip);
            }
            else
            {
                _target.onPause = true;
                AudioManager.Instance.Stop();
            }
        });
        closeBtn.onClick.AddListener(() =>
        {
            onClose?.Invoke();
            Close();
        });
        
        
    }

    public void Close()
    {
        _target = null;
    }
}
