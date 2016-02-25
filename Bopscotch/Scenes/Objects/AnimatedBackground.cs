using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;
using Leda.Core.Asset_Management;
using Leda.Core.Gamestate_Management;

namespace Bopscotch.Scenes.Objects
{
    public class AnimatedBackground : ISimpleRenderable, ICameraLinked
    {
        private string _texture;
        private Vector2 _worldDimensions;
        private Rectangle _targetArea;
        private AnimatedBackgroundSegment[] _segments;
        private AnimatedBackgroundCloud[] _clouds;
        private float _maximumY;
        private float _verticalModifier;
        private int _generatorSeed;
        private int _generatorLastValue;
        private int _generatorIteration;
        private int _segmentSequenceIndex;

        private Vector2 _cloudScaling;
        private Color _cloudTint;
        private float _cloudFloor;

        public bool Visible { get; set; }
        public int RenderLayer { get; set; }
        public Vector2 CameraPosition { set { UpdateComponentPositions(value); } }
        public int[] ComponentSequence { private get; set; }
        public bool Wrap { get; set; }

        public static AnimatedBackground Create(string reference, int[] componentSequence)
        {
            AnimatedBackground bg = new AnimatedBackground(reference, new Point(Fixed_Width, Definitions.Back_Buffer_Height), 0);
            bg.ComponentSequence = componentSequence;
            bg.CreateComponents();

            return bg;
        }

        public AnimatedBackground(string texture, Point worldDimensions, int segmentSeed)
        {
            _texture = "anim-" + texture;
            _worldDimensions = worldDimensions.ToVector2();

            _generatorSeed = segmentSeed;
            _generatorLastValue = 0;
            _generatorIteration = 0;
            _segmentSequenceIndex = 0;

            CalculateBackgroundTargetArea();
            RenderLayer = Render_Layer;
            Visible = true;
            Wrap = false;

            SetCloudMetrics(texture);
        }

        private void CalculateBackgroundTargetArea()
        {
            float heightRatio = (float)Definitions.Back_Buffer_Height / (float)GameBase.Instance.GraphicsDevice.Viewport.Height;
            float targetWidth = (float)Definitions.Back_Buffer_Width / heightRatio;
            float xOffset = ((float)GameBase.Instance.GraphicsDevice.Viewport.Width - targetWidth) / 2.0f;

            _targetArea = new Rectangle((int)xOffset, 0, (int)targetWidth, GameBase.Instance.GraphicsDevice.Viewport.Height);
        }
		
        private void SetCloudMetrics(string textureName)
        {
            // This is horrible, but I really can't be bothered any more...
            switch (textureName)
            {
                case "background-1":                // Hilltops
                    _cloudScaling = Vector2.One;
                    _cloudTint = Color.White;
                    _cloudFloor = 400.0f;
                    break;
                case "background-2":                // Stadium
                    _cloudScaling = new Vector2(0.75f, 0.75f);
                    _cloudTint = Color.White;
                    _cloudFloor = 150.0f;
                    break;
                case "background-3":                // Waterfall
                    _cloudScaling = new Vector2(2.0f, 0.333f);
                    _cloudTint = Color.Lerp(Color.White, Color.Transparent, 0.5f);
                    _cloudFloor = 550.0f;
                    break;
                case "background-4":                // Metropolis
                    _cloudScaling = new Vector2(2.0f, 0.333f);
                    _cloudTint = Color.Lerp(Color.DarkGray, Color.Transparent, 0.5f);
                    _cloudFloor = 550.0f;
                    break;
                case "background-6":                // Ghostberg
                    _cloudScaling = new Vector2(1.0f, 0.5f);
                    _cloudTint = Color.Black;
                    _cloudFloor = 250.0f;
                    break;
                case "background-7":                // Snowville
                    _cloudScaling = Vector2.One;
                    _cloudTint = Color.White;
                    _cloudFloor = 400.0f;
                    break;
                case "background-8":                // Lava Cave
                    _cloudScaling = new Vector2(2.0f, 0.333f);
                    _cloudTint = Color.Lerp(Color.Gray, Color.Transparent, 0.5f);
                    _cloudFloor = 550.0f;
                    break;
                default:                            // Override everything else to Bopper Island
                    _texture = "anim-background-tutorial";
                    _cloudScaling = Vector2.One * 0.5f;
                    _cloudTint = Color.White;
                    _cloudFloor = 250.0f;
                    break;
            }
        }

        public void Initialize() { }

        public virtual void Reset() { }

        public void CreateComponents()
        {
            _segments = CreateSegments();
            _clouds = CreateClouds();
        }

        private int NextGeneratorValue()
        {
            int valueBase = _generatorSeed + _generatorLastValue + (_generatorIteration % Generator_Seed_Modifier) + Generator_Seed_Modifier;
            valueBase *= valueBase;

            char[] digits = valueBase.ToString().ToCharArray();
            Array.Reverse(digits);

            _generatorLastValue = Convert.ToInt32(digits[1].ToString());
            _generatorIteration++;

            return _generatorLastValue;
        }

        private AnimatedBackgroundSegment[] CreateSegments()
        {
            float maximumCameraY = _worldDimensions.Y - Definitions.Back_Buffer_Height;
            float allowedVerticalRange = MathHelper.Min(maximumCameraY * Vertical_Range_Modifier, Maximum_Vertical_Range);
            float horizontalRange = Wrap ? _worldDimensions.X * 0.5f : _worldDimensions.X * 0.8f;

            _maximumY = allowedVerticalRange;
            _verticalModifier = maximumCameraY == 0.0f ? 0.0f : allowedVerticalRange / maximumCameraY;

            string segmentTexture = _texture + "-segments";
            float nextSegmentX = 0.0f;
            List<AnimatedBackgroundSegment> segments = new List<AnimatedBackgroundSegment>();

            while (nextSegmentX < horizontalRange)
            {
                int nextSegmentIndex = NextGeneratorValue();
                if ((ComponentSequence != null) && (ComponentSequence.Length > 0))
                {
                    nextSegmentIndex = ComponentSequence[_segmentSequenceIndex];
                    _segmentSequenceIndex = (_segmentSequenceIndex + 1) % ComponentSequence.Length;
                }

                AnimatedBackgroundSegment segment = new AnimatedBackgroundSegment(segmentTexture, segments.Count, nextSegmentIndex, NextGeneratorValue());
                segments.Add(segment);
                nextSegmentX += AnimatedBackgroundSegment.SegmentWidth(segmentTexture) + (NextGeneratorValue() * Segment_Spacing_Modifier);
            }

            return segments.ToArray();
        }

        private AnimatedBackgroundCloud[] CreateClouds()
        {
            float rowHeight = _cloudFloor / Cloud_Row_Count;
            int lastRow = 0;

            Point nextCloudPosition = new Point(0, 0);

            float horizontalRange = Wrap ? _worldDimensions.X * 0.2f : _worldDimensions.X * 0.5f;

            List<AnimatedBackgroundCloud> clouds = new List<AnimatedBackgroundCloud>();

            while (nextCloudPosition.X < horizontalRange)
            {
                int row = Leda.Core.Random.Generator.NextInt(Cloud_Row_Count - 1);
                row = row >= lastRow ? row + 1 : row;
                lastRow = row;
                nextCloudPosition.Y = (int)((row * rowHeight) + Leda.Core.Random.Generator.Next(rowHeight * 0.2f, rowHeight * 0.8f));

                AnimatedBackgroundCloud cloud = new AnimatedBackgroundCloud(nextCloudPosition, _cloudScaling, _cloudTint);
                clouds.Add(cloud);
                nextCloudPosition.X += (int)(cloud.Width + (Leda.Core.Random.Generator.Next(-Cloud_Spacing_Modifier, Cloud_Spacing_Modifier) * _cloudScaling.X));
            }

            return clouds.ToArray();
        }

        public void UpdateComponentPositions(Vector2 cameraPosition)
        {
            Vector2 offset = new Vector2(-(cameraPosition.X / 2.0f), MathHelper.Max(_maximumY - (cameraPosition.Y * _verticalModifier), 0.0f));

            UpdateComponents(_segments, offset, _worldDimensions.X * 0.75f);
            UpdateComponents(_clouds, -cameraPosition, _worldDimensions.X * 0.35f);
        }

        private void UpdateComponents(IAnimatedBackgroundComponent[] components, Vector2 step, float wrapWidth)
        {
            for (int i = 0; i < components.Length; i++)
            {
                if (Wrap)
                {
                    components[i].UpdateAbsolutePosition(step, wrapWidth);
                }
                else
                {
                    components[i].SetOffset(step);
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureManager.Textures[_texture], _targetArea, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, Render_Depth);

            for (int i = 0; i < _segments.Length; i++)
            {
                _segments[i].Draw(spriteBatch);
            }

            for (int i = 0; i < _clouds.Length; i++)
            {
                _clouds[i].Draw(spriteBatch);
            }
        }

        private const int Render_Layer = 0;
        private const float Render_Depth = 0.9f;
        private const float Maximum_Vertical_Range = 225.0f;
        private const float Vertical_Range_Modifier = 0.25f;
        private const int Generator_Seed_Modifier = 11;
        private const float Segment_Spacing_Modifier = 15.0f;
        private const int Cloud_Row_Count = 4;
        private const float Cloud_Spacing_Modifier = 100.0f;

        public const int Fixed_Width = 4800;
    }
}
