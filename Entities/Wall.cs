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
  public class Wall : Entity
  {
    public enum Types
    {
      Interal,
      External
    }

    private Texture2D _texture;

    public readonly Models.Place Place;

    public Rectangle Rectangle
    {
      get
      {
        return new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);
      }
    }

    public string TextureName
    {
      get
      {
        return _texture.Name;
      }
    }

    public Point Point
    {
      get
      {
        return (Position / Game1.TileSize).ToPoint();
      }
    }

    public readonly Types Type;

    public bool HasDoor { get; set; } = false;

    public Wall(Models.Place place, Texture2D texture, Vector2 position, Types type)
    {
      Place = place;
      _texture = texture;
      Position = position;
      Type = type;
    }

    public override void LoadContent()
    {
      AddComponent(new TextureComponent(this, _texture));
    }

    public void ChangeTexture(Texture2D texture)
    {
      if (_texture.Name == texture.Name)
        return;

      _texture = texture;

      Components.Clear();
      AddComponent(new TextureComponent(this, _texture));
    }
  }
}
