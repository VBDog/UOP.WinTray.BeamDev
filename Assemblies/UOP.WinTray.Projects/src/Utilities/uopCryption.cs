using System;
using System.Collections.Generic;

namespace UOP.WinTray.Projects.Utilities
{
    /// <summary>
    /// Decryption Class
    /// </summary>
    public static class uopCryption
    {
        // PC1 Cipher 128-bit key
        // (c) Alexander PUKALL 1991
        // Can be use freely even for commercial applications
        // Visual Basic 6.0
        // Modified by Brian Nowak May 2003

        #region Private Variables

        private static readonly List<long> x1a0 = new List<long>(new long[10]);
        private static readonly List<long> cle = new List<long>(new long[18]);
        private static long x1a2 = 0;
        private static long inter = 0;
        private static long res = 0;
        private static long ax = 0;
        private static long bx = 0;
        private static long cx = 0;
        private static long dx = 0;
        private static long si = 0;
        private static long tmp = 0;
        private static long i = 0;
        private static byte c = 0;

        #endregion

        #region Private Methods

        /// <summary>
        /// Assemble method
        /// </summary>
        private static void Assemble()
        {
            x1a0[0] = ((cle[1] * 256) + cle[2]) % 65536;
            Code();
            inter = res;

            x1a0[1] = x1a0[0] ^ ((cle[3] * 256) + cle[4]);
            Code();
            inter ^= res;

            x1a0[2] = x1a0[1] ^ ((cle[5] * 256) + cle[6]);
            Code();
            inter ^= res;

            x1a0[3] = x1a0[2] ^ ((cle[7] * 256) + cle[8]);
            Code();
            inter ^= res;

            x1a0[4] = x1a0[3] ^ ((cle[9] * 256) + cle[10]);
            Code();
            inter ^= res;

            x1a0[5] = x1a0[4] ^ ((cle[11] * 256) + cle[12]);
            Code();
            inter ^= res;

            x1a0[6] = x1a0[5] ^ ((cle[13] * 256) + cle[14]);
            Code();
            inter ^= res;

            x1a0[7] = x1a0[6] ^ ((cle[15] * 256) + cle[16]);
            Code();
            inter ^= res;

            i = 0;
        }

        /// <summary>
        /// Code Method
        /// </summary>
        private static void Code()
        {
            dx = (x1a2 + i) % 65536;
            ax = x1a0[(int)i];
            cx = 0x15A;
            bx = 0x4E35;

            tmp = ax;
            ax = si;
            si = tmp;

            tmp = ax;
            ax = dx;
            dx = tmp;

            if (ax != 0)
            {
                ax = ax * bx % 65536;
            }

            tmp = ax;
            ax = cx;
            cx = tmp;

            if (ax != 0)
            {
                ax = ax * si % 65536;
                cx = (ax + cx) % 65536;
            }

            tmp = ax;
            ax = si;
            si = tmp;
            ax = ax * bx % 65536;
            dx = (cx + dx) % 65536;

            ax++;

            x1a2 = dx;
            x1a0[(int)i] = ax;

            res = ax ^ dx;
            i++;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Decrypt Method
        /// </summary>
        /// <param name="Coded"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static string Decrypt(string Coded, string Key)
        {
            string decrypt = null;
            si = 0;
            x1a2 = 0;
            i = 0;

            for (int fois = 1; fois <= 16; fois++)
            {
                cle[fois] = 0;
            }

            string champ1 = Key;
            int lngchamp1 = champ1.Length;

            for (int fois = 0; fois < lngchamp1; fois++)
            {
                cle[fois + 1] = (int)champ1[fois];
            }

            champ1 = Coded;
            lngchamp1 = champ1.Length;

            for (int fois = 0; fois < lngchamp1; fois++)
            {
                int D = (int)champ1[fois];
                if ((D - 0x61) >= 0)
                {
                    D -= 0x61; // to transform the letter to the 4 high bits of c
                    if ((D >= 0) && (D <= 15))
                    {
                        D *= 16;
                    }
                }
                if (fois != lngchamp1)
                {
                    fois++;
                }
                int E = (int)champ1[fois];
                if ((E - 0x61) >= 0)
                {
                    E -= 0x61; // to transform the letter to the 4 low bits of c
                    if ((E >= 0) && (E <= 15))
                    {
                        c = (byte)(D + E);
                    }
                }

                Assemble();

                if (inter > 65535)
                {
                    inter -= 65536;
                }

                long cfc = (long)(((double)inter / 256 * 256) - (inter % 256)) / 256;
                long cfd = inter % 256;

                c = (byte)(c ^ (cfc ^ cfd));

                for (int compte = 1; compte <= 16; compte++)
                {
                    cle[compte] = cle[compte] ^ c;
                }

                decrypt += (Char)c;

            }
            return decrypt;
        }

        /// <summary>
        /// Encrypt Method
        /// </summary>
        /// <param name="UnCoded"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static string Crypt(string UnCoded, string Key)
        {
            string Crypt = null;
            si = 0;
            x1a2 = 0;
            i = 0;

            for (int fois = 0; fois <= 16; fois++)
            {
                cle[fois] = 0;
            }

            string champ1 = Key;
            int lngchamp1 = champ1.Length;

            for (int fois = 0; fois < lngchamp1; fois++)
            {
                cle[fois + 1] = (int)champ1[fois];
            }

            champ1 = UnCoded;
            lngchamp1 = champ1.Length;
            for (int fois = 0; fois < lngchamp1; fois++)
            {
                c = (byte)champ1[fois];

                Assemble();

                if (inter > 65535)
                {
                    inter -= 65536;
                }

                long cfc = (long)(((double)inter / 256 * 256) - (inter % 256)) / 256;
                long cfd = inter % 256;

                for (int compte = 1; compte <= 16; compte++)
                {
                    cle[compte] = cle[compte] ^ c;
                }

                c = (byte)(c ^ (cfc ^ cfd));
                int D = (int)(((double)c / 16 * 16) - (c % 16)) / 16;
                int E = c % 16;

                Crypt += (Char)(97 + D);
                Crypt += (Char)(97 + E);
            }
            return Crypt;
        }

        #endregion
    }
}
