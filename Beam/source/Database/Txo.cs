#nullable disable

namespace Beam.Database
{
    public partial class Txo
    {
        public long Id { get; set; }
        public byte[] Value { get; set; }
        public long? SpendHeight { get; set; }
    }
}
