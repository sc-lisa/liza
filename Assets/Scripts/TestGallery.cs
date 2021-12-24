using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TestGallery : MonoBehaviour
{
  private string slideshowsDirectory;

  [SerializeField]
  private SlideshowManager manager;
  [SerializeField]
  private Button controlButton;
  [SerializeField]
  private Button playButton;
  [SerializeField]
  private Button nextButton;

  private List<string> slideshows;
  private int pos = 0;
  private Coroutine showCoro = null;
  private Slideshow selected = null;

  List<string> ListFiles()
  {
    var res = new List<string>();
    var info = new DirectoryInfo(slideshowsDirectory);
    var files = info.GetFiles();
    foreach (var file in files)
    {
      if (file.Name != "prefs")
        res.Add(file.FullName);
    }
    return res;
  }

  void Start()
  {
    slideshowsDirectory = Application.persistentDataPath;
    slideshows = ListFiles();
    pos = slideshows.Count;
    nextButton.onClick.AddListener(pressNext);
    playButton.onClick.AddListener(pressPlay);
    controlButton.onClick.AddListener(pressControl);
  }

  void handleSelect()
  {
    controlButton.GetComponentInChildren<Text>().text = "Delete";
    playButton.enabled = true;
    selected = Slideshow.Load(slideshows[pos]);
    manager.updateViewport = false;
    manager.ShowTexture(selected.images[0]);
  }

  void StopShow()
  {
    if (showCoro != null)
    {
      StopCoroutine(showCoro);
      showCoro = null;
    }
  }

  void pressPlay()
  {
    if (selected == null) return;
    showCoro = StartCoroutine(manager.ShowSlideshow(selected));
  }

  async void pressControl()
  {
    var text = controlButton.GetComponentInChildren<Text>().text;
    controlButton.enabled = false;
    StopShow();
    switch (text)
    {
      case "Record":
        await StartRecording();
        break;
      case "Stop":
        await StopRecording();
        break;
      default:
        var prevPlayEnabled = playButton.enabled;
        var prevNextEnabled = nextButton.enabled;
        playButton.enabled = false;
        nextButton.enabled = false;
        var fname = slideshows[pos];
        manager.updateViewport = true;
        File.Delete(fname);
        slideshows.RemoveAt(pos);
        nextButton.enabled = prevNextEnabled;
        playButton.enabled = prevPlayEnabled;
        pos--;
        pressNext();
        break;
    }
    controlButton.enabled = true;
  }

  void pressNext()
  {
    StopShow();
    pos++;
    if (pos > slideshows.Count) pos = 0;
    if (pos == slideshows.Count)
    {
      controlButton.GetComponentInChildren<Text>().text = "Record";
      playButton.enabled = false;
      manager.updateViewport = true;
      selected = null;
    }
    else
    {
      handleSelect();
    }
  }

  async Task StopRecording()
  {
    var slideshow = manager.StopSlideshow();
    var task = slideshow.Save(slideshowsDirectory);
    slideshows.Add(await task);
    controlButton.GetComponentInChildren<Text>().text = "Record";
    pos = slideshows.Count - 1;
    nextButton.enabled = true;
    handleSelect();
  }

  async Task StartRecording()
  {
    if (manager.isRecording) await StopRecording();
    StopShow();
    playButton.enabled = false;
    nextButton.enabled = false;
    manager.updateViewport = true;
    manager.StartSlideshow();
    controlButton.GetComponentInChildren<Text>().text = "Stop";
  }
}
