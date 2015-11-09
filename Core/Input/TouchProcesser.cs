using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Leda.Core.Input
{
    public sealed class TouchProcessor : GameComponent
    {
        private static TouchProcessor _instance = null;
        private static TouchProcessor Instance
        {
            get
            {
                if (_instance == null) { _instance = new TouchProcessor(); }
                return _instance;
            }
        }

        public static List<Touch> Touches { get { return Instance._touchData; } }
        public static bool HasTouches { get { return Instance._touchData.Count > 0; } }
        public static Vector2 ResolutionOffset { set { Instance._resolutionOffset = value; } }
        public static float ResolutionScaling { set { Instance._resolutionScaling = value;} }

        public static void ClearInstance() { _instance = null; }

        private List<Touch> _touchData;
        private Vector2 _resolutionOffset;
        private float _resolutionScaling;

        public TouchProcessor()
            : base(GameBase.Instance)
        {
            GameBase.Instance.Components.Add(this);

            _touchData = new List<Touch>();
            _resolutionOffset = Vector2.Zero;
            _resolutionScaling = 1.0f;
        }

        public override void Update(GameTime gameTime)
        {
            _touchData.Clear();
            TouchCollection currentTouches = TouchPanel.GetState();
            foreach (TouchLocation touchLocation in currentTouches)
            {
                if ((touchLocation.State != TouchLocationState.Invalid) && (touchLocation.State != TouchLocationState.Released))
                {
                    TouchLocation previousLocationContainer = new TouchLocation();
                    if (touchLocation.TryGetPreviousLocation(out previousLocationContainer))
                    {
                        _touchData.Add(new Touch(
                            TranslatePositionFromScreenToBuffer(touchLocation.Position),
                            TranslatePositionFromScreenToBuffer(previousLocationContainer.Position)));
                    }
                    else
                    {
                        _touchData.Add(new Touch(
                            TranslatePositionFromScreenToBuffer(touchLocation.Position),
                            TranslatePositionFromScreenToBuffer(touchLocation.Position)));
                    }
                }
            }

            base.Update(gameTime);
        }

        private Vector2 TranslatePositionFromScreenToBuffer(Vector2 screenPosition)
        {
            return (screenPosition - _resolutionOffset) / _resolutionScaling;
        }

        public struct Touch
        {
            private Vector2 _currentLocation;
            private Vector2 _previousLocation;

            public Vector2 CurrentLocation { get { return _currentLocation; } }
            public Vector2 PreviousLocation { get { return _previousLocation; } }

            public Touch(Vector2 currentLocation, Vector2 previousLocation)
            {
                _currentLocation = currentLocation;
                _previousLocation = previousLocation;
            }
        }
    }
}
