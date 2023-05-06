using static WholesomeTBCAIO.Helpers.Enums;

namespace WholesomeTBCAIO.Rotations
{
    public interface IClassRotation
    {

        public abstract RotationType RotationType { get; }
        public abstract RotationRole RotationRole { get; }

        public void Initialize(IClassRotation specialization);
        public void Dispose();
    }
}
