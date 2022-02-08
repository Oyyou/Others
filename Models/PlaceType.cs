using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Others.Models
{
  public class PlaceType
  {
    [JsonProperty("isConstructable")]
    public bool IsConstructable { get; set; }
  }

  public class PlaceTypes
  {
    [JsonProperty("placeTypes")]
    public List<PlaceType> ListOfPlaceTypes { get; set; } = new List<PlaceType>();
  }
}
