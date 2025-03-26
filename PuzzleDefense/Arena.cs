using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Input;
using Mugen.Physics;
using System;
using System.Collections.Generic;


namespace PuzzleDefense
{
    internal class Arena : Node 
    {
        public enum States
        {
            Play,
            SelectGemToSwap,
            SwapGems,
            ExploseGems,
            PushGems,
            AddGems,
        }

        public Point GridSize { get; private set; }
        public Vector2 CellSize { get; private set; }
        Grid2D<Gem> _grid;

        PlayerIndex _playerIndex;

        Point _prevMapCursor;
        Point _mapCursor;
        Vector2 _cursorPos;
        RectangleF _rectCursor;

        Vector2 _stick;
        Point _swapDirection = new Point();

        public Gem CurGemOver;
        public Gem CurGemToSwap;

        GamePadState _pad;
        KeyboardState _key;

        public const string A = "A";
        public const string B = "B";

        public const string Up = "Up";
        public const string Down = "Down";
        public const string Left = "Left";
        public const string Right = "Right";

        Dictionary<string, bool> _controller = new Dictionary<string, bool>()
        {
            { A, false },
            { B, false },
            { Up, false },
            { Down, false },
            { Left, false },
            { Right, false },
        };

        public Arena(Point gridSize, Vector2 cellSize, PlayerIndex playerIndex) 
        { 
            GridSize = gridSize;
            CellSize = cellSize;

            _playerIndex = playerIndex;

            _grid = new Grid2D<Gem>(gridSize.X, gridSize.Y);

            SetSize(gridSize.ToVector2() * cellSize);

            SetState((int)States.Play);
        }
        public bool IsIngrid(Point mapPosition)
        {
            return _grid.IsInGrid(mapPosition.X, mapPosition.Y);
        }
        public bool AddGem(Point mapPosition, Color color)
        {
            if (!IsIngrid(mapPosition)) 
                return false;

            var gem = (Gem)new Gem(this, mapPosition, color).AppendTo(this);

            return true;
        }
        public Gem GetGem(Point mapPosition)
        {
            if (!IsIngrid(mapPosition))
                return null;

            return _grid.Get(mapPosition.X, mapPosition.Y);
        }
        public Gem SetGem(Point mapPosition, Gem gem)
        {
            if (!IsIngrid(mapPosition))
                return null;

            return _grid.Set(mapPosition.X, mapPosition.Y, gem);
        }
        public void ClearGrid()
        {
            for (int i = 0; i < GridSize.X; i++)
            {
                for (int j = 0; j < GridSize.Y; j++)
                {
                    _grid.Set(i, j, null);
                }
            }
        }
        public void InitGridRandom()
        {
            for (int i = 0; i < GridSize.X; i++)
            {
                for (int j = 0; j < GridSize.Y; j++)
                {
                    var gem = (Gem)new Gem(this, new Point(i, j), Gem.RandomColor()).AppendTo(this);
                    _grid.Set(i, j, gem);
                }
            }
        }
        public Vector2 MapPositionToVector2(Point mapPosition)
        {
            return new Vector2(mapPosition.X * CellSize.X, mapPosition.Y * CellSize.Y) + CellSize / 2;
        }
        public Point Vector2ToMapPosition(Vector2 position)
        {
            return new Point((int)Math.Floor(position.X / CellSize.X), (int)Math.Floor(position.Y / CellSize.Y));
        }
        private void HandleInput()
        {
            _key = Keyboard.GetState();
            _pad = GamePad.GetState(_playerIndex);

            _controller[A] = ButtonControl.OnePress(A + $"{_playerIndex}", _pad.Buttons.A == ButtonState.Pressed);
            _controller[B] = ButtonControl.OnePress(B + $"{_playerIndex}", _pad.Buttons.B == ButtonState.Pressed);

            //_controller[Up] = ButtonControl.OnPress(Up, _pad.DPad.Up == ButtonState.Pressed || _key.IsKeyDown(Keys.Up), 8);
            //_controller[Down] = ButtonControl.OnPress(Down, _pad.DPad.Down == ButtonState.Pressed || _key.IsKeyDown(Keys.Down), 8);
            //_controller[Left] = ButtonControl.OnPress(Left, _pad.DPad.Left == ButtonState.Pressed || _key.IsKeyDown(Keys.Left), 8);
            //_controller[Right] = ButtonControl.OnPress(Right, _pad.DPad.Right == ButtonState.Pressed || _key.IsKeyDown(Keys.Right), 8);

            //if (_controller[Up]) _mapCursor.Y += -1;
            //if (_controller[Down]) _mapCursor.Y += 1;
            //if (_controller[Left]) _mapCursor.X += -1;
            //if (_controller[Right]) _mapCursor.X += 1;

            //_rectCursor.Position = _mapCursor.ToVector2() * CellSize;

            _stick.X = _pad.ThumbSticks.Left.X * 10;
            _stick.Y = _pad.ThumbSticks.Left.Y * -10;

        }
        public bool SwapGem(Gem gemA, Gem gemB)
        {
            if (gemA == null || gemB == null)
                return false;

            var tempPositionA = gemA.MapPosition;

            gemA.MoveTo(gemB.MapPosition);
            gemB.MoveTo(tempPositionA);

            return true;
        }
        protected override void RunState(GameTime gameTime)
        {
            switch ((States)_state)
            {
                case States.Play:

                    Play();

                    break;

                case States.SelectGemToSwap:

                    _swapDirection.X = _stick.X > 0 ? 1 : _stick.X < 0 ? -1 : 0;
                    _swapDirection.Y = _stick.Y > 0 ? 1 : _stick.Y < 0 ? -1 : 0;

                    CurGemToSwap = GetGem(_mapCursor + _swapDirection);
                    if (CurGemToSwap != null)
                    {
                        if (CurGemToSwap != CurGemOver)
                        {
                            //if (_pad.Buttons.A == ButtonState.Pressed)
                            {
                                SwapGem(CurGemOver, CurGemToSwap);
                                ChangeState((int)States.SwapGems);
                            }
                        }
                    }

                    break;

                case States.SwapGems:

                    ChangeState((int)States.Play);

                    break;

                case States.ExploseGems:
                    break;

                case States.PushGems:
                    break;

                case States.AddGems:
                    break;

                default:
                    break;
            }

            base.RunState(gameTime);
        }
        private void Play()
        {
            _cursorPos.X += _pad.ThumbSticks.Left.X * 10;
            _cursorPos.Y += _pad.ThumbSticks.Left.Y * -10;

            _cursorPos = Vector2.Clamp(_cursorPos, Vector2.One * (CellSize / 2), new Vector2(_rect.Width, _rect.Height) - CellSize / 2);

            _prevMapCursor = _mapCursor;
            _mapCursor = Vector2ToMapPosition(_cursorPos);

            _rectCursor.Position = Vector2ToMapPosition(_cursorPos).ToVector2() * CellSize;

            _rectCursor.Width = CellSize.X;
            _rectCursor.Height = CellSize.Y;


            if (_mapCursor != _prevMapCursor)
            {
                var prevGem = GetGem(_prevMapCursor);
                if (prevGem != null)
                    prevGem.IsSelected = false;

                CurGemOver = GetGem(_mapCursor);
                if (CurGemOver != null)
                    CurGemOver.IsSelected = true;
            }


            if (_controller[A])
            {
                Misc.Log("A pressed");

                ChangeState((int)States.SelectGemToSwap);
            }


        }
        public override Node Update(GameTime gameTime)
        {
            
            HandleInput();

            RunState(gameTime);

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Game1.Layers.Main)
            {
                batch.FillRectangle(AbsRect, Color.Black * .5f);
                batch.Grid(AbsXY, _rect.Width, _rect.Height, CellSize.X, CellSize.Y, Color.Black * .5f, 3f);
                batch.Rectangle(AbsRectF.Extend(10f), Color.RoyalBlue * 1f, 3f);

                //batch.Rectangle(AbsXY + _cursorPos.ToVector2() * CellSize, CellSize, Color.Yellow, 3f);

            }

            if (indexLayer == (int)Game1.Layers.HUD)
            {
                batch.FillRectangle(_rectCursor.Translate(AbsXY), Color.White * .25f);
                //batch.BevelledRectangle(_rectCursor.Translate(AbsXY), Vector2.One * 8, Color.White * .25f, 4f);
                batch.FilledCircle(Game1._texCircle, AbsXY + _cursorPos, 16, Color.White * .5f);
            }
            if (indexLayer == (int)Game1.Layers.Debug) 
            {
                batch.CenterStringXY(Game1._fontMain, $"{_playerIndex} : {_swapDirection}", AbsRectF.TopCenter, Color.Yellow);
                batch.CenterStringXY(Game1._fontMain, $"{(States)_state}", AbsRectF.BottomCenter, Color.White);

                //if (CurGemOver != null)
                //    batch.Line(CurGemOver.AbsXY, CurGemOver.AbsXY + _stick * 5, Color.White, 8);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
