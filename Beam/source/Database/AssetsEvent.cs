#nullable disable

namespace Beam.Database
{
    public partial class AssetsEvent
    {
        public long Id { get; set; }
        public long Height { get; set; }
        public long Seq { get; set; }
        public byte[] Data { get; set; }
    }
}
