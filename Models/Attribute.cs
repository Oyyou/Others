using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Others.Models
{
  public class Attribute : ICloneable
  {
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("total")]
    public float Total { get; set; }

    [JsonProperty("taskType")]
    public string TaskType { get; set; }

    [JsonProperty("positiveTasks")]
    public Dictionary<string, float> PositiveTasks { get; set; } = new Dictionary<string, float>();

    [JsonProperty("negativeTasks")]
    public Dictionary<string, float> NegativeTasks { get; set; } = new Dictionary<string, float>();

    public object Clone()
    {
      var newAttr = (Attribute)this.MemberwiseClone();
      newAttr.PositiveTasks = new Dictionary<string, float>(PositiveTasks);
      newAttr.NegativeTasks = new Dictionary<string, float>(NegativeTasks);

      return newAttr;
    }
  }
}
