using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;

public class NonBlindPopUp : MonoBehaviour
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
    
    private string _targetUUID;
    
    public void Open(BeaconData data,Action onClose)
    {
        if(data == null) return;
        if (data.Equal(_targetUUID))
        {
            return;
        }
       
        _targetUUID = data.uuid;
        
        gameObject.SetActive(true);
        closeBtn.gameObject.SetActive(false);
        bg.enabled = true;
        titleTxt.text = data.title;
        beaconImg.sprite = data.sprite;
        
        data.onPause = false;
        AudioManager.Instance.Play(data.audioClip);
        
        playAudioBtn.onClick.AddListener(() =>
        {
            if (data.onPause)
            {
                data.onPause = false;
                AudioManager.Instance.Play(data.audioClip);
            }
            else
            {
                data.onPause = true;
                AudioManager.Instance.Stop();
            }
        });
        closeBtn.onClick.AddListener(() =>
        {
            onClose?.Invoke();
            Close();
        });
        //--
        panel.anchoredPosition = closedPos;
        panel.DOAnchorPos(openPos, speed).SetEase(ease).OnComplete(() =>
        {
            closeBtn.gameObject.SetActive(true);
        });
    }
   
    public void Close()
    {
        _targetUUID = "";
        bg.enabled = false;
        AudioManager.Instance.Stop();
        closeBtn.gameObject.SetActive(false);
        panel.DOAnchorPos(closedPos, speed).SetEase(ease).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
