using Microsoft.Xna.Framework;

namespace Bopscotch
{
    public class Definitions
    {
        public const int Wide_Back_Buffer_Height = 900;
        public const int Standard_Back_Buffer_Height = 1200;

		public const bool Overwrite_Profile = false;
		public const bool Trial_Version = false;

        public const int Back_Buffer_Width = 1600;
        public const int Back_Buffer_Height = 900;
        public static Vector2 Back_Buffer_Center { get { return new Vector2(Back_Buffer_Width, Back_Buffer_Height) / 2.0f; } }

        public static bool IsWideScreen = true;

        public const float Horizontal_Scroll_Boundary_Fraction = 0.25f;
        public const float Vertical_Scroll_Boundary_Fraction = 0.55f;

        public const int Grid_Cell_Pixel_Size = 80;
        public const int Button_Icon_Pixel_Size = 64;

        public const float Normal_Gravity_Value = 0.995f;
        public const float Zero_Vertical_Velocity = 0.14f;
        public const float Terminal_Velocity = 32.0f;

        public const float Left_Button_Column_X = 500.0f;
        public const float Right_Button_Column_X = 1100.0f;

        public const float Background_Texture_Thumbnail_Scale = 0.3f;

        public const string Avatar_Skeleton_Side = "player-side";
        public const string Avatar_Skeleton_Front = "player-front";
        public const int Avatar_Customisation_Slot_Count = 4;

        public enum AspectRatio
        {
            Standard,
            Wide
        }

        public enum RaceOutcome
        {
            OwnPlayerWin,
            OpponentPlayerWin,
            Draw,
            Incomplete
        }

        public enum PowerUp
        {
            None,
            Chilli,
            Wheel,
            Boots,
            Shades,
            Shell,
            Horn
        }
    }
}
