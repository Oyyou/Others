using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Others.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using ZonerEngine.GL.Entities;
using ZonerEngine.GL.Input;

namespace Others.Managers
{
  public class HouseBuildingManager
  {
    private Texture2D _texture;
    private List<Entity> _entities = new List<Entity>();
    private Entity _cursorEntity;

    private bool _isVisible = false;

    private Vector2? _startPosition;

    private Rectangle _currentRectangle;
    private Rectangle _previousRectangle;

    public HouseBuildingManager(ContentManager content)
    {
      _texture = content.Load<Texture2D>("GUI/Drawer");
    }

    public void Start()
    {
      _cursorEntity = new DrawingSquare(_texture, Color.Blue);
      _cursorEntity.LoadContent();

      _entities.Clear();

      foreach (var entity in _entities)
        entity.LoadContent();

      _isVisible = true;
      _startPosition = null;
    }

    public void Update(GameTime gameTime)
    {
      if (!_isVisible)
        return;

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

          _currentRectangle = rectangle;
        }

        if (_previousRectangle != _currentRectangle)
        {
          _entities.Clear();
          for (int y = _currentRectangle.Y; y <= _currentRectangle.Bottom; y += Game1.TileSize)
          {
            for (int x = _currentRectangle.X; x <= _currentRectangle.Right; x += Game1.TileSize)
            {
              var entity = new DrawingSquare(_texture, Color.Green);
              entity.Position = new Vector2(x, y);
              entity.LoadContent();

              _entities.Add(entity);
            }
          }
        }
      }
      else
      {
        _startPosition = null;
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
