#nullable disable

namespace Beam.Database
{
    public partial class State
    {
        public long Height { get; set; }
        public byte[] Hash { get; set; }
        public byte[] HashPrev { get; set; }
        public long Timestamp { get; set; }
        public byte[] Kernels { get; set; }
        public byte[] Definition { get; set; }
        public long Flags { get; set; }
        public long? RowPrev { get; set; }
        public long CountNext { get; set; }
        public long CountNextFunctional { get; set; }
        public byte[] PoW { get; set; }
        public byte[] Mmr { get; set; }
        public byte[] Perishable { get; set; }
        public byte[] Ethernal { get; set; }
        public byte[] Peer { get; set; }
        public byte[] ChainWork { get; set; }
        public long? Txos { get; set; }
        public byte[] Extra { get; set; }
        public byte[] Inputs { get; set; }
    }
}
