#nullable disable

namespace Beam.Database
{
    public partial class Peer
    {
        public byte[] Key { get; set; }
        public long Rating { get; set; }
        public long Address { get; set; }
        public long LastSeen { get; set; }
    }
}
