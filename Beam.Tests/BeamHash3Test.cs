using System;
using FluentAssertions;
using Xunit;

namespace Beam.Tests
{
    public class BeamHash3Test
    {
        // Mining protocol example:
        //
        // <- {"api_key":"{user}","id":"login","jsonrpc":"2.0","method":"login"}
        //
        // -> {"code":0,"description":"Login successful","id":"login","jsonrpc":"2.0","nonceprefix":"470fef51","method":"result"}
        // -> {"jsonrpc":"2.0","method":"job","id":"4191837464","input":"a0a9bdbc50d8f825e31e4449654b918fc3b9faafb5713a12bd0b7ac0664672cd","difficulty":155232350,"height":1067753,"nonceprefix":"470fef51"}
        // -> {"jsonrpc":"2.0","method":"job","id":"4191837465","input":"0a2065bedf55ebf9dcbc9aa25aef97742bcc779dc395f94e007fdba4d2829e77","difficulty":155232350,"height":1067753,"nonceprefix":"470fef51"}
        // -> {"jsonrpc":"2.0","method":"job","id":"4191837466","input":"0a2065bedf55ebf9dcbc9aa25aef97742bcc779dc395f94e007fdba4d2829e77","difficulty":155232350,"height":1067753,"nonceprefix":"1e149e89"}
        // -> {"jsonrpc":"2.0","method":"job","id":"4191837467","input":"700d8f8d1e03a213b584abedb7d4e39901256b7136499e3b09879fcb89a04875","difficulty":155232350,"height":1067753,"nonceprefix":"1e149e89"}
        //
        // -> {"jsonrpc":"2.0","method":"job","id":"2284471099","input":"fb1b64ed2a170ee919171aaa10e4f04d91f8bce204d52c0ee0405b1332e1b9d9","difficulty":138455134,"height":1067767,"nonceprefix":"471051c3"}
        // <- {"id":"2284471099","jsonrpc":"2.0","method":"solution","nonce":"471051c32c32cafc","output":"57050dd8ba98ac5d6d52d9ebde1bcf0c1a741abcd4c50accd2e1b6bd0030a16e81d51b476dde623b8f611b38d992c7f332b258d76c4cd402d9bd1d1ab6f6b92685a8d791d38a3e2cd08baab2a27ea004b54e9bfd4ce7d15db3d24d42582b941c3ccdc8f800000000"}

        // note
        // solution - 104 bytes
        // last 4 bytes of solution is extra nonce.
        private readonly byte[] _solution = Helper.StringToByteArray(
            "57050dd8ba98ac5d6d52d9ebde1bcf0c1a741abcd4c50accd2e1b6bd0030a16e81d51b476dde623b8f611b38d992c7f332b258d76c4cd402d9bd1d1ab6f6b92685a8d791d38a3e2cd08baab2a27ea004b54e9bfd4ce7d15db3d24d42582b941c3ccdc8f800000000");
        
        private readonly int _difficulty = 138455134;

        private readonly byte[] _input /* block hash */ =
            Helper.StringToByteArray("fb1b64ed2a170ee919171aaa10e4f04d91f8bce204d52c0ee0405b1332e1b9d9");

        private readonly byte[] _nonce = Helper.StringToByteArray("471051c32c32cafc");


        [Fact]
        public void CheckIsValidSolution()
        {
            BeamHash3.IsValidSolution(_input, _nonce, _solution).Should().BeTrue();
        }

        [Fact]
        public void CheckDifficulty()
        {
            Difficulty.TestDifficulty(_solution, _difficulty).Should().BeTrue();
        }


        [Fact]
        public void CheckValidationOfInvalidSolution()
        {
            _solution[0] = 0;
            BeamHash3.IsValidSolution(_input, _nonce, _solution).Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "Long")]
        public void Solve()
        {
            byte[] solution = null;
            var d = new Difficulty(_difficulty);

            var result = BeamHash3.Solve(_input, _nonce, x =>
            {
                if (!Difficulty.TestDifficulty(x, _difficulty))
                    return false;
                
                solution = x;
                return true;
            });
        }
    }
}
