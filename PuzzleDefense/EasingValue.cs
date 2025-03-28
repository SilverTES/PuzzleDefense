using Microsoft.Xna.Framework;
using Mugen.Animation;

namespace PuzzleDefense
{
    public class EasingValue
    {
        public float Value { get; set; }
        Animate _animate = new();
        public EasingValue(float initValue = 0f)
        {
            Value = initValue;
            _animate.Add("easing");
        }
        public float SetValue(float newValue, float duration = 32f)
        {
            float prevValue = Value;
            Value = newValue;

            _animate.SetMotion("easing", Easing.QuadraticEaseOut, new Tweening(prevValue, Value, duration));
            _animate.Start("easing");

            return Value;
        }

        public void Update(GameTime gameTime)
        {
            if (_animate.IsPlay())
            {
                Value = (int)_animate.Value();
            }

            _animate.NextFrame();
        }
    }
}
