using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Others.Entities;
using Others.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZonerEngine.GL;
using ZonerEngine.GL.Components;
using ZonerEngine.GL.Entities;
using ZonerEngine.GL.Input;
using ZonerEngine.GL.Maps;
using ZonerEngine.GL.Models;
using ZonerEngine.GL.States;

namespace Others.States
{
  public class BattleState : State
  {
    private Map _map;

    private List<Entity> _entities = new List<Entity>();

    private GameWorldManager _gwm;

    public bool ShowGrid { get; private set; } = false;

    public bool ShowCollisionBox { get; private set; } = false;

    public Point PreviousMousePosition { get; set; }

    public Point CurrentMousePosition { get; set; }

    public PathManager PathManager { get; set; }

    public BattleState(GameModel gameModel)
      : base(gameModel)
    {
    }

    public override void LoadContent()
    {
      _gwm = new GameWorldManager();
      _gwm.Load("save.json");

      //var mapData = File.ReadAllLines("Maps/Map_001.txt").Select(c => c.ToArray()).ToList();

      _map = new Map(40, 40, '0');

      var tileTexture = _content.Load<Texture2D>("Tiles/Floor");
      var crateTexture = _content.Load<Texture2D>("Cover/Crate");
      for (int y = 0; y < _map.Height; y++)
      {
        for (int x = 0; x < _map.Width; x++)
        {
          var value = _map.Data[y, x];

         // _entities.Add(new Tile(x, y, tileTexture, this));
        }
      }

      var villagerTexture = _content.Load<Texture2D>($"Villager");

      foreach (var villager in _gwm.GameWorld.Villagers)
      {
        var villagerEntity = new Villager(villager, villagerTexture, this) { PositionOffset = new Vector2(0, -Game1.TileSize) };
        _entities.Add(villagerEntity);
      }

      foreach (var place in _gwm.GameWorld.Places)
      {
        try
        {
          var texture = _content.Load<Texture2D>($"Places/{place.Name}");
          var xOffset = place.XOriginPercentage != 0 ? (place.XOriginPercentage / 100f) * texture.Width : 0;
          var yOffset = place.YOriginPercentage != 0 ? (place.YOriginPercentage / 100f) * texture.Height : 0;
          if(xOffset != 0)
          {

          }
          var placeEntity = new Place(place, texture, this) { Layer = 0.09f, PositionOffset = new Vector2(xOffset, yOffset), };
          _entities.Add(placeEntity);
        }
        catch(Exception e)
        {

        }
      }


      _map.WriteMap();
      foreach (var entity in _entities)
      {
        entity.LoadContent();

        var mappedComponent = entity.GetComponent<MappedComponent>();
        if (mappedComponent != null)
          _map.Add(mappedComponent);
      }
      _map.WriteMap();

      PathManager = new PathManager(_map);
      PathManager.LoadContent(_content);
    }

    public override void UnloadContent()
    {
      foreach (var entity in _entities)
        entity.Unload();
    }

    public override void Update(GameTime gameTime)
    {
      if (GameKeyboard.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.G))
        ShowGrid = !ShowGrid;

      if (GameKeyboard.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.C))
        ShowCollisionBox = !ShowCollisionBox;

      _gwm.Update();

      /*PreviousMousePosition = CurrentMousePosition;
      CurrentMousePosition = new Point((int)Math.Floor(GameMouse.Position.X / (double)Game1.TileSize), (int)Math.Floor(GameMouse.Position.Y / (double)Game1.TileSize));

      if (PreviousMousePosition != CurrentMousePosition)
      {
        Console.WriteLine(CurrentMousePosition);
        foreach (var path in _entities.Where(c => c is Path))
        {
          path.IsRemoved = true;
        }

        var pf = new Pathfinder(_map);
        var results = pf.GetPath(new Point(0, 0), CurrentMousePosition);

        foreach (var result in results)
        {
          var value = new Path(result.X, result.Y, _content.Load<Texture2D>("Path/Last_Outer"));
          value.LoadContent();
          _entities.Add(value);
        }
      }*/

      PathManager.Update(gameTime);

      foreach (var entity in _entities)
        entity.Update(gameTime, _entities);

      for (int i = 0; i < _entities.Count; i++)
      {
        if (_entities[i].IsRemoved)
        {
          _entities.RemoveAt(i);
          i--;
        }
      }
    }

    public override void Draw(GameTime gameTime)
    {
      _spriteBatch.Begin(SpriteSortMode.FrontToBack);

      //((TextureComponent)_entities[10].Components[0]).Layer = 0.02f;

      foreach (var entity in _entities)
        entity.Draw(gameTime, _spriteBatch);

      PathManager.Draw(gameTime, _spriteBatch);

      _spriteBatch.End();
    }
  }
}
