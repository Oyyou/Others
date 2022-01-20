using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Others.States;
using ZonerEngine.GL;
using ZonerEngine.GL.Input;

namespace Others
{
  public class Game1 : ZonerGame
  {
    public const int TileSize = 40;

    public int GameSpeed { get; private set; } = 1;

    public Game1()
    {

    }

    protected override void Initialize()
    {
      // TODO: Add your initialization logic here
      IsMouseVisible = true;
      base.Initialize();
    }

    protected override void LoadContent()
    {
      _state = new BattleState(GameModel);
      _state.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
      if (GameKeyboard.IsKeyPressed(Keys.D1))
        GameSpeed = 1;
      if (GameKeyboard.IsKeyPressed(Keys.D2))
        GameSpeed = 2;
      if (GameKeyboard.IsKeyPressed(Keys.D3))
        GameSpeed = 3;


      for (int i = 0; i < GameSpeed; i++)
      {
        _state.Update(gameTime);
      }

      base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(Color.CornflowerBlue);

      _state.Draw(gameTime);

      base.Draw(gameTime);
    }
  }
}
