using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Animation;
using Mugen.Core;
using Mugen.GFX;

namespace PuzzleDefense
{
    internal class Gem : Node
    {
        public enum States
        {
            None,
            Move,
            Dead,
        }

        static public Color[] Colors =
        [
            Color.Red,
            Color.DodgerBlue,
            Color.ForestGreen,
            //Color.DarkOrange,
            Color.BlueViolet,
            Color.Gold,
            //Color.Gray, // Gem do nothing !
            //Color.HotPink,
        ];

        public Color Color;
        public static float Radius = 48;

        public bool IsSelected = false;
        public bool IsSameColor = false;    // true if found some close gems with same color
        public int NbSameColor = 0;

        public Point MapPosition { get; private set; }

        Arena _arena;

        int _tempoMove = 12;
        int _ticMove = 0;
        Vector2 _from;
        Vector2 _goal;

        public Point GoalPosition;
        public bool IsFall = false;

        public Gem(Arena arena, Point mapPosition, Color color) 
        {

            _type = UID.Get<Gem>();

            MapPosition = mapPosition;
            Color = color;
            _arena = arena;

            SetSize(Vector2.One * Radius);

            SetPosition(arena.MapPositionToVector2(mapPosition));

            SetState((int)States.None);

        }
        public static Color RandomColor()
        {
            return Colors[Misc.Rng.Next(0, Colors.Length)];
        }
        public void MoveTo(Point goalPosition)
        {
            GoalPosition = goalPosition;

            _arena.SetGem(goalPosition, this);

            _from = _arena.MapPositionToVector2(MapPosition);
            _goal = _arena.MapPositionToVector2(goalPosition);

            ChangeState((int)States.Move);
            //Console.WriteLine("Gem Move Down");
        }
        protected override void RunState(GameTime gameTime)
        {
            switch ((States)_state)
            {
                case States.None:

                    break;

                case States.Move:

                    _x = Easing.GetValue(Easing.QuadraticEaseOut, _ticMove, _from.X, _goal.X, _tempoMove);
                    _y = Easing.GetValue(Easing.QuadraticEaseOut, _ticMove, _from.Y, _goal.Y, _tempoMove);

                    _ticMove++;

                    if (_ticMove > _tempoMove)
                    {
                        _ticMove = 0;

                        MapPosition = GoalPosition;

                        ChangeState((int)States.None);

                    }

                    break;

                case States.Dead:
                    break;

                default:
                    break;
            }

            base.RunState(gameTime);
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            RunState(gameTime);

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Game1.Layers.Main) 
            {
                batch.FilledCircle(Game1._texCircle, AbsXY, Radius, Color * .25f);
                batch.FilledCircle(Game1._texCircle, AbsXY, Radius - 8, Color * .5f);
                batch.FilledCircle(Game1._texCircle, AbsXY, Radius - 16, Color * .75f);
                
                if (IsSelected)
                {
                    //batch.FilledCircle(Game1._texCircle, AbsXY, Radius + 8, Color.White * 1f);
                    //batch.Circle(AbsXY, Radius/2 + 4, 24, Color.White * 1f, 3f);
                }

            }
            if (indexLayer == (int)Game1.Layers.BackFX)
            {

            }

            if (indexLayer == (int)Game1.Layers.Debug)
            {
                if (IsSameColor)
                {
                    batch.Circle(AbsXY, Radius / 2 + 4, 24, Color.White * 1f, 3f);
                }
                //batch.CenterStringXY(Game1._fontMain, $"{(States)_state}", AbsXY, Color.White);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
