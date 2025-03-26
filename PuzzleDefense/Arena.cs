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
        public Point GridSize { get; private set; }
        public Vector2 CellSize { get; private set; }
        Grid2D<Gem> _grid;

        PlayerIndex _playerIndex;

        Point _mapCursor;
        Vector2 _cursorPos;
        RectangleF _rectCursor;

        GamePadState _pad;
        KeyboardState _key;

        public const string Up = "Up";
        public const string Down = "Down";
        public const string Left = "Left";
        public const string Right = "Right";

        Dictionary<string, bool> _controller = new Dictionary<string, bool>()
        {
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

            new Gem(this, Gem.Colors[0]).AppendTo(this).SetPosition(GetVectorByMapPosition(new Point(2,2)));
        }
        public Vector2 GetVectorByMapPosition(Point mapPosition)
        {
            return new Vector2(mapPosition.X * CellSize.X, mapPosition.Y * CellSize.Y) + CellSize / 2;
        }
        public Point GetPointByVecPosition(Vector2 position)
        {
            return new Point((int)Math.Floor(position.X / CellSize.X), (int)Math.Floor(position.Y / CellSize.Y));
        }
        public override Node Update(GameTime gameTime)
        {
            _key = Keyboard.GetState();
            _pad = GamePad.GetState(_playerIndex);

            //_controller[Up] = ButtonControl.OnPress(Up, _pad.DPad.Up == ButtonState.Pressed || _key.IsKeyDown(Keys.Up), 8);
            //_controller[Down] = ButtonControl.OnPress(Down, _pad.DPad.Down == ButtonState.Pressed || _key.IsKeyDown(Keys.Down), 8);
            //_controller[Left] = ButtonControl.OnPress(Left, _pad.DPad.Left == ButtonState.Pressed || _key.IsKeyDown(Keys.Left), 8);
            //_controller[Right] = ButtonControl.OnPress(Right, _pad.DPad.Right == ButtonState.Pressed || _key.IsKeyDown(Keys.Right), 8);

            //if (_controller[Up]) _mapCursor.Y += -1;
            //if (_controller[Down]) _mapCursor.Y += 1;
            //if (_controller[Left]) _mapCursor.X += -1;
            //if (_controller[Right]) _mapCursor.X += 1;

            _cursorPos.X += _pad.ThumbSticks.Left.X * 10;
            _cursorPos.Y += _pad.ThumbSticks.Left.Y * -10;

            _cursorPos = Vector2.Clamp(_cursorPos, Vector2.One * (CellSize / 2), new Vector2(_rect.Width, _rect.Height) - CellSize / 2);

            //_rectCursor.Position = _mapCursor.ToVector2() * CellSize;

            _rectCursor.Position = GetPointByVecPosition(_cursorPos).ToVector2() * CellSize;

            _rectCursor.Width = CellSize.X;
            _rectCursor.Height = CellSize.Y;

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

                batch.Rectangle(_rectCursor.Translate(AbsXY), Color.Yellow, 3f);

                batch.Point(AbsXY + _cursorPos, 8, Color.White);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
