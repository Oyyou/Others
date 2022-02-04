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
    // Forgive my naming
    public enum States
    {
      Cursor,
      TooSmall,
      Colliding,
      Fine,
    }

    private Texture2D _texture;

    private Color _colour
    {
      get
      {
        switch (State)
        {
          case States.Cursor:
            return Color.Blue;

          case States.TooSmall:
            return Color.Yellow;

          case States.Colliding:
            return Color.Red;

          case States.Fine:
            return Color.Green;

          default:
            return Color.Green;
        }
      }
    }

    public readonly States State;

    public DrawingSquare(Texture2D texture, States state)
    {
      _texture = texture;
      State = state;
    }

    public override void LoadContent()
    {
      AddComponent(new TextureComponent(this, _texture) { Colour = _colour, });
    }
  }
}
