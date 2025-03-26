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
        public static float Radius = 48;

        public bool IsSelected = false;
        public bool IsSameColor = false;    // true if found some close gems with same color
        public int NbSameColor = 0;

        Point _mapPosition;

        Arena _arena;
        public Gem(Arena arena, Point mapPosition, Color color) 
        {

            _type = UID.Get<Gem>();

            _mapPosition = mapPosition;
            Color = color;
            _arena = arena;

            SetSize(Vector2.One * Radius);

            SetPosition(arena.GetVectorByMapPosition(mapPosition));

        }
        public static Color RandomColor()
        {
            return Colors[Misc.Rng.Next(0, Colors.Length)];
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Game1.Layers.Main) 
            {
                batch.FilledCircle(Game1._texCircle, AbsXY, Radius, Color * .5f);
                batch.FilledCircle(Game1._texCircle, AbsXY, Radius - 8, Color * .75f);
                
            }
            if (indexLayer == (int)Game1.Layers.Main)
            {
                if (IsSelected)
                {
                    batch.FilledCircle(Game1._texCircle, AbsXY, Radius - 12, Color.White * 1f);
                }
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
