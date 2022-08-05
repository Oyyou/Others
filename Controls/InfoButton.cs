using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using ZonerEngine.GL;
using ZonerEngine.GL.Input;

namespace Others.Controls
{
  public class InfoButton : Control
  {
    private Texture2D _texture;

    private Vector2 _position;

    private float _timer = 0f;

    private float opacity = 0f;

    private Color _colour = Color.White;

    public override Rectangle ClickRectangle => _position.ToRectangle(_texture.Width, _texture.Height);

    public InfoButton(Texture2D texture)
    {
      _texture = texture;
      IsVisible = false;
      opacity = 0f;

      Layer = 1f;

      OnHover = () =>
      {
        IsVisible = true;
        _timer = 0f;
        _colour = Color.Gray;
      };
    }

    public void Set(Rectangle rectangle)
    {
      IsVisible = true;
      _timer = 0f;

      _position = new Vector2(
        rectangle.Right - (_texture.Width + 5),
        rectangle.Top + 5
      );
    }

    public override void Update(GameTime gameTime)
    {
      if (!IsVisible)
      {
        opacity -= 0.1f;
        opacity = MathHelper.Clamp(opacity, 0f, 1f);
        return;
      }

      opacity += 0.1f;
      opacity = MathHelper.Clamp(opacity, 0f, 1f);

      _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

      if (_timer > 3f)
      {
        IsVisible = false;
      }

      _colour = Color.White;
      base.Update(gameTime); // Hover stuff happens in here 
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
      if (!IsVisible && opacity <= 0)
        return;

      spriteBatch.Draw(_texture, _position, null, _colour * opacity, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, DrawLayer);

      DrawChildren(gameTime, spriteBatch);
    }
  }
}
