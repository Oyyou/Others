using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Others.Entities;
using Others.GUI.VillagerDetails;
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

    private SpriteFont _font;

    public bool ShowGrid { get; private set; } = false;

    public bool ShowCollisionBox { get; private set; } = false;

    public Point PreviousMousePosition { get; set; }

    public Point CurrentMousePosition { get; set; }

    public PathManager PathManager { get; set; }

    public Pathfinder Pathfinder { get; private set; }

    #region GUI Stuff
    public Panel VillagerDetails;
    public GUI.Panel Panel;
    #endregion

    public BattleState(GameModel gameModel)
      : base(gameModel)
    {
    }

    public override void LoadContent()
    {
      _font = _content.Load<SpriteFont>("Font");
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

      Pathfinder = new Pathfinder(_map);

      _gwm = new GameWorldManager(_map);
      _gwm.Load("save.json");

      var villagerTexture = _content.Load<Texture2D>($"Villager");

      foreach (var villager in _gwm.GameWorld.Villagers)
      {
        var villagerEntity = new Villager(villager, villagerTexture, this, _gwm) { Layer = 0.09f, PositionOffset = new Vector2(0, -Game1.TileSize) };
        _entities.Add(villagerEntity);
      }

      foreach (var place in _gwm.GameWorld.Places)
      {
        try
        {
          var texture = _content.Load<Texture2D>($"Places/{place.Name}");
          var xOffset = place.Data.XOriginPercentage != 0 ? (place.Data.XOriginPercentage / 100f) * texture.Width : 0;
          var yOffset = place.Data.YOriginPercentage != 0 ? (place.Data.YOriginPercentage / 100f) * texture.Height : 0;
          if (xOffset != 0)
          {

          }
          var placeEntity = new Place(place, texture, this) { Layer = 0.09f, PositionOffset = new Vector2(xOffset, yOffset), };
          _entities.Add(placeEntity);
        }
        catch (Exception e)
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

      #region GUI Stuff
      VillagerDetails = new Panel(_content);
      Panel = new GUI.Panel(_content);
      #endregion
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

      if (GameKeyboard.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.S))
        _gwm.Save("save.json");

      _gwm.Update();

      //PathManager.Update(gameTime);

      foreach (var entity in _entities)
        entity.Update(gameTime, _entities);

      Panel.Clear();
      for (int i = 0; i < _entities.Count; i++)
      {
        var entity = _entities[i];
        if (entity is Place place)
        {
          if (place.Wrapper.IsRemoved)
          {
            entity.IsRemoved = true;
          }
        }

        if (entity.IsRemoved)
        {
          var mappedComponent = entity.GetComponent<MappedComponent>();
          if (mappedComponent != null)
            _map.Remove(mappedComponent);

          _entities.RemoveAt(i);
          i--;
        }
        else
        {
          var selectedComponent = entity.GetComponent<SelectableComponent>();
          if (selectedComponent != null)
          {
            if (selectedComponent.IsSelected)
            {
              Panel.Clear();
              Panel.SetMainHeader("Test header");
              Panel.AddSection("Test subheader 1", "Test value: 1", "Test value: 1");
              Panel.AddSection("Test subheader 2", "Test value: 2", "Test value: 2");
            }
          }
        }
      }

      VillagerDetails.SetVillager(null);
      foreach (Villager villager in _entities.Where(c => c is Villager))
      {
        if (villager.IsSelected)
        {
          //VillagerDetails.SetVillager(villager.Wrapper);
        }
      }
    }

    public override void Draw(GameTime gameTime)
    {
      _spriteBatch.Begin(SpriteSortMode.FrontToBack);

      //((TextureComponent)_entities[10].Components[0]).Layer = 0.02f;

      foreach (var entity in _entities)
        entity.Draw(gameTime, _spriteBatch);

      //PathManager.Draw(gameTime, _spriteBatch);

      _spriteBatch.End();

      _spriteBatch.Begin();

      //_spriteBatch.DrawString(_font, _gwm.GameWorld)
      VillagerDetails.Draw(_spriteBatch, gameTime);
      Panel.Draw(_spriteBatch, gameTime);

      _spriteBatch.End();
    }
  }
}
