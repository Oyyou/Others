using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Others.Models
{
  public class Place : ICloneable
  {
    public enum CollisionTypes
    {
      None,
      Full,
    }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("skill")]
    public string Skill { get; private set; }

    /// <summary>
    /// The type of place it is
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// A description of the place
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("collisionType")]
    public CollisionTypes CollisionType { get; set; }

    [JsonProperty("width")]
    public int Width { get; set; } = 1;

    [JsonProperty("height")]
    public int Height { get; set; } = 1;

    [JsonProperty("xOriginPercentage")]
    public int XOriginPercentage { get; set; }

    [JsonProperty("yOriginPercentage")]
    public int YOriginPercentage { get; set; }

    [JsonProperty("additionalProperties")]
    public Dictionary<string, AdditionalProperty> AdditionalProperties { get; set; } = new Dictionary<string, AdditionalProperty>();

    public object Clone()
    {
      var newObj = (Place)this.MemberwiseClone();
      newObj.AdditionalProperties = this.AdditionalProperties.ToDictionary(c => c.Key, v => (AdditionalProperty)v.Value.Clone());

      return newObj;
    }
  }

  public class Places
  {
    [JsonProperty("places")]
    public List<Place> ListOfPlaces { get; set; } = new List<Place>();
  }
}
