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

    public string Text
    {
      get { return _label.Text; }
      set
      {
        _label.Text = value;
      }
    }

    public Button(Texture2D texture, SpriteFont font, string text) : base()
    {
      _texture = texture;
      _label = new Label(font, text)
      {
        Position = new Vector2((_texture.Width / 2) - (font.MeasureString(text).X / 2), (_texture.Height / 2) - (font.MeasureString(text).Y / 2)),
      };

      AddChild(_label);
    }

    public override void Update(GameTime gameTime)
    {
      UpdateChildren(gameTime);
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
      spriteBatch.Draw(_texture, DrawPosition, Color.White);
      DrawChildren(gameTime, spriteBatch);      
    }
  }
}
