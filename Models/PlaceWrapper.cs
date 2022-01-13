using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Others.Models
{
  public class PlaceWrapper : IWrapper<Place>
  {
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; private set; }

    [JsonProperty("point")]
    public Point Point { get; set; }

    [JsonProperty("additionalProperties")]
    public Dictionary<string, object> AdditionalProperties { get; set; } = null;

    [JsonProperty("width")]
    public int Width { get; set; }

    [JsonProperty("height")]
    public int Height { get; set; }

    [JsonProperty("xOriginPercentage")]
    public int XOriginPercentage { get; set; }

    [JsonProperty("yOriginPercentage")]
    public int YOriginPercentage { get; set; }

    [JsonIgnore]
    public bool IsRemoved { get; set; } = false;

    [JsonIgnore]
    public Place Place { get; set; }

    public PlaceWrapper()
    {

    }

    public void LoadFromData(Place place)
    {
      Place = place;
      Name = Place.Name;
      Width = Place.Width > 0 ? Place.Width : 1;
      Height = Place.Height > 0 ? Place.Height : 1;
      XOriginPercentage = Place.XOriginPercentage;
      YOriginPercentage = Place.YOriginPercentage;
      AdditionalProperties = Place.AdditionalProperties;
    }

    public void LoadFromSave(Place place)
    {
      Place = place;
    }
  }
}
