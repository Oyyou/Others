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
    public Attribute Attribute { get; set; }

    public AttributeWrapper()
    {

    }

    public void LoadFromData(Attribute attribute)
    {
      Attribute = attribute;
      Name = Attribute.Name;
      Total = Attribute.Total;
    }

    public void LoadFromSave(Attribute attribute)
    {
      Attribute = attribute;
    }
  }
}
