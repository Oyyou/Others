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
  public class Tile : Entity
  {
    /// <summary>
    /// Where the tile is on the map
    /// </summary>
    private readonly Point _point;
    private Texture2D _texture;
    private readonly BattleState _state;

    private Texture2D _borderTexture;

    public Tile(int x, int y, Texture2D texture, BattleState state)
    {
      _point = new Point(x, y);
      _texture = texture;
      _state = state;

      Position = _point.ToVector2() * Game1.TileSize;
      _borderTexture = new Texture2D(state.GameModel.GraphicsDevice, _texture.Width, _texture.Height);
      _borderTexture.SetData<Color>(Helpers.GetBorder(_texture, 1, Color.White));
    }

    public override void LoadContent()
    {
      AddComponent(new TextureComponent(this, _texture));
      AddComponent(new TextureComponent(this, _borderTexture, () => _state.ShowGrid) { Layer = 0.95f, });
    }
  }
}
