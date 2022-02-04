using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using ZonerEngine.GL.Components;
using ZonerEngine.GL.Entities;

namespace Others.Entities
{
  public class DrawingSquare : Entity
  {
    private Texture2D _texture;

    private Color _colour;

    public DrawingSquare(Texture2D texture, Color colour)
    {
      _texture = texture;
      _colour = colour;
    }

    public override void LoadContent()
    {
      AddComponent(new TextureComponent(this, _texture) { Colour = _colour, });
    }
  }
}
