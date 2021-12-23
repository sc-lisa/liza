using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlideshowManager : MonoBehaviour
{
  const int FPS = 12;
  const float DELTA_T = 1f / FPS;
  const int CYCLE_DURATION = 2;
  const int CYCLE_SIZE = FPS * CYCLE_DURATION;

  [SerializeField]
  private RawImage viewport;
  [SerializeField]
  private AspectRatioFitter fitter;
  private WebCamTexture cam;

  private Texture2D[] cycle;
  private int cyclePos;

  private bool available = false;

  public bool IsAvailable()
  {
    return available;
  }

  void setCamera(WebCamDevice device)
  {
    var rect = viewport.rectTransform.rect;
    cam = new WebCamTexture(device.name, (int)rect.width, (int)rect.height);
    available = true;
    cam.Play();
  }

  void Start()
  {
    foreach (var device in WebCamTexture.devices)
    {
      if (!device.isFrontFacing)
      {
        setCamera(device);
        break;
      }
    }
    if (!available)
    {
      if (WebCamTexture.devices.Length == 0)
      {
        Debug.LogError("No back camera found");
        return;
      }
      else
      {
        setCamera(WebCamTexture.devices[0]);
      }
    }
    cycle = new Texture2D[CYCLE_SIZE];
    for (int i = 0; i < CYCLE_SIZE; ++i)
      cycle[i] = new Texture2D(cam.width, cam.height);
    StartCoroutine(CaptureCycle());
  }

  public void ShowSlideshow() { }

  public void TakeSlideshow() { }

  public IEnumerator CaptureCycle()
  {
    while (available)
    {
      var currTime = Time.time;
      yield return new WaitForEndOfFrame();
      cycle[cyclePos].SetPixels(cam.GetPixels());
      cycle[cyclePos].Apply();
      cyclePos++;
      if (cyclePos == CYCLE_SIZE) cyclePos = 0;
      var sleepTime = Time.time - currTime + DELTA_T;
      if (sleepTime > 0)
        yield return new WaitForSecondsRealtime(sleepTime);
    }
  }
}
