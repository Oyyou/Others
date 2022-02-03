using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

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
    public Dictionary<string, AdditionalProperty> AdditionalProperties { get; set; } = null;

    [JsonProperty("width")]
    public int Width { get; set; }

    [JsonProperty("height")]
    public int Height { get; set; }

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
      Width = Data.Width;
      Height = Data.Height;
      AdditionalProperties = Data.AdditionalProperties;

      //var rgb = place.Tint.Split('.').Select(c => int.Parse(c)).ToList();
      //Tint = place.Tint;// new Color(rgb[0], rgb[1], rgb[2]);
    }

    public void LoadFromSave(Place place)
    {
      Data = place;
    }
  }
}
