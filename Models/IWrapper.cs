using System;
using System.Collections.Generic;
using System.Text;

namespace Others.Models
{
  public interface IWrapper<T>
  {
    /// <summary>
    /// When the object is initially created it has to load the {data}.json
    /// </summary>
    /// <param name="t"></param>
    void LoadFromData(T t);

    /// <summary>
    /// When the object is loaded from a save file, it needs to load the {data}.json
    /// </summary>
    /// <param name="t"></param>
    void LoadFromSave(T t);
  }
}