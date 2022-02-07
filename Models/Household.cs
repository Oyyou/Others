using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Others.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Others.Models
{
  public class Household
  {
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("villagerIds")]
    public List<long> VillagerIds { get; set; } = new List<long>();

    /// <summary>
    /// Surname of the family
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonIgnore]
    public List<Villager> Villagers { get; set; } = new List<Villager>();

    public void Load(GameWorldManager gwm)
    {
      Villagers = VillagerIds.Select(c => gwm.GetVillagerById(c)).ToList();
    }

    public void AssignVillager(Villager villager)
    {
      // When we load, it should take care of this! 
      if (!VillagerIds.Contains(villager.Id))
        VillagerIds.Add(villager.Id);

      if (!Villagers.Contains(villager))
        Villagers.Add(villager);

      villager.HouseholdId = this.Id;
      villager.Household = this;
    }
  }
}
