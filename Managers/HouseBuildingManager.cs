using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Others.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZonerEngine.GL.Entities;
using ZonerEngine.GL.Input;
using ZonerEngine.GL.Maps;

namespace Others.Managers
{
  public class HouseBuildingManager
  {
    private Map _map;

    private Texture2D _texture;
    private List<Entity> _entities = new List<Entity>();
    private Entity _cursorEntity;

    private bool _isVisible = false;

    private Vector2? _startPosition;

    private Rectangle _currentRectangle;
    private Rectangle _previousRectangle;

    public Action OnCancel;

    public Action<Rectangle> OnFinish;

    public HouseBuildingManager(ContentManager content, Map map)
    {
      _map = map;
      _texture = content.Load<Texture2D>("GUI/Drawer");
    }

    public void Start()
    {
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
      if(_entities.Count > 0)
      {
        OnFinish?.Invoke(_currentRectangle);
      }

      Cancel();
    }

    public void Update(GameTime gameTime)
    {
      if (!_isVisible)
        return;

      if(GameKeyboard.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Escape))
      {
        Cancel();
      }

      if(GameKeyboard.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Enter))
      {
        Finish();
      }

      var mousePosition = new Point((int)Math.Floor(GameMouse.Position.X / (double)Game1.TileSize) * Game1.TileSize, (int)Math.Floor(GameMouse.Position.Y / (double)Game1.TileSize) * Game1.TileSize).ToVector2();
      _cursorEntity.Position = mousePosition;

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
        if(_entities.Any(c => ((DrawingSquare)c).State != DrawingSquare.States.Fine))
        {
          _entities.Clear();
        }
      }

      _cursorEntity.Update(gameTime, _entities);
      foreach (var entity in _entities)
        entity.Update(gameTime, _entities);
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
      if (!_isVisible)
        return;

      spriteBatch.Begin();

      _cursorEntity.Draw(gameTime, spriteBatch);

      foreach (var entity in _entities)
        entity.Draw(gameTime, spriteBatch);

      spriteBatch.End();
    }
  }
}
