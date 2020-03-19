using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BlindAutoMode : MonoBehaviour
{
   [SerializeField] private Text logTxt;
   [SerializeField] private Button soundControlBtn;
   
   private List<BeaconData> _ourBeacons;
   private List<Beacon> _beaconsInRange;

   private BeaconData _targetBeacon;
   private BeaconData _lastTargetBeacon;
   public void Initialize(List<BeaconData> list)
   {
      _targetBeacon = null;
      _lastTargetBeacon = null;
      _ourBeacons = list;
      soundControlBtn.onClick.AddListener(SoundController);
   }
   
   private void SoundController()
   {
      PlayPauseTarget();
   }

   public void OnOpen()
   {
      print("OnOpen");
      iBeaconReceiver.BeaconRangeChangedEvent += OnBeaconRangeChanged;
      _targetBeacon = null;
      _lastTargetBeacon = null;
      #if UNITY_EDITOR
            print("start scan !");
      #elif UNITY_ANDROID
            DefineBeaconRegion();
            StartScan();
      #endif
   }
   
   public void StartScan()
   {
      Log("Start Scan");
      iBeaconReceiver.Stop();
      DefineBeaconRegion();
      iBeaconReceiver.Scan();
   }
   private void Update()
   {
      #if UNITY_ANDROID && !UNITY_EDITOR
      return;
      #endif
      if (Input.GetKeyUp(KeyCode.A))
      {
         var result = new Beacon[] {new Beacon("e7cd46b3-b65f-414d-814b-fcd2195fa930", 1, 1)};
         OnBeaconRangeChanged(result);
      }
      else  if (Input.GetKeyUp(KeyCode.Z))
      {
         var result = new Beacon[] {new Beacon("972ea272-1e53-4652-9895-2c8783f76f16", 1, 1)};
         OnBeaconRangeChanged(result);
      }
      else  if (Input.GetKeyUp(KeyCode.E))
      {
         var result = new Beacon[] {new Beacon("89082614-1b20-4794-96df-c8a23842685b", 1, 1)};
         OnBeaconRangeChanged(result);
      }
      else  if (Input.GetKeyUp(KeyCode.R))
      {
         var result = new Beacon[] {new Beacon("2d32eefa-10a4-4b69-af15-b92947d84c8d", 1, 1),new Beacon("2d32eefa-10a4-4b69-af15-b92947d84c8d", 1, 1)};
         OnBeaconRangeChanged(result);
      }
      else  if (Input.GetKeyUp(KeyCode.T))
      {
         var result = new Beacon[] {new Beacon("89082614-1b20-4794-96df-c8a23842685b", 1, 1),new Beacon("2d32eefa-10a4-4b69-af15-b92947d84c8d", 1, 1)};
         OnBeaconRangeChanged(result);
      }
      else  if (Input.GetKeyUp(KeyCode.Y))
      {
         var result = new Beacon[] {};
         OnBeaconRangeChanged(result);
      }
   }
   private void DefineBeaconRegion()
   {
      iBeaconReceiver.regions = new iBeaconRegion[]{new iBeaconRegion("Any", new Beacon())};
      return;
      var region = new iBeaconRegion[_ourBeacons.Count];
      Log("______________");
      for (var i = 0; i < _ourBeacons.Count; i++)
      {
         region[i] = new iBeaconRegion("museum",
            new Beacon(_ourBeacons[i].uuid, Convert.ToInt32(0), Convert.ToInt32(0)));
         Log("uuid range: " + _ourBeacons[i].uuid);
      }
      //iBeaconReceiver.regions = region;
      Log("______________");
   }
   private void OnBeaconRangeChanged(Beacon[] beacons)
   {
      Log("________________");
      Log("OnBeaconRangeChanged > " + beacons.Length);
      var detectedBeacons = new List<Beacon>(beacons);
       
      try
      {
         _beaconsInRange = detectedBeacons.Where(b => b.accuracy <= 1 && UUIDExists(b.UUID)).ToList();
         Log("_beaconsInRange > " + _beaconsInRange.Count);
          
         if (_beaconsInRange.Count == 1)
         {
            _targetBeacon = GetBeaconByUUID(_beaconsInRange[0].UUID);
            ExecuteBeacon(_targetBeacon);
         }
         else
         {
            //get min range beacon
            var min = _beaconsInRange.Min(b => b.accuracy);
            _targetBeacon = GetBeaconByUUID(_beaconsInRange.First(b => b.accuracy == min).UUID);
            ExecuteBeacon(_targetBeacon);
         }
      }
      catch
      {
         _targetId = "";
      }
   }

   private void OnBeaconRangeChangedOld(Beacon[] beacons)
   {
      Log("OnBeaconRangeChanged > " + beacons.Length);
      var detectedBeacons = new List<Beacon>(beacons);
      try
      {
         _beaconsInRange = detectedBeacons.Where(b => b.accuracy <= 1 && UUIDExists(b.UUID)).ToList();
         Log("_beaconsInRange > " + _beaconsInRange.Count);
         
         if (_beaconsInRange.Count == 1)
         {
            _targetBeacon = GetBeaconByUUID(_beaconsInRange[0].UUID);
            if (_targetBeacon == null)
            {
               Log($"beacon {_beaconsInRange[0].UUID} not Found");
               _lastTargetBeacon = null;
               AudioManager.Instance.Stop();
               return;
            }
            
            if(_lastTargetBeacon != _targetBeacon) _targetBeacon.onPause = false;
            ExecuteBeacon();
         }
         else
         {
            //get min range beacon
            var min = _beaconsInRange.Min(b => b.accuracy);
            _targetBeacon = GetBeaconByUUID(_beaconsInRange.First(b => b.accuracy == min).UUID);
            if (_targetBeacon == null)
            {
               Log($"beacon not Found");
               _lastTargetBeacon = null;
               AudioManager.Instance.Stop();
               return;
            }
            ExecuteBeacon();
         }
      }
      catch
      {
        _targetBeacon = null;
        AudioManager.Instance.Stop();
        print("no beacon in range or matching uuid");
        Log("no beacon in range or matching uuid");
      }
   }

   private string _targetId;
   private void ExecuteBeacon(BeaconData beacon)
   {
      if(beacon == null) return;
      if (beacon.Equal(_targetId))
      {
         return;
      }
       
      _targetId = beacon.uuid;
      AudioManager.Instance.Play(beacon.audioClip);
   }
   private void ExecuteBeacon()
   {
      if (_targetBeacon == null)
      {
         AudioManager.Instance.Stop();
         return;
      }

      if (_lastTargetBeacon == _targetBeacon)
      {
         return;
      }

      if (_targetBeacon.onPause)
      {
         return;
      }
      
      _lastTargetBeacon = _targetBeacon;
      Log("ExecuteBeacon > " + _targetBeacon.uuid);
      AudioManager.Instance.Play(_targetBeacon.audioClip);
   }
   private void PlayPauseTarget()
   {
      if (_targetBeacon == null)
      {
         AudioManager.Instance.Stop();
         return;
      }

      if (_targetBeacon.onPause)
      {
         _targetBeacon.onPause = false;
         AudioManager.Instance.Play(_targetBeacon.audioClip);
      }
      else
      {
         _targetBeacon.onPause = true;
         AudioManager.Instance.Stop();
      }
   }
   private BeaconData GetBeaconByUUID(string uuid)
   {
      try
      {
         return this._ourBeacons.First(b => b.uuid == uuid);
      }
      catch
      {
         return null;
      }
   }
   private bool UUIDExists(string uuid)
   {
      try
      {
         return _ourBeacons.Exists(b => string.Equals(b.uuid, uuid, StringComparison.CurrentCultureIgnoreCase));
      }
      catch
      {
         return false;
      }
   }
   public void OnClose()
   {
      print("OnClose");
      _targetId = "";
      iBeaconReceiver.BeaconRangeChangedEvent -= OnBeaconRangeChanged;
      AudioManager.Instance.Stop();
      #if UNITY_EDITOR
            print("stop scan !");
      #elif UNITY_ANDROID
            iBeaconReceiver.Stop();
      #endif
   }

   private void Log(string str)
   {
      if (logTxt == null)
      {
         print(str);
         return;
      }
      logTxt.text = str + "\n" + logTxt.text;
   }
}
