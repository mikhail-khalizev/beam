#nullable disable

namespace Beam.Database
{
    public partial class Stream
    {
        public long Id { get; set; }
        public byte[] Value { get; set; }
    }
}
