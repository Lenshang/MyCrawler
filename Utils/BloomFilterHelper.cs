using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MyCrawler.Utils
{
    public class BloomFilterHelper
    {
        Dictionary<int,BitArray> HashK { get; set; }
        int arraySize { get; set; }
        public BloomFilterHelper(int kSize=8,int arraySize=1024)
        {
            HashK = new Dictionary<int, BitArray>();
            this.arraySize = arraySize;
            for (int i = 0; i < kSize; i++)
            {
                int _seed = 0;
                do
                {
                    _seed = GetRandom(1, 1000);
                } while ((HashK.ContainsKey(_seed)));
                HashK.Add(_seed, new BitArray(arraySize));
            }
        }
        public bool ContainsValue(string value)
        {
            int length = this.HashK.Keys.Count;
            int[] keyArray = new int[length];
            this.HashK.Keys.CopyTo(keyArray, 0);
            for (int i = 0; i < length; i++)
            {
                int key = keyArray[i];
                int _hash = this.Hash1(value, key);
                if (!this.HashK[key][_hash % this.arraySize])
                {
                    return false;
                }
            }
            return true;
        }
        public void AddValue(string value)
        {
            int length = this.HashK.Keys.Count;
            int[] keyArray = new int[length];
            this.HashK.Keys.CopyTo(keyArray,0);
            for (int i = 0; i < length; i++)
            {
                int key = keyArray[i];
                int _hash = this.Hash1(value, key);
                this.HashK[key][_hash % this.arraySize] = true;
            }
        }
        /// <summary>
        /// BKDR Hash Function
        /// </summary>
        /// <param name="str"></param>
        /// <param name="_seed"></param>
        /// <returns></returns>
        public int Hash1(string str,int _seed=131)
        {
            //var r= MurmurHash(_seed, str);
            //if (r < 0)
            //{
            //    r = Math.Abs(r);
            //}
            //return r;

            int seed = _seed;
            int hash = 0;
            int count;
            char[] bitarray = str.ToCharArray();
            count = bitarray.Length;
            while (count > 0)
            {
                hash = hash * seed + (bitarray[bitarray.Length - count]);
                count--;
            }

            return (hash & 0x7FFFFFFF);
        }
        /// <summary>
        /// 生成随机数
        /// </summary>
        /// <param name="minVal">最小值（包含）</param>
        /// <param name="maxVal">最大值（不包含）</param>
        /// <returns></returns>
        public int GetRandom(int minVal, int maxVal)
        {
            //这样产生0 ~ 100的强随机数（不含100）
            int m = maxVal - minVal;
            int rnd = int.MinValue;
            decimal _base = (decimal)long.MaxValue;
            byte[] rndSeries = new byte[8];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(rndSeries);
            long l = BitConverter.ToInt64(rndSeries, 0);
            rnd = (int)(Math.Abs(l) / _base * m);
            return minVal + rnd;
        }
        public int MurmurHash(int seed,string value)
        {
            var mhashFac = System.Data.HashFunction.MurmurHash.MurmurHash3Factory.Instance;
            var config = new System.Data.HashFunction.MurmurHash.MurmurHash3Config();
            config.Seed = (uint)seed;
            var mhash = mhashFac.Create(config);

            var r=mhash.ComputeHash(Encoding.UTF8.GetBytes(value));
            int res = 0;

            var bit = r.AsBitArray();
            for (int i = bit.Count - 1; i >= 0; i--)
            {
                res = bit[i] ? res + (1 << i) : res;
            }

            return res;
            //return Convert.ToInt32(r.AsBitArray());
        }

        public static void test()
        {
            BloomFilterHelper bf = new BloomFilterHelper(16, 20480000);
            int count = 0;
            int errorCount = 0;
            int eachError = 0;
            while (true)
            {
                count++;
                string _g = Guid.NewGuid().ToString();
                if (bf.ContainsValue(_g))
                {
                    errorCount++;
                    eachError++;
                }
                bf.AddValue(_g);

                if (count % 1000000 == 0)
                {
                    Console.WriteLine($"{count}条数据误差率为{Convert.ToDecimal(errorCount) / Convert.ToDecimal(count) * 100M}%");
                    Console.WriteLine($"每百万条重的误差率为{Convert.ToDecimal(eachError) / 1000000M * 100m}%");
                    eachError = 0;
                }
            }
        }
    }
}
