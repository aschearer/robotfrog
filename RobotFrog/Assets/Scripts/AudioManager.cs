using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] sounds;
    public AudioSource source;

    public void PlaySound(int soundIndex)
    {
        source.PlayOneShot(sounds[soundIndex]);
    }

    // Use this for initialization
    void Start()
    {
        source = gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}