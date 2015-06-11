using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStateManagement;
using GameStateManagementSample;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BlackholeBattle
{
    class InstructionMenuScreen : GameScreen
    {
        public InstructionMenuScreen()
        {

        }
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                ExitScreen();
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, "Insert Instructions", new Vector2(50,50), Color.Yellow);
            ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
