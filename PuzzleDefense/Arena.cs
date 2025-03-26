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

        Vector2 _offSetGem;
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
            _grid.Set(mapPosition.X, mapPosition.Y, gem);

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

            KillAll(UID.Get<Gem>());
        }
        public void Shuffle()
        {
            ClearGrid();
            InitGrid();
        }
        public void InitGrid()
        {
            for (int i = 0; i < GridSize.X; i++)
            {
                for (int j = 0; j < GridSize.Y; j++)
                {
                    //var gem = (Gem)new Gem(this, new Point(i, j), Gem.RandomColor()).AppendTo(this);
                    //_grid.Set(i, j, gem);
                    //AddGem(new Point(i, j), Gem.RandomColor());
                    AddGem(new Point(i, j), GetSafeColor(i, j));
                }
            }
        }
        // Obtenir une couleur qui ne crée pas de match-3
        private Color GetSafeColor(int x, int y)
        {
            while (true)
            {
                var color = Gem.RandomColor(); // Couleurs de 1 à 4

                // Vérifier horizontalement (à gauche)
                var left1 = GetGem(new Point(x - 1, y));
                var left2 = GetGem(new Point(x - 2, y));

                if (left1 != null && left2 != null)
                    if (x >= 2 && left1.Color == color && left2.Color == color)
                        continue;

                // Vérifier verticalement (au-dessus)
                var up1 = GetGem(new Point(x, y - 1));
                var up2 = GetGem(new Point(x, y - 2));
                
                if (up1 != null && up2 != null)
                    if (y >= 2 && up1.Color == color && up2.Color == color)
                        continue;

                // Si aucune condition de match-3 n'est violée, accepter cette couleur
                return color;
            }
        }
        public bool HasMatch3()
        {
            // Vérifier horizontalement
            for (int y = 0; y < _grid.Height; y++)
            {
                for (int x = 0; x < _grid.Width - 2; x++)
                {
                    var gem = GetGem(new Point(x, y));
                    var gemRight1 = GetGem(new Point(x + 1, y));
                    var gemRight2 = GetGem(new Point(x + 2, y));

                    if (gem != null && gemRight1 != null && gemRight2 != null)
                        if (gem == gemRight1 && gem == gemRight2)
                            return true;
                }
            }

            // Vérifier verticalement
            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height - 2; y++)
                {
                    var gem = GetGem(new Point(x, y));
                    var gemDown1 = GetGem(new Point(x, y + 1));
                    var gemDown2 = GetGem(new Point(x, y + 2));

                    if (gem == gemDown1 && gem == gemDown2)
                        return true;
                }
            }

            return false;
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

                    if (Math.Abs(_swapDirection.X) > 0 && Math.Abs(_swapDirection.Y) > 0) 
                        _swapDirection = Point.Zero;

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

                    _cursorPos.X = CurGemOver._x + _offSetGem.X;
                    _cursorPos.Y = CurGemOver._y + _offSetGem.Y;

                    if (CurGemOver.GetState() == (int)Gem.States.None &&
                        CurGemToSwap.GetState() == (int)Gem.States.None)

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
            if (_pad.Buttons.A == ButtonState.Released)
            {
                _cursorPos.X += _stick.X;
                _cursorPos.Y += _stick.Y;
            }

            _cursorPos = Vector2.Clamp(_cursorPos, Vector2.One * (CellSize / 2), new Vector2(_rect.Width, _rect.Height) - CellSize / 2);

            _prevMapCursor = _mapCursor;
            _mapCursor = Vector2ToMapPosition(_cursorPos);

            _rectCursor.Position = Vector2ToMapPosition(_cursorPos).ToVector2() * CellSize;

            _rectCursor.Width = CellSize.X;
            _rectCursor.Height = CellSize.Y;

            var prevGem = GetGem(_prevMapCursor);
            if (prevGem != null)
                prevGem.IsSelected = false;

            CurGemOver = GetGem(_mapCursor);
            if (CurGemOver != null)
                CurGemOver.IsSelected = true;


            if (_controller[A])
            {
                //Misc.Log("A pressed");

                _offSetGem = _cursorPos - CurGemOver.XY;
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
                //batch.FilledCircle(Game1._texCircle, AbsXY + _cursorPos, 16, Color.White * .5f);

                batch.Draw(Game1._texCursorA,new Rectangle((AbsXY + _cursorPos - Vector2.One * 10).ToPoint(), (Game1._texCursorA.Bounds.Size.ToVector2()/ 3).ToPoint()), Color.White);
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
