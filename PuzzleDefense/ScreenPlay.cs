using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using Mugen.GUI;

namespace PuzzleDefense
{
    internal class ScreenPlay : Node
    {
        Arena[] _arena = new Arena[4];

        Container _divMain;
        float _angle;

        Vector2 _mousePos;
        public ScreenPlay() 
        {
            _arena[0] = (Arena)new Arena(new Point(8, 8), new Vector2(64, 64), PlayerIndex.One).AppendTo(this);

            _divMain = new Container(Style.Space.One * 10, Style.Space.One * 10, Mugen.Physics.Position.VERTICAL);
            _divMain.AppendTo(this);
            
            _divMain.Insert(_arena[0]);
            _divMain.SetPosition((Game1.ScreenW - _divMain.Rect.Width) / 2, (Game1.ScreenH - _divMain.Rect.Height) / 2);
            _divMain.Refresh();


        }
        public override Node Update(GameTime gameTime)
        {
            _mousePos = WindowManager.GetMousePosition();

            _angle += .005f;

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            batch.GraphicsDevice.Clear(Color.Transparent);
            
            if (indexLayer == (int)Game1.Layers.Main) 
            {
                batch.GraphicsDevice.Clear(Color.DarkSlateBlue);
                batch.Grid(Vector2.Zero, Game1.ScreenW, Game1.ScreenH, 40, 40, Color.DarkGray * .5f);

                batch.Circle(Vector2.One * 400, 160, 8, Color.Yellow, 3f, _angle);

                batch.LineTexture(Game1._texLine, Vector2.One * 10, _mousePos, 5, Color.Gold);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
