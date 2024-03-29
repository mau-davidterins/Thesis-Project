﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public static class Serialization
{

  public static T DeserializeXMLFileToObject<T>(string XmlFilename)
  {
    T returnObject = default(T);
    if (string.IsNullOrEmpty(XmlFilename)) return default(T);

    try
    {
      StreamReader xmlStream = new StreamReader(XmlFilename);
      XmlSerializer serializer = new XmlSerializer(typeof(T));
      returnObject = (T)serializer.Deserialize(xmlStream);
    }
    catch (Exception ex)
    {
      Debug.LogWarning(ex);
    }
    return returnObject;
  }

}
