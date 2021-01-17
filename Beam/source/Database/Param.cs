#nullable disable

namespace Beam.Database
{
    public partial class Param
    {
        public long Id { get; set; }
        public long? ParamInt { get; set; }
        public byte[] ParamBlob { get; set; }
    }
}
