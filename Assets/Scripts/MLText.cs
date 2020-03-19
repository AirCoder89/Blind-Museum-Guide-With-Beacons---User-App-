using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UPersian.Components;

[System.Serializable]
public class MLValue
{
    public SystemLanguage language = SystemLanguage.Arabic;
    [TextArea] public string textValue;
}
public class MLText : MonoBehaviour
{
    [SerializeField] private Talkback talkBack;
    [SerializeField] private List<MLValue> languageSource;
    private RtlText _target;
    
    void Start()
    {
        _target = GetComponent<RtlText>();
        MLController.OnLanguageChanged += UpdateLanguage;
    }
    private void UpdateLanguage(SystemLanguage language)
    {
        if(_target == null) return;
        try
        {
            var txt = languageSource.First(l => l.language == language).textValue;
            _target.text = txt;
            UpdateTalkBack(txt);
        }
        catch
        {
            UpdateTalkBack("NULL");
            _target.text = "Language Not Found";
        }
    }

    private void UpdateTalkBack(string text)
    {
        if(talkBack == null) return;
        talkBack.SetTextValue(text);
    }
    private void OnDisable()
    {
        UpdateLanguage(MLController.Instance.currentLanguage);
    }

    private void OnEnable()
    {
        try
        {
            UpdateLanguage(MLController.Instance.currentLanguage);
        }
        catch
        {
          //hh
        }
    }
}
