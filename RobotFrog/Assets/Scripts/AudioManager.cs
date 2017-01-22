using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioClip[] sounds;
    private AudioSource musicSource;
    private List<AudioSource> sources = new List<AudioSource>();

    public void PlaySound(int soundIndex)
    {
        AudioSource source = null;
        for (int i = 0; i < this.sources.Count; i++)
        {
            if (!this.sources[i].isPlaying)
            {
                source = this.sources[i];
                break;
            }
        }

        if (source == null)
        {
            Debug.LogError(string.Format("No audio sources available for sound: {0}", soundIndex));
        }
        else
        {
            source.clip = sounds[soundIndex];
            source.Play();
        }
    }

    public void SetMusic(int soundIndex)
    {
        if(musicSource.clip != sounds[soundIndex])
        {
            musicSource.Stop();
            musicSource.clip = sounds[soundIndex];
            musicSource.Play();
        }
        
    }

    // Use this for initialization
    void Start()
    {
        musicSource = this.GetComponent<AudioSource>();
        AudioManager.Instance = this;
        for (int i = 0; i < 10; i++)
        {
            var o = new GameObject();
            o.name = "AudioSource" + i;
            o.transform.SetParent(this.transform);
            var source = o.AddComponent<AudioSource>();
            this.sources.Add(source);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}