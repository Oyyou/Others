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
    private Map _map;
    private Matrix _camera;

    private Texture2D _texture;
    private List<Entity> _entities = new List<Entity>();
    private Entity _cursorEntity;

    #region Adding internal walls
    private Building _building;
    #endregion

    private bool _isVisible = false;

    private Vector2? _startPosition;

    private Rectangle _currentRectangle;
    private Rectangle _previousRectangle;

    #region GUI
    private List<Control> _controls = new List<Control>();

    private Label _title;
    private Button _prevButton;
    private Button _nextButton;
    #endregion

    public Action OnCancel;

    public Action<Building> OnFinish;

    public States State { get; private set; } = States.LayingFoundation;

    public HouseBuildingManager(GameModel gameModel, Map map, Matrix camera)
    {
      _gameModel = gameModel;
      _map = map;
      _camera = camera;
      _texture = _gameModel.Content.Load<Texture2D>("GUI/Drawer");

      _building = new Building();

      InitializeGUI(gameModel);
    }

    private void InitializeGUI(GameModel gameModel)
    {
      var font = gameModel.Content.Load<SpriteFont>("Font");

      _title = new Label(font, "Laying foundation");
      UpdateTitle();

      var buttonTexture = new Texture2D(gameModel.GraphicsDevice, 120, 30);
      buttonTexture.SetData(Helpers.GetBorder(buttonTexture.Width, buttonTexture.Height, 2, Color.Black, Color.White));

      _prevButton = new Button(buttonTexture, font, "Cancel")
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
                wall.Value.HasDoor = false;

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
      _nextButton = new Button(buttonTexture, font, "Next")
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
                var walls = GameWorldManager.GetOuterWalls(_currentRectangle.Divide(40));
                _building.Walls = new Dictionary<Point, Wall>();
                _building.Doors = new Dictionary<Point, Basic>();
                _building.Rectangle = _currentRectangle;
                foreach (var wall in walls)
                {
                  var wallEntity = new Wall(_gameModel.Content.Load<Texture2D>($"Places/Walls/{wall.Value}"), wall.Key.Multiply(Game1.TileSize).ToVector2(), Wall.Types.External);
                  wallEntity.LoadContent();
                  _building.Walls.Add(wall.Key, wallEntity);
                }
              }
              break;
            case States.AddingInternalWalls:
              State = States.AddingDoors;
              _prevButton.SetText("Prev");


              foreach (var door in _building.Doors)
              {
                var wall = _building.Walls.FirstOrDefault(c => c.Value.Position == door.Value.Position);
                wall.Value.HasDoor = true;
              }

              UpdateWallTextures();

              break;
            case States.AddingDoors:
              State = States.AddingFurniture;
              _prevButton.SetText("Prev");
              _nextButton.SetText("Finish");
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

      _controls.Add(_title);
      _controls.Add(_prevButton);
      _controls.Add(_nextButton);
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

      var mousePosition = new Point((int)Math.Floor(GameMouse.Position.X / (double)Game1.TileSize) * Game1.TileSize, (int)Math.Floor(GameMouse.Position.Y / (double)Game1.TileSize) * Game1.TileSize).ToVector2();
      _cursorEntity.Position = mousePosition;


      foreach (var control in _controls)
        control.Update(gameTime);

      UpdateTitle();
      UpdateCursor();

      LayingFoundationUpdate(gameTime);

      AddingInternalWalls(gameTime);

      AddingDoors(gameTime);

      _cursorEntity.Update(gameTime);
      foreach (var entity in _entities)
        entity.Update(gameTime);
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

      foreach (var wall in _building.Walls)
        wall.Value.Update(gameTime);

      if (!GameMouse.Intersects(_currentRectangle))
        return;

      if (GameMouse.IsLeftPressed)
      {
        if (_building.Walls.Any(c => c.Value.Position == _cursorEntity.Position))
          return;

        var wallEntity = new Wall(_gameModel.Content.Load<Texture2D>($"Places/Walls/Wall"), _cursorEntity.Position, Wall.Types.Interal);
        wallEntity.LoadContent();
        _building.Walls.Add(wallEntity.Point, wallEntity);

        var newPoints = GameWorldManager.GetWalls(_building.Walls.Select(c => c.Value.Rectangle.Divide(40)).ToList());
        foreach (var wall in _building.Walls)
        {
          wall.Value.ChangeTexture(_gameModel.Content.Load<Texture2D>($"Places/Walls/{newPoints[wall.Value.Point]}"));
        }
      }
      else if (GameMouse.IsRightPressed)
      {
        var wall = _building.Walls.Where(c => c.Value.Type == Wall.Types.Interal).FirstOrDefault(c => c.Value.Position == _cursorEntity.Position);
        _building.Walls.Remove(wall.Key);

        UpdateWallTextures();
      }
    }

    private void UpdateWallTextures()
    {
      var walls = _building.Walls.Where(c => !c.Value.HasDoor);
      var newPoints = GameWorldManager.GetWalls(walls.Select(c => c.Value.Rectangle.Divide(40)).ToList());
      foreach (var w in walls)
      {
        w.Value.ChangeTexture(_gameModel.Content.Load<Texture2D>($"Places/Walls/{newPoints[w.Value.Point]}"));
      }
    }

    private void AddingDoors(GameTime gameTime)
    {
      if (State != States.AddingDoors)
        return;

      foreach (var wall in _building.Walls)
        wall.Value.Update(gameTime);

      foreach (var door in _building.Doors)
        door.Value.Update(gameTime);

      if (!GameMouse.Intersects(_currentRectangle))
        return;

      if (GameMouse.IsLeftClicked)
      {

        var key = (_cursorEntity.Position / 40).ToPoint();
        if (!_building.Walls.ContainsKey(key))
          return;

        var wall = _building.Walls[key];

        wall.HasDoor = true;

        var door = new Basic(_gameModel.Content.Load<Texture2D>($"Places/woodenDoor"), wall.Position);
        door.LoadContent();

        _building.Doors.Add(key, door);
        UpdateWallTextures();
      }
      else if (GameMouse.IsRightClicked)
      {
        var key = (_cursorEntity.Position / 40).ToPoint();
        if (!_building.Walls.ContainsKey(key))
          return;

        var wall = _building.Walls[key];

        if (!_building.Doors.ContainsKey(key))
          return;

        var door = _building.Walls[key];

        wall.HasDoor = false;
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
        foreach (var entity in _building.Walls)
          entity.Value.Draw(gameTime, spriteBatch);
      }

      if (State > States.AddingInternalWalls)
      {
        foreach (var door in _building.Doors)
          door.Value.Draw(gameTime, spriteBatch);
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
