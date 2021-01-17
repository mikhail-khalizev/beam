#nullable disable

namespace Beam.Database
{
    public partial class Event
    {
        public long Height { get; set; }
        public byte[] Body { get; set; }
        public byte[] Key { get; set; }
    }
}
