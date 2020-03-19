using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.Events;

public class StorageHandler2 : MonoBehaviour
{
    public delegate void StorageEvents();
    public delegate void StorageEvents2();
    public static event StorageEvents OnUploadComplete;
    public static event StorageEvents2 OnDownloadComplete;
    public static event StorageEvents2 OnDeleteComplete;
    
    private FirebaseStorage storage;
    private StorageReference storage_ref;
    private string rootStorageUrl = "gs://adminappalecso.appspot.com";

    private string local_file;
    private StorageReference file_ref;
    private string _meta;

    void Start()
    {
        storage = FirebaseStorage.DefaultInstance;
        storage_ref = storage.GetReferenceFromUrl(rootStorageUrl);
    }

    public void UploadFile(string from, string to,string meta)
    {
        local_file = from;
        file_ref = storage_ref.Child(to);
        _meta = meta;
        Upload();
    }

    private void Upload()
    {
        var metadataChange = new Firebase.Storage.MetadataChange();
        var customMetadata = new Dictionary<string, string>();
        
        customMetadata["folder"] = "uploadFolder";
        metadataChange.CustomMetadata = customMetadata;
        metadataChange.ContentType = _meta;

        file_ref.PutFileAsync(local_file, metadataChange)
            .ContinueWith (task => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("Error upload");
                    //Debug.Log("folder:::" + task.Result.GetCustomMetadata("folder"));
                    Debug.Log(task.Exception.ToString());
                } else {
                    var download_url = file_ref.GetDownloadUrlAsync();
                    Debug.Log("Finished uploading");
                    Debug.Log("download url = " + download_url);
                    OnUploadComplete?.Invoke();
                }
            });
    }
    
    public void DownloadFile(string from, string to)
    {
        local_file = to;
        print("Download To : " + local_file);
        file_ref = storage_ref.Child(from);
        // Download to the local filesystem
        file_ref.GetFileAsync(local_file).ContinueWith(task => {
            if (!task.IsFaulted && !task.IsCanceled) {
                Debug.Log("File downloaded.");
                
                OnDownloadComplete?.Invoke();
            }
        });
    }

    
    public void DeleteFile(string from)
    {
        file_ref = storage_ref.Child(from);
        // Delete the file
        file_ref.DeleteAsync().ContinueWith(task => {
            if (task.IsCompleted) {
                Debug.Log("File deleted successfully.");
                OnDeleteComplete?.Invoke();
            } else {
                // Uh-oh, an error occurred!
                Debug.Log("an error occurred");
            }
        });
    }
    
    public static void RemoveEventListener()
    {
        OnUploadComplete = null;
        OnDownloadComplete = null;
        OnDeleteComplete = null;
    }
    
}
