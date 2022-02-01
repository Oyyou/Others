using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Others.Models
{
  public class GameWorld
  {
    [JsonIgnore]
    public Dictionary<string, Attribute> AttributeData { get; set; } = new Dictionary<string, Attribute>();

    [JsonIgnore]
    public Dictionary<string, Task> TaskData { get; set; } = new Dictionary<string, Task>();
    
    [JsonIgnore]
    public Dictionary<string, Place> PlaceData { get; set; } = new Dictionary<string, Place>();
    
    [JsonIgnore]
    public Dictionary<string, Item> ItemData { get; set; } = new Dictionary<string, Item>();

    [JsonProperty("ids")]
    public Dictionary<string, int> Ids { get; set; } = new Dictionary<string, int>();

    [JsonProperty("villagers")]
    public List<Villager> Villagers { get; set; } = new List<Villager>();

    [JsonProperty("tasks")]
    public List<TaskWrapper> Tasks { get; set; } = new List<TaskWrapper>();

    [JsonProperty("places")]
    public List<PlaceWrapper> Places { get; set; } = new List<PlaceWrapper>();

    [JsonProperty("households")]
    public List<Household> Households { get; set; } = new List<Household>();

    public GatherableResources GatherableResources { get; set; } = new GatherableResources();

    //[JsonProperty("attributes")]
    //public List<AttributeWrapper> Attributes { get; set; } = new List<AttributeWrapper>();
  }
}
