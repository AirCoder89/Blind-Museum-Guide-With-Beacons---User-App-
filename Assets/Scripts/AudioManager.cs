using UnityEngine.Audio;
using UnityEngine;
using System;
using System.Collections;

[System.Serializable]
public class Sound{
	public string beaconId;
	public string name;
	public AudioClip clip;

	[Range(0f,1f)] public float volume;
	[Range(0f, 2f)] public float pitch;
	public bool loop;
	public bool play = true;

	[HideInInspector] public AudioSource source;
}

public enum SoundList
{
	
}

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance;
	[SerializeField] private AudioSource talkback;
	[SerializeField] private AudioSource src;
	public AudioClip b1;
	public AudioClip b2;
	public Sound[] sounds;

	public static bool lastSrcState;
	void Awake () {
		if(Instance != null) return;
		Instance = this;
		
		foreach (var s in sounds)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;
			s.source.volume = s.volume;
			s.source.pitch = s.pitch;
			s.source.loop = s.loop;
		}
	}

	public void PlayTalkBack(AudioClip clip)
	{
		talkback.Stop();
		talkback.loop = false;
		talkback.clip = clip;
		talkback.Play();
	}

	public void StopTalkBack()
	{
		talkback.Stop();
	}
	public void Play(AudioClip clip)
	{
		lastSrcState = true;
		src.Stop();
		src.clip = clip;
		src.Play();
	}

	public void Stop()
	{
		lastSrcState = false;
		src.Stop();
	}

	public AudioClip GetAudioById(string beaconId)
	{
		try
		{
			return Array.Find(sounds, sound => sound.beaconId == beaconId.ToString()).clip;
			
		}
		catch
		{
			return null;
		}
	}
	
	public void Play(string beaconId)
	{
		try
		{
			var s = Array.Find(sounds, sound => sound.beaconId == beaconId.ToString());
			if(s.play) s.source.Play();
		}
		catch
		{
			throw new Exception("Sound not Found");
		}
	}
	
	public void Play(SoundList sName)
	{
		try
		{
			var s = Array.Find(sounds, sound => sound.name == sName.ToString());
			if(s.play) s.source.Play();
		}
		catch
		{
			throw new Exception("Sound not Found");
		}
	}
	
	public void Play(SoundList sName,float delay)
	{
		StopAllCoroutines();
		StartCoroutine(SoundDelay(sName, delay));
	}

	private IEnumerator SoundDelay(SoundList sName,float delay)
	{
		yield return new WaitForSeconds(delay);
		try
		{
			var s = Array.Find(sounds, sound => sound.name == sName.ToString());
			if(s.play) s.source.Play();
		}
		catch
		{
			throw new Exception("Sound not Found");
		}
	}
	
	public void Stop(SoundList sName)
	{
		try
		{
			var s = Array.Find(sounds, sound => sound.name == sName.ToString());
			s.source.Stop();
		}
		catch
		{
			throw new Exception("Sound not Found");
		}
	}
}
