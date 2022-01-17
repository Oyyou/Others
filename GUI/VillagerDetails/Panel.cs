using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Others.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Others.GUI.VillagerDetails
{
  public class Panel
  {
    private readonly ContentManager _content;

    private Villager _villager = null;

    private readonly Texture2D _texture;

    private readonly SpriteFont _font;

    public Vector2 Position { get; set; }

    public Panel(ContentManager content)
    {
      _content = content;
      _texture = _content.Load<Texture2D>("GUI/VillagerDetails/Panel");
      _font = _content.Load<SpriteFont>("Font");
      Position = new Vector2(0, Game1.ScreenHeight - _texture.Height);
    }

    public void SetVillager(Villager villager)
    {
      if (_villager == villager)
        return;

      _villager = villager;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
      if (_villager == null)
        return;

      spriteBatch.Draw(_texture, Position, Color.White);
      spriteBatch.DrawString(_font, _villager.Name, Position + new Vector2(10f, 20f), Color.Black);


      spriteBatch.DrawString(_font, "Stats:", Position + new Vector2(10f, 50f), Color.Black);

      var y = 70f;
      foreach (var item in _villager.Attributes)
      {
        spriteBatch.DrawString(_font, $"{item.Key}: {item.Value.Total}", Position + new Vector2(10f, y), Color.Black);
        y += 20f;
      }


      y += 10f;
      spriteBatch.DrawString(_font, "Inventory:", Position + new Vector2(10f, y), Color.Black);

      y += 20f;
      foreach (var item in _villager.Inventory)
      {
        spriteBatch.DrawString(_font, $"{item.Key}: {item.Value.Count}", Position + new Vector2(10f, y), Color.Black);
        y += 20f;
      }
    }
  }
}
