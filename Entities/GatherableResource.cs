using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Others.Models;
using System;
using System.Collections.Generic;
using System.Text;
using ZonerEngine.GL.Components;
using ZonerEngine.GL.Entities;

namespace Others.Entities
{
  public class GatherableResource : Entity
  {
    /// <summary>
    /// Where the tile is on the map
    /// </summary>
    private readonly Point _point;
    private Texture2D _texture;
    private Item _item;

    public GatherableResource(int x, int y, Texture2D texture, Item item)
    {
      _point = new Point(x, y);
      _texture = texture;
      _item = item;

      Position = _point.ToVector2() * Game1.TileSize;
    }

    public override void LoadContent()
    {
      AddComponent(new TextureComponent(this, _texture) { Layer = (Layer + _point.Y / 100f) });
      AddComponent(new MappedComponent(this, '0', () => new Rectangle(_point.X, _point.Y, 1, 1)));
    }
  }
}
