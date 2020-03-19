using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Net.Http;
using Proyecto26;

public class DBManager : MonoBehaviour
{
    public delegate void DBEvents(List<BeaconData> result);
    public static event DBEvents OnLoadComplete;

    [SerializeField] private GameObject errorPopUp;
    [SerializeField] private Sprite sp1;
    [SerializeField] private Sprite sp2;
    private string _currentMap;
    [System.Serializable]
    private class BeaconId
    {
        public string name;
    }

    private Action<List<BeaconData>> callback;
    public void LoadBeacons()
    {
        errorPopUp.SetActive(false);
        var request = new RequestHelper
        {
            Timeout = 15,
            Retries = 3,
            EnableDebug = true,
            RetrySecondsDelay = 5,
            ContentType = "application/json",
            Method = "GET",
            Uri = "https://adminappalecso.firebaseio.com/Beacons.json",
        };
       
        RestClient.Request(request).Then(response =>
            {
                OnGetBeaconsComplete(response.Text);
            })
            .Catch(err =>
            {
                print("error on post beacon");
                
            }).Finally(() =>
            {
            });
        
        /*
        //get map
        var request = new RequestHelper
        {
            Timeout = 15,
            Retries = 3,
            EnableDebug = true,
            RetrySecondsDelay = 3,
            ContentType = "application/json",
            Method = "GET",
            Uri = "https://adminappalecso.firebaseio.com/Map.json",
        };
        
        RestClient.Request(request).Then(response =>
            {
                OnLoadMapComplete(response.Text);
            })
            .Catch(err =>
            {
                print("error on get map");
                errorPopUp.SetActive(true);
                
            }).Finally(() =>
            {
            });*/
    }
 public void LoadBeacons(Action<List<BeaconData>> cb)
 {
     callback = cb;
        errorPopUp.SetActive(false);
        var request = new RequestHelper
        {
            Timeout = 15,
            Retries = 3,
            EnableDebug = true,
            RetrySecondsDelay = 5,
            ContentType = "application/json",
            Method = "GET",
            Uri = "https://adminappalecso.firebaseio.com/Beacons.json",
        };
       
        RestClient.Request(request).Then(response =>
            {
                OnGetBeaconsComplete2(response.Text);
            })
            .Catch(err =>
            {
                print("error on post beacon");
                
            }).Finally(() =>
            {
            });
        
        /*
        //get map
        var request = new RequestHelper
        {
            Timeout = 15,
            Retries = 3,
            EnableDebug = true,
            RetrySecondsDelay = 3,
            ContentType = "application/json",
            Method = "GET",
            Uri = "https://adminappalecso.firebaseio.com/Map.json",
        };
        
        RestClient.Request(request).Then(response =>
            {
                OnLoadMapComplete(response.Text);
            })
            .Catch(err =>
            {
                print("error on get map");
                errorPopUp.SetActive(true);
                
            }).Finally(() =>
            {
            });*/
    }

 private void OnGetBeaconsComplete2(string jsonResult)
 {
     if (jsonResult == "null")
     {
         return;
     }
     print("result > " + jsonResult);
     var str = @jsonResult;
     var pList = JsonConvert.DeserializeObject<Dictionary<string, BeaconData>>(str);
     var allBeacons = new List<BeaconData>();
	    
     foreach (var p in pList)
     {
         if(!p.Value.isAvailable) continue;
         print("firebaseId : " + p.Key);
         p.Value.beaconId = p.Key; //set firebase id to our BeaconData
         allBeacons.Add(p.Value);
     }
        
     callback?.Invoke(allBeacons);
 }

 private void OnLoadMapComplete(string result)
    {
       LoadingScript.MapPath = JsonUtility.FromJson<Map>(result).currentMap;
        
        var request = new RequestHelper
        {
            Timeout = 15,
            Retries = 3,
            EnableDebug = true,
            RetrySecondsDelay = 5,
            ContentType = "application/json",
            Method = "GET",
            Uri = "https://adminappalecso.firebaseio.com/Beacons.json",
        };
        
        RestClient.Request(request).Then(response =>
            {
                OnGetBeaconsComplete(response.Text);
            })
            .Catch(err =>
            {
                errorPopUp.SetActive(true);
                print("error on post beacon");
                
            }).Finally(() =>
            {
            });
    }

    public void ByPass()
    {
        var b1 = new BeaconData()
        {
            audioClip = AudioManager.Instance.b1,
            sprite = sp1,
            title = "فينوس",
            uuid = "903ae853-a7ba-4a89-a52b-7bd70a7060d9",
            xPos = -485.48f,
            yPos = 339.23f
        };
        var b2 = new BeaconData()
        {
            audioClip = AudioManager.Instance.b2,
            sprite = sp2,
            title = "تانيت",
            uuid = "342faaa8-3467-47a9-8b1a-ba83631ff2ee",
            xPos = 412.78f,
            yPos = 443.07f
        };
        var allBeacons = new List<BeaconData>();
        allBeacons.Add(b1);
        allBeacons.Add(b2);
    }
    
    private void OnGetBeaconsComplete(string jsonResult)
    {
        if (jsonResult == "null")
        {
            return;
        }
        print("result > " + jsonResult);
        var str = @jsonResult;
        var pList = JsonConvert.DeserializeObject<Dictionary<string, BeaconData>>(str);
        var allBeacons = new List<BeaconData>();
	    
        foreach (var p in pList)
        {
            if(!p.Value.isAvailable) continue;
                print("firebaseId : " + p.Key);
                p.Value.beaconId = p.Key; //set firebase id to our BeaconData
                allBeacons.Add(p.Value);
        }
        
        OnLoadComplete?.Invoke(allBeacons);
        OnLoadComplete?.Invoke(allBeacons);
    }

}

[System.Serializable]
public class Map
{
    public string currentMap;
}