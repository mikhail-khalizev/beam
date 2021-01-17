#nullable disable

namespace Beam.Database
{
    public partial class Bb
    {
        public long Id { get; set; }
        public byte[] Key { get; set; }
        public long Channel { get; set; }
        public long Time { get; set; }
        public byte[] Message { get; set; }
        public long? Nonce { get; set; }
    }
}
