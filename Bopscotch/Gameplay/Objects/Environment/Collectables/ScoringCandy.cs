using System;
using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Serialization;

namespace Bopscotch.Gameplay.Objects.Environment.Collectables
{
    public class ScoringCandy : Collectable
    {
        private float _rotationController;

        public int ScoreValue { get; set; }

        public ScoringCandy()
            : base()
        {
            _rotationController = Leda.Core.Random.Generator.Next(Rotator_Modulo);
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            base.Update(millisecondsSinceLastUpdate);

            if (!Paused)
            {
                _rotationController = (_rotationController + (millisecondsSinceLastUpdate * Rotator_Speed_Modifier)) % Rotator_Modulo;
                Rotation = (float)Math.Sin(MathHelper.ToRadians(_rotationController)) * Rotator_Angle_Modifer;
            }
        }

        protected override XElement Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.AddDataItem("score-value", ScoreValue);

            return serializer.SerializedData;
        }

        protected override Serializer Deserialize(Serializer serializer)
        {
            base.Deserialize(serializer);

            ScoreValue = serializer.GetDataItem<int>("score-value");

            return serializer;
        }

        private const float Rotator_Modulo = 360.0f;
        private const float Rotator_Speed_Modifier = 0.25f;
        private const float Rotator_Angle_Modifer = 0.4f;

        public const string Data_Node_Name = "scoring-candy";
    }
}
