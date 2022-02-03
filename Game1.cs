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
    private int _previousGameSpeed = 0;

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
      if (GameSpeed == 0)
        SetGameSpeed(gameTime);

      for (int i = 0; i < GameSpeed; i++)
      {
        SetGameSpeed(gameTime);

        _state.Update(gameTime);
      }
    }

    private void SetGameSpeed(GameTime gameTime)
    {
      base.Update(gameTime);

      if (GameKeyboard.IsKeyPressed(Keys.Space))
      {
        if (GameSpeed > 0)
        {
          _previousGameSpeed = GameSpeed;
          GameSpeed = 0;
        }
        else
        {
          GameSpeed = _previousGameSpeed;
        }
      }
      if (GameKeyboard.IsKeyPressed(Keys.D1))
        GameSpeed = 1;
      if (GameKeyboard.IsKeyPressed(Keys.D2))
        GameSpeed = 2;
      if (GameKeyboard.IsKeyPressed(Keys.D3))
        GameSpeed = 3;
    }

    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(Color.CornflowerBlue);

      _state.Draw(gameTime);

      base.Draw(gameTime);
    }
  }
}
