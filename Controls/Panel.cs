using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Others.Controls
{
  public class Panel : Control
  {
    private Texture2D _texture;

    public override Rectangle ClickRectangle => new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);

    public Panel(Texture2D texture, Vector2 position)
    {
      _texture = texture;
      Position = position;
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
      spriteBatch.Draw(_texture, DrawPosition, null, Color.White, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, DrawLayer);

      DrawChildren(gameTime, spriteBatch);
    }
  }
}
