using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;

public class BlinBeaconItem : MonoBehaviour
{
    private BeaconData _data;
    [SerializeField] private RtlText titleTxt;
    [SerializeField] private Talkback talkback;
    public void Initialize(BeaconData data)
    {
        this._data = data;
        this.titleTxt.text = _data.title;
        talkback.SetTextValue(_data.title);
        GetComponent<Button>().onClick.AddListener(OnClickPlay);
    }

    private void OnClickPlay()
    {
        AudioManager.Instance.Play(this._data.audioClip);
    }
}
