using Newtonsoft.Json;
using Others.Managers;
using System;
using System.Collections.Generic;
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
    public int Id;

    [JsonProperty("name")]
    public string Name { get; set; }

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
    public int CurrentPlaceId { get; set; }

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

    public VillagerStates States { get; private set; }

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
      MethodInfo magicMethod = this.GetType().GetMethod($"Do{CurrentTask.Task.Type}Task");
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
      var storagePlaces = gwm.GameWorld.Places.Where(c => c.Place.Type == "Storage");

      var taskData = gwm.GameWorld.TaskData["storingItems"];

      var task = new TaskWrapper();
      task.PlaceId = storagePlaces.FirstOrDefault().Id;
      task.LoadFromData(taskData);//, 1);

      Tasks.Insert(0, task);

      //CurrentTask = task;
    }

    public void DoCollectionTask(GameWorldManager gwm)
    {
      var items = gwm.GameWorld.ItemData;
      var place = GetCurrentPlace(gwm);

      _taskTimer += 0.01f;

      if (_taskTimer > CurrentTask.Task.Rate)
      {
        _taskTimer = 0;

        var rand = GameWorldManager.Random.NextDouble();

        var chance = 0.0f;
        foreach (var item in CurrentTask.Task.ProducedItems)
        {
          chance += item.Chance;

          if (rand < chance)
          {
            if (!Inventory.ContainsKey(item.Name))
            {
              var itemWrapper = new ItemWrapper();
              itemWrapper.LoadFromData(items[item.Name]);

              Inventory.Add(item.Name, itemWrapper);
            }

            Inventory[item.Name].Count++;

            var health = (long)place.AdditionalProperties["health"] - 1;
            place.AdditionalProperties["health"] = health;

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

    public void DoSleepingTask(GameWorldManager gwm)
    {
      if (Attributes["energy"].Total >= Attributes["energy"].Attribute.Total)
      {
        CurrentTask = null;
      }
    }

    public void SetIdleTask(GameWorldManager gwm)
    {
      Console.Write("doing something random while Idle");

      var taskData = gwm.GameWorld.TaskData["idle"];

      var task = new TaskWrapper();
      var idleLocation = gwm.AddPlace("idleSpot", 0, 0); // TODO: Set x/y to current position
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
        place.AdditionalProperties.Add("inventory", new Dictionary<string, int>());
      }

      var placeInventory = place.AdditionalProperties["inventory"].ToDictionary<string, int>();

      foreach (var item in Inventory)
      {
        if (!placeInventory.ContainsKey(item.Key))
          placeInventory.Add(item.Key, 0);

        placeInventory[item.Key] += item.Value.Count;
      }

      Inventory = new Dictionary<string, ItemWrapper>();

      place.AdditionalProperties["inventory"] = placeInventory;
      CurrentTask = null;
    }

    public void DoTravellingTask(GameWorldManager gwm)
    {
      var place = GetCurrentPlace(gwm);

      if (GameWorldManager.Random.Next(0, 100) == 1)
      {
        CurrentPlaceId = place.Id;
        Console.WriteLine($"{Name} is now at {place.Name}");
        CurrentTask = null;
      }

      _taskTimer = 0;
    }

    public void UpdateAttributes()
    {
      foreach (var attrKvp in Attributes)
      {
        var attr = attrKvp.Value;
        if (CurrentTask != null)
        {
          var taskType = CurrentTask.Task.Type;

          if (attr.Attribute.PositiveTasks.ContainsKey(taskType))
          {

            attr.Total += attr.Attribute.PositiveTasks[taskType];
          }

          if (attr.Attribute.NegativeTasks.ContainsKey(taskType))
          {

            attr.Total -= attr.Attribute.NegativeTasks[taskType];
          }

          if (attr.Total < 0)
            attr.Total = 0;

          if (attr.Total > attr.Attribute.Total)
            attr.Total = attr.Attribute.Total;

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
          var taskData = gwm.GameWorld.TaskData[attribute.Value.Attribute.TaskType];
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

            var sleepingPlaces = gwm.GameWorld.Places.Where(c => c.Place.Type == taskData.Type);

            var task = new TaskWrapper();
            task.PlaceId = sleepingPlaces.FirstOrDefault().Id;
            task.LoadFromData(taskData);

            Tasks.Insert(0, task);
          }
        }
      }
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
    }
  }
}