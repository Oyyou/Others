using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Others.Models
{
  public class Task
  {
    public class ProducedItem
    {
      [JsonProperty("name")]
      public string Name { get; set; }
      [JsonProperty("chance")]
      public float Chance { get; set; }
    }

    public class SkillRequirement
    {
      [JsonProperty("name")]
      public string Name { get; set; }

      [JsonProperty("level")]
      public int Level { get; set; }
    }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("rate")]
    public float Rate { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("producedItems")]
    public List<ProducedItem> ProducedItems { get; set; }

    [JsonProperty("skillRequirements")]
    public List<SkillRequirement> SkillRequirements { get; set; }
  }
}
