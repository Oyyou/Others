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
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonIgnore]
    public Item Item { get; set; }

    /// <summary>
    /// This is the combined weight of this stack
    /// </summary>
    [JsonIgnore]
    public float StackWeight 
    { 
      get
      {
        return Item.Weight * Count;
      }
    }

    public ItemWrapper()
    {

    }

    public void LoadFromData(Item item)
    {
      Item = item;
      Name = Item.Name;
    }

    public void LoadFromSave(Item item)
    {
      Item = item;
    }
  }
}
