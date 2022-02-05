using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Others.Controls;
using Others.Entities;
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

    private Map _map;
    private Matrix _camera;

    private Texture2D _texture;
    private List<Entity> _entities = new List<Entity>();
    private Entity _cursorEntity;

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

    public Action<Rectangle> OnFinish;

    public States State { get; private set; } = States.LayingFoundation;

    public HouseBuildingManager(GameModel gameModel, Map map, Matrix camera)
    {
      _map = map;
      _camera = camera;
      _texture = gameModel.Content.Load<Texture2D>("GUI/Drawer");

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
              State = States.AddingInternalWalls;
              _prevButton.SetText("Prev");
              break;
            case States.AddingInternalWalls:
              State = States.AddingDoors;
              _prevButton.SetText("Prev");
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
      if (_entities.Count > 0)
      {
        OnFinish?.Invoke(_currentRectangle);
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

      LayingFoundationUpdate();

      _cursorEntity.Update(gameTime, _entities);
      foreach (var entity in _entities)
        entity.Update(gameTime, _entities);
    }

    private void LayingFoundationUpdate()
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

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Matrix camera)
    {
      if (!_isVisible)
        return;

      spriteBatch.Begin(transformMatrix: camera);

      _cursorEntity.Draw(gameTime, spriteBatch);

      foreach (var entity in _entities)
        entity.Draw(gameTime, spriteBatch);

      spriteBatch.End();

      spriteBatch.Begin();

      foreach (var control in _controls)
        control.Draw(gameTime, spriteBatch);

      spriteBatch.End();
    }
  }
}
