using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Slideshow
{
  public System.DateTime timestamp;
  public List<Texture2D> images;

  public int Length
  {
    get
    {
      return images.Count;
    }
  }

  public byte[] GetBytes()
  {
    var bytes = new List<byte>();
    foreach (var image in images)
    {
      var im = image.EncodeToPNG();
      bytes.AddRange(BitConverter.GetBytes(im.Length));
      bytes.AddRange(BitConverter.GetBytes(image.width));
      bytes.AddRange(BitConverter.GetBytes(image.height));
      bytes.AddRange(im);
    }
    return bytes.ToArray();
  }

  public static Slideshow FromBytes(byte[] bytes)
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
      Array.Copy(bytes, i, im, 0, len);
      var tex = new Texture2D(w, h);
      tex.LoadImage(im);
      tex.Apply();
      ss.images.Add(tex);
    }
    return ss;
  }
}
