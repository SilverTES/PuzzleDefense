﻿using Microsoft.Xna.Framework;
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
        Container _divTop;
        Container _divBottom;

        Vector2 _mousePos;
        public ScreenPlay() 
        {
            SetSize(Game1.ScreenW, Game1.ScreenH);

            _arena[0] = (Arena)new Arena(new Point(6, 6), new Vector2(64, 64), PlayerIndex.One).AppendTo(this);
            _arena[1] = (Arena)new Arena(new Point(6, 6), new Vector2(64, 64), PlayerIndex.Two).AppendTo(this);

            _divMain = new Container(Style.Space.One * 10, Style.Space.One * 10, Mugen.Physics.Position.VERTICAL);
            _divTop = new Container(Style.Space.One * 10, Style.Space.One * 10, Mugen.Physics.Position.HORIZONTAL);
            _divBottom = new Container(Style.Space.One * 10, Style.Space.One * 20, Mugen.Physics.Position.HORIZONTAL);


            _divBottom.Insert(_arena[0]);
            _divBottom.Insert(_arena[1]);


            _divMain.Insert(_divTop);
            _divMain.Insert(_divBottom);
            _divMain.SetPosition((Game1.ScreenW - _divMain.Rect.Width) / 2, (Game1.ScreenH - _divMain.Rect.Height) / 2);
            _divMain.Refresh();

            _arena[0].InitGridRandom();

        }
        public override Node Update(GameTime gameTime)
        {
            _mousePos = WindowManager.GetMousePosition();

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            batch.GraphicsDevice.Clear(Color.Transparent);
            
            if (indexLayer == (int)Game1.Layers.Main) 
            {
                //batch.GraphicsDevice.Clear(Color.DarkSlateBlue);
                batch.FillRectangle(AbsRectF, Color.Black * .25f);
                //batch.Grid(Vector2.Zero, Game1.ScreenW, Game1.ScreenH, 40, 40, Color.Black * .5f);

                //batch.LineTexture(Game1._texLine, Vector2.One * 10, _mousePos, 5, Color.Gold);
            }

            if (indexLayer == (int)Game1.Layers.BackFX) 
            {
                batch.Draw(Game1._texBG00, new Rectangle(-4, -4, Game1.ScreenW, Game1.ScreenH), Color.White);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
