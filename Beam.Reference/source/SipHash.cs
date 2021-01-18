namespace Beam
{
    public class SipHash
    {
        public static ulong Rotl(ulong x, int b)
        {
            return (x << b) | (x >> (64 - b));
        }

        public static ulong Siphash24(ulong state0, ulong state1, ulong state2, ulong state3, ulong nonce)
        {
            ulong v0, v1, v2, v3;

            v0 = state0;
            v1 = state1;
            v2 = state2;
            v3 = state3;
            v3 ^= nonce;

            for (var i = 0; i < 2; i++)
            {
                // sipRound
                v0 += v1;
                v2 += v3;
                v1 = Rotl(v1, 13);
                v3 = Rotl(v3, 16);
                v1 ^= v0;
                v3 ^= v2;
                v0 = Rotl(v0, 32);
                v2 += v1;
                v0 += v3;
                v1 = Rotl(v1, 17);
                v3 = Rotl(v3, 21);
                v1 ^= v2;
                v3 ^= v0;
                v2 = Rotl(v2, 32);
            }

            v0 ^= nonce;
            v2 ^= 0xff;

            for (var i = 0; i < 4; i++)
            {
                // sipRound
                v0 += v1;
                v2 += v3;
                v1 = Rotl(v1, 13);
                v3 = Rotl(v3, 16);
                v1 ^= v0;
                v3 ^= v2;
                v0 = Rotl(v0, 32);
                v2 += v1;
                v0 += v3;
                v1 = Rotl(v1, 17);
                v3 = Rotl(v3, 21);
                v1 ^= v2;
                v3 ^= v0;
                v2 = Rotl(v2, 32);
            }

            return v0 ^ v1 ^ v2 ^ v3;
        }
    }
}