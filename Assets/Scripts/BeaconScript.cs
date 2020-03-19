using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public enum BeaconMode
{
    Local,Auto
}
public class BeaconScript : MonoBehaviour
{
    [SerializeField] private Color selectedColor;
    [SerializeField]  private Button button;
    [SerializeField]  private RectTransform rt;
    [SerializeField] private Image img;
    [SerializeField] private float speed;
    [SerializeField] private Vector2 scale;
    [SerializeField] private Ease ease;
    public BeaconMode mode;
    public AudioClip clipAudio;
    public Sprite sprite;
    private Sequence _mySequence;
    public BeaconData data;
    private BeaconsManager _parent;

    private void Start()
    {
        _mySequence = DOTween.Sequence();
        _mySequence.Append(rt.DOScale(scale,speed).SetEase(ease))
            .Append(img.DOFade(0,speed).SetEase(ease)).SetLoops(-1);
    }

    public void Initialize(BeaconData d,AudioClip clip,Sprite sp)
    {
        this.clipAudio = clip;
        this.sprite = sp;
        this.data = d;
        button.onClick.AddListener(OnClickBeacon);
        GetComponent<RectTransform>().anchoredPosition = this.data.GetPosition();
    }

    public void SetParent(BeaconsManager parent)
    {
        this._parent = parent;
    }

    public void SetMode(BeaconMode m)
    {
        this.mode = m;
    }
    private void OnClickBeacon()
    {
        if (mode == BeaconMode.Local)
        {
            this._parent.OnClickLocalBeacon(this.data);
        }
        else if (mode == BeaconMode.Auto)
        {
            print("Auto beacon !");
        }
    }

    public void Select()
    {
        img.color = this.selectedColor;
        GetComponent<Image>().color = this.selectedColor;
    }

    public void Unselect()
    {
        img.color = Color.white;
        GetComponent<Image>().color = Color.white;
    }
}

[System.Serializable]
public class BeaconData
{
    public string beaconId;// id on the realtime Database
    public string beaconName;
    public string title;
    public string uuid;
    public string picturePath;// firebase storage destination
    public string soundPath; // firebase storage destination
    public float xPos;
    public float yPos;
    public bool isAvailable;
    public AudioClip audioClip;
    public Sprite sprite;
    public bool onPause;

    public bool Equal(string beaconUuid)
    {
        return string.Equals(this.uuid, beaconUuid, StringComparison.CurrentCultureIgnoreCase);
    }
    public void SetPosition(Vector2 pos)
    {
        this.xPos = pos.x;
        this.yPos = pos.y;
    }

    public Vector2 GetPosition()
    {
        return new Vector2(this.xPos,this.yPos);
    }
    
    public BeaconData()
    {
        
    }
    public BeaconData(string id,string name,string title,string uid,string imgPath,string sound,float x,float y)
    {
        this.beaconId = id;
        this.beaconName = name;
        this.uuid = uid;
        this.title = title;
        this.soundPath = sound;
        this.picturePath = imgPath;
        this.xPos = x;
        this.yPos = y;
        this.isAvailable = true;
    }

    public BeaconData(BeaconData data)
    {
        this.beaconId = data.beaconId;
        this.beaconName = data.beaconName;
        this.uuid = data.uuid;
        this.title = data.title;
        this.soundPath = data.soundPath;
        this.picturePath = data.picturePath;
        this.isAvailable = data.isAvailable;
        this.xPos = data.xPos;
        this.yPos = data.yPos;
    }
}