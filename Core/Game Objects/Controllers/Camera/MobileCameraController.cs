using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Motion;

namespace Leda.Core.Game_Objects.Controllers.Camera
{
    public class MobileCameraController : CameraControllerBase, IMobile
    {
        public IMotionEngine MotionEngine { get; set; }
        public bool WorldPositionIsFixed { get { return false;}}

        public MobileCameraController()
            : base()
        {
        }

        public virtual void Update(int millisecondsSinceLastUpdate)
        {
            if (MotionEngine != null)
            {
                MotionEngine.Update(millisecondsSinceLastUpdate);
                WorldPosition += MotionEngine.Delta;
            }
        }
    }
}
