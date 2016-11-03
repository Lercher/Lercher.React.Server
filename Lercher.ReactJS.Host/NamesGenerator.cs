using System;
using System.Security.Cryptography;

namespace Lercher.ReactJS.Host
{
    public class NamesGenerator
    {
        private readonly System.IO.MemoryStream ms;
        readonly private System.IO.BinaryWriter bw;
        private int seed = 0;
        readonly private long seedpos = 0;
        readonly private HashAlgorithm ha = HashAlgorithm.Create("SHA1"); // 160bit of entropy

        public NamesGenerator(Guid itemid, int dataversion, byte[] salt)
        {
            ms = new System.IO.MemoryStream();
            bw = new System.IO.BinaryWriter(ms);
            bw.Write(itemid.ToByteArray());
            bw.Write(dataversion);
            bw.Write(salt);
            bw.Flush();
            seedpos = ms.Position;
            bw.Write(seed);
            bw.Flush();
            ms.SetLength(ms.Position);
        }

        public void reset()
        {
            seed = 0;
        }

        public string getNextName()
        {
            lock (ha)
            {
                seed++;
                ms.Position = seedpos;
                bw.Write(seed);
                ms.Position = 0;
                var b = ha.ComputeHash(ms);
                return MakeNameFrom(b);
            }
        }

        private static string MakeNameFrom(byte[] b)
        {
            /*
            The base-64 digits in ascending order from zero are the uppercase characters "A" to "Z", 
            the lowercase characters "a" to "z", the numerals "0" to "9", and the symbols "+" and "/". 
            The valueless character, "=", is used for trailing padding.
            */
            var r = Convert.ToBase64String(b);
            var l3 = b.Length % 3;
            r = r.Substring(0, r.Length - 3 + l3); // remove trailing = 
            r = r.Replace('+', '$').Replace('/', '_');
            return r;
        }
    }
}
