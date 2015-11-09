using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;

namespace Bopscotch.Interface.Dialogs.Carousel
{
    public interface ICarouselDialogItem : ITransformationAnimatable
    {
        string SelectionValue { get; }
        float AngleOffsetAtZeroRotation { set; }
        Vector2 CarouselCenter { set; }
        Vector2 CarouselRadii { set; }
        Range DepthRange { set; }
        Range ScaleRange { set; }
        Color Tint { set; }

        void PositionRelativeToDialog(Vector2 carouselPosition, float carouselRotation);
    }
}
