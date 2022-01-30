﻿using Microsoft.Xna.Framework;
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

    public override Rectangle Rectangle => new Rectangle((int)DrawPosition.X, (int)DrawPosition.Y, (int)_font.MeasureString(Text).X, (int)_font.MeasureString(Text).Y);

    public Label(SpriteFont font, string text) : base()
    {
      _font = font;
      Text = text;
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
      spriteBatch.DrawString(_font, Text, DrawPosition, Color.Black, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, DrawLayer);

      DrawChildren(gameTime, spriteBatch);
    }
  }
}