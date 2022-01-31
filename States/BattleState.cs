using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
    private Viewport _viewport;

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
      _viewport = GameModel.GraphicsDevice.Viewport;

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
        GetControlsPanel(),
        GetCraftingPanel(),
        GetBuildingPanel(),
      };
      #endregion
    }

    /// <summary>
    /// The panel at the bottom of the screen
    /// </summary>
    /// <returns></returns>
    private Control GetControlsPanel()
    {
      var panelTexture = new Texture2D(GameModel.GraphicsDevice, ZonerGame.ScreenWidth, 100);
      panelTexture.SetData(Helpers.GetBorder(panelTexture.Width, panelTexture.Height, 2, Color.Black, Color.White));

      var panel = new Panel(panelTexture, new Vector2(0, ZonerGame.ScreenHeight - panelTexture.Height));
      panel.IsVisible = true;

      var buttonTexture = new Texture2D(GameModel.GraphicsDevice, 100, panelTexture.Height - 10);
      buttonTexture.SetData(Helpers.GetBorder(buttonTexture.Width, buttonTexture.Height, 2, Color.Black, Color.Gray));
      var font = _content.Load<SpriteFont>("Font");

      panel.AddChild(new Button(buttonTexture, font, "Build") { OnClicked = () => SetMainVisibility((Panel)_controls.FirstOrDefault(c => c.HasTag("Building"))) });
      panel.AddChild(new Button(buttonTexture, font, "Craft") { OnClicked = () => SetMainVisibility((Panel)_controls.FirstOrDefault(c => c.HasTag("Crafting"))) });
      panel.AddChild(new Button(buttonTexture, font, "Tasks") { OnClicked = () => SetMainVisibility((Panel)_controls.FirstOrDefault(c => c.HasTag("Tasks"))) });

      var x = 5f;
      foreach (var control in panel.Children)
      {
        control.Position = new Vector2(x, 5);
        x += buttonTexture.Width + 5;
      }

      return panel;
    }

    private Control GetCraftingPanel()
    {
      var panelTexture = new Texture2D(GameModel.GraphicsDevice, 300, 150);
      panelTexture.SetData(Helpers.GetBorder(panelTexture.Width, panelTexture.Height, 2, Color.Black, Color.White));
      var font = _content.Load<SpriteFont>("Font");
      var buttonTexture = _content.Load<Texture2D>("GUI/Button");

      var panel = new Panel(panelTexture, new Vector2(0, 0));
      panel.Viewport = new Rectangle(0, (ZonerGame.ScreenHeight - 100) - panelTexture.Height, panelTexture.Width, panelTexture.Height);
      panel.OnAddChild = (panel) =>
      {
        var children = panel.Children.Where(c => !c.IsFixedPosition);
        if (children.Count() == 0)
          return;

        var rectangle = new Rectangle(0, 0, 10, 10);
        foreach(var child in children)
        {
          if (child.Rectangle.X < rectangle.X)
            rectangle = new Rectangle(child.Rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

          if (child.Rectangle.Y < rectangle.Y)
            rectangle = new Rectangle(rectangle.X, child.Rectangle.Y, rectangle.Width, rectangle.Height);

          if(child.Rectangle.Right > rectangle.Right)
            rectangle = new Rectangle(rectangle.X, rectangle.Y, child.Rectangle.Right - rectangle.X, rectangle.Height);


          if (child.Rectangle.Bottom > rectangle.Bottom)
            rectangle = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, child.Rectangle.Bottom - rectangle.Y);
        }

        rectangle.Height += 10; // Padding :)

        var scrollBar = (ScrollBar)panel.Children.Where(c => c is ScrollBar).FirstOrDefault();
        if (scrollBar != null)
        {
          scrollBar.SetRectangle(rectangle);
        }
      };
      panel.AddChild(new Label(font, "Crafting") { Position = new Vector2(10, 20) });
      panel.AddChild(new ScrollBar(GameModel.GraphicsDevice, font, panelTexture.Height - 10) { Position = new Vector2(panelTexture.Width - 25, 5), IsFixedPosition = true });
      panel.AddChild(new Button(buttonTexture, font, "Craft Hatchet") { Position = new Vector2(10, 40) });
      panel.AddChild(new Button(buttonTexture, font, "Craft Pickaxe") { Position = new Vector2(10, 90) });
      panel.AddChild(new Button(buttonTexture, font, "Craft Pickaxe") { Position = new Vector2(10, 140) });
      panel.AddChild(new Button(buttonTexture, font, "Craft Pickaxe") { Position = new Vector2(10, 190) });
      panel.AddChild(new Button(buttonTexture, font, "Craft Pickaxe") { Position = new Vector2(10, 240) });
      panel.AddChild(new Button(buttonTexture, font, "Craft Pickaxe") { Position = new Vector2(10, 290) });
      panel.AddChild(new Button(buttonTexture, font, "Craft Pickaxe") { Position = new Vector2(10, 340) });
      panel.AddTag("Main");
      panel.AddTag("Crafting");

      panel.GetVisibility = () => GetVisibility(panel, Keys.C);

      return panel;
    }

    private Control GetBuildingPanel()
    {
      var panelTexture = new Texture2D(GameModel.GraphicsDevice, 300, 150);
      panelTexture.SetData(Helpers.GetBorder(panelTexture.Width, panelTexture.Height, 2, Color.Black, Color.White));
      var font = _content.Load<SpriteFont>("Font");
      var buttonTexture = _content.Load<Texture2D>("GUI/Button");

      var panel = new Panel(panelTexture, new Vector2(0, (ZonerGame.ScreenHeight - 100) - panelTexture.Height));
      panel.AddChild(new Label(font, "Building") { Position = new Vector2(10, 20) });
      panel.AddChild(new Button(buttonTexture, font, "Build House") { Position = new Vector2(10, 40) });
      panel.AddTag("Main");
      panel.AddTag("Building");

      panel.GetVisibility = () => GetVisibility(panel, Keys.B);

      return panel;
    }

    private bool GetVisibility(Panel panel, Keys key)
    {
      if (GameKeyboard.IsKeyPressed(key))
      {
        SetMainVisibility(panel);
      }

      return panel.IsVisible;
    }

    private void SetMainVisibility(Panel panel)
    {
      if (panel == null)
        return;

      panel.IsVisible = !panel.IsVisible;

      if (panel.IsVisible)
      {
        foreach (var p in _controls.Where(c => c.HasTag("Main")))
        {
          p.IsVisible = false;
        }

        panel.IsVisible = true;
      }
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
      GameModel.GraphicsDevice.Viewport = _viewport;
      _spriteBatch.Begin(SpriteSortMode.FrontToBack);

      foreach (var entity in _entities)
        entity.Draw(gameTime, _spriteBatch);

      _spriteBatch.End();


      foreach (var control in _controls.Where(c => c.HasViewport))
      {
        if (!control.IsVisible)
          continue;

        //_spriteBatch.Begin(SpriteSortMode.FrontToBack, transformMatrix: control.ViewMatrix);
        _spriteBatch.Begin(SpriteSortMode.FrontToBack);

        GameModel.GraphicsDevice.Viewport = new Viewport(control.Viewport);
        control.Draw(gameTime, _spriteBatch);
        _spriteBatch.End();
      }

      _spriteBatch.Begin(SpriteSortMode.FrontToBack);
      foreach (var control in _controls.Where(c => !c.HasViewport))
      {
        if (!control.IsVisible)
          continue;

        GameModel.GraphicsDevice.Viewport = _viewport;
        control.Draw(gameTime, _spriteBatch);
      }

      Panel.Draw(_spriteBatch, gameTime);

      _spriteBatch.End();
    }
  }
}
