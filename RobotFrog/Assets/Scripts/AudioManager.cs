using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioClip[] sounds;
    public AudioSource source;

    public void PlaySound(int soundIndex)
    {
        source.PlayOneShot(sounds[soundIndex]);
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