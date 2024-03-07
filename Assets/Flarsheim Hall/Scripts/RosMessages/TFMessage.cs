using System;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Geometry; // Add this line to reference the geometry message types

namespace RosMessageTypes.Tf
{
    [Serializable]
    public class TFMessage : Message
    {
        public new const string RosMessageName = "";
        public TransformMsg[] transforms; // Use TransformMsg instead of GeometryTransform

        public TFMessage()
        {
            transforms = new TransformMsg[0];
        }

        public TFMessage(TransformMsg[] transforms)
        {
            this.transforms = transforms;
        }

        public override string ToString()
        {
            return "TFMessage";
        }
    }
}
