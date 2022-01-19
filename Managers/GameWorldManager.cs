using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Others.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZonerEngine.GL;
using ZonerEngine.GL.Maps;

namespace Others.Managers
{
  public class GameWorldManager
  {
    public static Random Random = new Random();

    public static Dictionary<string, string> Statics = new Dictionary<string, string>();

    public GameWorld GameWorld { get; set; }

    public readonly Pathfinder Pathfinder;

    public List<int> RemovedPlaces { get; private set; } = new List<int>();

    public GameWorldManager(Map map)
    {
      Pathfinder = new Pathfinder(map);
    }

    public int GetId(string dataType)
    {
      if (!GameWorld.Ids.ContainsKey(dataType))
        GameWorld.Ids.Add(dataType, 0);

      return ++GameWorld.Ids[dataType];
    }

    public void AddTask(string taskName, int priority, int placeId)
    {
      if (!GameWorld.TaskData.ContainsKey(taskName))
      {
        Console.WriteLine($"No task found for {taskName}");
        return;
      }
      var taskData = GameWorld.TaskData[taskName];

      var task = new TaskWrapper();
      task.PlaceId = placeId;
      task.LoadFromData(taskData);//, priority);
      GameWorld.Tasks.Add(task);
    }

    public bool AssignTask(TaskWrapper task)
    {
      var requiredSkills = task.Data.SkillRequirements;
      List<Villager> capableVillagers = new List<Villager>();
      foreach (var villager in GameWorld.Villagers)
      {
        bool isValid = true;

        foreach (var skill in requiredSkills)
        {
          if (villager.Skills == null)
          {
            isValid = false;
            break;
          }
          if (!villager.Skills.ContainsKey(skill.Name))
          {
            isValid = false;
            break;
          }

          if (villager.Skills[skill.Name] < skill.Level)
          {
            isValid = false;
            break;
          }
        }
        if (isValid)
        {
          capableVillagers.Add(villager);
        }
      }
      if (capableVillagers.Count == 0)
      {
        Console.WriteLine($"No capable villagers for this task {task.Name}");
        return false;
      }
      if (capableVillagers.Count == 1)
      {
        capableVillagers[0].Tasks.Add(task);
        Console.WriteLine($"{task.Name} has been assigned to {capableVillagers[0].Name}");

        return true;
      }
      if (capableVillagers.Count > 1)
      {
        foreach (var villager in capableVillagers)
        {
          if (villager.Tasks.Count == 0)
          {
            villager.Tasks.Add(task);

            return true;
          }
        }
      }
      return false;
    }

    public PlaceWrapper AddPlace(string placeName, int x, int y)
    {
      if (!GameWorld.PlaceData.ContainsKey(placeName))
      {
        Console.WriteLine($"{placeName}");
        throw new Exception("ya place don't exist bruv");
      }

      var place = new PlaceWrapper();
      place.Id = GetId("place");
      place.Point = new Microsoft.Xna.Framework.Point(x, y);
      place.LoadFromData((Place)GameWorld.PlaceData[placeName].Clone());
      GameWorld.Places.Add(place);

      return place;
    }

    /*public ItemWrapper AddGatherableItem(string itemName, int x, int y)
    {
      if(!GameWorld.ItemData.ContainsKey(itemName))
      {
        Console.WriteLine($"{itemName}");
        throw new Exception("Da hell is this item supposed to be!?");
      }

      var item = new ItemWrapper();
      item.Id = GetId("item");
    }*/

    public void DeletePlaceById(int id)
    {
      for (int i = 0; i < GameWorld.Places.Count; i++)
      {
        if (GameWorld.Places[i].Id == id)
        {
          GameWorld.Places.RemoveAt(i);
          return;
        }
      }
    }

    public void AddVillager(string name, Point mapPoint, Dictionary<string, float> skills = null)
    {
      GameWorld.Villagers.Add(new Villager(name)
      {
        Id = GetId("villager"),
        MapPoint = mapPoint,
        Skills = skills,
        Attributes = GameWorld.AttributeData.ToDictionary(c => c.Key, v =>
        {
          var wrapper = new AttributeWrapper();
          wrapper.LoadFromData(GameWorld.AttributeData[v.Key]);
          return wrapper;
        }),
      });
    }

    public void Update()
    {
      AssignTasks();

      UpdateVillager();

      UpdatePlaces();
    }

    private void UpdatePlaces()
    {
      RemovedPlaces = new List<int>();
      for (int i = 0; i < GameWorld.Places.Count; i++)
      {
        if (GameWorld.Places[i].IsRemoved)
        {
          GameWorld.Places.RemoveAt(i);
          RemovedPlaces.Add(GameWorld.Places[i].Id);
        }
      }
    }

    private void UpdateVillager()
    {
      foreach (var villager in GameWorld.Villagers)
      {
        var state = villager.GetState();

        switch (state)
        {
          case Villager.VillagerStates.Idle:
            villager.SetIdleTask(this);
            break;
          case Villager.VillagerStates.SeekingTask:
            villager.SetCurrentTask(this);
            break;
          //case Villager.VillagerStates.GoToTask:
          //  villager.GoToPlace(GetPlaceById(villager.CurrentTask.PlaceId));
          //  break;
          case Villager.VillagerStates.ExecuteTask:
            villager.DoTask(this);
            break;
          default:
            break;
        }

        villager.UpdateAttributes();
        villager.CheckAttributes(this);
      }
    }

    public PlaceWrapper GetPlaceById(int id)
    {
      return GameWorld.Places.FirstOrDefault(
                      c => c.Id == id);
    }

    private void AssignTasks()
    {
      for (int i = 0; i < GameWorld.Tasks.Count; i++)
      {
        var hasAssigned = AssignTask(GameWorld.Tasks[i]);

        if (hasAssigned)
        {
          GameWorld.Tasks.RemoveAt(i);
          i--;
        }
      }
    }

    public Dictionary<string, int> GetInventory()
    {
      var value = GameWorld.Places
        .Where(c => c.Data.Type == "Storage" && c.AdditionalProperties.ContainsKey("inventory"))
        .SelectMany(c => c.AdditionalProperties["inventory"].ToDictionary<string, int>())
        .GroupBy(c => c.Key)
        .ToDictionary(c => c.Key, v => v.Sum(b => b.Value));

      return value;
    }

    public void Save(string fileName)
    {
      using (StreamWriter file = File.CreateText(fileName))
      {
        JsonSerializer serializer = new JsonSerializer() { Formatting = Formatting.Indented };
        //serialize object directly into file stream
        serializer.Serialize(file, GameWorld);
      }
    }

    public void Load(string fileName)
    {
      if (File.Exists(fileName))
      {
        using (var r = new StreamReader(fileName))
        {
          string json = r.ReadToEnd();
          GameWorld = JsonConvert.DeserializeObject<GameWorld>(json);
        }
        LoadData();

        //foreach (var attribute in GameWorld.Attributes)
        //{
        //  attribute.LoadFromSave(GameWorld.AttributeData[attribute.Name]);
        //}

        // Need to assign the actual data content to the associated wrappers
        foreach (var task in GameWorld.Tasks)
        {
          task.LoadFromSave(GameWorld.TaskData[task.Name]);//, task.Priority);
        }

        foreach (var place in GameWorld.Places)
        {
          place.LoadFromSave((Place)GameWorld.PlaceData[place.Name].Clone());
        }

        foreach (var Villager in GameWorld.Villagers)
        {
          Villager.Load(GameWorld);
        }
      }
      else
      {
        GameWorld = new GameWorld();
        LoadData();

        SetDefaultWorld();
      }
    }

    private void SetDefaultWorld()
    {
      AddVillager("Kyle", new Point(0, 0), new Dictionary<string, float>() { { "mining", 1 }, { "chopping", 1 } });
      //AddVillager("Niall", new Dictionary<string, float>() { { "chopping", 1 } });

      AddPlace("goldOre", 1, 1);
      AddPlace("goldOre", 2, 1);
      AddPlace("goldOre", 4, 4);
      AddPlace("goldOre", 5, 3);
      AddPlace("goldOre", 5, 1);
      AddPlace("goldOre", 5, 6);
      AddPlace("goldOre", 7, 2);
      AddPlace("goldOre", 10, 4);
      AddPlace("goldOre", 5, 8);
      AddPlace("goldOre", 1, 4);
      AddPlace("goldOre", 2, 2);

      AddPlace("rocks", 1, 2);

      AddPlace("normalTree", 3, 1);

      foreach (var place in GameWorld.Places.Where(c => c.Data.Skill == "mining"))
      {
        AddTask("miningGold", 1, place.Id);
      }

      //foreach (var place in GameWorld.Places.Where(c => c.Place.Skill == "chopping"))
      //{
      //  AddTask("choppingNormalTree", 1, place.Id);
      //}

      AddPlace("storageChest", 5, 5);
      AddPlace("singleBed", 6, 5);
      AddPlace("singleBed", 8, 5);
    }

    /// <summary>
    /// Load the preset content of the game
    /// </summary>
    private void LoadData()
    {
      var staticFiles = Directory.GetFiles("Data/", "statics.json");

      foreach (var staticFile in staticFiles)
      {
        using (var r = new StreamReader(staticFile))
        {
          string json = r.ReadToEnd();
          dynamic staticValues = JsonConvert.DeserializeObject(json);

          foreach (var value in staticValues.values)
          {
            Statics.Add(value.name.Value, value.value.Value);
          }
        }
      }

      var attributeFiles = Directory.GetFiles("Data/Attributes/", "*-attr.json");

      foreach (var attributeFile in attributeFiles)
      {
        using (var r = new StreamReader(attributeFile))
        {
          string json = r.ReadToEnd();
          var attribute = JsonConvert.DeserializeObject<Models.Attribute>(json);

          GameWorld.AttributeData.Add(attribute.Name, attribute);
        }
      }

      var taskFiles = Directory.GetFiles("Data/Tasks", "*-task.json");

      foreach (var taskFile in taskFiles)
      {
        using (var r = new StreamReader(taskFile))
        {
          string json = r.ReadToEnd();
          var task = JsonConvert.DeserializeObject<Task>(json);

          GameWorld.TaskData.Add(task.Name, task);
        }
      }

      var placeFiles = Directory.GetFiles("Data/", "places.json");

      foreach (var placeFile in placeFiles)
      {
        using (var r = new StreamReader(placeFile))
        {
          string json = r.ReadToEnd();
          var places = JsonConvert.DeserializeObject<Places>(json);

          foreach (var place in places.ListOfPlaces)
          {
            GameWorld.PlaceData.Add(place.Name, place);
          }
        }
      }

      var itemFiles = Directory.GetFiles("Data/", "items.json");

      foreach (var itemFile in itemFiles)
      {
        using (var r = new StreamReader(itemFile))
        {
          string json = r.ReadToEnd();
          var items = JsonConvert.DeserializeObject<Items>(json);

          foreach (var item in items.ListOfItems)
          {
            GameWorld.ItemData.Add(item.Name, item);
          }
        }
      }
    }
  }
}
