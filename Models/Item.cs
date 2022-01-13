using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Others.Models
{
  public class Item
  {
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("weight")]
    public float Weight { get; set; }
  }

  public class Items
  {
    [JsonProperty("items")]
    public List<Item> ListOfItems { get; set; } = new List<Item>();
  }
}
