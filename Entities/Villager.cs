using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Others.Managers;
using Others.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZonerEngine.GL;
using ZonerEngine.GL.Components;
using ZonerEngine.GL.Entities;

namespace Others.Entities
{
  public class Villager : Entity
  {
    public Models.Villager Wrapper;

    /// <summary>
    /// Where the tile is on the map
    /// </summary>
    private readonly Point _point;
    private Texture2D _texture;
    private Rectangle _rectangle;
    private readonly BattleState _state;
    private readonly GameWorldManager _gwm;

    private Texture2D _collisionTexture;
    private Texture2D _selectedTexture;
    private Texture2D _hoveringTexture;

    public Vector2 PositionOffset = Vector2.Zero;

    public bool IsSelected { get; set; } = false;

    public bool IsHovering { get; set; } = false;

    public Villager(Models.Villager villager, Texture2D texture, BattleState state, GameWorldManager gwm)
    {
      Wrapper = villager;
      _point = new Point(Wrapper.MapPoint.X, Wrapper.MapPoint.Y);
      _texture = texture;
      _state = state;
      _gwm = gwm;

      Position = _point.ToVector2() * Game1.TileSize;

      var collisionWidth = texture.Width / (texture.Width / Game1.TileSize);
      var collisionHeight = texture.Height / (texture.Height / Game1.TileSize);

      _rectangle = new Rectangle((int)Position.X, (int)Position.Y, collisionWidth, collisionHeight);

      _collisionTexture = new Texture2D(state.GameModel.GraphicsDevice, collisionWidth, collisionHeight);
      _collisionTexture.SetData<Color>(Helpers.GetBorder(collisionWidth, collisionHeight, 1, Color.Red));

      _selectedTexture = new Texture2D(state.GameModel.GraphicsDevice, collisionWidth, collisionHeight);
      _selectedTexture.SetData<Color>(Helpers.GetBorder(collisionWidth, collisionHeight, 1, Color.Yellow));

      _hoveringTexture = new Texture2D(state.GameModel.GraphicsDevice, collisionWidth, collisionHeight);
      _hoveringTexture.SetData<Color>(Helpers.GetBorder(collisionWidth, collisionHeight, 1, Color.Gray));
    }

    public override void LoadContent()
    {
      var origin = new Vector2(_texture.Width / 2, _texture.Height / 2);

      AddComponent(new TextureComponent(this, _texture) { Layer = (Layer + (Position + origin).Y / 1000f), PositionOffset = PositionOffset, GetLayer = () => { var value = (Layer + (Position + origin).Y / 1000f); return value; } });
      AddComponent(new TextureComponent(this, _collisionTexture, () => _state.ShowCollisionBox) { Layer = 0.96f, });
      AddComponent(new TextureComponent(this, _hoveringTexture, () => IsHovering) { Layer = 0.961f, });
      AddComponent(new TextureComponent(this, _selectedTexture, () => IsSelected) { Layer = 0.962f, });
      AddComponent(new MoveComponent(this, Move));
      AddComponent(new SelectableComponent(this, () => _rectangle)
      {
        OnHover = () =>
        {
          IsHovering = true;
        },
        OffHover = () =>
        {
          IsHovering = false;
        },
        OnSelected = () =>
        {
          IsSelected = true;
        },
        OffSelected = () =>
        {
          IsSelected = false;
        },
        GetInformation = () =>
        {
          return new ZonerEngine.GL.Models.EntityInformation()
          {
            Header = Wrapper.Name,
            Sections = new List<ZonerEngine.GL.Models.EntityInformation.Content>()
            {
              new ZonerEngine.GL.Models.EntityInformation.Content()
              {
                Header = "Inventory",
                Values = Wrapper.Inventory.Select(c => $"{c.Key}: {c.Value.Count}").ToArray(),
              },
            }
          };
        }
      });
    }

    public void Move(GameTime gameTime)
    {
      Position = Wrapper.Position;
      _rectangle.X = (int)Position.X;
      _rectangle.Y = (int)Position.Y;
    }
  }
}
