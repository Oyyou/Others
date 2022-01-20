using System;
using System.Collections.Generic;
using System.Text;

namespace Others.Models
{
  public class GatherableResources
  {
    public class ResourceInfo
    {
      public int Count;

      public float SpawnAmount;
    }

    public float Timer = 0f;

    public Dictionary<string, ResourceInfo> Values = new Dictionary<string, ResourceInfo>();
  }
}
