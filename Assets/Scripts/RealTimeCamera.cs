using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RealTimeCamera : MonoBehaviour
{
  //   private Texture defaultViewport;
  private bool available = false;
  private WebCamTexture cTexture;

  public RawImage viewport;
  public AspectRatioFitter fitter;
  // Start is called before the first frame update

  void setDevice(WebCamDevice device)
  {
    var rect = viewport.rectTransform.rect;
    cTexture = new WebCamTexture(device.name, (int)rect.width, (int)rect.height);
    available = true;
    viewport.texture = cTexture;
    cTexture.Play();
  }

  void Start()
  {
    // defaultViewport = viewport.texture;
    foreach (var device in WebCamTexture.devices)
    {
      if (!device.isFrontFacing)
      {
        setDevice(device);
        break;
      }
    }
    if (!available)
    {
      if (WebCamTexture.devices.Length == 0)
      {
        Debug.LogError("No back camera found");
      }
      else
      {
        // setDevice(WebCamTexture.devices[WebCamTexture.devices.Length - 1]);
        setDevice(WebCamTexture.devices[0]);
      }
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (!available)
    {
      return;
    }

    var ratio = (float)cTexture.width / (float)cTexture.height;
    fitter.aspectRatio = ratio;

    var scaleY = cTexture.videoVerticallyMirrored ? -1f : 1f;
    viewport.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

    var orient = -cTexture.videoRotationAngle;
    viewport.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
  }
}