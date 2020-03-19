using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BeaconsManager : MonoBehaviour
{
   public BeaconMode mode;
   [SerializeField] private Image map;
   [SerializeField] private GameObject beaconPrefab;
   [SerializeField] private Transform beaconHolder;

   [SerializeField] private NonBlindPopUp localPopup;
   private List<BeaconScript> _allBeacons; 
   public void Initialize()
   {
      _allBeacons = new List<BeaconScript>();
   }

   public void SetMap(Sprite mSprite)
   {
      map.sprite = mSprite;
   }
   public void GenerateBeacon(BeaconData data,AudioClip clip,Sprite sprite)
   {
      print("GenerateBeacon");
      var b = Instantiate(beaconPrefab, beaconHolder);
      b.transform.localScale = Vector3.one;
      var bScript = b.GetComponent<BeaconScript>();
      bScript.Initialize(data,clip,sprite);
      
      bScript.SetParent(this);
      bScript.SetMode(this.mode);
      _allBeacons.Add(bScript);
   }

   public void OnClickLocalBeacon(BeaconData data)
   {
      SelectBeaconByUUID(data.uuid);
      localPopup.Open(data,OnClosePopUp);
   }

   private void OnClosePopUp()
   {
      AudioManager.Instance.Stop();
   }

   public void SelectBeaconByUUID(string uuid)
   {
      foreach (var beacon in _allBeacons)
      {
         SelectTargetBeacon(beacon, uuid);
      }
   }

   private void SelectTargetBeacon(BeaconScript beacon, string uuid)
   {
      if(beacon.data.uuid == uuid) beacon.Select();
      else beacon.Unselect();
   }
}
