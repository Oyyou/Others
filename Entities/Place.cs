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
  public class Place : Entity
  {
    private readonly Models.PlaceWrapper _place;
    /// <summary>
    /// Where the tile is on the map
    /// </summary>
    private readonly Point _point;
    private Texture2D _texture;
    private readonly BattleState _state;

    private Texture2D _collisionTexture;

    public Vector2 PositionOffset = Vector2.Zero;

    public Place(Models.PlaceWrapper place, Texture2D texture, BattleState state)
    {
      _place = place;
      _point = new Point(_place.Point.X, _place.Point.Y);
      _texture = texture;
      _state = state;

      Position = _point.ToVector2() * Game1.TileSize;

      var collisionWidth = Game1.TileSize * _place.Width;
      var collisionHeight = Game1.TileSize * _place.Height;
      _collisionTexture = new Texture2D(state.GameModel.GraphicsDevice, collisionWidth, collisionHeight);
      _collisionTexture.SetData<Color>(Helpers.GetBorder(collisionWidth, collisionHeight, 1, Color.Red));
    }

    public override void LoadContent()
    {
      var origin = new Vector2(_texture.Width / 2, _texture.Height / 2);

      AddComponent(new TextureComponent(this, _texture) { Layer = (Layer + (Position + origin).Y / 1000f), PositionOffset = PositionOffset });
      AddComponent(new TextureComponent(this, _collisionTexture, () => _state.ShowCollisionBox) { Layer = 0.96f, });
      AddComponent(new MappedComponent(this, '1', () => new Rectangle(_point.X, _point.Y, _place.Width, _place.Height)));
    }
  }
}
