using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class RealTimeMicro : MonoBehaviour
{
  const int FREQ = 44100;
  const double DELTA_T = 1d / FREQ;
  const int CLIP_DURATION = 1;
  const int N = FREQ * CLIP_DURATION;

  private AudioSource source;

  void Start()
  {
    if (Microphone.devices.Length == 0)
    {
      Debug.LogError("Not found microphone");
      return;
    }
    source = GetComponent<AudioSource>();
    StartCoroutine(MicroLoop());
  }

  IEnumerator MicroLoop()
  {
    while (true)
    {
      AudioClip clip = Microphone.Start(null, false, CLIP_DURATION, FREQ);
      float currentTime = Time.time;
      if (source.clip != null)
        ProcessClip(source.clip);
      float sleepTime = Time.time - currentTime + CLIP_DURATION;
      if (sleepTime > 0)
        yield return new WaitForSecondsRealtime(sleepTime);
      source.clip = clip;
      source.Play();
    }
  }

  void ProcessClip(AudioClip clip)
  {
    print(clip.length);
  }
}
