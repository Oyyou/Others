using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Others.Models
{
  public class AdditionalProperty : ICloneable
  {
    //[JsonProperty("name")]
    //public string Name { get; set; }

    [JsonProperty("value")]
    public dynamic Value { get; set; }

    [JsonProperty("isVisible")]
    public bool IsVisible { get; set; }

    public object Clone()
    {
      return this.MemberwiseClone();
    }
  }
}
