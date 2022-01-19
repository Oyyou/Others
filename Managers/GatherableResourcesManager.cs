using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Others.Managers
{
  public class GatherableResourcesManager
  {
    private class ResourceInfo
    {
      public int Count;

      public float SpawnAmount;
    }

    private GameWorldManager _gwm;

    private float _timer = 0f;

    private Dictionary<string, ResourceInfo> _gatherableResources = new Dictionary<string, ResourceInfo>();

    public GatherableResourcesManager(GameWorldManager gwm)
    {
      _gwm = gwm;
      UpdateValues();
    }

    public void Update(GameTime gameTime)
    {
      _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
      
      // We check what resources we have every minute
      if (_timer > 60f)
      {
        _timer = 0f;
        UpdateValues();
      }
    }

    private void UpdateValues()
    {
      var gatherableResourcesData = _gwm.GameWorld.PlaceData.Where(c => c.Value.Type == "Gathering").ToDictionary(c => c.Key, v => v.Value);

      _gatherableResources = _gwm.GameWorld.Places
        .Where(c => c.Data.Type == "Gathering")
        .GroupBy(c => c.Name)
        .ToDictionary(c => c.Key, v => new ResourceInfo() { Count = v.Count(), SpawnAmount = _gatherableResources.ContainsKey(v.Key) ? (float)(_gatherableResources[v.Key].SpawnAmount + (double)gatherableResourcesData[v.Key].AdditionalProperties["spawnRate"]) : 0f, });

      foreach (var resource in _gatherableResources)
      {
        if (resource.Value.Count >= (long)gatherableResourcesData[resource.Key].AdditionalProperties["max"])
          continue;

        var count = (int)Math.Floor(resource.Value.SpawnAmount);
        if (count >= 1)
        {
          // Commented out until I can figure out how to add to '_entities' too
          //_gwm.AddGatherablePlace(resource.Key, count);
          resource.Value.SpawnAmount = 0f;
        }
      }
    }
  }
}
