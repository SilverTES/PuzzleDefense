using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Animation;
using Mugen.GFX;
using Mugen.Physics;

namespace PuzzleDefense
{
    public class ProgressBar
    {
        // display 
        Rectangle _rect;
        Color _colorBorder;
        Color _colorFG;
        Color _colorBG;
        float _borderWidth = 1f;
        Position _position;

        // value
        public float MaxValue => _maxValue;
        public float Value => _currentValue;
        float _maxValue;
        float _currentValue;

        // display value
        float _nbDiv = 1;
        int _widthValue;
        bool _isVisible = true;

        Animate _animate = new();
        public ProgressBar(float currentValue, float maxValue, int width, int height, Color colorFG, Color colorBG, Color colorBorder, float borderWidth = 1f, float nbDiv = 1f, Position position = Position.CENTER)
        {
            _currentValue = currentValue;
            _maxValue = maxValue;

            _rect = new Rectangle(0, 0, width, height);
            _position = position;
            _colorFG = colorFG;
            _colorBG = colorBG;
            _colorBorder = colorBorder;
            _borderWidth = borderWidth;

            _nbDiv = nbDiv;

            _animate.Add("set");
        }

        public void SetColor(Color colorFG = default, Color colorBG = default, Color colorBorder = default)
        {
            if (colorFG != default) _colorFG = colorFG;
            if (colorBG != default) _colorBG = colorBG;
            if (colorBorder != default) _colorBorder = colorBorder;

        }
        public void SetMaxValue(float maxValue)
        {
            _maxValue = maxValue;
        }
        public void SetVisible(bool isVisible)
        {
            _isVisible = isVisible;
        }
        public void SetValue(float currentValue)
        {
            _currentValue = float.Clamp(currentValue, 0, _maxValue);
        }
        public void AddValue(float amount)
        {
            _animate.SetMotion("set", Easing.QuadraticEaseOut, new Tweening(_currentValue, _currentValue + amount, 32));
            _animate.Start("set");
        }
        public Vector2 ValueXY()
        {
            return new Vector2(_rect.X + _rect.Width -_widthValue, _rect.Y + _rect.Height / 2);
        }
        public void Render(SpriteBatch batch, Vector2 position)
        {
            if (_animate.IsPlay())
            {
                _currentValue = float.Clamp(_animate.Value(), 0, _maxValue);
            }

            _animate.NextFrame();

            if (_isVisible)
            {
                if (_position == Position.CENTER)
                {
                    _rect.X = (int)(position.X - _rect.Width / 2);
                    _rect.Y = (int)(position.Y - _rect.Height / 2);
                }

                _widthValue = _rect.Width - (int)(_rect.Width * _currentValue / _maxValue);

                batch.FillRectangle(_rect, _colorBG);
                batch.FillRectangle(RectangleF.Add(_rect, 0, 0, -_widthValue, 0), _colorFG);

                batch.Line(_rect.Left, _rect.Top + 1, _rect.Right, _rect.Top + 1, Color.White * .6f, 2);
                batch.Line(_rect.Left, _rect.Bottom - 1, _rect.Right, _rect.Bottom - 1, Color.Black * .6f, 2);

                float div = _rect.Width / _nbDiv;

                for (float i = 0; i < _nbDiv; i++)
                {
                    float divx = i * div + _rect.X;
                    batch.Line(divx, _rect.Y, divx, _rect.Y + _rect.Height, _colorBorder, 2f);
                }

                batch.Rectangle(_rect, _colorBorder, _borderWidth);
            }
        }

    }
}
