using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class LoadingScript : MonoBehaviour
{
    public static string MapPath;
    [SerializeField] private Text logTxt;
    [SerializeField] private Text loadingText;
    [SerializeField] private GameObject nextLoading;
    [SerializeField] private StorageHandler2 storage;
    [SerializeField] private DBManager dbManager;
    [SerializeField] private List<BeaconData> loadedBeacons;
    [SerializeField] private BeaconsManager localMode;
    [SerializeField] private BeaconsManager autoMode;

    private int _index;
    private string _localSoundPath;
    private string _localPicturePath;
    private BeaconData _tmpBeacon;
    private List<BeaconData> _tmpLoadedBeacons;
    private bool _isStorageLoadingComplete;
    private AudioClip _tmpAudioClip;
    private Sprite _tmpSprite;
    private string _locaMapPath;

    private bool loadLocal;
        
    
    private void Update()
    {
        if (loadLocal)
        {
            StartLocalLoading();
            loadLocal = false;
        }
       /* if(_tmpLoadedBeacons == null || _isStorageLoadingComplete) return;
        if (_tmpLoadedBeacons.Count == 0 && !_isStorageLoadingComplete)
        {
            _tmpLoadedBeacons = null;
            _isStorageLoadingComplete = true;
            print("Start load local");
            StartLocalLoading();
        }*/
    }
    
    private void Start()
    {
        Log("Start Loading");
        loadLocal = false;
        _localSoundPath = CreateNewDirectory("Sounds"); print(_localSoundPath);
        _localPicturePath = CreateNewDirectory("Pictures"); print(_localPicturePath);
        _locaMapPath = CreateNewDirectory("Map"); print(_locaMapPath);
        DBManager.OnLoadComplete += OnLoadBeaconsComplete;
        dbManager.LoadBeacons();
        localMode.Initialize();
        autoMode.Initialize();
    }

    public void Reload()
    {
        dbManager.LoadBeacons(OnLoadBeaconsComplete);
    }
    public bool IsDirectoryExists(string directoryName)
    {
        return Directory.Exists(Application.persistentDataPath + "/" + directoryName);
    }
    
    public string CreateNewDirectory(string directoryName)
    {
        if(!IsDirectoryExists(directoryName))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/" + directoryName);
            return Application.persistentDataPath + "/" + directoryName;
        }
        else
        {
            return Application.persistentDataPath + "/" + directoryName;
        }
    }

    private void OnLoadBeaconsComplete(List<BeaconData> beacons)
    {
        DBManager.OnLoadComplete -= OnLoadBeaconsComplete;
        print("load beacons from DB Complete! > " + beacons.Count);
        
        if (UIManager.Instance.isLocal)
        {
            print("local");
            localMode.Initialize();
            autoMode.Initialize();
            UIManager.Instance.allBeaconsData = new List<BeaconData>();
            foreach (var beacon in beacons)
            {
                var b = beacon;
                var aClip = AudioManager.Instance.GetAudioById(b.beaconId);
                var sp = UIManager.Instance.beaconSprite;
                b.audioClip = aClip;
                b.sprite = sp;
                localMode.GenerateBeacon(b,aClip,sp);
                autoMode.GenerateBeacon(b,aClip,sp);
                UIManager.Instance.allBeaconsData.Add(b);
            }
           
            UIManager.Instance.Initialize();
            UIManager.Instance.UpdateMap(null);
            
            gameObject.SetActive(false);
        }
        else
        {
            this.loadedBeacons = beacons;
            print("hosted");
            _tmpLoadedBeacons = new List<BeaconData>(beacons);
            _index = 0;
            LoadAllBeacons();
        }
    }

    private void LoadMap()
    {
        var extension  = "." + MapPath.Split(char.Parse("."))[1];
        this._locaMapPath = this._locaMapPath + "/CurrentMap" + extension;
        print("map path: " + this._locaMapPath);
        StorageHandler2.OnDownloadComplete += OnDownloadMapComplete;
        storage.DownloadFile(MapPath,this._locaMapPath);
    }

    private void OnDownloadMapComplete()
    {
        print("Load map complete");
        LoadAllBeacons();
    }
    
    private void LoadAllBeacons()
    {
        Log("Load beacon asset(sound & picture)");
        if (_tmpLoadedBeacons.Count > 0)
        {
            _tmpBeacon = _tmpLoadedBeacons[0];
            LoadAudio();
        }
        else
        {
            loadLocal = true;
        }
    }
    
    private void LoadAudio()
    {
        Log("Load Audio");
        var extension = "." + _tmpBeacon.soundPath.Split(char.Parse("."))[1];
        var fullSoundPath = this._localSoundPath + "/" + _tmpBeacon.beaconName + extension;
        Log("sound path: " + fullSoundPath);
        StorageHandler2.OnDownloadComplete += OnDownloadAudioComplete;
        storage.DownloadFile(_tmpBeacon.soundPath,fullSoundPath);
    }

    private void OnDownloadAudioComplete()
    {
        Log("Download Audio Complete");
        StorageHandler2.OnDownloadComplete -= OnDownloadAudioComplete;
       //load picture
       Log("Load picture");
       var extension = "." + _tmpBeacon.picturePath.Split(char.Parse("."))[1];
       var fullPicPath = this._localPicturePath + "/" + _tmpBeacon.beaconName + extension;
       Log("picture path: " + fullPicPath);
       StorageHandler2.OnDownloadComplete += OnDownloadPictureComplete;
       storage.DownloadFile(_tmpBeacon.picturePath,fullPicPath);
    }

    private void OnDownloadPictureComplete()
    {
        Log("Load picture complete");
        StorageHandler2.OnDownloadComplete -= OnDownloadPictureComplete;
        _tmpLoadedBeacons.RemoveAt(0);
        LoadAllBeacons();
    }
    
    //---- Local Loading
    public void StartLocalLoading()
    {
        Log("Start Local Loading");
        _index = 0;
        LoadLocalBeacon();
    }

    private void LoadLocalBeacon()
    {
        if (_index < this.loadedBeacons.Count)
        {
            Log("Load beacon index=" + _index);
            _tmpBeacon = loadedBeacons[_index];
            ImportLocalAudio();
        }
        else
        {
            Log("Local Loading Complete");
            StartCoroutine(LoadLocalPictureMap(this._locaMapPath));
        }
    }

    private void ImportLocalAudio()
    {
        print("Import Audio");
        var extension = "." + _tmpBeacon.soundPath.Split(char.Parse("."))[1];
        var fullSoundPath = this._localSoundPath + "/" + _tmpBeacon.beaconName + extension;
        StartCoroutine(LoadLocalAudioFileCr(fullSoundPath));
    }
    
    IEnumerator LoadLocalAudioFileCr(string path)
    {
        WWW www = new WWW("file://" + path);
        print("loading local audio : " + path);
 
        var clip = www.GetAudioClip(false);
        while(!clip.isReadyToPlay)
            yield return www;
 
        Log("audio done loading");
        clip.name = Path.GetFileName(path);
        _tmpAudioClip = clip;
        ImportLocalPicture();
        //clips.Add(clip);
    }
    
    private void ImportLocalPicture()
    {
        Log("import picture");
        var extension = "." + _tmpBeacon.picturePath.Split(char.Parse("."))[1];
        var fullPicPath = this._localPicturePath + "/" + _tmpBeacon.beaconName + extension;
        StartCoroutine(LoadLocalPictureFileCr(fullPicPath));
        //LoadSprite(fullPicPath);
    }
    
    private IEnumerator LoadLocalPictureFileCr(string path)
    {
        var loader = new WWW("file://" + path);
        //var loader = new WWW(path); print("Loading image in Progress");
        yield return loader;

        if (loader.texture == null)
        {
            Log(">> texture is null");
        }
        else
        {
            Log("Import picture Success");
        }
        Texture2D texture = new Texture2D(1, 1);
        loader.LoadImageIntoTexture(texture);
        _tmpSprite = Sprite.Create(texture,
            new Rect(0, 0, texture.width, texture.height),
            Vector2.one / 2); //set pivot to center

        _tmpBeacon.sprite = _tmpSprite;
        _tmpBeacon.audioClip = _tmpAudioClip;
        Log("Generate Beacon into holder");
        localMode.GenerateBeacon(_tmpBeacon,_tmpAudioClip,_tmpSprite);
        autoMode.GenerateBeacon(_tmpBeacon,_tmpAudioClip,_tmpSprite);
        UIManager.Instance.allBeaconsData.Add(_tmpBeacon);
        _index++;
        LoadLocalBeacon();
    }
    
    IEnumerator LoadLocalPictureMap (string path) {
        print(path);
        WWW www = new WWW("file://" + path);
        while(!www.isDone)
            yield return null;
       
        Texture2D texture = www.texture;
        var mapSprite = Sprite.Create(texture,
            new Rect(0, 0, texture.width, texture.height),
            Vector2.one / 2); //set pivot to center
        
        UIManager.Instance.UpdateMap(mapSprite);
        #if UNITY_EDITOR
                print("enable bluetooth !");
        #elif UNITY_ANDROID
                                    Log("Enable Bluetooth");
                                    BluetoothState.Init();
                                    BluetoothState.EnableBluetooth();
        #endif
        UIManager.Instance.Initialize();
        gameObject.SetActive(false);
    }
    
    private IEnumerator LoadLocalPictureMapd(string path)
    {
        var loader = new WWW("file://" + path);
        //var loader = new WWW(path); print("Loading image in Progress");
        yield return loader;

        if (loader.texture == null)
        {
            print(">> texture is null");
        }
        else
        {
            print("Import picture Success");
        }
        Texture2D texture = new Texture2D(1, 1);
        loader.LoadImageIntoTexture(texture);
        var mapSprite = Sprite.Create(texture,
            new Rect(0, 0, texture.width, texture.height),
            Vector2.one / 2); //set pivot to center

        //UIManager.Instance.UpdateMap(mapSprite);
        #if UNITY_EDITOR
                print("enable bluetooth !");
        #elif UNITY_ANDROID
                            Log("Enable Bluetooth");
                            BluetoothState.Init();
                            BluetoothState.EnableBluetooth();
        #endif
        UIManager.Instance.Initialize();
        gameObject.SetActive(false);
    }
    
    
    private void LoadSprite(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(64, 64, TextureFormat.ARGB32, false);
        texture.LoadImage(data);
        //texture.name = Path.GetFileNameWithoutExtension(path);
        _tmpSprite = Sprite.Create(texture,
            new Rect(0, 0, texture.width, texture.height),
            Vector2.one / 2); //set pivot to center

        _tmpBeacon.sprite = _tmpSprite;
        _tmpBeacon.audioClip = _tmpAudioClip;
        Log("Generate Beacon into holder");
        localMode.GenerateBeacon(_tmpBeacon,_tmpAudioClip,_tmpSprite);
        autoMode.GenerateBeacon(_tmpBeacon,_tmpAudioClip,_tmpSprite);
        UIManager.Instance.allBeaconsData.Add(_tmpBeacon);
        _index++;
        LoadLocalBeacon();
    }
    private void Log(string str)
    {
        if(logTxt ==null) return;
        logTxt.text = str + "\n" + logTxt.text;
    }
  
}
