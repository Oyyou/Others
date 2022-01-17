using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Others.Managers;
using Others.States;
using System;
using System.Collections.Generic;
using System.Text;
using ZonerEngine.GL;
using ZonerEngine.GL.Components;
using ZonerEngine.GL.Entities;

namespace Others.Entities
{
  public class Villager : Entity
  {
    private readonly Models.Villager _villager;

    /// <summary>
    /// Where the tile is on the map
    /// </summary>
    private readonly Point _point;
    private Texture2D _texture;
    private readonly BattleState _state;
    private readonly GameWorldManager _gwm;

    private Texture2D _collisionTexture;

    public Vector2 PositionOffset = Vector2.Zero;

    public Villager(Models.Villager villager, Texture2D texture, BattleState state, GameWorldManager gwm)
    {
      _villager = villager;
      _point = new Point(_villager.MapPoint.X, _villager.MapPoint.Y);
      _texture = texture;
      _state = state;
      _gwm = gwm;

      Position = _point.ToVector2() * Game1.TileSize;

      var collisionWidth = texture.Width / (texture.Width / Game1.TileSize); ;
      var collisionHeight = texture.Height / (texture.Height / Game1.TileSize);
      _collisionTexture = new Texture2D(state.GameModel.GraphicsDevice, collisionWidth, collisionHeight);
      _collisionTexture.SetData<Color>(Helpers.GetBorder(collisionWidth, collisionHeight, 1, Color.Red));
    }

    public override void LoadContent()
    {
      var origin = new Vector2(_texture.Width / 2, _texture.Height / 2);

      AddComponent(new TextureComponent(this, _texture) { Layer = (Layer + (Position + origin).Y / 1000f), PositionOffset = PositionOffset, GetLayer = () => { var value = (Layer + (Position + origin).Y / 1000f); Console.WriteLine(value); return value; } });
      AddComponent(new TextureComponent(this, _collisionTexture, () => _state.ShowCollisionBox) { Layer = 0.96f, });
      AddComponent(new MoveComponent(this, Move));
    }

    public void Move(GameTime gameTime, List<Entity> entities)
    {
      Position = _villager.Position;
    }
  }
}
