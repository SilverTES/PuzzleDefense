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
            Swap,
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
        public static float Radius = 40;

        public bool IsSelected = false;
        public bool IsSameColor = false;    // true if found some close gems with same color
        public int NbSameColor = 0;

        public Point MapPosition { get; private set; }

        Arena _arena;

        int _tempoMove = 12;
        int _ticMove = 0;
        Vector2 _from;
        Vector2 _goal;
        public bool IsFinishMove = true;

        public Point GoalPosition;
        public bool IsFall = false;

        float _radius;
        float _ticRadius = 0;

        int _tempoDead = 32;

        public Gem(Arena arena, Point mapPosition, Color color) 
        {

            _type = UID.Get<Gem>();

            MapPosition = mapPosition;
            Color = color;
            _arena = arena;
            _radius = Radius;

            SetSize(Vector2.One * Radius);

            SetPosition(arena.MapPositionToVector2(mapPosition));

            SetState((int)States.None);

        }
        public void ExploseMe()
        {
            new FxExplose(AbsXY, Color, 20, 40).AppendTo(_parent);
            new PopInfo(NbSameColor.ToString(), Color.White, Color, 0, 32, 32).SetPosition(XY).AppendTo(_parent);

            Game1._soundPop.Play(.4f * Game1.VolumeMaster, .5f, 0f);

            ChangeState((int)States.Dead);
        }
        public static Color RandomColor()
        {
            return Colors[Misc.Rng.Next(0, Colors.Length)];
        }
        public void MoveTo(Point goalPosition)
        {
            IsFinishMove = false;
            _tempoMove = 30;
            GoalPosition = goalPosition;

            _arena.SetInGrid(goalPosition, this);

            _from = _arena.MapPositionToVector2(MapPosition);
            _goal = _arena.MapPositionToVector2(goalPosition);

            ChangeState((int)States.Move);
            //Console.WriteLine("Gem Move Down");
        }
        public void SwapTo(Point goalPosition)
        {
            IsFinishMove = false;
            _tempoMove = 12;
            GoalPosition = goalPosition;

            _arena.SetInGrid(goalPosition, this);

            _from = _arena.MapPositionToVector2(MapPosition);
            _goal = _arena.MapPositionToVector2(goalPosition);

            ChangeState((int)States.Swap);
            //Console.WriteLine("Gem Move Down");
        }
        protected override void RunState(GameTime gameTime)
        {
            switch ((States)_state)
            {
                case States.None:

                    break;

                case States.Move:

                    _x = Easing.GetValue(Easing.BounceEaseOut, _ticMove, _from.X, _goal.X, _tempoMove);
                    _y = Easing.GetValue(Easing.BounceEaseOut, _ticMove, _from.Y, _goal.Y, _tempoMove);

                    _ticMove++;

                    if (_ticMove > _tempoMove)
                    {
                        _ticMove = 0;

                        MapPosition = GoalPosition;

                        ChangeState((int)States.None);
                        IsFinishMove = true;
                    }

                    break;

                case States.Swap:

                    _x = Easing.GetValue(Easing.QuadraticEaseOut, _ticMove, _from.X, _goal.X, _tempoMove);
                    _y = Easing.GetValue(Easing.QuadraticEaseOut, _ticMove, _from.Y, _goal.Y, _tempoMove);

                    _ticMove++;

                    if (_ticMove > _tempoMove)
                    {
                        _ticMove = 0;

                        MapPosition = GoalPosition;

                        ChangeState((int)States.None);
                        IsFinishMove = true;
                    }

                    break;

                case States.Dead:

                    _tempoDead--;

                    if (_tempoDead <= 0)
                    {
                        KillMe();
                    }

                    _ticRadius++;

                    _radius = Easing.GetValue(Easing.BounceEaseOut, _ticRadius, Radius, 0, 32);

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
                //batch.FilledCircle(Game1._texCircle, AbsXY, _radius + 2, Color.Black * .5f);
                batch.FilledCircle(Game1._texCircle, AbsXY, _radius, Color * .5f);
                batch.FilledCircle(Game1._texCircle, AbsXY, _radius - 8, Color * .75f);
                batch.FilledCircle(Game1._texCircle, AbsXY, _radius - 16, Color * 1f);
                
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
