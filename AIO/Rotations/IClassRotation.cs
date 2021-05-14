using static WholesomeTBCAIO.Helpers.Enums;

namespace WholesomeTBCAIO.Rotations
{
    public interface IClassRotation
    {
        public RotationType RotationType { get; set; }
        public RotationRole RotationRole { get; set; }
        public void Initialize(IClassRotation specialization);
        public void Dispose();
    }
}
