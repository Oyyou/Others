using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZonerEngine.GL;
using ZonerEngine.GL.Input;

namespace Others.Controls
{
  public abstract class Control : IClickable
  {
    public Control Parent { get; private set; }

    public List<Control> Children { get; private set; } = new List<Control>();

    public Vector2 Position { get; set; }

    public float Layer { get; set; }

    public float LayerOffset { get; set; }

    public bool IsVisible { get; set; } = true;

    public bool IsDrawingVisible { get; private set; }

    public List<string> Tags { get; set; } = new List<string>();

    public bool IsFixedPosition { get; set; } = false;

    public Vector2 DrawPosition
    {
      get
      {
        return Parent != null ? (Parent.DrawPosition + Position + (!IsFixedPosition ? Parent.ChildrenOffset : Vector2.Zero)) : Position;
      }
    }

    public float DrawLayer
    {
      get
      {
        return (Parent != null ? Parent.DrawLayer + 0.01f : Layer) + LayerOffset;
      }
    }

    public float ClickLayer => DrawLayer;

    public abstract Rectangle ClickRectangle { get; }// => new Rectangle((int)DrawPosition.X, (int)DrawPosition.Y, 10, 10);// _texture.Width, _texture.Height); // TODO: Fix

    public virtual bool ClickIsVisible => Parent != null ? Parent.IsVisible && IsVisible : IsVisible;

    public Vector2 ChildrenOffset = Vector2.Zero;

    public Rectangle Viewport = new Rectangle(0, 0, ZonerGame.ScreenWidth, ZonerGame.ScreenHeight);

    public Rectangle DrawViewport
    {
      get
      {
        return Parent != null ? Parent.DrawViewport : Viewport;
      }
    }

    public bool HasViewport
    {
      get
      {
        return DrawViewport.Width > 0 && DrawViewport.Height > 0;
      }
    }

    public Vector2 MousePosition
    {
      get
      {
        if (HasViewport)
          return GameMouse.Position.ToVector2() - new Vector2(DrawViewport.X, DrawViewport.Y);

        return GameMouse.Position.ToVector2();
      }
    }

    public Rectangle MouseRectangle
    {
      get
      {
        return new Rectangle((int)MousePosition.X, (int)MousePosition.Y, 1, 1);
      }
    }

    public bool IsMouseOver { get; protected set; } = false;

    public bool IsHeld { get; protected set; } = false;

    public bool IsMouseDown { get; protected set; } = false;

    public bool IsMouseClicked { get; protected set; } = false;

    public Action OnHover { get; set; } = null;

    public Action OnHeld { get; set; } = null;

    public Action<Control> OnClicked { get; set; } = null;

    public Action<Control> OnAddChild { get; set; } = null;

    public bool IsClickable
    {
      get
      {
        return GameMouse.ValidObject == this ||
          this.Children.Any(c => c.IsClickable);
      }
    }

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

      OnAddChild?.Invoke(this);
    }

    public void RemoveChild(Control control)
    {
      Children.Remove(control);
    }

    public virtual void Update(GameTime gameTime)
    {
      if (!IsVisible)
        return;

      IsMouseOver = false;
      IsMouseDown = false;
      IsMouseClicked = false;

      if (MouseRectangle.Intersects(ClickRectangle))
      {
        GameMouse.AddObject(this);

        if (IsClickable)
        {
          IsMouseOver = true;
          OnHover?.Invoke();

          if (GameMouse.IsLeftPressed)
          {
            IsMouseDown = true;
            IsHeld = true;
          }
          else
          {
            IsHeld = false;
          }

          if (GameMouse.IsLeftClicked)
          {
            IsMouseClicked = true;
            OnClicked?.Invoke(this);
          }
        }
      }
      else
      {
        if (!GameMouse.IsLeftPressed)
        {
          IsHeld = false;
        }
      }

      if (IsHeld)
      {
        OnHeld?.Invoke();
      }

      UpdateChildren(gameTime);
    }

    protected void UpdateChildren(GameTime gameTime)
    {
      foreach (var child in Children)
      {
        if (!child.IsVisible)
          continue;

        child.Update(gameTime);
      }
    }

    public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
      if (!IsVisible)
        return;

      DrawChildren(gameTime, spriteBatch);
    }

    protected void DrawChildren(GameTime gameTime, SpriteBatch spriteBatch)
    {
      foreach (var child in Children)
      {
        if (!child.IsVisible)
          continue;

        child.Draw(gameTime, spriteBatch);
      }
    }

    public void AddTag(string value)
    {
      var tag = value.Trim().ToUpper();
      if (HasTag(tag))
        return;

      Tags.Add(tag);
    }

    public bool HasTag(string value)
    {
      var v = value.Trim().ToUpper();

      foreach (var tag in Tags)
      {
        if (tag.Trim().ToUpper() == v)
          return true;
      }

      return false;
    }
  }
}
