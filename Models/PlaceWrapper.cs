using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Others.Models
{
  public class PlaceWrapper : IWrapper<Place>
  {
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; private set; }

    [JsonProperty("point")]
    public Point Point { get; set; }

    [JsonProperty("additionalProperties")]
    public Dictionary<string, object> AdditionalProperties { get; set; } = null;

    [JsonIgnore]
    public bool IsRemoved { get; set; } = false;

    [JsonIgnore]
    public Place Data { get; set; }

    public PlaceWrapper()
    {

    }

    public void LoadFromData(Place place)
    {
      Data = place;
      Name = Data.Name;
      AdditionalProperties = Data.AdditionalProperties;
    }

    public void LoadFromSave(Place place)
    {
      Data = place;
    }
  }
}
