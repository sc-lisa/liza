using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlideshowManager : MonoBehaviour
{
  public const int FPS = 12;
  public const float DELTA_T = 1f / FPS;
  const int CYCLE_DURATION = 2;
  const int CYCLE_SIZE = FPS * CYCLE_DURATION + 2;

  [SerializeField]
  private RawImage _viewport;
  [SerializeField]
  private AspectRatioFitter fitter;
  private WebCamTexture cam;
  private bool available = false;

  private Texture2D[] cycle;
  private int cyclePos;

  private Slideshow slideshow = null;

  public RawImage viewport
  {
    get
    {
      return _viewport;
    }
  }


  public bool IsAvailable()
  {
    return available;
  }

  private bool _updateViewport = true;
  public bool updateViewport
  {
    get
    {
      return _updateViewport;
    }
    set
    {
      if (_updateViewport == value) return;
      _updateViewport = value;
      if (!value)
      {
        _viewport.texture = null;
      }
      else
      {
        ShowTexture(cycle[cyclePos]);
      }
    }
  }

  void setCamera(WebCamDevice device)
  {
    var rect = _viewport.rectTransform.rect;
    if (device.isFrontFacing)
      cam = new WebCamTexture(device.name, 600, 300);
    else
      cam = new WebCamTexture(device.name, (int)rect.width, (int)rect.height);
    available = true;
    var scaleY = cam.videoVerticallyMirrored ? -1f : 1f;
    _viewport.rectTransform.localScale = new Vector3(1f, scaleY, 1f);
    var orient = -cam.videoRotationAngle;
    _viewport.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
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

  public IEnumerator ShowSlideshow(Slideshow slideshow)
  {
    var prevUpdateViewport = _updateViewport;
    var prevTexture = viewport.texture;
    updateViewport = false;
    foreach (var tex in slideshow.images)
    {
      ShowTexture(tex);
      yield return new WaitForSecondsRealtime(DELTA_T);
    }
    yield return new WaitForSecondsRealtime(5);
    viewport.texture = prevTexture;
    if (prevUpdateViewport)
      updateViewport = true;
  }

  public void StartSlideshow()
  {
    var pos = cyclePos;
    var ss = new Slideshow();
    var tex = new Texture2D(cam.width, cam.height);
    Graphics.CopyTexture(cycle[pos == 0 ? CYCLE_SIZE - 1 : pos - 1], tex);
    tex.Apply();
    ss.images.Add(tex);
    tex = new Texture2D(cam.width, cam.height);
    Graphics.CopyTexture(cycle[pos], tex);
    tex.Apply();
    ss.images.Add(tex);
    slideshow = ss;
  }

  public Slideshow StopSlideshow()
  {
    var res = slideshow;
    slideshow = null;
    return res;
  }

  public void ShowTexture(Texture2D tex)
  {
    _viewport.texture = tex;
    fitter.aspectRatio = (float)tex.width / (float)tex.height;
  }

  public IEnumerator CaptureCycle()
  {
    while (available)
    {
      var currTime = Time.time;
      yield return new WaitForEndOfFrame();
      cycle[cyclePos].SetPixels(cam.GetPixels());
      cycle[cyclePos].Apply();
      if (_updateViewport)
        ShowTexture(cycle[cyclePos]);
      if (slideshow != null)
      {
        var tex = new Texture2D(cam.width, cam.height);
        Graphics.CopyTexture(cycle[cyclePos], tex);
        tex.Apply();
        slideshow.images.Add(tex);
      }
      cyclePos++;
      if (cyclePos == CYCLE_SIZE) cyclePos = 0;
      var sleepTime = Time.time - currTime + DELTA_T;
      if (sleepTime > 0)
        yield return new WaitForSecondsRealtime(sleepTime);
    }
  }

  public bool isRecording
  {
    get
    {
      return slideshow != null;
    }
  }
}
