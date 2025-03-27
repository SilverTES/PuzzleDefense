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
            PushGemsToDown,
            AddNewGemsToDown,
            Action,
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

        
        int _score = 0;
        //private List<Gem> _matchedGems; // Liste des positions des gemmes matchées

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

            _cursorPos = _rect.Center;

            SetState((int)States.Play);
        }
        public bool IsIngrid(Point mapPosition)
        {
            return _grid.IsInGrid(mapPosition.X, mapPosition.Y);
        }
        public bool IsIngrid(int x, int y)
        {
            return _grid.IsInGrid(x, y);
        }
        public Gem AddInGrid(Point mapPosition, Color color)
        {
            var gem = (Gem)new Gem(this, mapPosition, color).AppendTo(this);
            SetInGrid(mapPosition.X, mapPosition.Y, gem);

            return gem;
        }
        public Gem DeleteInGrid(Gem gem)
        {
            _grid.Set(gem.MapPosition.X, gem.MapPosition.Y, null);

            return gem;
        }
        public void DeleteInGrid(Point mapPosition)
        {
            _grid.Set(mapPosition.X, mapPosition.Y, null);
        }
        public Gem GetInGrid(Point mapPosition)
        {
            return _grid.Get(mapPosition.X, mapPosition.Y);
        }
        public Gem GetInGrid(int x, int y)
        {
            return _grid.Get(x, y);
        }
        public Gem SetInGrid(Point mapPosition, Gem gem)
        {
            return _grid.Set(mapPosition.X, mapPosition.Y, gem);
        }
        public Gem SetInGrid(int x, int y, Gem gem)
        {
            return _grid.Set(x, y, gem);
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
                    AddInGrid(new Point(i, j), GetSafeColor(i, j));
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
                var left1 = GetInGrid(new Point(x - 1, y));
                var left2 = GetInGrid(new Point(x - 2, y));

                if (left1 != null && left2 != null)
                    if (x >= 2 && left1.Color == color && left2.Color == color)
                        continue;

                // Vérifier verticalement (au-dessus)
                var up1 = GetInGrid(new Point(x, y - 1));
                var up2 = GetInGrid(new Point(x, y - 2));
                
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
                    var gem = GetInGrid(x, y);
                    var gemRight1 = GetInGrid(x + 1, y);
                    var gemRight2 = GetInGrid(x + 2, y);

                    if (gem != null && gemRight1 != null && gemRight2 != null)
                        if (gem.Color == gemRight1.Color && gem.Color == gemRight2.Color)
                            return true;
                }
            }

            // Vérifier verticalement
            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height - 2; y++)
                {
                    var gem = GetInGrid(x, y);
                    var gemDown1 = GetInGrid(x, y + 1);
                    var gemDown2 = GetInGrid(x, y + 2);

                    if (gem != null && gemDown1 != null && gemDown2 != null)
                        if (gem.Color == gemDown1.Color && gem.Color == gemDown2.Color)
                            return true;
                }
            }

            return false;
        }
        // Trouver tous les match-3 (ou plus)
        public List<Gem> FindMatches()
        {
            var matchedGems = new List<Gem>();

            // Vérifier horizontalement
            for (int y = 0; y < _grid.Height; y++)
            {
                for (int x = 0; x < _grid.Width - 2; x++)
                {
                    var gem = GetInGrid(x, y);
                    var gemRight1 = GetInGrid(x + 1, y);
                    var gemRight2 = GetInGrid(x + 2, y);

                    if (gem != null && gemRight1 != null && gemRight2 != null)
                        if (x + 2 < _grid.Width && gemRight1.Color == gem.Color && gemRight2.Color == gem.Color)
                        {
                            // Ajouter toutes les gemmes du match à la liste
                            int matchLength = 3;
                            while (x + matchLength < _grid.Width && GetInGrid(x + matchLength, y) != null ? GetInGrid(x + matchLength, y)!.Color == gem.Color : false)
                                matchLength++;

                            for (int i = 0; i < matchLength; i++)
                                matchedGems.Add(GetInGrid(x + i, y));

                            x += matchLength - 1; // Sauter les gemmes déjà matchées
                        }
                }
            }

            // Vérifier verticalement
            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height - 2; y++)
                {
                    var gem = GetInGrid(x, y);
                    var gemDown1 = GetInGrid(x, y + 1);
                    var gemDown2 = GetInGrid(x, y + 2);

                    if (gem != null && gemDown1 != null && gemDown2 != null)
                        if (y + 2 < _grid.Height && gemDown1.Color == gem.Color && gemDown2.Color == gem.Color)
                        {
                            int matchLength = 3;
                            while (y + matchLength < _grid.Height && GetInGrid(x, y + matchLength) != null ? GetInGrid(x, y + matchLength).Color == gem.Color : false)
                                matchLength++;

                            for (int i = 0; i < matchLength; i++)
                                matchedGems.Add(GetInGrid(x, y + i));

                            y += matchLength - 1;
                        }
                }
            }

            return matchedGems;
        }
        public void ExploseGems()
        {
            var gems = FindMatches();
            foreach (var gem in gems)
            {
                gem.ExploseMe();
                DeleteInGrid(gem);
                _score++;
            }
        }
        public void PushGemsToDown()
        {
            for (int row = _grid.Height; row >= 0; row--)
            {
                for (int col = 0; col < _grid.Width; col++)
                {
                    var gem = _grid.Get(col, row);

                    if (gem != null)
                    {
                        gem.IsFall = false;
                        // scan vertical
                        for (int scanY = row + 1; scanY < _grid.Height; scanY++)
                        {
                            if (_grid.Get(col, scanY) == null)
                            {
                                gem.IsFall = true;
                                gem.GoalPosition = new Point(col, scanY);
                            }
                        }

                        if (gem.IsFall)
                        {
                            DeleteInGrid(gem);
                            gem.MoveTo(gem.GoalPosition);
                        }
                    }
                }
            }
        }
        public void AddNewGemsToDown()
        {
            for (int row = _grid.Height - 1; row >= 0; row--)
            {
                for (int col = 0; col < _grid.Width; col++)
                {
                    if (_grid.Get(col, row) == null)
                    {
                        var gem = AddInGrid(new Point(col, -1), Gem.RandomColor());
                        
                        if (gem != null)
                            gem.MoveTo(new Point(col, row));

                    }
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

            _controller[A] = ButtonControl.OnPress(A + $"{_playerIndex}", _pad.Buttons.A == ButtonState.Pressed, 8);
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
        public bool IsStickMove()
        {
            return Math.Abs(_stick.X) > 0 || Math.Abs(_stick.Y) > 0;
        }
        public bool SwapGem(Gem gemA, Gem gemB)
        {
            if (gemA == null || gemB == null)
                return false;

            var tempPositionA = gemA.MapPosition;

            gemA.SwapTo(gemB.MapPosition);
            gemB.SwapTo(tempPositionA);

            return true;
        }
        public bool IsAllGemsFinishMove()
        {
            var gems = GroupOf<Gem>();

            for (int i = 0; i < gems.Count; i++)
            {
                var gem = gems[i];
                if (gem == null) 
                    continue;

                if (gem.IsFinishMove == false)
                    return false;
            }

            return true;
        }
        protected override void RunState(GameTime gameTime)
        {
            switch ((States)_state)
            {
                case States.Play:

                    Play();

                    #region Debug
                    //var gems = GroupOf<Gem>();
                    //foreach (var gem in gems)
                    //{
                    //    gem.IsSameColor = false;
                    //}

                    //gems = FindMatches();
                    //foreach (var gem in gems)
                    //{
                    //    gem.IsSameColor = true;
                    //}
                    #endregion

                    break;

                case States.SelectGemToSwap:

                    if (_pad.Buttons.A == ButtonState.Released)
                        ChangeState((int)States.Play);

                    _swapDirection.X = _stick.X > 0 ? 1 : _stick.X < 0 ? -1 : 0;
                    _swapDirection.Y = _stick.Y > 0 ? 1 : _stick.Y < 0 ? -1 : 0;

                    if (Math.Abs(_swapDirection.X) > 0 && Math.Abs(_swapDirection.Y) > 0) 
                        _swapDirection = Point.Zero;

                    CurGemToSwap = GetInGrid(_mapCursor + _swapDirection);
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

                    _cursorPos = CurGemOver.XY + _offSetGem;

                    if (CurGemOver.GetState() == (int)Gem.States.None &&
                        CurGemToSwap.GetState() == (int)Gem.States.None)
                    {

                        if (HasMatch3())
                        {
                            ChangeState((int)States.ExploseGems);
                            //Game1._soundClick.Play(.5f * Game1.VolumeMaster, .05f, 0f);
                        }
                        else
                        {
                            // reSwap si pas de match 3
                            SwapGem(CurGemOver, CurGemToSwap);
                            ChangeState((int)States.Play);
                        }
                    }


                    break;

                case States.ExploseGems:

                    ExploseGems();
                    ChangeState((int)States.PushGemsToDown);

                    break;

                case States.PushGemsToDown:

                    PushGemsToDown();
                    ChangeState((int)States.AddNewGemsToDown);
                    break;

                case States.AddNewGemsToDown:

                    AddNewGemsToDown();
                    ChangeState((int)States.Action);

                    break;

                case States.Action:

                    ChangeState((int)States.Play);

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

            if (_mapCursor != _prevMapCursor)
                Game1._soundClick.Play(.5f * Game1.VolumeMaster, 1f, 0f);

            if (IsAllGemsFinishMove())
            {
                if (HasMatch3())
                {
                    Misc.Log("Match 3 found ! explose them all !");
                    ChangeState((int)States.ExploseGems);
                }
                else
                {
                    var prevGem = GetInGrid(_prevMapCursor);
                    if (prevGem != null)
                        prevGem.IsSelected = false;

                    CurGemOver = GetInGrid(_mapCursor);
                    if (CurGemOver != null)
                        CurGemOver.IsSelected = true;


                    if (_controller[A])
                    {
                        //Misc.Log("A pressed");
                        if (CurGemOver != null)
                        {
                            Game1._soundClick.Play(.5f * Game1.VolumeMaster, .1f, 0f);

                            _offSetGem = _cursorPos - CurGemOver.XY;
                            ChangeState((int)States.SelectGemToSwap);
                        }
                    }
                }
            }

            // Si le stick ne bouge pas alors on attire le curseur sur le CurGemOver
            if (!IsStickMove())
            {
                if (_offSetGem.Length() < Gem.Radius)
                {
                    _offSetGem /= 1.5f;
                    _cursorPos = MapPositionToVector2(_mapCursor) + _offSetGem;
                }
            }
            else
            {
                _offSetGem = _cursorPos - MapPositionToVector2(_mapCursor);
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

                if (_state == (int)States.SwapGems)
                    batch.FillRectangle(_rectCursor.Translate(AbsXY).Extend(-4f), Color.White * .5f);

                if (_state == (int)States.Play)
                    batch.FillRectangle(_rectCursor.Translate(AbsXY).Extend(-4f), Color.Black * .5f);

                //batch.Grid(AbsXY, _rect.Width, _rect.Height, CellSize.X, CellSize.Y, Color.Black * .5f, 3f);
                //batch.Rectangle(AbsRectF.Extend(4f), Color.Black * 1f, 3f);

                //batch.Rectangle(AbsXY + _cursorPos.ToVector2() * CellSize, CellSize, Color.Yellow, 3f);

            }

            if (indexLayer == (int)Game1.Layers.BackFX)
            {
            }
            if (indexLayer == (int)Game1.Layers.HUD)
            {
                //batch.BevelledRectangle(_rectCursor.Translate(AbsXY), Vector2.One * 8, Color.White * .25f, 4f);
                //batch.FilledCircle(Game1._texCircle, AbsXY + _cursorPos, 16, Color.White * .5f);

                batch.Draw(Game1._texCursorA,new Rectangle((AbsXY + _cursorPos - Vector2.One * 5).ToPoint() + new Point(4, 4), (Game1._texCursorA.Bounds.Size.ToVector2()/ 2).ToPoint()), Color.Black * .5f);
                batch.Draw(Game1._texCursorA,new Rectangle((AbsXY + _cursorPos - Vector2.One * 5).ToPoint(), (Game1._texCursorA.Bounds.Size.ToVector2()/ 2).ToPoint()), Color.White);
            }
            if (indexLayer == (int)Game1.Layers.Debug) 
            {
                batch.CenterStringXY(Game1._fontMain, $"{_playerIndex} : {_score}", AbsRectF.TopCenter, Color.Yellow);
                batch.CenterStringXY(Game1._fontMain, $"{(States)_state} : {HasMatch3()}", AbsRectF.BottomCenter, Color.White);

                //if (CurGemOver != null)
                //    batch.Line(CurGemOver.AbsXY, CurGemOver.AbsXY + _stick * 5, Color.White, 8);
                for (int i = 0; i < GridSize.X; i++)
                {
                    for (int j = 0; j < GridSize.Y; j++)
                    {
                        Vector2 pos = new Vector2(i * CellSize.X, j * CellSize.Y) + CellSize / 2;

                        var gem = _grid.Get(i, j);
                        int type = -1; 
                        
                        if (gem != null)
                            type = gem._type;


                        //batch.CenterStringXY(Game1._fontMain, $"{type}", AbsXY + pos, Color.White * .75f);
                    }
                }

            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
