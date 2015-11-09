using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Base_Classes;

namespace Bopscotch.Interface.Dialogs.Carousel
{
    public class CarouselAvatar : DialogAvatar, ICarouselDialogItem
    {
        private float _distanceFader;
        private Color _tint;
        private float _overlayRenderDepth;

        public string SelectionValue { get; private set; }
        public float AngleOffsetAtZeroRotation { private get; set; }
        public Vector2 CarouselCenter { private get; set; }
        public Vector2 CarouselRadii { private get; set; }
        public Range DepthRange { private get; set; }
        public Range ScaleRange { private get; set; }
        public new Color Tint { protected get { return _tint; } set { _tint = value; base.Tint = Color.Lerp(Color.Black, _tint, 0.5f + _distanceFader); } }

        public override float RenderDepth { set { base.RenderDepth = value; _overlayRenderDepth = value - Overlay_Render_Depth_Offset; } }
        public string Annotation { private get; set; }

        public CarouselAvatar(string selectionValue, float initialBrightness)
            : this(selectionValue)
        {
            _distanceFader = initialBrightness * 0.5f;
        }

        public CarouselAvatar(string selectionValue)
            : base()
        {
            SelectionValue = selectionValue;

            _distanceFader = 0.0f;
            _tint = Color.White;

            Visible = true;
            Annotation = "";
        }

        public void PositionRelativeToDialog(Vector2 carouselPosition, float carouselRotation)
        {
            Vector2 relativePosition = CarouselCenter + new Vector2(
                (float)(CarouselRadii.X * Math.Cos(carouselRotation + AngleOffsetAtZeroRotation)),
                (float)(CarouselRadii.Y * Math.Sin(carouselRotation + AngleOffsetAtZeroRotation)));
            WorldPosition = carouselPosition + relativePosition;

            float distanceFromApex = relativePosition.Y - (CarouselCenter.Y - CarouselRadii.Y);

            RenderDepth = DepthRange.Maximum - ((distanceFromApex / (CarouselRadii.Y * 2.0f)) * DepthRange.Interval);
            Scale = ScaleRange.Minimum + ((distanceFromApex / (CarouselRadii.Y * 2.0f)) * ScaleRange.Interval);

            _distanceFader = (distanceFromApex / (CarouselRadii.Y * 2.0f)) * 0.5f;
            base.Tint = Color.Lerp(Color.Black, _tint, 0.5f + _distanceFader);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if ((!string.IsNullOrEmpty(Annotation)) && (Visible))
            {
                TextWriter.Write(Translator.Translation(Annotation), spriteBatch, WorldPosition + new Vector2(0.0f, Annotation_Vertical_Offset),
                    Color.Lerp(Color.Black, Color.White, _distanceFader * 2.0f), Color.Black, 2.0f, 0.7f, _overlayRenderDepth,
                    TextWriter.Alignment.Center);

            }
        }

        private const float Overlay_Render_Depth_Offset = 0.00001f;
        private const float Annotation_Vertical_Offset = -100.0f;
    }
}