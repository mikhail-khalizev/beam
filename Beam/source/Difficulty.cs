using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace Beam
{
    public class Difficulty
    {
        const int s_MantissaBits = 24;
        const int s_MaxOrder = (32 << 3) - s_MantissaBits - 1;
        const uint s_Inf = (uint)(s_MaxOrder + 1) << s_MantissaBits;

        private readonly uint m_Packed;
        
        public static bool TestDifficulty(byte[] solution, int difficulty)
        {
            var data = SHA256.HashData(solution);
            var d = new Difficulty(difficulty);
            return d.IsTargetReached(data);
        }

        public Difficulty(int value)
        {
            m_Packed = (uint)value;
        }

        public bool IsTargetReached(byte[] hv)
        {
            if (m_Packed > s_Inf)
                return false; // invalid

            // multiply by (raw) difficulty, check if the result fits wrt normalization.
            var val = Unpack(); // 32 bytes.

            var a = new BigInteger(hv, isBigEndian: true) * val; // would be 512 bits (64 byte)
            var p = a.ToByteArray(isBigEndian: true);
            if (p.Length != 64)
                p = Enumerable.Repeat((byte)0, 64 - p.Length).Concat(p).ToArray();

            return memis0(p, 32 - (s_MantissaBits >> 3));
        }
        
        private bool memis0(byte[] p, int n)
        {
            for (var i = 0; i < n; i++)
                if (p[i] != 0)
                    return false;
            return true;
        }

        private BigInteger Unpack() 
        {
            var res = new BigInteger(); // 32 bytes.

            if (m_Packed < s_Inf)
            {
                var (order, mantissa) = Unpack2();
                res = mantissa;
                res <<= order;
            }
            else
            {
                // All bytes is 'ff'
                res = 1;
                res <<= 32 * 8;
                res -= 1;
            }

            return res;
        }

        private (int order, uint mantissa) Unpack2()
        {
            var order = (int)(m_Packed >> s_MantissaBits);

            var nLeadingBit = 1U << s_MantissaBits;
            var mantissa = nLeadingBit | (m_Packed & (nLeadingBit - 1));

            return (order, mantissa);
        }
    }
}