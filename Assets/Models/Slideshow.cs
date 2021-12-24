using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;

public class Slideshow
{
  public System.DateTime timestamp;
  public List<Texture2D> images;

  public Slideshow()
  {
    timestamp = DateTime.Now;
    images = new List<Texture2D>();
  }

  public int Length
  {
    get
    {
      return images.Count;
    }
  }
  public async Task<string> Save(string dir)
  {
    var fname = dir + "/" + timestamp.ToString("o");
    File.Create(fname).Dispose();
    var file = File.Open(fname, FileMode.Create, FileAccess.Write, FileShare.None);
    foreach (var image in images)
    {
      var im = image.EncodeToPNG();
      await file.WriteAsync(BitConverter.GetBytes(im.Length), 0, 4);
      await file.WriteAsync(BitConverter.GetBytes(image.width), 0, 4);
      await file.WriteAsync(BitConverter.GetBytes(image.height), 0, 4);
      await file.WriteAsync(im, 0, im.Length);
    }
    return fname;
  }

  static Slideshow FromBytes(byte[] bytes)
  {
    var ss = new Slideshow();
    int i = 0;
    while (i < bytes.Length)
    {
      int len = BitConverter.ToInt32(bytes, i);
      int w = BitConverter.ToInt32(bytes, i + 4);
      int h = BitConverter.ToInt32(bytes, i + 8);
      i += 12;
      var im = new byte[len];
      for (int j = 0; j < len;)
      {
        im[j] = bytes[i];
        i++;
        j++;
      }
      var tex = new Texture2D(w, h);
      tex.LoadImage(im);
      tex.Apply();
      ss.images.Add(tex);
    }
    return ss;
  }

  public static Slideshow Load(string fname)
  {
    var bytes = File.ReadAllBytes(fname);
    var res = Slideshow.FromBytes(bytes);
    res.timestamp = DateTime.Parse(Path.GetFileName(fname), null, System.Globalization.DateTimeStyles.RoundtripKind);
    return res;
  }
}
