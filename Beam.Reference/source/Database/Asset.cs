#nullable disable

namespace Beam.Database
{
    public partial class Asset
    {
        public long Id { get; set; }
        public byte[] Owner { get; set; }
        public byte[] MetaData { get; set; }
        public long? LockHeight { get; set; }
        public byte[] Value { get; set; }
    }
}
