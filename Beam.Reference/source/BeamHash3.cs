using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using Blake2Sharp;

namespace Beam
{
    public class BeamHash3
    {
        private const int WorkBitSize = 448; // 56 bytes
        private const int CollisionBitSize = 24; // 3 bytes.
        private const int NumRounds = 5;
        private const int IndexesCount = 1 << (CollisionBitSize + 1); // 33_554_432

        private class StepElem
        {
            private BigInteger _workBits = new(); // workBitSize bits
            public List<int> IndexTree = new(); // item size is (CollisionBitSize + 1) bit.

            public StepElem(byte[] preWork, int index)
            {
                for (var i = 6; 0 <= i; i--)
                {
                    _workBits = _workBits << 64;
                    var hash = SipHash.Siphash24(
                        BitConverter.ToUInt64(preWork, 0),
                        BitConverter.ToUInt64(preWork, 8),
                        BitConverter.ToUInt64(preWork, 16),
                        BitConverter.ToUInt64(preWork, 24),
                        (ulong) ((index << 3) + i));
                    _workBits |= hash;
                }

                IndexTree.Add(index);
            }

            public StepElem(StepElem a, StepElem b, int remLen)
            {
                // Create a new rounds step element from matching two ancestors

                _workBits = a._workBits ^ b._workBits;
                _workBits >>= CollisionBitSize;

                var mask = new BigInteger(); // workBitSize bits
                mask = 1;
                mask <<= remLen;
                mask -= 1;

                _workBits &= mask;

                if (a.IndexTree[0] < b.IndexTree[0])
                {
                    IndexTree.AddRange(a.IndexTree);
                    IndexTree.AddRange(b.IndexTree);
                }
                else
                {
                    IndexTree.AddRange(b.IndexTree);
                    IndexTree.AddRange(a.IndexTree);
                }
            }

            public bool IsZero()
            {
                return _workBits == 0;
            }

            public void ApplyMix(int remLen)
            {
                var tempBits = new BigInteger(); // 512 bits.
                tempBits = _workBits;

                // Add in the bits of the index tree to the end of work bits
                var padNum = (512 - remLen + CollisionBitSize) / (CollisionBitSize + 1);
                padNum = Math.Min(padNum, IndexTree.Count);

                // Concat with IndexTree.
                for (var i = 0; i < padNum; i++)
                {
                    var tmp = new BigInteger(); // 512 bits.
                    tmp = IndexTree[i];
                    tmp <<= remLen + i * (CollisionBitSize + 1);
                    tempBits |= tmp;
                }

                // Apply in the mix from the lined up bits
                // var mask = new BigInteger(); // 512 bits.
                var mask = 0xFFFFFFFF_FFFFFFFFUL; // 64 bits.
                ulong result = 0;
                for (var i = 0; i < 8; i++)
                {
                    var tmp = (ulong) (tempBits & mask);
                    tempBits >>= 64;
                    result += SipHash.Rotl(tmp, (29 * (i + 1)) & 0x3F /* 63 */);
                }

                result = SipHash.Rotl(result, 24);

                // Wipe out lowest 64 bits in favor of the mixed bits
                _workBits = _workBits >> 64;
                _workBits = _workBits << 64;
                _workBits |= result;
            }

            public uint GetCollisionBits()
            {
                // var mask = new BigInteger(); // workBitSize bits
                uint mask = (1 << CollisionBitSize) - 1;
                return (uint) (_workBits & mask);
            }
        }

        private class StepElemComparer : IComparer<StepElem>
        {
            public static StepElemComparer Instance { get; } = new StepElemComparer();

            public int Compare(StepElem x, StepElem y)
            {
                return x.GetCollisionBits().CompareTo(y.GetCollisionBits());
            }
        }

        public static bool IsValidSolution(byte[] input, byte[] nonce, byte[] solution)
        {
            if (solution.Length != 104)
                throw new ArgumentException("solution.Length != 104");

            var blake = CreateAndPrepareBlake(input, nonce);
            blake.Update(solution, 100, 4);
            var preWork = blake.Finish();

            var indices = GetIndicesFromMinimal(solution);

            var x = new List<StepElem>();
            for (var i = 0; i < indices.Count; i++)
                x.Add(new StepElem(preWork, indices[i]));

            var round = 1;

            while (1 < x.Count)
            {
                var xtmp = new List<StepElem>();

                for (var i = 0; i < x.Count; i += 2)
                {
                    var remLen = WorkBitSize - (round - 1) * CollisionBitSize;
                    if (round == 5)
                        remLen -= 64;

                    x[i].ApplyMix(remLen);
                    x[i + 1].ApplyMix(remLen);

                    if (!HasCollision(x[i], x[i + 1]))
                        return false;

                    if (!DistinctIndices(x[i], x[i + 1]))
                        return false;

                    if (!IndexAfter(x[i], x[i + 1]))
                        return false;

                    remLen = WorkBitSize - round * CollisionBitSize;
                    if (round == 4)
                        remLen -= 64;
                    if (round == 5)
                        remLen = CollisionBitSize;

                    xtmp.Add(new StepElem(x[i], x[i + 1], remLen));
                }

                x = xtmp;
                round++;
            }

            return x[0].IsZero();
        }

        private static Hasher CreateAndPrepareBlake(byte[] input, byte[] nonce)
        {
            if (input.Length != 32)
                throw new ArgumentException("input.Length != 32");
            if (nonce.Length != 8)
                throw new ArgumentException("nonce.Length != 8");

            var cfg = new Blake2BConfig();
            cfg.Personalization = new byte[16];
            Encoding.ASCII.GetBytes("Beam-PoW").CopyTo(cfg.Personalization, 0);
            BitConverter.GetBytes(WorkBitSize).CopyTo(cfg.Personalization, 8);
            BitConverter.GetBytes(NumRounds).CopyTo(cfg.Personalization, 12);

            cfg.OutputSizeInBytes = 32;

            var alg = Blake2B.Create(cfg);
            alg.Update(input);
            alg.Update(nonce);
            return alg;
        }

        private static bool DistinctIndices(StepElem a, StepElem b)
        {
            foreach (var indexA in a.IndexTree)
            foreach (var indexB in b.IndexTree)
            {
                if (indexA == indexB)
                    return false;
            }

            return true;
        }

        private static bool IndexAfter(StepElem a, StepElem b)
        {
            return a.IndexTree[0] < b.IndexTree[0];
        }

        private static bool SortStepElement(StepElem a, StepElem b)
        {
            return a.GetCollisionBits() < b.GetCollisionBits();
        }

        private static bool HasCollision(StepElem a, StepElem b)
        {
            return a.GetCollisionBits() == b.GetCollisionBits();
        }

        private static List<int> GetIndicesFromMinimal(byte[] solution)
        {
            var mask = new BigInteger(IndexesCount - 1); // 100 bytes only.
            var inStream = new BigInteger(solution); // 100 bytes only.

            var res = new List<int>(); // Each item 25 bits.
            for (var i = 0; i < 32; i++)
            {
                res.Add((int) (uint) (inStream & mask));
                inStream >>= CollisionBitSize + 1;
            }

            return res;
        }

        private static byte[] GetMinimalFromIndices(List<int> indexes)
        {
            var inStream = new BigInteger(); // 100 bytes

            for (var i = indexes.Count - 1; 0 <= i; i--)
            {
                inStream <<= (CollisionBitSize + 1);
                inStream |= (ulong) indexes[i];
            }

            return inStream.ToByteArray().Take(100).ToArray();
        }


        public static bool Solve(byte[] input, byte[] nonce, Func<byte[], bool> isValidSolution)
        {
            var blake = CreateAndPrepareBlake(input, nonce);
            var extraNonce = new byte[4];
            blake.Update(extraNonce);
            var preWork = blake.Finish(); // 32 bytes

            var elements = new List<StepElem>(IndexesCount);

            // Seeding
            for (var i = 0; i < IndexesCount; i++)
                elements.Add(new StepElem(preWork, i));

            // Round 1 to 5
            int round;
            int remLen;
            for (round = 1; round < 5; round++)
            {
                remLen = WorkBitSize - (round - 1) * CollisionBitSize;

                // Mixing of elements
                for (var i = 0; i < elements.Count; i++)
                    elements[i].ApplyMix(remLen);

                // Sorting
                elements.Sort(StepElemComparer.Instance);

                // Set length of output bits
                remLen = WorkBitSize - round * CollisionBitSize;
                if (round == 4)
                    remLen -= 64;

                // Creating matches
                var outElements = new List<StepElem>(IndexesCount);

                for (var i = 0; i < elements.Count - 1; i++)
                for (var j = i + 1; j < elements.Count; j++)
                {
                    if (!HasCollision(elements[i], elements[j]))
                        break;

                    outElements.Add(new StepElem(elements[i], elements[j], remLen));
                }

                elements = outElements;
            }


            // Check the output of the last round for solutions
            remLen = WorkBitSize - (round - 1) * CollisionBitSize - 64;

            // Mixing of elements
            for (var i = 0; i < elements.Count; i++)
                elements[i].ApplyMix(remLen);

            // Sorting
            elements.Sort(StepElemComparer.Instance);

            // Set length of output bits
            remLen = CollisionBitSize;

            // Creating matches
            for (var i = 0; i < elements.Count - 1; i++)
            for (var j = i + 1; j < elements.Count; j++)
            {
                if (!HasCollision(elements[i], elements[j]))
                    break;

                var temp = new StepElem(elements[i], elements[j], remLen);
                if (!temp.IsZero())
                    continue;

                var sol = GetMinimalFromIndices(temp.IndexTree);

                // Adding the extra nonce
                sol = sol.Concat(extraNonce).ToArray();

                if (isValidSolution(sol))
                    return true;
            }

            return false;
        }
    }
}