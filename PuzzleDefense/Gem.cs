using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;

namespace PuzzleDefense
{
    internal class Gem : Node
    {
        static public Color[] Colors =
        [
            Color.Red,
            Color.DodgerBlue,
            Color.ForestGreen,
            //Color.DarkOrange,
            //Color.BlueViolet,
            Color.Gold,
            Color.Gray, // Gem do nothing !
            //Color.HotPink,
        ];

        public Color Color;

        public static float Radius = 40;

        Arena _arena;
        public Gem(Arena arena, Color color) 
        { 
            Color = color;
            _arena = arena;
        }
        public override Node Update(GameTime gameTime)
        {
            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Game1.Layers.Main) 
            {
                batch.FilledCircle(Game1._texCircle, AbsXY, Radius, Color);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
