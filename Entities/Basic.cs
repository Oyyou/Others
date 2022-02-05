using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Others.States;
using System;
using System.Collections.Generic;
using System.Text;
using ZonerEngine.GL;
using ZonerEngine.GL.Components;
using ZonerEngine.GL.Entities;
using ZonerEngine.GL.States;

namespace Others.Entities
{
  public class Basic : Entity
  {
    private Texture2D _texture;

    public Rectangle Rectangle
    {
      get
      {
        return new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);
      }
    }

    public Point Point
    {
      get
      {
        return (Position / Game1.TileSize).ToPoint();
      }
    }

    public Basic(Texture2D texture, Vector2 position)
    {
      _texture = texture;
      Position = position;
    }

    public override void LoadContent()
    {
      AddComponent(new TextureComponent(this, _texture));
    }
  }
}
