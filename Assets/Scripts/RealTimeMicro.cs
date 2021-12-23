using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RealTimeMicro : MonoBehaviour
{
  const int FREQ = 44100;
  const double DELTA_T = 1d / FREQ;
  const int CLIP_DURATION = 1;
  const int N = FREQ * CLIP_DURATION;
  private AudioClip mic;
  private int lastPos, pos;
  private double[] values, times;

  void Start()
  {
    if (Microphone.devices.Length == 0)
    {
      Debug.LogError("Not found microphone");
      return;
    }
    values = new double[N];
    times = new double[N];
    mic = Microphone.Start(null, true, CLIP_DURATION, FREQ);
  }

  void Update()
  {
    if (mic == null) return;
    if ((pos = Microphone.GetPosition(null)) > 0)
    {
      if (lastPos > pos) lastPos = 0;
      if (pos - lastPos > 0)
      {
        int len = (pos - lastPos) * mic.channels;
        float[] samples = new float[len];
        mic.GetData(samples, lastPos);
        for (int i = 0; i < len; ++i)
        {
          values[lastPos + i] = samples[i];
          times[lastPos + i] = times[i == 0 ? N - 1 : i - 1] + DELTA_T;
        }
        lastPos = pos;
      }
    }

    // DEBUG CODE
    double min = values[0], max = values[0], mean = 0;
    foreach (var val in values)
    {
      if (val < min) min = val;
      if (val > max) max = val;
      mean += val;
    }
    mean /= (double)N;
    Debug.LogFormat("min = {0}, max = {1}, mean = {2}", min, max, mean);
  }
}
