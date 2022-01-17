using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Others.States;
using ZonerEngine.GL;

namespace Others
{
  public class Game1 : ZonerGame
  {
    public const int TileSize = 40;

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
      //for(int i =0; i < 5; i++)
      _state.Update(gameTime);

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
