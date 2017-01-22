using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioClip[] sounds;
    public AudioSource source;

    public void PlaySound(int soundIndex)
    {
        if(sounds.Length > soundIndex)
        {
            source.PlayOneShot(sounds[soundIndex]);
        }
    }

    public void SetMusic(int soundIndex)
    {
        if(source.clip != sounds[soundIndex])
        {
            source.Stop();
            source.clip = sounds[soundIndex];
            source.Play();
        }
        
    }

    // Use this for initialization
    void Start()
    {
        AudioManager.Instance = this;
        source = gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}