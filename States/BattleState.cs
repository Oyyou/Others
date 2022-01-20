using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
    public Map Map { get; private set; }

    private List<Entity> _entities = new List<Entity>();

    private GameWorldManager _gwm;

    public bool ShowGrid { get; private set; } = false;

    public bool ShowCollisionBox { get; private set; } = false;

    public Point PreviousMousePosition { get; set; }

    public Point CurrentMousePosition { get; set; }

    public PathManager PathManager { get; set; }

    public Pathfinder Pathfinder { get; private set; }

    #region GUI Stuff
    public GUI.Panel Panel;
    public GUI.CraftingDetails.Panel craftingPanel;
    #endregion

    public BattleState(GameModel gameModel)
      : base(gameModel)
    {
    }

    public override void LoadContent()
    {
      Map = new Map(1280 / 40, 800 / 40, '0');

      Pathfinder = new Pathfinder(Map);

      _gwm = new GameWorldManager(this);
      _gwm.Load("save.json");

      PathManager = new PathManager(Map);
      PathManager.LoadContent(_content);

      #region GUI Stuff
      Panel = new GUI.Panel(_content);
      craftingPanel = new GUI.CraftingDetails.Panel(_content);
      #endregion
    }

    private void AddEntity(Entity entity)
    {
      entity.LoadContent();

      var mappedComponent = entity.GetComponent<MappedComponent>();
      if (mappedComponent != null)
        Map.Add(mappedComponent);

      _entities.Add(entity);
    }

    public Entity AddPlaceEntity(Models.PlaceWrapper place)
    {
      try
      {
        var texture = _content.Load<Texture2D>($"Places/{place.Name}");
        var xOffset = place.Data.XOriginPercentage != 0 ? (place.Data.XOriginPercentage / 100f) * texture.Width : 0;
        var yOffset = place.Data.YOriginPercentage != 0 ? (place.Data.YOriginPercentage / 100f) * texture.Height : 0;
        var placeEntity = new Place(place, texture, this) { Layer = 0.09f, PositionOffset = new Vector2(xOffset, yOffset), };

        AddEntity(placeEntity);

        return placeEntity;
      }
      catch (ContentLoadException e)
      {
        // Place doesn't exist. lol
      }
      catch (Exception e)
      {

      }

      return null;
    }

    public Entity AddVillagerEntity(Models.Villager villager)
    {
      var villagerTexture = _content.Load<Texture2D>($"Villager");
      var villagerEntity = new Villager(villager, villagerTexture, this, _gwm) { Layer = 0.09f, PositionOffset = new Vector2(0, -Game1.TileSize) };
      AddEntity(villagerEntity);

      return villagerEntity;
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

      _gwm.Update(gameTime);

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
            Map.Remove(mappedComponent);

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

      Panel.Draw(_spriteBatch, gameTime);

      _spriteBatch.End();
    }
  }
}
