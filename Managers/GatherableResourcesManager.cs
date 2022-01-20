using Microsoft.Xna.Framework;
using Others.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Others.Models.GatherableResources;

namespace Others.Managers
{
  public class GatherableResourcesManager
  {
    private GameWorldManager _gwm;

    private float _timerLimit = 0f;

    public GatherableResources GatherableResources
    {
      get
      {
        return _gwm.GameWorld.GatherableResources;
      }
    }

    public GatherableResourcesManager(GameWorldManager gwm)
    {
      _gwm = gwm;
      _timerLimit = float.Parse(GameWorldManager.Statics["gatherableResourcesTimer"]);

      UpdateValues();

    }

    public void Update(GameTime gameTime)
    {
      GatherableResources.Timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
      
      // We check what resources we have every minute
      if (GatherableResources.Timer > _timerLimit)
      {
        GatherableResources.Timer = 0f;
        UpdateValues();
      }
    }

    private void UpdateValues()
    {
      var gatherableResourcesData = _gwm.GameWorld.PlaceData.Where(c => c.Value.Type == "Gathering").ToDictionary(c => c.Key, v => v.Value);

      GatherableResources.Values = _gwm.GameWorld.Places
        .Where(c => c.Data.Type == "Gathering")
        .GroupBy(c => c.Name)
        .ToDictionary(c => c.Key, v => new ResourceInfo() { Count = v.Count(), SpawnAmount = GatherableResources.Values.ContainsKey(v.Key) ? (float)(GatherableResources.Values[v.Key].SpawnAmount + (double)gatherableResourcesData[v.Key].AdditionalProperties["spawnRate"]) : 0f, });

      foreach (var resource in GatherableResources.Values)
      {
        if (resource.Value.Count >= (long)gatherableResourcesData[resource.Key].AdditionalProperties["max"])
          continue;

        var count = (int)Math.Floor(resource.Value.SpawnAmount);
        if (count >= 1)
        {
          _gwm.AddGatherablePlace(resource.Key, count);
          resource.Value.SpawnAmount = 0f;
        }
      }
    }
  }
}
