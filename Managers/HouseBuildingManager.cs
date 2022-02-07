using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Others.Controls;
using Others.Entities;
using Others.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZonerEngine.GL;
using ZonerEngine.GL.Entities;
using ZonerEngine.GL.Input;
using ZonerEngine.GL.Maps;
using ZonerEngine.GL.Models;

namespace Others.Managers
{
  public class HouseBuildingManager
  {
    public enum States
    {
      LayingFoundation,
      AddingInternalWalls,
      AddingDoors,
      AddingFurniture
    }

    private GameModel _gameModel;
    private GameWorldManager _gwm;
    private Map _map;
    private Matrix _camera;

    private Texture2D _texture;
    private SpriteFont _font;
    private List<Entity> _entities = new List<Entity>();
    private Entity _cursorEntity;

    #region Walls and stuff
    private Building _building;
    private List<Wall> _walls;
    private List<Basic> _doors;
    #endregion

    private bool _isVisible = false;

    private Vector2? _startPosition;

    private Rectangle _currentRectangle;
    private Rectangle _previousRectangle;

    #region GUI
    private List<Control> _controls = new List<Control>();

    private Panel _informationPanel;

    private Label _title;
    private Button _prevButton;
    private Button _nextButton;
    #endregion

    public Action OnCancel;

    public Action<Building> OnFinish;

    public States State { get; private set; } = States.LayingFoundation;

    public HouseBuildingManager(GameModel gameModel, GameWorldManager gwm, Map map, Matrix camera)
    {
      _gameModel = gameModel;
      _gwm = gwm;
      _map = map;
      _camera = camera;
      _texture = _gameModel.Content.Load<Texture2D>("GUI/Drawer");
      _font = gameModel.Content.Load<SpriteFont>("Font");

      _building = new Building();

      InitializeGUI(gameModel);
    }

    private void InitializeGUI(GameModel gameModel)
    {
      var infoTexture = new Texture2D(gameModel.GraphicsDevice, 250, 400);
      infoTexture.SetData(Helpers.GetBorder(infoTexture.Width, infoTexture.Height, 2, Color.Black, Color.White));

      _informationPanel = new Panel(infoTexture, new Vector2(0, ZonerGame.ScreenHeight - infoTexture.Height));

      _title = new Label(_font, "Laying foundation");
      UpdateTitle();

      var buttonTexture = new Texture2D(gameModel.GraphicsDevice, 120, 30);
      buttonTexture.SetData(Helpers.GetBorder(buttonTexture.Width, buttonTexture.Height, 2, Color.Black, Color.White));

      _prevButton = new Button(buttonTexture, _font, "Cancel")
      {
        OnClicked = (self) =>
        {
          switch (State)
          {
            case States.LayingFoundation:
              Cancel();
              break;
            case States.AddingInternalWalls:
              State = States.LayingFoundation;
              _prevButton.SetText("Cancel");
              _nextButton.SetText("Next");
              break;
            case States.AddingDoors:
              State = States.AddingInternalWalls;
              _nextButton.SetText("Next");
              foreach (var wall in _building.Walls)
                wall.Value.AdditionalProperties["hasDoor"] = new AdditionalProperty() { IsVisible = false, Value = false };

              UpdateWallTextures();
              break;
            case States.AddingFurniture:
              State = States.AddingDoors;
              _nextButton.SetText("Next");
              break;
            default:
              break;
          }
        },
      };
      _nextButton = new Button(buttonTexture, _font, "Next")
      {
        OnClicked = (self) =>
        {
          switch (State)
          {
            case States.LayingFoundation:
              if (_entities.Count > 0 && _entities.All(c => ((DrawingSquare)c).State == DrawingSquare.States.Fine))
              {
                State = States.AddingInternalWalls;
                _prevButton.SetText("Prev");
                var walls = GameWorldManager.GetOuterWalls(_currentRectangle.Divide(Game1.TileSize));
                _building.Walls = new Dictionary<Point, Models.Place>();
                _building.Doors = new Dictionary<Point, Models.Place>();
                _building.Rectangle = _currentRectangle;
                foreach (var wall in walls)
                {
                  AddWall(wall.Key);
                }

                UpdateWallTextures();
              }
              break;
            case States.AddingInternalWalls:
              State = States.AddingDoors;
              _prevButton.SetText("Prev");


              foreach (var door in _building.Doors)
              {
                if (!_building.Walls.ContainsKey(door.Key))
                  continue;

                _building.Walls[door.Key].AdditionalProperties["hasDoor"] = new AdditionalProperty() { IsVisible = false, Value = true };
              }

              UpdateWallTextures();

              break;
            case States.AddingDoors:
              if (_building.Doors.Count > 0)
              {
                State = States.AddingFurniture;
                _prevButton.SetText("Prev");
                _nextButton.SetText("Finish");
              }
              break;
            case States.AddingFurniture:
              Finish();
              break;
            default:
              break;
          }
          //_nextButton.IsVisible = false;
        },
      };

      _prevButton.IsVisible = true;
      _nextButton.IsVisible = true;

      _prevButton.Position = new Vector2((ZonerGame.ScreenWidth / 2) - ((buttonTexture.Width + 20)), 50);
      _nextButton.Position = new Vector2((ZonerGame.ScreenWidth / 2) + 20, 50);

      _controls.Add(_informationPanel);
      _controls.Add(_title);
      _controls.Add(_prevButton);
      _controls.Add(_nextButton);
    }

    private void UpdateWallTextures()
    {
      var walls = _building.Walls.Where(c => !hasDoor(c));

      var newPoints = GameWorldManager.GetWalls(walls.Select(c => new Rectangle(c.Key.X, c.Key.Y, Game1.TileSize, Game1.TileSize)).ToList());
      foreach (var wall in walls)
      {
        if (!wall.Value.AdditionalProperties.ContainsKey("wallType"))
          wall.Value.AdditionalProperties.Add("wallType", new AdditionalProperty() { IsVisible = false });

        wall.Value.AdditionalProperties["wallType"].Value = newPoints[wall.Key];
      }
    }

    private void AddWall(Point key)
    {
      var wrapper = (Models.Place)_gwm.GameWorld.PlaceData["woodenWall"].Clone();
      wrapper.AdditionalProperties.Add("hasDoor", new AdditionalProperty() { Value = false, IsVisible = false });
      _building.Walls.Add(key, wrapper);
    }

    private void UpdateTitle()
    {
      switch (State)
      {
        case States.LayingFoundation:
          _title.Text = "Laying foundation";
          break;
        case States.AddingInternalWalls:
          _title.Text = "Adding internals walls";
          break;
        case States.AddingDoors:
          _title.Text = "Adding doors";
          break;
        case States.AddingFurniture:
          _title.Text = "Adding furniture";
          break;
        default:
          break;
      }

      _title.UpdatePosition(new Rectangle(0, 0, ZonerGame.ScreenWidth, 40));
    }

    private void UpdateCursor()
    {
      var position = _cursorEntity.Position;
      switch (State)
      {
        case States.LayingFoundation:
          _cursorEntity = new DrawingSquare(_texture, DrawingSquare.States.Cursor);
          _cursorEntity.LoadContent();
          break;
        case States.AddingInternalWalls:
          _cursorEntity = new DrawingSquare(_gameModel.Content.Load<Texture2D>($"Places/Walls/Wall"), DrawingSquare.States.Cursor);
          _cursorEntity.LoadContent();
          break;
        case States.AddingDoors:
          _cursorEntity = new DrawingSquare(_gameModel.Content.Load<Texture2D>($"Places/woodenDoor"), DrawingSquare.States.Cursor);
          _cursorEntity.LoadContent();
          break;
        case States.AddingFurniture:
          break;
        default:
          break;
      }

      _cursorEntity.Position = position;
    }

    public void Start()
    {
      State = States.LayingFoundation;
      _cursorEntity = new DrawingSquare(_texture, DrawingSquare.States.Cursor);
      _cursorEntity.LoadContent();

      _entities.Clear();

      foreach (var entity in _entities)
        entity.LoadContent();

      _isVisible = true;
      _startPosition = null;
    }

    public void Cancel()
    {
      _startPosition = null;
      _isVisible = false;
      _entities.Clear();
      OnCancel?.Invoke();
    }

    public void Finish()
    {
      if (_building.Doors.Count > 0 && _building.Walls.Count > 0)
      {
        OnFinish?.Invoke(_building);
      }

      Cancel();
    }

    public void Update(GameTime gameTime)
    {
      if (!_isVisible)
        return;

      if (GameKeyboard.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Escape))
      {
        Cancel();
      }

      if (GameKeyboard.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Enter))
      {
        Finish();
      }

      var mousePosition = new Point((int)Math.Floor(GameMouse.PositionWithCamera.X / (double)Game1.TileSize) * Game1.TileSize, (int)Math.Floor(GameMouse.PositionWithCamera.Y / (double)Game1.TileSize) * Game1.TileSize).ToVector2();
      _cursorEntity.Position = mousePosition;


      foreach (var control in _controls)
        control.Update(gameTime);

      UpdateResourceCosts();
      UpdateTitle();
      UpdateCursor();

      LayingFoundationUpdate(gameTime);

      AddingInternalWalls(gameTime);

      AddingDoors(gameTime);

      AddEntities();

      _cursorEntity.Update(gameTime);
      foreach (var entity in _entities)
        entity.Update(gameTime);
    }

    private void AddEntities()
    {
      _walls = new List<Wall>();
      _doors = new List<Basic>();
      foreach(var wall in _building.Walls.Where(c => !hasDoor(c)))
      {
        var type = wall.Value.AdditionalProperties["wallType"].Value;

        var wallEntity = new Wall(wall.Value, _gameModel.Content.Load<Texture2D>($"Places/Walls/{type}"), wall.Key.Multiply(Game1.TileSize).ToVector2(), Wall.Types.External);
        wallEntity.LoadContent();

        _walls.Add(wallEntity);
      }

      foreach(var door in _building.Doors)
      {
        var doorEntity = new Basic(_gameModel.Content.Load<Texture2D>($"Places/woodenDoor"), door.Key.Multiply(Game1.TileSize).ToVector2());
        doorEntity.LoadContent();

        _doors.Add(doorEntity);
      }
    }

    private void UpdateResourceCosts()
    {
      var costs = new Dictionary<string, long>();

      foreach (var wall in _building.Walls.Where(c => !hasDoor(c)))
      {

        foreach (var resource in wall.Value.ResourceCost)
        {
          if (!costs.ContainsKey(resource.Key))
            costs.Add(resource.Key, 0);

          costs[resource.Key] += resource.Value;
        }
      }

      _informationPanel.Children.Clear();
      _informationPanel.AddChild(new Label(_font, "Resource cost:") { Position = new Vector2(10, 10), });

      var y = 30f;
      foreach (var cost in costs)
      {
        _informationPanel.AddChild(new Label(_font, $"{cost.Key}: {cost.Value}") { Position = new Vector2(10, y), });
        y += (_font.MeasureString(cost.Key).Y + 5);
      }
    }

    private static bool hasDoor(KeyValuePair<Point, Models.Place> c)
    {
      return c.Value.AdditionalProperties.ContainsKey("hasDoor") && (bool)c.Value.AdditionalProperties["hasDoor"].Value;
    }

    private void LayingFoundationUpdate(GameTime gameTime)
    {
      if (State != States.LayingFoundation)
        return;

      // Don't lay wall while over a button
      if (GameMouse.ClickableObjects.Count > 0)
        return;

      var mousePosition = _cursorEntity.Position;
      if (GameMouse.IsLeftPressed)
      {
        if (_startPosition == null)
          _startPosition = mousePosition;

        if (_startPosition != mousePosition)
        {
          var rectangle = new Rectangle();

          if (_startPosition.Value.X < mousePosition.X)
          {
            rectangle.X = (int)_startPosition.Value.X;
            rectangle.Width = (int)(mousePosition.X - _startPosition.Value.X);
          }
          else
          {
            rectangle.X = (int)mousePosition.X;
            rectangle.Width = (int)(_startPosition.Value.X - mousePosition.X);
          }

          if (_startPosition.Value.Y < mousePosition.Y)
          {
            rectangle.Y = (int)_startPosition.Value.Y;
            rectangle.Height = (int)(mousePosition.Y - _startPosition.Value.Y);
          }
          else
          {
            rectangle.Y = (int)mousePosition.Y;
            rectangle.Height = (int)(_startPosition.Value.Y - mousePosition.Y);
          }

          rectangle.Width += Game1.TileSize;
          rectangle.Height += Game1.TileSize;

          _currentRectangle = rectangle;

          if (_previousRectangle != _currentRectangle)
          {
            var width = _currentRectangle.Width / Game1.TileSize;
            var height = _currentRectangle.Height / Game1.TileSize;

            bool isInvalid = false;

            if (width < 5)
              isInvalid = true;

            if (height < 5)
              isInvalid = true;

            _previousRectangle = _currentRectangle;
            _entities.Clear();
            for (int y = 0; y < height; y++)
            {
              for (int x = 0; x < width; x++)
              {
                DrawingSquare.States state = DrawingSquare.States.Fine;

                if (isInvalid)
                  state = DrawingSquare.States.TooSmall;

                var newX = _currentRectangle.X + (x * Game1.TileSize);
                var newY = _currentRectangle.Y + (y * Game1.TileSize);

                if (_map.Collides(new Point(newX / Game1.TileSize, newY / Game1.TileSize), new Point(1, 1)))
                {
                  state = DrawingSquare.States.Colliding;
                }

                var entity = new DrawingSquare(_texture, state);
                entity.Position = new Vector2(newX, newY);
                entity.LoadContent();

                _entities.Add(entity);
              }
            }
          }
        }
        else
        {
          _entities.Clear();
        }
      }
      else
      {
        _startPosition = null;
        if (_entities.Any(c => ((DrawingSquare)c).State != DrawingSquare.States.Fine))
        {
          _entities.Clear();
        }
      }
    }

    private void AddingInternalWalls(GameTime gameTime)
    {
      if (State != States.AddingInternalWalls)
        return;

      //foreach (var wall in _building.Walls)
      //  wall.Value.Update(gameTime);

      if (!GameMouse.Intersects(_currentRectangle, true))
        return;

      if (GameMouse.IsLeftPressed)
      {
        var key = (_cursorEntity.Position / Game1.TileSize).ToPoint();
        if (_building.Walls.ContainsKey(key))
          return;

        AddWall(key);

        UpdateWallTextures();
      }
      else if (GameMouse.IsRightPressed)
      {
        /*
        var wall = _building.Walls.Where(c => c.Value.Type == Wall.Types.Interal).FirstOrDefault(c => c.Value.Position == _cursorEntity.Position);
        _building.Walls.Remove(wall.Key);

        UpdateWallTextures();
        */
      }
    }

    private void AddingDoors(GameTime gameTime)
    {
      if (State != States.AddingDoors)
        return;

      /*
      foreach (var wall in _walls)
        wall.Update(gameTime);

      foreach (var door in _doors)
        door.Update(gameTime);
      */

      if (!GameMouse.Intersects(_currentRectangle, true))
        return;

      if (GameMouse.IsLeftClicked)
      {

        var key = (_cursorEntity.Position / Game1.TileSize).ToPoint();
        if (!_building.Walls.ContainsKey(key))
          return;

        if (_building.Doors.ContainsKey(key))
          return;

        var wall = _building.Walls[key];

        wall.AdditionalProperties["hasDoor"].Value = true;

        var wrapper = (Models.Place)_gwm.GameWorld.PlaceData["woodenDoor"].Clone();
        _building.Doors.Add(key, wrapper);

        UpdateWallTextures();
      }
      else if (GameMouse.IsRightClicked)
      {
        var key = (_cursorEntity.Position / Game1.TileSize).ToPoint();
        if (!_building.Walls.ContainsKey(key))
          return;

        var wall = _building.Walls[key];

        if (!_building.Doors.ContainsKey(key))
          return;

        wall.AdditionalProperties["hasDoor"].Value = false;
        _building.Doors.Remove(key);
        UpdateWallTextures();
      }
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Matrix camera)
    {
      if (!_isVisible)
        return;

      spriteBatch.Begin(transformMatrix: camera);

      foreach (var entity in _entities)
        entity.Draw(gameTime, spriteBatch);

      if (State > States.LayingFoundation)
      {
        foreach (var entity in _walls)
          entity.Draw(gameTime, spriteBatch);
      }

      if (State > States.AddingInternalWalls)
      {
        foreach (var door in _doors)
          door.Draw(gameTime, spriteBatch);
      }

      _cursorEntity.Draw(gameTime, spriteBatch);

      spriteBatch.End();

      spriteBatch.Begin();

      foreach (var control in _controls)
        control.Draw(gameTime, spriteBatch);

      spriteBatch.End();
    }
  }
}
