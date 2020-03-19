using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UPersian.Components;

public enum PanelName
{
    StartPage,BlindSelectMode,NonBlindSelectMode,BlindAutoMode,NonBlindAutoMode,BlindLocalMode,NonBlindLocalMode
}
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public bool isLocal;
    public Sprite beaconSprite;
    [SerializeField] private Sprite map;

    [SerializeField] private BeaconsManager beaconsManagerLocal;
    [SerializeField] private BeaconsManager beaconsManagerAuto;
    
    [SerializeField] private BlindAutoMode blindAutoMode;
    [SerializeField] private BlindLocalMode blindLocalMode;
    [SerializeField] private NonBlindAutoMode nonBlindAutoMode;
    [Header("panel list")]
    [SerializeField] private Talkback languageTalkback;
    [SerializeField] private Button languageBtn;
    [SerializeField] private List<UIPanel> allPanels;
    private UIPanel _currentPanel;
    public List<BeaconData> allBeaconsData;
    private SystemLanguage _currentLanguage;

    private void Awake()
    {
        if(Instance != null) return;
        Instance = this;
        allBeaconsData = new List<BeaconData>();
    }

    public void Initialize()
    {
        print("UI init");
        _currentPanel = null;
        foreach (var panel in allPanels)
        {
            panel.Initialize();
        }

        OpenPanel(PanelName.StartPage);
        _currentLanguage = SystemLanguage.Arabic;
        languageBtn.onClick.AddListener(ChangeLanguage);
        blindAutoMode.Initialize(this.allBeaconsData);
        blindLocalMode.Initialize(this.allBeaconsData);
        nonBlindAutoMode.Initialize(this.allBeaconsData);
      
    }

    public void UpdateMap(Sprite sprite)
    {
        print("Update map");
        beaconsManagerLocal.SetMap(map);
        beaconsManagerAuto.SetMap(map);
    }
    
     public void CloseApplication()
    {
        Application.Quit();
    }
    
    private void ChangeLanguage()
    {
        if (_currentLanguage == SystemLanguage.Arabic)
        {
            //change to english
            _currentLanguage = SystemLanguage.English;
            MLController.Instance.SetLanguage(_currentLanguage);
            languageBtn.GetComponentInChildren<RtlText>().text = "English";
            languageTalkback.SetTextValue("English");
        }
        else
        {
            //change to arabic
            _currentLanguage = SystemLanguage.Arabic;
            MLController.Instance.SetLanguage(_currentLanguage);
            languageBtn.GetComponentInChildren<RtlText>().text = "العربيه";
            languageTalkback.SetTextValue("العربية");
        }
    }

    public void OpenStartPage(){OpenPanel(PanelName.StartPage);}
    public void OpenBlindSelectMode(){OpenPanel(PanelName.BlindSelectMode);}
    public void OpenNonBlindSelectMode(){OpenPanel(PanelName.NonBlindSelectMode);}

    public void OpenBlindLocalMode()
    {
        blindLocalMode.OnOpen();
        OpenPanel(PanelName.BlindLocalMode);
    }

    public void OpenBlindAutoMode()
    {
        blindAutoMode.OnOpen();
        OpenPanel(PanelName.BlindAutoMode);
    }

    public void OpenNonBlindAutoMode()
    {
        nonBlindAutoMode.OnOpen();
        OpenPanel(PanelName.NonBlindAutoMode);
    }
    public void OpenNonBlindLocalMode(){OpenPanel(PanelName.NonBlindLocalMode);}
  
    private void OpenPanel(PanelName pName)
    {
        try
        {
            var panel = allPanels.First(p => p.pName == pName);
            if(panel == null) return;
            if (_currentPanel == null)
            {
                _currentPanel = panel;
                _currentPanel.Open(null);
            }
            else
            {
                if(_currentPanel.pName == PanelName.BlindLocalMode) blindLocalMode.OnClose();
                else if(_currentPanel.pName == PanelName.BlindAutoMode) blindAutoMode.OnClose();
                else if(_currentPanel.pName == PanelName.NonBlindAutoMode) nonBlindAutoMode.OnClose();
                _currentPanel.Close(() =>
                {
                    _currentPanel = panel;
                    _currentPanel.Open(null);
                });
            }
        }
        catch
        {
           print("there is no panel with the name :" + pName.ToString());
        }
    }

}
