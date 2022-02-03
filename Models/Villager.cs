using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Others.Managers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZonerEngine.GL;

namespace Others.Models
{
  public class Villager
  {
    public Villager(string name)
    {
      Name = name;
    }

    public Villager()
    {

    }

    [JsonProperty("taskTimer")]
    private float _taskTimer;

    [JsonProperty("id")]
    public long Id;

    [JsonProperty("householdId")]
    public long HouseholdId;

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("mapPoint")]
    public Point MapPoint { get; set; }

    [JsonProperty("path")]
    public List<Point> Path { get; set; } = new List<Point>();

    [JsonProperty("position")]
    public Vector2 Position { get; set; }

    [JsonProperty("skills")]
    public Dictionary<string, float> Skills { get; set; }

    [JsonProperty("attributes")]
    public Dictionary<string, AttributeWrapper> Attributes { get; set; } = new Dictionary<string, AttributeWrapper>();

    [JsonProperty("inventory")]
    public Dictionary<string, ItemWrapper> Inventory { get; set; } = new Dictionary<string, ItemWrapper>();

    [JsonProperty("tasks")]
    public List<TaskWrapper> Tasks { get; set; } = new List<TaskWrapper>();

    [JsonProperty("currentTask")]
    public TaskWrapper CurrentTask { get; set; }

    [JsonProperty("currentPlaceId")]
    public long CurrentPlaceId { get; set; }

    [JsonIgnore]
    public Household Household { get; set; }

    public enum VillagerStates
    {
      Idle,
      HasFullInventory,
      SeekingTask,
      GoToTask,
      ExecuteTask,
    }

    public VillagerStates GetState()
    {
      if (CurrentTask == null && Tasks.Count > 0)
        return VillagerStates.SeekingTask;

      //if (CurrentTask != null && CurrentPlaceId != CurrentTask.PlaceId)
      //  return VillagerStates.GoToTask;

      if (CurrentTask != null /*&& CurrentPlaceId == CurrentTask.PlaceId*/)
        return VillagerStates.ExecuteTask;

      return VillagerStates.Idle;
    }

    public VillagerStates State => GetState();

    public void SetCurrentTask(GameWorldManager gwm)
    {
      CurrentTask = Tasks[0];
      Console.WriteLine($"{Name} is working on {CurrentTask.Name}");
      Tasks.RemoveAt(0);

      SetTravelling(gwm);
    }

    private void SetTravelling(GameWorldManager gwm)
    {
      if (CurrentPlaceId == CurrentTask.PlaceId)
        return;

      Tasks.Insert(0, CurrentTask);

      var place = gwm.GameWorld.Places.FirstOrDefault(c => c.Id == CurrentTask.PlaceId);
      var taskData = gwm.GameWorld.TaskData["walking"];
      var task = new TaskWrapper();
      task.PlaceId = place.Id;
      task.LoadFromData(taskData);

      CurrentTask = task;
      Console.WriteLine($"{Name} is going to {place.Name}");
    }

    public void GoToPlace(PlaceWrapper place)
    {
      if (GameWorldManager.Random.Next(0, 100) == 10)
      {
        CurrentPlaceId = place.Id;
        Console.WriteLine($"{Name} is now at {place.Name}");
      }

      _taskTimer = 0;
    }

    public bool IsInventoryFull()
    {
      return Inventory.Values.Sum(c => c.StackWeight) > float.Parse(GameWorldManager.Statics["carryWeight"]);
    }

    public void DoTask(GameWorldManager gwm)
    {
      var place = gwm.GameWorld.Places.FirstOrDefault(c => c.Id == CurrentTask.PlaceId);
      MethodInfo magicMethod = this.GetType().GetMethod($"Do{CurrentTask.Data.Type}Task");
      magicMethod.Invoke(this, new object[] { gwm });

      CheckStorage(gwm);
    }

    private void CheckStorage(GameWorldManager gwm)
    {
      if (IsInventoryFull())
      {
        if (Tasks.Any(c => c.Name == "storingItems"))
        {
          // Move the task so it's always 2nd?                  
        }
        else
        {
          Console.WriteLine($"{Name}'s inventory is full");
          if (CurrentTask != null)
          {
            Tasks.Insert(0, CurrentTask);
            CurrentTask = null;
          }

          SetStorageTask(gwm);
        }
      }
    }

    private void SetStorageTask(GameWorldManager gwm)
    {
      // Gets all the storage places 
      var storagePlaces = gwm.GameWorld.Places.Where(c => c.Data.Type == "Storage");

      var taskData = gwm.GameWorld.TaskData["storingItems"];

      var task = new TaskWrapper();
      task.PlaceId = storagePlaces.FirstOrDefault().Id;
      task.LoadFromData(taskData);//, 1);

      Tasks.Insert(0, task);

      //CurrentTask = task;
    }

    public void DoCollectionTask(GameWorldManager gwm)
    {
      var place = GetCurrentPlace(gwm);

      _taskTimer += 0.01f;

      if (_taskTimer > CurrentTask.Data.Rate)
      {
        _taskTimer = 0;

        var rand = GameWorldManager.Random.NextDouble();

        var chance = 0.0f;
        foreach (var item in CurrentTask.Data.ProducedItems)
        {
          chance += item.Chance;

          if (rand < chance)
          {
            AddToInventory(gwm, item);

            var health = (long)place.AdditionalProperties["health"].Value - 1;
            place.AdditionalProperties["health"].Value = health;

            Console.WriteLine($"{Name} recieved {item.Name}");

            if (health <= 0)
            {
              place.IsRemoved = true;
              Console.WriteLine($"{place.Name} has been depleted");
              CurrentTask = null;
            }

            break;
          }
        }
      }
    }

    private void AddToInventory(GameWorldManager gwm, Task.ProducedItem item)
    {
      var items = gwm.GameWorld.ItemData;

      if (!Inventory.ContainsKey(item.Name))
      {
        var itemWrapper = new ItemWrapper();
        itemWrapper.LoadFromData(items[item.Name]);

        Inventory.Add(item.Name, itemWrapper);
      }

      Inventory[item.Name].Count++;
    }

    public void DoSleepingTask(GameWorldManager gwm)
    {
      if (Attributes["energy"].Total >= Attributes["energy"].Data.Total)
      {
        CurrentTask = null;
      }
    }

    public void SetIdleTask(GameWorldManager gwm)
    {
      Console.Write("doing something random while Idle");

      var taskData = gwm.GameWorld.TaskData["idle"];

      var task = new TaskWrapper();
      var idleLocation = gwm.AddPlace("idleSpot", MapPoint); // TODO: Set x/y to current position
      task.PlaceId = idleLocation.Id;
      task.LoadFromData(taskData);

      Tasks.Insert(0, task);
    }

    public void DoIdleTask(GameWorldManager gwm)
    {
      if (Tasks.Count > 0)
      {
        gwm.DeletePlaceById(CurrentPlaceId);
        CurrentTask = null; // Stop being idle
      }
    }

    public void DoStorageTask(GameWorldManager gwm)
    {
      var place = GetCurrentPlace(gwm);

      if (!place.AdditionalProperties.ContainsKey("inventory"))
      {
        place.AdditionalProperties.Add("inventory", new AdditionalProperty() { Value = new Dictionary<string, int>() });
      }

      var placeInventory = place.AdditionalProperties["inventory"].Value;

      foreach (var item in Inventory)
      {
        if (!placeInventory.ContainsKey(item.Key))
          placeInventory.Add(item.Key, 0);

        placeInventory[item.Key] += item.Value.Count;
      }

      Inventory = new Dictionary<string, ItemWrapper>();

      place.AdditionalProperties["inventory"].Value = placeInventory;
      CurrentTask = null;
    }

    public void DoCraftingTask(GameWorldManager gwm)
    {
      //var places = Current;
      var task = CurrentTask.Data;
      var inv = gwm.GetInventory();
      var producedItem = CurrentTask.Data.ProducedItems[0];
      var item = producedItem.Name;
      bool HasRequiredResources = true;

      TextInfo textInfo = new CultureInfo("en-EU", false).TextInfo;

      //loop task for resources required
      foreach (var requireResource in task.RequiredResources)
      {
        if (!inv.ContainsKey(requireResource.Name) && !this.Inventory.ContainsKey(requireResource.Name))
        {
          HasRequiredResources = false;
          var place = gwm.GameWorld.Places.FirstOrDefault(c => c.Name == requireResource.Name);
          gwm.AddTask($"gather{textInfo.ToTitleCase(requireResource.Name)}", 0, place.Id);
        }
      }
      //check if inv has resouces

      //check if has resouce required is true
      if (HasRequiredResources)
      {
        AddToInventory(gwm, producedItem);
        CurrentTask = null;
      }
      else
      {
        gwm.AddTask(CurrentTask.Name, CurrentTask.Priority, CurrentTask.PlaceId);
        CurrentTask = null;
      }
    }

    public void DoTravellingTask(GameWorldManager gwm)
    {
      var place = GetCurrentPlace(gwm);

      if (Path.Count == 0)
      {
        Path = gwm.Pathfinder.GetPathNextTo(MapPoint, place.Point);
      }

      // Notice how this isn't and else if (that was intentional)
      if (Path.Count > 0)
      {
        var nextPoint = Path[0];

        var speed = 1f;

        if (nextPoint.X > MapPoint.X) // Go right
        {
          Position = new Vector2(Position.X + speed, Position.Y);
        }
        else if (nextPoint.X < MapPoint.X) // Go Left
        {
          Position = new Vector2(Position.X - speed, Position.Y);
        }
        else if (nextPoint.Y > MapPoint.Y) // Go down
        {
          Position = new Vector2(Position.X, Position.Y + speed);
        }
        else if (nextPoint.Y < MapPoint.Y) // Go up
        {
          Position = new Vector2(Position.X, Position.Y - speed);
        }

        var distance = Vector2.Distance(Position, nextPoint.ToVector2());
        var pos = Position / Game1.TileSize;

        if (Helpers.NearlyEqual(pos, nextPoint.ToVector2()))
        {
          Path.RemoveAt(0);
          MapPoint = nextPoint;
          //Position = MapPoint.ToVector2();
        }
      }

      if (Path.Count == 0)
      {
        CurrentPlaceId = place.Id;
        Console.WriteLine($"{Name} is now at {place.Name}");
        CurrentTask = null;
        Path = new List<Point>();
      }

      _taskTimer = 0;
    }

    public bool IsAtPlace(PlaceWrapper place)
    {
      var distance = Vector2.Distance(this.MapPoint.ToVector2(), place.Point.ToVector2());

      return distance < 1.5f;
    }

    public void UpdateAttributes()
    {
      foreach (var attrKvp in Attributes)
      {
        var attr = attrKvp.Value;
        if (CurrentTask != null)
        {
          var taskType = CurrentTask.Data.Type;

          if (attr.Data.PositiveTasks.ContainsKey(taskType))
          {

            attr.Total += attr.Data.PositiveTasks[taskType];
          }

          if (attr.Data.NegativeTasks.ContainsKey(taskType))
          {

            attr.Total -= attr.Data.NegativeTasks[taskType];
          }

          if (attr.Total < 0)
            attr.Total = 0;

          if (attr.Total > attr.Data.Total)
            attr.Total = attr.Data.Total;

          //attr.Total = Math.Clamp(attr.Total, 0, attr.Attribute.Total);
        }
      }
    }

    public void CheckAttributes(GameWorldManager gwm)
    {
      foreach (var attribute in Attributes)
      {
        if (attribute.Value.Total <= 0)
        {
          var taskData = gwm.GameWorld.TaskData[attribute.Value.Data.TaskType];
          if (Tasks.Any(c => c.Name == taskData.Name))
          {
            // Move the task so it's always 2nd?                  
          }
          else
          {
            if (CurrentTask != null)
            {
              Tasks.Insert(0, CurrentTask);
              CurrentTask = null;
            }

            var bed = GetBed(gwm, taskData.Type);

            var task = new TaskWrapper();
            task.PlaceId = bed != null ? bed.Id : this.Id;
            task.LoadFromData(taskData);

            Tasks.Insert(0, task);
          }
        }
      }
    }

    private PlaceWrapper GetBed(GameWorldManager gwm, string taskType)
    {
      PlaceWrapper bed = null;

      var beds = gwm.GameWorld.Places.Where(c => c.Data.Type == taskType);
      var villagersBed = beds.FirstOrDefault(c => (long)c.AdditionalProperties["ownerId"].Value == this.Id);

      // If the villager doesn't have a bed =(
      if (villagersBed == null)
      {
        var availableBeds = beds.Where(c => (long)c.AdditionalProperties["ownerId"].Value == -1);

        // If there are no available beds anywhere =(((((
        if (availableBeds.Count() == 0)
        {
          bed = null;
        }
        else
        {
          // Get the bed closest
          bed = availableBeds.OrderBy(c => Vector2.Distance(c.Point.ToVector2(), this.MapPoint.ToVector2())).FirstOrDefault();
          bed.AdditionalProperties["ownerId"].Value = this.Id;
        }
      }
      else
      {
        bed = villagersBed;
      }


      return bed;
    }

    private PlaceWrapper GetCurrentPlace(GameWorldManager gwm)
    {
      return gwm.GameWorld.Places.FirstOrDefault(c => c.Id == CurrentTask.PlaceId);
    }

    public void Load(GameWorld gameWord)
    {
      CurrentTask?.LoadFromSave(gameWord.TaskData[CurrentTask.Name]);//, CurrentTask.Priority);

      foreach (var task in Tasks)
      {
        task.LoadFromSave(gameWord.TaskData[task.Name]);//, task.Priority);
      }

      foreach (var inventoryItem in Inventory)
      {
        inventoryItem.Value.LoadFromSave(gameWord.ItemData[inventoryItem.Value.Name]);
      }

      foreach (var attribute in gameWord.AttributeData)
      {
        if (!Attributes.ContainsKey(attribute.Key))
        {
          var attributeWrapper = new AttributeWrapper();
          Attributes.Add(attribute.Key, attributeWrapper);
        }

        Attributes[attribute.Key].LoadFromSave(attribute.Value);
      }

      for (int i = 0; i < Attributes.Count; i++)
      {
        var attribute = Attributes.ElementAt(i);

        if (!gameWord.AttributeData.ContainsKey(attribute.Key))
        {
          Attributes.Remove(attribute.Key);
          i--;
        }
      }

      var household = gameWord.Households.FirstOrDefault(c => c.Id == this.HouseholdId);
      if (household == null)
        throw new ApplicationException($"{Name} is homeless!"); // Delete your save.json to load the new default game

      household.AssignVillager(this);
    }
  }
}