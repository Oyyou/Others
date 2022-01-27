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

    public float Layer { get; set; }

    public bool IsVisible { get; set; } = false;

    public bool IsDrawingVisible { get; private set; }

    public Vector2 DrawPosition
    {
      get
      {
        return Parent != null ? Parent.DrawPosition + Position : Position;
      }
    }

    public float DrawLayer
    {
      get
      {
        return Parent != null ? Parent.DrawLayer + 0.01f : Layer;
      }
    }

    public abstract Rectangle Rectangle { get; }// => new Rectangle((int)DrawPosition.X, (int)DrawPosition.Y, 10, 10);// _texture.Width, _texture.Height); // TODO: Fix

    public bool IsMouseOver { get; protected set; } = false;

    public bool IsMouseDown { get; protected set; } = false;

    public bool IsMouseClicked { get; protected set; } = false;

    public Action OnHover { get; set; } = null;

    public Action OnClicked { get; set; } = null;

    public Func<bool> GetVisibility = null;

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
        OnHover?.Invoke();

        if (GameMouse.IsLeftPressed)
        {
          IsMouseDown = true;
        }

        if (GameMouse.IsLeftClicked)
        {
          IsMouseClicked = true;
          OnClicked?.Invoke();
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
