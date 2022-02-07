using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using ZonerEngine.GL.Components;
using ZonerEngine.GL.Entities;
using ZonerEngine.GL.Input;
using ZonerEngine.GL.Maps;

namespace Others.Managers
{
  public class PathManager
  {
    private Map _map;

    /// <summary>
    /// The area around the moving sprite
    /// </summary>
    private List<Entity> _radiusEntities;

    /// <summary>
    /// The line from A to B
    /// </summary>
    private List<Entity> _pathEntities = new List<Entity>();

    /// <summary>
    /// The entity on the hovered tile
    /// </summary>
    private Entity _tileCursor;

    private Dictionary<string, Entity> _pathPrefabs;

    public Point PreviousMousePosition { get; set; }

    public Point CurrentMousePosition { get; set; }

    public PathManager(Map map)
    {
      _map = map;
    }


    public void LoadContent(ContentManager content)
    {
      var ld = new Entity();
      var lr = new Entity();
      var lu = new Entity();
      var rd = new Entity();
      var ru = new Entity();
      var ud = new Entity();

      ld.AddComponent(new TextureComponent(ld, content.Load<Texture2D>("Path/LD")) { Layer = 0.94f, });
      lr.AddComponent(new TextureComponent(lr, content.Load<Texture2D>("Path/LR")) { Layer = 0.94f, });
      lu.AddComponent(new TextureComponent(lu, content.Load<Texture2D>("Path/LU")) { Layer = 0.94f, });
      rd.AddComponent(new TextureComponent(rd, content.Load<Texture2D>("Path/RD")) { Layer = 0.94f, });
      ru.AddComponent(new TextureComponent(ru, content.Load<Texture2D>("Path/RU")) { Layer = 0.94f, });
      ud.AddComponent(new TextureComponent(ud, content.Load<Texture2D>("Path/UD")) { Layer = 0.94f, });

      _pathPrefabs = new Dictionary<string, Entity>()
      {
        { "LD", ld },
        { "LR", lr },
        { "LU", lu },
        { "RD", rd },
        { "RU", ru },
        { "UD", ud },
      };

      foreach (var prefab in _pathPrefabs)
        prefab.Value.LoadContent();
    }

    private Entity GetPathEntity(string key, Vector2 position)
    {
      var result = (Entity)_pathPrefabs[key].Clone();
      result.Position = position;

      return result;
    }

    private string GetPathEntityKey(Point previousPoint, Point currentPoint, Point nextPoint)
    {
      string result = "";

      // If this is the last point
      if (nextPoint == Point.Zero)
      {
        if (previousPoint.Y != currentPoint.Y)
          result = "UD";
        else
          result = "LR";
      }
      else
      {
        if (previousPoint.Y < currentPoint.Y)
        {
          if (nextPoint.Y > currentPoint.Y)
            result = "UD";
          else if (nextPoint.X < currentPoint.X)
            result = "LU";
          else if (nextPoint.X > currentPoint.X)
            result = "RU";
          else
            throw new Exception("wut");
        }
        else if (previousPoint.Y > currentPoint.Y)
        {
          if (nextPoint.Y < currentPoint.Y)
            result = "UD";
          else if (nextPoint.X < currentPoint.X)
            result = "LD";
          else if (nextPoint.X > currentPoint.X)
            result = "RD";
          else
            throw new Exception("wut");
        }
        else if (previousPoint.X < currentPoint.X)
        {
          if (nextPoint.X > currentPoint.X)
            result = "LR";
          else if (nextPoint.Y < currentPoint.Y)
            result = "LU";
          else if (nextPoint.Y > currentPoint.Y)
            result = "LD";
          else
            throw new Exception("wut");
        }
        else if (previousPoint.X > currentPoint.X)
        {
          if (nextPoint.X < currentPoint.X)
            result = "LR";
          else if (nextPoint.Y < currentPoint.Y)
            result = "RU";
          else if (nextPoint.Y > currentPoint.Y)
            result = "RD";
          else
            throw new Exception("wut");
        }
      }

      return result;
    }

    public void Update(GameTime gameTime)
    {
      PreviousMousePosition = CurrentMousePosition;
      CurrentMousePosition = new Point((int)Math.Floor(GameMouse.PositionWithCamera.X / (double)Game1.TileSize), (int)Math.Floor(GameMouse.PositionWithCamera.Y / (double)Game1.TileSize));

      if (PreviousMousePosition != CurrentMousePosition)
      {
        _pathEntities = new List<Entity>();

        var pf = new Pathfinder(_map);
        var results = pf.GetPath(new Point(0, 0), CurrentMousePosition);


        for (int i = 0; i < results.Count; i++)
        {
          var previousPoint = i > 0 ? results[i - 1] : Point.Zero;
          var currentPoint = results[i];
          var nextPoint = i < results.Count - 1 ? results[i + 1] : Point.Zero;
          _pathEntities.Add(GetPathEntity(GetPathEntityKey(previousPoint, currentPoint, nextPoint), currentPoint.ToVector2() * Game1.TileSize));
        }
      }
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
      foreach (var entity in _pathEntities)
        entity.Draw(gameTime, spriteBatch);
    }
  }
}
