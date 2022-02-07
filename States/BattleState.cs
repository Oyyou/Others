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
    public enum States
    {
      Playing,
      Building,
    }

    private Viewport _viewport;

    private Matrix _camera = Matrix.CreateTranslation(0, 0, 0);

    public float _scale = 1f;

    public Map Map { get; private set; }

    private List<Entity> _entities = new List<Entity>();

    private GameWorldManager _gwm;

    public HouseBuildingManager _hbm;

    public bool ShowGrid { get; private set; } = false;

    public bool ShowCollisionBox { get; private set; } = false;

    public Point PreviousMousePosition { get; set; }

    public Point CurrentMousePosition { get; set; }

    public PathManager PathManager { get; set; }

    public Pathfinder Pathfinder { get; private set; }


    public States State { get; private set; } = States.Playing;

    #region Input Stuff
    private float _previousScroll;
    private float _currentScroll;
    #endregion

    #region GUI Stuff
    //public GUI.Panel Panel;
    //public GUI.CraftingDetails.Panel CraftingPanel;

    private List<Control> _controls;

    private List<Control> _buildingControls;
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

      _hbm = new HouseBuildingManager(GameModel, Map, _camera)
      {
        OnCancel = () => State = States.Playing,
        OnFinish = (Models.Building building) => _gwm.AddHousehold("", building),
      };

      PathManager = new PathManager(Map);
      PathManager.LoadContent(_content);

      #region GUI Stuff

      _controls = new List<Control>()
      {
        GetControlsPanel(),
        GetBuildingItemsPanel(),
      };

      _buildingControls = new List<Control>()
      {
        GetBuildingItemsPanel(),
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
      panel.AddTag("Playing");

      var buttonTexture = new Texture2D(GameModel.GraphicsDevice, 100, panelTexture.Height - 10);
      buttonTexture.SetData(Helpers.GetBorder(buttonTexture.Width, buttonTexture.Height, 2, Color.Black, Color.Gray));
      var font = _content.Load<SpriteFont>("Font");

      panel.AddChild(new Button(buttonTexture, font, "Build") { OnClicked = (self) => SetMainVisibility((Panel)panel.Children.FirstOrDefault(c => c.HasTag("Building"))) });
      panel.AddChild(new Button(buttonTexture, font, "Furniture") { OnClicked = (self) => SetMainVisibility((Panel)panel.Children.FirstOrDefault(c => c.HasTag("Furniture"))) });
      panel.AddChild(new Button(buttonTexture, font, "Craft") { OnClicked = (self) => SetMainVisibility((Panel)panel.Children.FirstOrDefault(c => c.HasTag("Crafting"))) });
      panel.AddChild(new Button(buttonTexture, font, "Tasks") { OnClicked = (self) => SetMainVisibility((Panel)panel.Children.FirstOrDefault(c => c.HasTag("Tasks"))) });

      var x = 5f;
      foreach (var control in panel.Children)
      {
        control.IsVisible = true;
        control.Position = new Vector2(x, 5);
        x += buttonTexture.Width + 5;
      }

      var subMenu = new List<Control>()
      {
        GetCraftingPanel(),
        GetBuildingPanel(),
      };

      foreach (var control in subMenu)
      {
        control.Position = new Vector2(0, -control.Rectangle.Height);
        control.IsVisible = false;
        panel.AddChild(control);
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
        foreach (var child in children)
        {
          if (child.Rectangle.X < rectangle.X)
            rectangle = new Rectangle(child.Rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

          if (child.Rectangle.Y < rectangle.Y)
            rectangle = new Rectangle(rectangle.X, child.Rectangle.Y, rectangle.Width, rectangle.Height);

          if (child.Rectangle.Right > rectangle.Right)
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

      //var craftingLocations = _gwm.GameWorld.Places.Where(c => c.Data.Type == "Crafting");
      panel.AddChild(new Label(font, "Crafting") { Position = new Vector2(10, 20) });
      panel.AddChild(new ScrollBar(GameModel.GraphicsDevice, font, panelTexture.Height - 10) { Position = new Vector2(panelTexture.Width - 25, 5), IsFixedPosition = true });
      panel.AddChild(new Button(buttonTexture, font, "Craft Hatchet") { Position = new Vector2(10, 40), OnClicked = (self) => _gwm.AddTask("craftAxe", 0, _gwm.GameWorld.Places.Where(c => c.Data.Type == "Crafting").FirstOrDefault().Id) });
      panel.AddChild(new Button(buttonTexture, font, "Craft Pickaxe") { Position = new Vector2(10, 90) });
      panel.AddTag("Main");
      panel.AddTag("Crafting");

      return panel;
    }

    private Control GetFurniturePanel()
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
        foreach (var child in children)
        {
          if (child.Rectangle.X < rectangle.X)
            rectangle = new Rectangle(child.Rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

          if (child.Rectangle.Y < rectangle.Y)
            rectangle = new Rectangle(rectangle.X, child.Rectangle.Y, rectangle.Width, rectangle.Height);

          if (child.Rectangle.Right > rectangle.Right)
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
      panel.AddChild(new Label(font, "Furniture") { Position = new Vector2(10, 20) });
      panel.AddChild(new ScrollBar(GameModel.GraphicsDevice, font, panelTexture.Height - 10) { Position = new Vector2(panelTexture.Width - 25, 5), IsFixedPosition = true });
      panel.AddChild(new Button(buttonTexture, font, "Craft Hatchet") { Position = new Vector2(10, 40) });
      panel.AddChild(new Button(buttonTexture, font, "Craft Pickaxe") { Position = new Vector2(10, 90) });
      panel.AddTag("Main");
      panel.AddTag("Crafting");

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
      panel.AddChild(new Button(buttonTexture, font, "Build House") { Position = new Vector2(10, 40), OnClicked = (self) => { State = States.Building; _hbm.Start(); } });
      panel.AddTag("Main");
      panel.AddTag("Building");

      return panel;
    }

    /// <summary>
    /// The panel that shows up once we're in building mode
    /// </summary>
    /// <returns></returns>
    private Control GetBuildingItemsPanel()
    {
      var panelTexture = new Texture2D(GameModel.GraphicsDevice, 300, 150);
      panelTexture.SetData(Helpers.GetBorder(panelTexture.Width, panelTexture.Height, 2, Color.Black, Color.White));

      var panel = new Panel(panelTexture, new Vector2(0, ZonerGame.ScreenHeight - panelTexture.Height));
      panel.AddTag("Building");
      panel.IsVisible = false;

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
        foreach (var p in panel.Parent.Children.Where(c => c.HasTag("Main")))
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
        string textureName = $"Places/{place.Name}";

        // Special wall logic that requires us to know what part of the wall we're in
        // UUUUUUGH

        if (place.Data.Type == "Wall")
        {
          textureName = $"Places/Walls/{place.AdditionalProperties["wallType"].Value}";
        }

        var texture = _content.Load<Texture2D>(textureName);
        var xOffset = place.Data.XOriginPercentage != 0 ? (place.Data.XOriginPercentage / 100f) * texture.Width : 0;
        var yOffset = place.Data.YOriginPercentage != 0 ? (place.Data.YOriginPercentage / 100f) * texture.Height : 0;
        var placeEntity = new Place(place, texture, this) { Layer = place.Data.Layer >= 0 ? place.Data.Layer : 0.09f, PositionOffset = new Vector2(xOffset, yOffset), Colour = place.Data.Tint };

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
      GameMouse.AddCamera(_camera);
      _previousScroll = _currentScroll;
      _currentScroll = GameMouse.ScrollWheelValue;

      if (_currentScroll < _previousScroll)
      {
        _scale -= 0.01f;
      }
      else if (_currentScroll > _previousScroll)
      {
        _scale += 0.01f;

      }

      _scale = MathHelper.Clamp(_scale, 0.5f, 1f);
      _camera = Matrix.CreateTranslation(0, 0, 0) * Matrix.CreateScale(_scale);

      if (GameKeyboard.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.G))
        ShowGrid = !ShowGrid;

      //if (GameKeyboard.IsKeyPressed(Keys.M))
      //  Map.WriteMap();

      //if (GameKeyboard.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.C))
      //  ShowCollisionBox = !ShowCollisionBox;

      if (GameKeyboard.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.S))
        _gwm.Save("save.json");

      foreach (var control in _controls)
      {
        if (!control.HasTag(State.ToString()))
          continue;

        control.Update(gameTime);
      }

      _gwm.Update(gameTime);
      _hbm.Update(gameTime);

      foreach (var entity in _entities)
        entity.Update(gameTime);

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
      }
    }

    public override void Draw(GameTime gameTime)
    {
      GameModel.GraphicsDevice.Viewport = _viewport;
      _spriteBatch.Begin(SpriteSortMode.FrontToBack, transformMatrix: _camera);

      foreach (var entity in _entities)
        entity.Draw(gameTime, _spriteBatch);

      _spriteBatch.End();

      _hbm.Draw(gameTime, _spriteBatch, _camera);

      DrawGUI(gameTime);
    }

    private void DrawGUI(GameTime gameTime)
    {
      foreach (var control in _controls.Where(c => c.HasViewport))
      {
        if (!control.HasTag(State.ToString()))
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
        if (!control.HasTag(State.ToString()))
          continue;

        GameModel.GraphicsDevice.Viewport = _viewport;
        control.Draw(gameTime, _spriteBatch);
      }

      _spriteBatch.End();
    }
  }
}
