using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Leda.Core
{
    public sealed class TextWriter
    {
		private static SpriteFont _font = null;
        private static float _scale = 1.0f;
        private static Vector2 _padding = Vector2.Zero;
		private static bool _resaturationRequired = false;

        public static Rectangle LastTextArea { get; private set; }
		public static string FontFile { private get; set; }

        public static void SetDefaults(SpriteFont font, float scale)
        {
            SetDefaults(font, scale, 0.0f);
        }

        public static void SetDefaults(SpriteFont font, float scale, float padding)
        {
            _font = font;
            _scale = scale;
            _padding.Y = padding;

            LastTextArea = Rectangle.Empty;
        }

        public static void Write(string text, SpriteBatch spriteBatch, Vector2 position, Color color, float depth, Alignment alignment)
        {
            Write(text, spriteBatch, _font, position, color, _scale, depth, alignment);
        }

        public static void Write(string text, SpriteBatch spriteBatch, Vector2 position, Color color, float scale, float depth, Alignment alignment)
        {
            Write(text, spriteBatch, _font, position, color, scale, depth, alignment);
        }

        public static void Write(string text, SpriteBatch spriteBatch, SpriteFont font, Vector2 position, Color color, float scale,
            float depth, Alignment alignment)
        {
			if (_resaturationRequired) { ReinitialiseAfterActivityResume(); }

			if (font != null)
			{
				DrawText (text, spriteBatch, font, position, color, GetOriginForAlignment (text, font, alignment), scale, depth);
			}
        }

        public static void Write(string text, SpriteBatch spriteBatch, Vector2 position, Color innerColor, Color outlineColor,
            float outlineThickness, float depth, Alignment alignment)
        {
            Write(text, spriteBatch, _font, position, innerColor, outlineColor, outlineThickness, _scale, depth, alignment);
        }

        public static void Write(string text, SpriteBatch spriteBatch, Vector2 position, Color innerColor, Color outlineColor,
            float outlineThickness, float scale, float depth, Alignment alignment)
        {
            Write(text, spriteBatch, _font, position, innerColor, outlineColor, outlineThickness, scale, depth, alignment);
        }

        public static void Write(string text, SpriteBatch spriteBatch, SpriteFont font, Vector2 position, Color innerColor, Color outlineColor,
            float outlineThickness, float scale, float depth, Alignment alignment)
		{
			if (_resaturationRequired) { ReinitialiseAfterActivityResume (); }

			if (font != null)
			{
				Vector2 origin = GetOriginForAlignment (text, font, alignment);

				DrawText (text, spriteBatch, font, position, outlineColor, origin + (new Vector2 (-1.0f, -1.0f) * scale * outlineThickness),
					scale, depth + Outline_Render_Depth_Offset);
				DrawText (text, spriteBatch, font, position, outlineColor, origin + (new Vector2 (1.0f, -1.0f) * scale * outlineThickness),
					scale, depth + Outline_Render_Depth_Offset);
				DrawText (text, spriteBatch, font, position, outlineColor, origin + (new Vector2 (1.0f, 1.0f) * scale * outlineThickness),
					scale, depth + Outline_Render_Depth_Offset);
				DrawText (text, spriteBatch, font, position, outlineColor, origin + (new Vector2 (-1.0f, 1.0f) * scale * outlineThickness),
					scale, depth + Outline_Render_Depth_Offset);

				DrawText (text, spriteBatch, font, position, innerColor, origin, scale, depth);
			}
        }

        private static Vector2 GetOriginForAlignment(string text, SpriteFont font, Alignment alignment)
        {
            if (alignment == Alignment.Center) { return new Vector2(font.MeasureString(text).X / 2.0f, 0.0f); }
            else if (alignment == Alignment.Right) { return new Vector2(font.MeasureString(text).X, 0.0f); }
            return Vector2.Zero;
        }

        private static void DrawText(string text, SpriteBatch spriteBatch, SpriteFont font, Vector2 position, Color color, Vector2 origin,
            float scale, float depth)
        {
			spriteBatch.DrawString(font, text, GameBase.ScreenPosition(position + (_padding * scale * Platform_Scale_Modifier)), color, 0.0f, origin, 
				GameBase.ScreenScale(scale * Platform_Scale_Modifier), SpriteEffects.None, depth);

			CalculateLastTextArea(text, font, position + (_padding * scale * Platform_Scale_Modifier), origin, scale);
        }

        private static void CalculateLastTextArea(string text, SpriteFont font, Vector2 position, Vector2 origin, float scale)
        {
			LastTextArea = new Rectangle(
				(int)(position.X - (origin.X * scale * Platform_Scale_Modifier)), 
				(int)position.Y, 
				(int)(font.MeasureString(text).X * scale * Platform_Scale_Modifier), 
				(int)(font.MeasureString(text).Y * scale * Platform_Scale_Modifier));
        }

        public static Vector2 CalculateTextDimensions(string text, float scale)
        {
            return _font.MeasureString(text) * scale * Platform_Scale_Modifier;
        }

		// Required by Android to make sure spritefont does not corrupt
		public static void CleanDownForActivityPause()
		{
			_font = null;
			_resaturationRequired = true;
			GC.Collect ();
		}

		private static void ReinitialiseAfterActivityResume()
		{
			_resaturationRequired = false;
			if (!string.IsNullOrEmpty(FontFile)) { _font = GameBase.Instance.Content.Load<SpriteFont>(FontFile); }
		}

        public static Vector2 Dimensions(string toRender)
        {
            return _font.MeasureString(toRender);
        }

        public enum Alignment
        {
            Left,
            Center,
            Right
        }

        private const float Outline_Render_Depth_Offset = 0.000001f;
	
#if WINDOWS_PHONE
		private const float Platform_Scale_Modifier = 1.0f;
#else
		private const float Platform_Scale_Modifier = 0.95f;
#endif
	}
}
