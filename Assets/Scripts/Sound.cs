using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour
{
    public AudioClip sphereASound;
    public AudioClip sphereBSound;
    public AudioClip sphereCSound;
    private AudioClip[] sphereSounds = new AudioClip[3];
    public AudioClip finishedSound;

    public AudioSource audioSource;
    
    private float lastPlayedAtTime;
    private float timeFactor = 10f;
    private float minTimeDistanceSound = 0.2f;

    void Start()
    {
        if (sphereASound == null || sphereBSound == null || sphereCSound == null)
            print($"Error in Sound.Start(), a sphere reference was read as null.");
        sphereSounds[0] = sphereASound;
        sphereSounds[1] = sphereBSound;
        sphereSounds[2] = sphereCSound;

        lastPlayedAtTime = Time.time;
    }

    public void playDistanceSound(int index, float distance, bool debug)
    {
        // initial load without distance sounds
        //if (Time.time <= 1)
        //    return;
        
        // sound frequency based on real-time and current distance 
        if (Time.time - lastPlayedAtTime >= distance * timeFactor
            && Time.time - lastPlayedAtTime >= minTimeDistanceSound)
        {
            if (debug)
                print($"Sound playDistanceSound() chose to play aid-sound for distance.\n Time.time - lastPlayedAtTime = {Time.time - lastPlayedAtTime}\tdistance * timeFactor = {distance * timeFactor}");
            
            audioSource.PlayOneShot(sphereSounds[index]);
            //audioSource.PlayOneShot(finishedSound);
            lastPlayedAtTime = Time.time;
        }
        
    }

    public void playFinishedSound(int index)
    {
        audioSource.PlayOneShot(finishedSound);
        lastPlayedAtTime = Time.time;
    }
}
