using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Others.Controls
{
  public class Button : Control
  {
    private readonly Texture2D _texture;
    private readonly Label _label;

    public Color HoverColour = Color.Gray;

    public string Text
    {
      get { return _label.Text; }
      protected set
      {
        _label.Text = value;
      }
    }

    public override Rectangle Rectangle => new Rectangle((int)DrawPosition.X, (int)DrawPosition.Y, _texture.Width, _texture.Height);

    public Button(Texture2D texture) : this(texture, null, "")
    {

    }

    public Button(Texture2D texture, SpriteFont font, string text) : base()
    {
      _texture = texture;
      if (font != null)
      {
        _label = new Label(font, text)
        {
          IsVisible = true,
        };

        AddChild(_label);
        _label.UpdatePosition(); // Notice how I'm setting the position AFTER I've assigned the parent
      }
    }

    public void SetText(string text)
    {
      if (_label == null)
        return;

      _label.Text = text;
      _label.UpdatePosition();
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
      if (!IsVisible)
        return;

      spriteBatch.Draw(_texture, DrawPosition, null, IsMouseOver ? HoverColour : Color.White, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, DrawLayer);
      DrawChildren(gameTime, spriteBatch);
    }

    public override string ToString()
    {
      if (!string.IsNullOrEmpty(Text))
        return $"{Text} button";

      return base.ToString();
    }
  }
}
