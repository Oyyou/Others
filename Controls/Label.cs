using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Others.Controls
{
  public class Label : Control
  {
    protected SpriteFont _font;

    public string Text { get; set; }

    public Label(SpriteFont font, string text) : base()
    {
      _font = font;
      Text = text;
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
      spriteBatch.DrawString(_font, Text, DrawPosition, Color.Black);

      DrawChildren(gameTime, spriteBatch);
    }
  }
}
