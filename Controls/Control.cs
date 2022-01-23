using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using ZonerEngine.GL.Input;

namespace Others.Controls
{
  public abstract class Control
  {
    public Control Parent { get; private set; }

    public List<Control> Children { get; private set; } = new List<Control>();

    public Vector2 Position { get; set; }

    public Vector2 DrawPosition
    {
      get
      {
        return Parent != null ? Parent.Position + Position : Position;
      }
    }

    public Rectangle Rectangle => new Rectangle((int)DrawPosition.X, (int)DrawPosition.Y, 10, 10);// _texture.Width, _texture.Height); // TODO: Fix

    public bool IsMouseOver { get; protected set; } = false;

    public bool IsMouseDown { get; protected set; } = false;

    public bool IsMouseClicked { get; protected set; } = false;

    public Control()
    {

    }

    public void LoadContent()
    {

    }

    public void AssignParent(Control parent)
    {
      Parent = parent;
    }

    public void AddChild(Control control)
    {
      control.AssignParent(this);
      Children.Add(control);
    }

    public virtual void Update(GameTime gameTime)
    {
      IsMouseOver = false;
      IsMouseDown = false;
      IsMouseClicked = false;

      if (GameMouse.Intersects(Rectangle))
      {
        IsMouseOver = true;

        if (GameMouse.IsLeftPressed)
        {
          IsMouseDown = true;
        }

        if (GameMouse.IsLeftPressed)
        {
          IsMouseClicked = true;
        }
      }

      UpdateChildren(gameTime);
    }

    protected void UpdateChildren(GameTime gameTime)
    {
      foreach (var child in Children)
        child.Update(gameTime);
    }

    public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
      DrawChildren(gameTime, spriteBatch);
    }

    protected void DrawChildren(GameTime gameTime, SpriteBatch spriteBatch)
    {
      foreach (var child in Children)
        child.Draw(gameTime, spriteBatch);
    }
  }
}
