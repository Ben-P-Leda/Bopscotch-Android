using System.Xml.Linq;

using Leda.Core.Asset_Management;
using Leda.Core.Serialization;

namespace Bopscotch.Gameplay.Objects.Environment.Flags
{
    public class CheckpointFlag : Flag
    {
        public int CheckpointIndex { get; private set; }

        public CheckpointFlag()
            :base()
        {
        }

        public CheckpointFlag(int checkpointIndex)
            : base()
        {
            TextureReference = Texture_Name;
            Texture = TextureManager.Textures[Texture_Name];

            CheckpointIndex = checkpointIndex;

            SetFrameAndAnimation();
        }

        protected override XElement Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.AddDataItem("checkpoint-index", CheckpointIndex);

            return serializer.SerializedData;
        }

        protected override Serializer Deserialize(Serializer serializer)
        {
            base.Deserialize(serializer);

            CheckpointIndex = serializer.GetDataItem<int>("checkpoint-index");

            return serializer;
        }


        private const string Texture_Name = "flag-checkpoint";

        public const string Data_Node_Name = "checkpoint-flag";
    }
}
