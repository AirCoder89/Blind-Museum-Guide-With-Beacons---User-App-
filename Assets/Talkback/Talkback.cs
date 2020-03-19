using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UPersian.Components;

public class Talkback : MonoBehaviour,IPointerEnterHandler,IPointerDownHandler,IPointerExitHandler
{
   public bool ignoreOnPress;
   public bool ignoreOnEnter;
   public bool singleLanguage;
   public RtlText textSrc;
   public string singleLanguagePrefix = "Ar";
   
   public string targetText;
   private string _languagePrefix;

   public void OnPointerEnter(PointerEventData eventData)
   {
      if(ignoreOnEnter) return;
      Speak();
   }

   public void OnPointerDown(PointerEventData eventData)
   {
     if(ignoreOnPress) return;
      Speak();
   }

   public void OnPointerExit(PointerEventData eventData)
   {
      Stop();
   }
   private void Start()
   {
      _languagePrefix = "Ar";
      if (textSrc != null) this.targetText = textSrc.BaseText;
      UpdateLanguage(SystemLanguage.Arabic);
      if(!singleLanguage) MLController.OnLanguageChanged += UpdateLanguage;
   }

   private void UpdateLanguage(SystemLanguage language)
   {
      switch (language)
      {
         case SystemLanguage.Arabic:
            _languagePrefix = "Ar";
            break;
         case SystemLanguage.English:
            _languagePrefix = "En";
            break;
      }
   }

   public void SetTextValue(string value)
   {
      this.targetText = value;
   }
   public void Speak()
   {
      if(string.IsNullOrEmpty(this.targetText) || this.targetText == "NULL") return;
      StartCoroutine(DownloadAudio());
   }

   public void Stop()
   {
      AudioManager.Instance.StopTalkBack();
   }
   private IEnumerator DownloadAudio()
   {
      var prefix = singleLanguage ? singleLanguagePrefix : _languagePrefix;
      string url =
         "https://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&textlen=32&client=tw-ob&q=" + targetText + "&tl=" + prefix + "-gb";
      WWW www = new WWW(url);
      yield return www;
      var audioClip = www.GetAudioClip(false, true, AudioType.MPEG);
      AudioManager.Instance.PlayTalkBack(audioClip);
   }

  
}
