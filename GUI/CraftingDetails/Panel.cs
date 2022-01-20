using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Others.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Others.GUI.CraftingDetails
{
  public class Panel
  {
    private readonly ContentManager _content;

    private readonly Texture2D _texture;

    private readonly SpriteFont _font;

    private string _mainHeader;

    private Dictionary<string, string[]> info = new Dictionary<string, string[]>();

    public Vector2 Position { get; set; }

    public Panel(ContentManager content)
    {
      _content = content;
      _texture = _content.Load<Texture2D>("GUI/VillagerDetails/Panel");
      _font = _content.Load<SpriteFont>("Font");
      Position = new Vector2(0, Game1.ScreenHeight - _texture.Height);
    }

    public void SetMainHeader(string header)
    {
      _mainHeader = header;
    }

    public void AddSection(string header, params string[] content)
    {
      if (info.ContainsKey(header))
        return; // We ain't dealing with that

      info.Add(header, content);
    }

    public void Clear()
    {
      _mainHeader = "";
      info = new Dictionary<string, string[]>();
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
      if (info.Count == 0)
        return;


      spriteBatch.Draw(_texture, Position, Color.White);

      var position = Position + new Vector2(10f, 20f);

      if (!string.IsNullOrEmpty(_mainHeader))
      {
        spriteBatch.DrawString(_font, _mainHeader, position, Color.Black);

        position.Y += _font.MeasureString(_mainHeader).Y + 20f;
      }

      foreach (var value in info)
      {
        var sectionHeader = value.Key + ":";

        spriteBatch.DrawString(_font, sectionHeader, position, Color.Black);
        position.Y += _font.MeasureString(sectionHeader).Y + 10f;

        foreach (var content in value.Value)
        {
          spriteBatch.DrawString(_font, content, position, Color.Black);
          position.Y += _font.MeasureString(content).Y + 5f;
        }

        position.Y += 10f;
      }
    }
  }
}
