using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using ZonerEngine.GL;
using ZonerEngine.GL.Components;
using ZonerEngine.GL.Entities;

namespace Others.Entities
{
  public class Path : Entity
  {
    /// <summary>
    /// Where the tile is on the map
    /// </summary>
    private readonly Point _point;
    private Texture2D _texture;

    public Path(int x, int y, Texture2D texture)
    {
      _point = new Point(x, y);
      _texture = texture;

      Position = _point.ToVector2() * Game1.TileSize;
    }

    public override void LoadContent()
    {
      AddComponent(new TextureComponent(this, _texture) { Layer = 0.93f, Colour = Color.Red });
    }
  }
}
