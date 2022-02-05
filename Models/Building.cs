using Microsoft.Xna.Framework;
using Others.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Others.Models
{
  public class Building
  {
    public Dictionary<Point, Wall> Walls { get; set; } = new Dictionary<Point, Wall>();

    public Dictionary<Point, Basic> Doors { get; set; } = new Dictionary<Point, Basic>();

    public Rectangle Rectangle { get; set; }
  }
}
