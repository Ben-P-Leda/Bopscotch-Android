using System;

using Microsoft.Xna.Framework;

using Leda.Core;

using Bopscotch.Interface.Content;

namespace Bopscotch.Interface.Dialogs.Carousel
{
    public class CarouselFlatImage : ImageContent, ICarouselDialogItem
    {
        private float _distanceFader;
        private Color _tint;

        public string SelectionValue { get; private set; }
        public float AngleOffsetAtZeroRotation { private get; set; }
        public float MasterScale { private get; set; }
        public new float Scale { get { return base.Scale; } set { base.Scale = value * MasterScale; } }
        public float Rotation { get { return 0.0f; } set { } }
        public Vector2 CarouselCenter { private get; set; }
        public Vector2 CarouselRadii { private get; set; }
        public Range DepthRange { private get; set; }
        public Range ScaleRange { private get; set; }
        public new Color Tint { protected get { return _tint; } set { _tint = value; base.Tint = Color.Lerp(Color.Black, _tint, DistanceFadeModifier); } }

        protected float DistanceFadeModifier { get { return 0.5f + _distanceFader; } }

        public CarouselFlatImage(string selectionValue, string textureReference)
            : base(textureReference, Vector2.Zero)
        {
            SelectionValue = selectionValue;
            MasterScale = 1.0f;

            _distanceFader = 0.5f;
            _tint = Color.White;
        }

        public void PositionRelativeToDialog(Vector2 carouselPosition, float carouselRotation)
        {
            Vector2 relativePosition = CarouselCenter + new Vector2(
                (float)(CarouselRadii.X * Math.Cos(carouselRotation + AngleOffsetAtZeroRotation)),
                (float)(CarouselRadii.Y * Math.Sin(carouselRotation + AngleOffsetAtZeroRotation)));
            _position = carouselPosition + relativePosition;

            float distanceFromApex = relativePosition.Y - (CarouselCenter.Y - CarouselRadii.Y);

            RenderDepth = DepthRange.Maximum - ((distanceFromApex / (CarouselRadii.Y * 2.0f)) * DepthRange.Interval);
            Scale = ScaleRange.Minimum + ((distanceFromApex / (CarouselRadii.Y * 2.0f)) * ScaleRange.Interval);

            _distanceFader = (distanceFromApex / (CarouselRadii.Y * 2.0f)) * 0.5f;
            base.Tint = Color.Lerp(Color.Black, _tint, DistanceFadeModifier);
        }
    }
}
