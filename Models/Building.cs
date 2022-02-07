using Microsoft.Xna.Framework;
using Others.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Others.Models
{
  public class Building
  {
    public Dictionary<Point, Place> Walls { get; set; } = new Dictionary<Point, Place>();

    public Dictionary<Point, Place> Doors { get; set; } = new Dictionary<Point, Place>();

    public Rectangle Rectangle { get; set; }
  }
}
