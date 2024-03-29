﻿using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Others.Models;
using Others.States;
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
    public readonly BattleState State;

    private GatherableResourcesManager _grm;

    public static Random Random = new Random();

    public static Dictionary<string, string> Statics = new Dictionary<string, string>();

    public GameWorld GameWorld { get; set; }

    public readonly Pathfinder Pathfinder;

    public GameWorldManager(BattleState state)
    {
      State = state;
      Pathfinder = new Pathfinder(State.Map);
    }

    public int GetId(string dataType)
    {
      if (!GameWorld.Ids.ContainsKey(dataType))
        GameWorld.Ids.Add(dataType, 0);

      return ++GameWorld.Ids[dataType];
    }

    public void AddTask(string taskName, int priority, long placeId)
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

    public void RemoveTask(string taskName, long placeId)
    {
      var task = GameWorld.Tasks.FirstOrDefault(c => c.Name == taskName && c.PlaceId == placeId);
      if (task == null)
        return;

      GameWorld.Tasks.Remove(task);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="placeName">The type of place</param>
    /// <param name="x">Map x coord</param>
    /// <param name="y">Map y coor</param>
    /// <param name="householdId">The household this place belongs to</param>
    /// <param name="additionalValues"></param>
    /// <returns></returns>
    public PlaceWrapper AddPlace(string placeName, int x, int y, long householdId = -1, Dictionary<string, string> additionalValues = null)
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
      if (householdId > 0)
      {
        place.HouseholdId = householdId;
        place.Household = GameWorld.Households.FirstOrDefault(c => c.Id == place.HouseholdId);
      }

      if (additionalValues == null)
        additionalValues = new Dictionary<string, string>();

      if (place.Data.PlaceType.IsConstructable)
        additionalValues.Add("construction%", "0");

      if (additionalValues != null)
      {
        foreach (var value in additionalValues)
        {
          if (place.AdditionalProperties.ContainsKey(value.Key))
            continue;

          place.AdditionalProperties.Add(value.Key, new AdditionalProperty() { Value = value.Value, IsVisible = false });
        }
      }

      if (place.AdditionalProperties.ContainsKey("construction%"))
      {
        AddTask("construct", 1, place.Id);
      }

      GameWorld.Places.Add(place);

      State.AddPlaceEntity(place);

      return place;
    }

    public PlaceWrapper AddPlace(string placeName, Point point)
    {
      return this.AddPlace(placeName, point.X, point.Y);
    }

    /// <summary>
    /// Adds a gatherable place in a random location
    /// ##warning if you trigger this multiple times in a row, you can hit the same point > 1 time(s)
    /// </summary>
    /// <param name="placeName"></param>
    /// <returns></returns>
    public void AddGatherablePlace(string placeName, int amount = 1)
    {
      var emptyPoints = State.Map.GetEmptyPoints().ToList();

      for (int i = 0; i < amount; i++)
      {
        var index = Random.Next(0, emptyPoints.Count);
        var randomPoint = emptyPoints[index];

        AddPlace(placeName, randomPoint);

        emptyPoints.RemoveAt(index);
      }
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

    public void DeletePlaceById(long id)
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

    public Household AddHousehold(string name, Building building)
    {
      var household = new Household()
      {
        Id = GetId("household"),
        Name = name,
        Rectangle = building.Rectangle.Divide(Game1.TileSize),
      };
      household.Load(this);
      GameWorld.Households.Add(household);

      var size = household.Rectangle;

      for (int y = 0; y < size.Height; y++)
      {
        for (int x = 0; x < size.Width; x++)
        {
          var point = new Point(x + size.X, y + size.Y);
          string type = "woodenFloor";
          var additionalProperties = new Dictionary<string, string>();

          if (building.Doors.ContainsKey(point))
          {
            var door = building.Doors[point];
            type = door.Name;
          }
          else if (building.Walls.ContainsKey(point))
          {
            var wall = building.Walls[point];
            type = wall.Name;
            additionalProperties.Add("wallType", Path.GetFileName(wall.AdditionalProperties["wallType"].Value));
          }

          household.AddPlace(type, x, y, additionalProperties);
        }
      }

      return household;
    }

    public void InstabuildHousehold(Household household)
    {
      var places = GameWorld.Places.Where(c => c.HouseholdId == household.Id);
      foreach (var place in places)
      {
        if (place.Data.PlaceType.IsConstructable)
          place.AdditionalProperties["construction%"].Value = "100";

        State.EditPlaceEntity(place);
        RemoveTask("construct", place.Id);
      }
    }

    private static string GetWallType(Point wall, List<Point> points)
    {
      var x = wall.X;
      var y = wall.Y;
      var hasLeft = points.Contains(new Point(x - 1, y));
      var hasRight = points.Contains(new Point(x + 1, y));
      var hasTop = points.Contains(new Point(x, y - 1));
      var hasBottom = points.Contains(new Point(x, y + 1));

      var wallType = "";
      if (hasTop)
        wallType += "D";

      if (hasRight)
        wallType += "R";

      if (hasBottom)
        wallType += "U";

      if (hasLeft)
        wallType += "L";

      if (string.IsNullOrEmpty(wallType))
        wallType = "Wall";

      return wallType;
    }

    public static Dictionary<Point, string> GetOuterWalls(Rectangle rectangle)
    {
      var points = new List<Point>();

      for (int y = rectangle.Y; y < rectangle.Bottom; y++)
      {
        for (int x = rectangle.X; x < rectangle.Right; x++)
        {
          var addWall = y == rectangle.Y ||
            x == rectangle.X ||
            y == (rectangle.Bottom - 1) ||
            x == (rectangle.Right - 1);

          if (addWall)
            points.Add(new Point(x, y));
        }
      }

      var result = new Dictionary<Point, string>();
      foreach (var wall in points)
      {
        var x = wall.X;
        var y = wall.Y;
        var hasLeft = points.Contains(new Point(x - 1, y));
        var hasRight = points.Contains(new Point(x + 1, y));
        var hasTop = points.Contains(new Point(x, y - 1));
        var hasBottom = points.Contains(new Point(x, y + 1));

        var wallType = "";
        if (hasTop)
          wallType += "D";

        if (hasRight)
          wallType += "R";

        if (hasBottom)
          wallType += "U";

        if (hasLeft)
          wallType += "L";

        if (string.IsNullOrEmpty(wallType))
          wallType = "Wall";

        result.Add(wall, wallType);
      }

      return result;
    }

    public static Dictionary<Point, string> GetWalls(List<Rectangle> rectangles)
    {
      var points = new List<Point>();

      foreach (var rectangle in rectangles)
      {
        points.Add(new Point(rectangle.X, rectangle.Y));
      }

      var result = new Dictionary<Point, string>();
      foreach (var wall in points)
      {
        var x = wall.X;
        var y = wall.Y;
        var hasLeft = points.Contains(new Point(x - 1, y));
        var hasRight = points.Contains(new Point(x + 1, y));
        var hasTop = points.Contains(new Point(x, y - 1));
        var hasBottom = points.Contains(new Point(x, y + 1));

        var wallType = "";
        if (hasTop)
          wallType += "D";

        if (hasRight)
          wallType += "R";

        if (hasBottom)
          wallType += "U";

        if (hasLeft)
          wallType += "L";

        if (string.IsNullOrEmpty(wallType))
          wallType = "Wall";

        result.Add(wall, wallType);
      }

      return result;
    }

    public void TestWalls(int startX, int startY)
    {
      var points = new List<Point>();

      var map = new int[,]
      {
        { 0, 1, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, },
        { 0, 0, 0, 1, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, },
        { 0, 0, 0, 1, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, },
        { 0, 0, 0, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, },
        { 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, },
        { 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, },
        { 1, 1, 1, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, },
        { 0, 0, 0, 0, 1, 1, 1, 0, 1, 0, 1, 0, 0, 0, },
        { 0, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 0, 0, },
        { 0, 1, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0, 0, },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, },
      };

      for (int y = 0; y < map.GetLength(0); y++)
      {
        for (int x = 0; x < map.GetLength(1); x++)
        {
          if (map[y, x] == 1)
          {
            points.Add(new Point(x, y));
          }
        }
      }

      foreach (var wall in points)
      {
        string wallType = GetWallType(wall, points);

        AddPlace("woodenWall", wall.X + startX, wall.Y + startY, additionalValues: new Dictionary<string, string>() { { "wallType", wallType } });
      }
    }

    public Villager AddVillager(string name, Point mapPoint, Dictionary<string, float> skills = null)
    {
      var villager = new Villager(name)
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
      };

      GameWorld.Villagers.Add(villager);
      State.AddVillagerEntity(villager);

      return villager;
    }

    public void Update(GameTime gameTime)
    {
      AutoAssignTasks();

      UpdateVillager();

      UpdatePlaces();

      foreach(var household in GameWorld.Households)
      {
        household.Update(gameTime);
      }

      _grm.Update(gameTime);
    }

    private void UpdatePlaces()
    {
      for (int i = 0; i < GameWorld.Places.Count; i++)
      {
        if (GameWorld.Places[i].IsRemoved)
        {
          GameWorld.Places.RemoveAt(i);
        }
        else
        {
          State.EditPlaceEntity(GameWorld.Places[i]);
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

    public Villager GetVillagerById(long id)
    {
      return GameWorld.Villagers.FirstOrDefault(
        c => c.Id == id);
    }

    public PlaceWrapper GetPlaceById(long id)
    {
      return GameWorld.Places.FirstOrDefault(
                      c => c.Id == id);
    }

    private void AutoAssignTasks()
    {
      foreach (var villager in GameWorld.Villagers)
      {
        // Don't give a villager with work more work
        if (villager.Tasks.Count(task => task.Priority >= 0) > 0)
          continue;

        var orderedTasks = GameWorld.Tasks
          .Where(task => task.Data.SkillRequirements.Count == 0 || task.Data.SkillRequirements.Any(skill => villager.Skills.ContainsKey(skill.Name) && villager.Skills[skill.Name] >= skill.Level))
          .OrderBy(task => task.Priority)
          .ThenBy(task =>
          {
            var place = GameWorld.Places.FirstOrDefault(c => c.Id == task.PlaceId);
            return Vector2.Distance(villager.MapPoint.ToVector2(), place.Point.ToVector2());
          });

        var task = orderedTasks.FirstOrDefault();

        if (task == null)
          continue;

        villager.Tasks.Add(task);
        GameWorld.Tasks.Remove(task);
      }
    }

    /// <summary>
    /// Might not work
    /// </summary>
    /// <returns>Error (maybe?)</returns>
    /// (object)???
    public Dictionary<string, int> GetInventory()
    {
      var value = GameWorld.Places
        .Where(c => c.Data.Type == "Storage" && c.AdditionalProperties.ContainsKey("inventory"))
        .SelectMany(c => ((object)c.AdditionalProperties["inventory"].Value).ToDictionary<string, int>())
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

        foreach (var villager in GameWorld.Villagers)
        {
          villager.Load(GameWorld);
          State.AddVillagerEntity(villager);
        }

        foreach (var household in GameWorld.Households)
        {
          household.Load(this);
        }

        foreach (var place in GameWorld.Places)
        {
          place.LoadFromSave((Place)GameWorld.PlaceData[place.Name].Clone());
          if (place.HouseholdId > 0)
          {
            place.Household = GameWorld.Households.FirstOrDefault(c => c.Id == place.HouseholdId);
          }
          State.AddPlaceEntity(place);
        }
      }
      else
      {
        GameWorld = new GameWorld();
        LoadData();

        SetDefaultWorld();
      }

      _grm = new GatherableResourcesManager(this);
    }

    private void SetDefaultWorld()
    {
      var kyle = AddVillager("Kyle", new Point(0, 0), new Dictionary<string, float>() { { "mining", 1 }, { "chopping", 1 }, { "crafting", 1 }, { "gathering", 1 } });
      Building building = GetDefaultBuilding();
      var umneyHousehold = AddHousehold("Umney", building);
      umneyHousehold.AddPlace("singleBed", 1, 1);
      umneyHousehold.AddPlace("storageChest", 3, 1);
      umneyHousehold.AddPlace("craftingBench", 3, 3);

      umneyHousehold.AssignVillager(kyle);
      InstabuildHousehold(umneyHousehold);

      AddPlace("goldOre", 1, 1);
      AddPlace("rocks", 1, 2);
      AddPlace("sticks", 1, 3);
      AddPlace("normalTree", 3, 1);

      foreach (var place in GameWorld.Places.Where(c => c.Data.Skill == "mining"))
      {
        AddTask("miningGold", 1, place.Id);
      }
    }

    private Building GetDefaultBuilding()
    {
      var x = 5;
      var y = 3;
      var width = 5;
      var height = 5;
      var size = new Rectangle(x, y, width, height);

      var result = new Dictionary<Point, Place>();
      var points = new List<Point>();

      for (int newY = size.Y; newY < size.Bottom; newY++)
      {
        for (int newX = size.X; newX < size.Right; newX++)
        {
          var addWall = newY == size.Y ||
            newX == size.X ||
            newY == (size.Bottom - 1) ||
            newX == (size.Right - 1);

          if (!addWall)
            continue;

          points.Add(new Point(newX, newY));
        }
      }

      foreach (var point in points)
      {
        var wallType = GetWallType(point, points);

        var place = (Models.Place)GameWorld.PlaceData["woodenWall"].Clone();
        place.AdditionalProperties.Add("wallType", new AdditionalProperty() { IsVisible = false, Value = wallType });

        result.Add(point, place);
      }

      var building = new Building()
      {
        Rectangle = new Rectangle(x * Game1.TileSize, y * Game1.TileSize, width * Game1.TileSize, height * Game1.TileSize),
        Doors = new Dictionary<Point, Place>()
        {
          { new Point(x + (width/ 2) , (y + height) - 1), GameWorld.PlaceData["woodenDoor"] }
        },
        Walls = result,
      };
      return building;
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

      var placeFilesFiles = Directory.GetFiles("Data/", "placeTypes.json");

      foreach (var placeFile in placeFilesFiles)
      {
        using (var r = new StreamReader(placeFile))
        {
          string json = r.ReadToEnd();
          var places = JsonConvert.DeserializeObject<dynamic>(json);
          GameWorld.PlaceTypeDate = ((object)places["placeTypes"]).ToDictionary<string, PlaceType>();
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
            try
            {
              place.PlaceType = GameWorld.PlaceTypeDate[place.Type];
            }
            catch
            {
              throw new ApplicationException($"Place type '{place.Type}' hasn't been added to 'placeTypes.json'");
            }

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
