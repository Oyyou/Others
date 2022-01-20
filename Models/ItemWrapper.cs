using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Others.Models
{
  public class ItemWrapper : IWrapper<Item>
  {
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonIgnore]
    public Item Data { get; set; }

    /// <summary>
    /// This is the combined weight of this stack
    /// </summary>
    [JsonIgnore]
    public float StackWeight 
    { 
      get
      {
        return Data.Weight * Count;
      }
    }

    public ItemWrapper()
    {

    }

    public void LoadFromData(Item item)
    {
      Data = item;
      Name = Data.Name;
    }

    public void LoadFromSave(Item item)
    {
      Data = item;
    }
  }
}
