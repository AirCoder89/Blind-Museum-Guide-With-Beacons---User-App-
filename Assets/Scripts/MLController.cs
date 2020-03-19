using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;

public class MLController : MonoBehaviour
{
    public static MLController Instance;
    
    public delegate void LanguageEvent(SystemLanguage language);
    public static event LanguageEvent OnLanguageChanged;
    public  SystemLanguage currentLanguage;

    private void Awake()
    {
        if(Instance != null) return;
        Instance = this;
    }

    void Start()
    {
        SetLanguage(SystemLanguage.Arabic);
    }

    public void SetLanguage(SystemLanguage language)
    {
        currentLanguage = language;
        OnLanguageChanged?.Invoke(language);
    }
}
