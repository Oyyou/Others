using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using ZonerEngine.GL;
using ZonerEngine.GL.Input;

namespace Others.Controls
{
  public class ScrollBar : Control
  {
    private GraphicsDevice _graphicsDevice;

    private Texture2D _backgroundTexture;

    private Button _upButton;
    private Button _downButton;
    private Button _barButton;

    private float _min;
    private float _max;

    private float _speed = 10f;

    private float _remaining;

    private int _previousScrollValue;

    public override Rectangle Rectangle => new Rectangle((int)DrawPosition.X, (int)DrawPosition.Y, _backgroundTexture.Width, _backgroundTexture.Height);

    public ScrollBar(GraphicsDevice graphicsDevice, SpriteFont font, int height)
    {
      _graphicsDevice = graphicsDevice;

      _backgroundTexture = new Texture2D(graphicsDevice, 20, height);
      _backgroundTexture.SetData(Helpers.GetBorder(_backgroundTexture, 1, Color.Black, Color.LightGray));

      var upTexture = new Texture2D(graphicsDevice, _backgroundTexture.Width - 4, _backgroundTexture.Width - 4);
      upTexture.SetData(Helpers.GetBorder(upTexture, 1, Color.Black, Color.Gray));

      var downTexture = new Texture2D(graphicsDevice, _backgroundTexture.Width - 4, _backgroundTexture.Width - 4);
      downTexture.SetData(Helpers.GetBorder(downTexture, 1, Color.Black, Color.Gray));

      var barTexture = new Texture2D(graphicsDevice, _backgroundTexture.Width - 4, 50);
      barTexture.SetData(Helpers.GetBorder(barTexture, 1, Color.Black, Color.Gray));

      _upButton = new Button(upTexture) { Position = new Vector2(2, 2) };
      _downButton = new Button(downTexture) { Position = new Vector2(2, height - (_backgroundTexture.Width - 2)) };

      _min = _upButton.Rectangle.Bottom + 2;
      _max = _downButton.Position.Y - 2 - barTexture.Height;

      _barButton = new Button(barTexture) { Position = new Vector2(2, _min), OnHeld = Bar_OnHeld };

      AddChild(_upButton);
      AddChild(_barButton);
      AddChild(_downButton);
    }

    public void SetRectangle(Rectangle rectangle)
    {
      if (rectangle.Height < this.Parent.Rectangle.Height)
      {
        IsVisible = false;
        return;
      }

      var area = (_downButton.Rectangle.Top - 2f) - (_upButton.Rectangle.Bottom + 2f);
      var ratio = (area / rectangle.Height);
      var size = (int)(area * ratio);
      var rationReverse = (area * (1 - ratio));

      _speed = 10f;// area - size;

      var a = (float)this.Parent.Rectangle.Height / (float)rectangle.Height;
      var b = (this.Parent.Rectangle.Height * (1 - a));
      var c = (float)rectangle.Height - (float)this.Parent.Rectangle.Height;
      _remaining = c / (rationReverse / _speed);

      var barTexture = new Texture2D(_graphicsDevice, _backgroundTexture.Width - 4, size);
      barTexture.SetData(Helpers.GetBorder(barTexture, 1, Color.Black, Color.Gray));

      _min = (_upButton.Position.Y + _upButton.Rectangle.Height) + 2;
      _max = _downButton.Position.Y - 2 - barTexture.Height;

      RemoveChild(_barButton);
      _barButton = new Button(barTexture) { Position = new Vector2(2, _min), OnHeld = Bar_OnHeld };
      AddChild(_barButton);

      IsVisible = true;
    }

    private void Bar_OnHeld()
    {
      var position = MousePosition - this.DrawPosition;

      SetBarButtonY(position.Y - (_barButton.Rectangle.Height / 2));
    }

    private void SetBarButtonY(float y)
    {
      var newY = MathHelper.Clamp(y, _min, _max);

      var change = newY - _min;
      var percentage = change / _speed;
      var offset = _remaining * percentage;

      _barButton.Position = new Vector2(_barButton.Position.X, newY);
      this.Parent.ChildrenOffset = new Vector2(0, -offset);
    }

    public override void Update(GameTime gameTime)
    {
      if (!IsVisible)
        return;

      Scroll();
      base.Update(gameTime);
    }


    private void Scroll()
    {
      if (!MouseRectangle.Intersects(this.Parent.Rectangle))
      {
        _previousScrollValue = GameMouse.ScrollWheelValue;
        return;
      }

      if (GameMouse.ScrollWheelValue < _previousScrollValue)
        SetBarButtonY(_barButton.Position.Y + _speed);

      if (GameMouse.ScrollWheelValue > _previousScrollValue)
        SetBarButtonY(_barButton.Position.Y - _speed);

      _previousScrollValue = GameMouse.ScrollWheelValue;
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
      if (!IsVisible)
        return;

      spriteBatch.Draw(_backgroundTexture, DrawPosition, null, Color.White, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, DrawLayer);
      DrawChildren(gameTime, spriteBatch);
    }
  }
}
