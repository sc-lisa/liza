using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlideShowController : MonoBehaviour
{
  [SerializeField]
  private SlideshowManager manager;

  [SerializeField]
  private Button startStopButton;
  [SerializeField]
  private Button showButton;
  public Slideshow slideshow = null;

  private Coroutine showCoro = null;
  void Start()
  {
    startStopButton.onClick.AddListener(pressStartStop);
    updateTextOnButton();

    showButton.onClick.AddListener(pressShow);
  }

  void updateTextOnButton()
  {
    startStopButton.GetComponentInChildren<Text>().text = manager.isRecording ? "Stop slideshow" : "Start slideshow";
  }

  void pressStartStop()
  {
    if (manager.isRecording)
    {
      slideshow = manager.StopSlideshow();
    }
    else
    {
      manager.StartSlideshow();
    }
    updateTextOnButton();
  }

  void pressShow()
  {
    if (showCoro != null)
      StopCoroutine(showCoro);
    if (manager.isRecording)
    {
      slideshow = manager.StopSlideshow();
      updateTextOnButton();
    }
    showCoro = StartCoroutine(manager.ShowSlideshow(slideshow));
  }
}
