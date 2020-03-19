using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlindLocalMode : MonoBehaviour
{
   [SerializeField] private GameObject blindBeaconPrefab;
   [SerializeField] private Transform beaconsHolder;

   private List<BlinBeaconItem> _beaconItems;
   
   public void Initialize(List<BeaconData> list)
   {_beaconItems = new List<BlinBeaconItem>();
      foreach (var beacon in list)
      {
         var bObj = Instantiate(blindBeaconPrefab, beaconsHolder);
         bObj.transform.localScale = Vector3.one;
         var sBeacon = bObj.GetComponent<BlinBeaconItem>();
         sBeacon.Initialize(beacon);
         _beaconItems.Add(sBeacon);
      }      
      LayoutRebuilder.ForceRebuildLayoutImmediate(beaconsHolder.gameObject.GetComponent<RectTransform>());
   }

   public void OnOpen()
   {
      
   }
   public void OnClose()
   {
      AudioManager.Instance.Stop();
   }
}
