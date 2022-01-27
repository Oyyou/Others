using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Others.Controls;
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
    public GUI.CraftingDetails.Panel CraftingPanel;

    private List<Control> _controls;
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
      CraftingPanel = new GUI.CraftingDetails.Panel(_content);

      _controls = new List<Control>()
      {
        GetCraftingPanel(),
      };
      #endregion
    }

    private Control GetCraftingPanel()
    {
      var panelTexture = _content.Load<Texture2D>("GUI/Panel");
      var font = _content.Load<SpriteFont>("Font");
      var buttonTexture = _content.Load<Texture2D>("GUI/Button");

      var panel = new Panel(panelTexture, new Vector2(0, ZonerGame.ScreenHeight - panelTexture.Height));
      panel.AddChild(new Label(font, "Crafting") { Position = new Vector2(10, 20) });
      panel.AddChild(new Button(buttonTexture, font, "Craft Hatchet") { Position = new Vector2(10, 40) });
      panel.AddChild(new Button(buttonTexture, font, "Craft Pickaxe") { Position = new Vector2(10, 90) });

      panel.GetVisibility = () =>
      {
        if (GameKeyboard.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.C))
          panel.IsVisible = !panel.IsVisible;

        return panel.IsVisible;
      };

      return panel;
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

      //if (GameKeyboard.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.C))
      //  ShowCollisionBox = !ShowCollisionBox;

      if (GameKeyboard.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.S))
        _gwm.Save("save.json");

      _gwm.Update(gameTime);

      foreach (var control in _controls)
      {
        // TODO: Sort out this mess
        if (control.GetVisibility != null)
          control.IsVisible = control.GetVisibility();

        if (!control.IsVisible)
          continue;

        control.Update(gameTime);
      }

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
              if (selectedComponent.Information != null)
              {
                Panel.SetMainHeader(selectedComponent.Information.Header);
                foreach (var info in selectedComponent.Information.Sections)
                {
                  Panel.AddSection(info.Header, info.Values);
                }
              }
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

      _spriteBatch.Begin(SpriteSortMode.FrontToBack);

      foreach (var control in _controls)
      {
        if (!control.IsVisible)
          continue;

        control.Draw(gameTime, _spriteBatch);
      }

      Panel.Draw(_spriteBatch, gameTime);

      _spriteBatch.End();
    }
  }
}
