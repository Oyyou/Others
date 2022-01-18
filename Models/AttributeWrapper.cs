using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Others.Models
{
  public class AttributeWrapper : IWrapper<Attribute>
  {
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("total")]
    public float Total { get; set; }

    [JsonIgnore]
    public Attribute Data { get; set; }

    public AttributeWrapper()
    {

    }

    public void LoadFromData(Attribute attribute)
    {
      Data = attribute;
      Name = Data.Name;
      Total = Data.Total;
    }

    public void LoadFromSave(Attribute attribute)
    {
      Data = attribute;
    }
  }
}
