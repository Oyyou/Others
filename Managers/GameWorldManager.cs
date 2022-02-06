using Microsoft.Xna.Framework;
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
    private BattleState _state;

    private GatherableResourcesManager _grm;

    public static Random Random = new Random();

    public static Dictionary<string, string> Statics = new Dictionary<string, string>();

    public GameWorld GameWorld { get; set; }

    public readonly Pathfinder Pathfinder;

    public GameWorldManager(BattleState state)
    {
      _state = state;
      Pathfinder = new Pathfinder(_state.Map);
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

    public PlaceWrapper AddPlace(string placeName, int x, int y, Dictionary<string, string> additionalValues = null)
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

      //if (additionalValues != null)
        //place.AdditionalProperties = place.AdditionalProperties.Add("wallType", new AdditionalProperty().Value);

      GameWorld.Places.Add(place);

      _state.AddPlaceEntity(place, additionalValues);

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
      var emptyPoints = _state.Map.GetEmptyPoints().ToList();

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

    public Household AddHousehold(string name, Rectangle size)
    {
      var place = AddPlace("house", new Point(size.X, size.Y));
      place.Width = size.Width;
      place.Height = size.Height;

      var household = new Household()
      {
        Id = GetId("household"),
        Name = name,
      };

      var points = new List<Point>();

      for (int y = size.Y; y < size.Bottom; y++)
      {
        for (int x = size.X; x < size.Right; x++)
        {
          var addWall = y == size.Y ||
            x == size.X ||
            y == (size.Bottom - 1) ||
            x == (size.Right - 1);

          var addDoor = y == (size.Bottom - 1) && (x == size.X + (size.Width / 2));

          if (addDoor)
            AddPlace("woodenDoor", x, y);
          else if (addWall)
            points.Add(new Point(x, y));
          else
            AddPlace("woodenFloor", x, y);
        }
      }

      foreach (var wall in points)
      {
        string wallType = GetWallType(wall, points);

        AddPlace("woodenWall", wall.X, wall.Y, new Dictionary<string, string>() { { "wallType", wallType } });
      }

      GameWorld.Households.Add(household);

      return household;
    }

    public Household AddHousehold(string name, Building building)
    {
      var household = new Household()
      {
        Id = GetId("household"),
        Name = name,
      };

      var size = building.Rectangle.Divide(Game1.TileSize);

      for (int y = size.Y; y < size.Bottom; y++)
      {
        for (int x = size.X; x < size.Right; x++)
        {
          var point = new Point(x, y);

          if (building.Doors.ContainsKey(point))
            AddPlace("woodenDoor", x, y);
          else if (building.Walls.ContainsKey(point))
            AddPlace("woodenWall", x, y, new Dictionary<string, string>() { { "wallType", Path.GetFileName(building.Walls[point].TextureName) } });
          else
            AddPlace("woodenFloor", x, y);
        }
      }

      GameWorld.Households.Add(household);
      return household;
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

        AddPlace("woodenWall", wall.X + startX, wall.Y + startY, new Dictionary<string, string>() { { "wallType", wallType } });
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
      _state.AddVillagerEntity(villager);

      return villager;
    }

    public void Update(GameTime gameTime)
    {
      AssignTasks();

      UpdateVillager();

      UpdatePlaces();

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

        foreach (var place in GameWorld.Places)
        {
          place.LoadFromSave((Place)GameWorld.PlaceData[place.Name].Clone());
          _state.AddPlaceEntity(place);
        }

        foreach (var villager in GameWorld.Villagers)
        {
          villager.Load(GameWorld);
          _state.AddVillagerEntity(villager);
        }

        foreach (var household in GameWorld.Households)
        {
          household.Load(this);
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

      var umneyHousehold = AddHousehold("Umney", new Rectangle(5, 3, 5, 5));
      AddPlace("storageChest", 8, 4);
      AddPlace("singleBed", 6, 4);
      umneyHousehold.AssignVillager(kyle);
      //AddVillager("Niall", new Dictionary<string, float>() { { "chopping", 1 } });

      //TestWalls(12, 2);

      AddPlace("goldOre", 1, 1);
      //AddPlace("goldOre", 2, 1);
      //AddPlace("goldOre", 4, 4);
      //AddPlace("goldOre", 5, 3);
      //AddPlace("goldOre", 5, 1);
      //AddPlace("goldOre", 5, 6);
      //AddPlace("goldOre", 7, 2);
      //AddPlace("goldOre", 10, 4);
      //AddPlace("goldOre", 5, 8);
      //AddPlace("goldOre", 1, 4);
      //AddPlace("goldOre", 2, 2);

      AddPlace("craftingBench", 3, 5);

      AddPlace("rocks", 1, 2);

      AddPlace("sticks", 1, 3);

      AddPlace("normalTree", 3, 1);

      foreach (var place in GameWorld.Places.Where(c => c.Data.Skill == "mining"))
      {
        AddTask("miningGold", 1, place.Id);
      }

      //foreach (var place in GameWorld.Places.Where(c => c.Place.Skill == "chopping"))
      //{
      //  AddTask("choppingNormalTree", 1, place.Id);
      //}
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
