using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Others.States;
using System;
using System.Collections.Generic;
using System.Text;
using ZonerEngine.GL;
using ZonerEngine.GL.Components;
using ZonerEngine.GL.Entities;

namespace Others.Entities
{
  public class Crate : Entity
  {
    /// <summary>
    /// Where the tile is on the map
    /// </summary>
    private readonly Point _point;
    private Texture2D _texture;
    private readonly BattleState _state;

    private Texture2D _collisionTexture;

    public Crate(int x, int y, Texture2D texture, BattleState state)
    {
      _point = new Point(x, y);
      _texture = texture;
      _state = state;

      Position = _point.ToVector2() * Game1.TileSize;
      _collisionTexture = new Texture2D(state.GameModel.GraphicsDevice, Game1.TileSize, Game1.TileSize);
      _collisionTexture.SetData<Color>(Helpers.GetBorder(Game1.TileSize, Game1.TileSize, 1, Color.Red));
    }

    public override void LoadContent()
    {
      AddComponent(new TextureComponent(this, _texture) { Layer = (Layer + _point.Y / 100f), PositionOffset = new Vector2(0, -Game1.TileSize) });
      AddComponent(new TextureComponent(this, _collisionTexture, () => _state.ShowCollisionBox) { Layer = 0.96f, });
      AddComponent(new MappedComponent(this, '1', () => new Rectangle(_point.X, _point.Y, 1, 1)));
    }
  }
}
