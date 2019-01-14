using System;
using System.Collections.Generic;
using System.Text;

namespace Kagamin2
{
    //
    // DES.cs: DES Encryption algorithm implementation in managed code.
    //
    // This program is free software; you can redistribute it and/or
    // modify it under the terms of the GNU General Public License as
    // published by the Free Software Foundation; either version 2 of the
    // License, or (at your option) any later version.
    //
    // This program is distributed in the hope that it will be useful,
    // but WITHOUT ANY WARRANTY; without even the implied warranty of
    // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    // General Public License for more details.
    //
    // You should have received a copy of the GNU General Public
    // License along with this program; if not, write to the
    // Free Software Foundation, Inc., 59 Temple Place - Suite 330,
    // Boston, MA 02111-1307, USA.


    /// <summary>
    /// DES Encryption algorithm.
    /// Based on FreeBSD's libcrypt:
    ///    secure/lib/libcrypt/crypt-des.c
    /// </summary>
    public class DES
    {
        private byte[] IP = new byte[64] {
			58, 50, 42, 34, 26, 18, 10,  2, 60, 52, 44, 36, 28, 20, 12,  4,
			62, 54, 46, 38, 30, 22, 14,  6, 64, 56, 48, 40, 32, 24, 16,  8,
			57, 49, 41, 33, 25, 17,  9,  1, 59, 51, 43, 35, 27, 19, 11,  3,
			61, 53, 45, 37, 29, 21, 13,  5, 63, 55, 47, 39, 31, 23, 15,  7
			};

        private byte[] inv_key_perm = new byte[64];
        private byte[] key_perm = new byte[56] {
			57, 49, 41, 33, 25, 17,  9,  1, 58, 50, 42, 34, 26, 18,
			10,  2, 59, 51, 43, 35, 27, 19, 11,  3, 60, 52, 44, 36,
			63, 55, 47, 39, 31, 23, 15,  7, 62, 54, 46, 38, 30, 22,
			14,  6, 61, 53, 45, 37, 29, 21, 13,  5, 28, 20, 12,  4
			};

        private byte[] key_shifts = new byte[16] {
			1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1
			};

        private byte[] inv_comp_perm = new byte[56];
        private byte[] comp_perm = new byte[48] {
			14, 17, 11, 24,  1,  5,  3, 28, 15,  6, 21, 10,
			23, 19, 12,  4, 26,  8, 16,  7, 27, 20, 13,  2,
			41, 52, 31, 37, 47, 55, 30, 40, 51, 45, 33, 48,
			44, 49, 39, 56, 34, 53, 46, 42, 50, 36, 29, 32
			};
        private byte[,] u_sbox = new byte[8, 64];
        private byte[,] sbox = new byte[8, 64] {
			{
				14,  4, 13,  1,  2, 15, 11,  8,  3, 10,  6, 12,  5,  9,  0,  7,
				0, 15,  7,  4, 14,  2, 13,  1, 10,  6, 12, 11,  9,  5,  3,  8,
				4,  1, 14,  8, 13,  6,  2, 11, 15, 12,  9,  7,  3, 10,  5,  0,
				15, 12,  8,  2,  4,  9,  1,  7,  5, 11,  3, 14, 10,  0,  6, 13
			},
			{
				15,  1,  8, 14,  6, 11,  3,  4,  9,  7,  2, 13, 12,  0,  5, 10,
				3, 13,  4,  7, 15,  2,  8, 14, 12,  0,  1, 10,  6,  9, 11,  5,
				0, 14,  7, 11, 10,  4, 13,  1,  5,  8, 12,  6,  9,  3,  2, 15,
				13,  8, 10,  1,  3, 15,  4,  2, 11,  6,  7, 12,  0,  5, 14,  9
			},
			{
				10,  0,  9, 14,  6,  3, 15,  5,  1, 13, 12,  7, 11,  4,  2,  8,
				13,  7,  0,  9,  3,  4,  6, 10,  2,  8,  5, 14, 12, 11, 15,  1,
				13,  6,  4,  9,  8, 15,  3,  0, 11,  1,  2, 12,  5, 10, 14,  7,
				1, 10, 13,  0,  6,  9,  8,  7,  4, 15, 14,  3, 11,  5,  2, 12
			},
			{
				7, 13, 14,  3,  0,  6,  9, 10,  1,  2,  8,  5, 11, 12,  4, 15,
				13,  8, 11,  5,  6, 15,  0,  3,  4,  7,  2, 12,  1, 10, 14,  9,
				10,  6,  9,  0, 12, 11,  7, 13, 15,  1,  3, 14,  5,  2,  8,  4,
				3, 15,  0,  6, 10,  1, 13,  8,  9,  4,  5, 11, 12,  7,  2, 14
			},
			{
				2, 12,  4,  1,  7, 10, 11,  6,  8,  5,  3, 15, 13,  0, 14,  9,
				14, 11,  2, 12,  4,  7, 13,  1,  5,  0, 15, 10,  3,  9,  8,  6,
				4,  2,  1, 11, 10, 13,  7,  8, 15,  9, 12,  5,  6,  3,  0, 14,
				11,  8, 12,  7,  1, 14,  2, 13,  6, 15,  0,  9, 10,  4,  5,  3
			},
			{
				12,  1, 10, 15,  9,  2,  6,  8,  0, 13,  3,  4, 14,  7,  5, 11,
				10, 15,  4,  2,  7, 12,  9,  5,  6,  1, 13, 14,  0, 11,  3,  8,
				9, 14, 15,  5,  2,  8, 12,  3,  7,  0,  4, 10,  1, 13, 11,  6,
				4,  3,  2, 12,  9,  5, 15, 10, 11, 14,  1,  7,  6,  0,  8, 13
			},
			{
				4, 11,  2, 14, 15,  0,  8, 13,  3, 12,  9,  7,  5, 10,  6,  1,
				13,  0, 11,  7,  4,  9,  1, 10, 14,  3,  5, 12,  2, 15,  8,  6,
				1,  4, 11, 13, 12,  3,  7, 14, 10, 15,  6,  8,  0,  5,  9,  2,
				6, 11, 13,  8,  1,  4, 10,  7,  9,  5,  0, 15, 14,  2,  3, 12
			},
			{
				13,  2,  8,  4,  6, 15, 11,  1, 10,  9,  3, 14,  5,  0, 12,  7,
				1, 15, 13,  8, 10,  3,  7,  4, 12,  5,  6, 11,  0, 14,  9,  2,
				7, 11,  4,  1,  9, 12, 14,  2,  0,  6, 10, 13, 15,  3,  5,  8,
				2,  1, 14,  7,  4, 10,  8, 13, 15, 12,  9,  0,  3,  5,  6, 11
			}
		};
        private byte[] un_pbox = new byte[32];
        private byte[] pbox = new byte[32] {
			16,  7, 20, 21, 29, 12, 28, 17,  1, 15, 23, 26,  5, 18, 31, 10,
			2,  8, 24, 14, 32, 27,  3,  9, 19, 13, 30,  6, 22, 11,  4, 25
			};

        private uint[] bits32 = new uint[32]
			{
				0x80000000, 0x40000000, 0x20000000, 0x10000000,
				0x08000000, 0x04000000, 0x02000000, 0x01000000,
				0x00800000, 0x00400000, 0x00200000, 0x00100000,
				0x00080000, 0x00040000, 0x00020000, 0x00010000,
				0x00008000, 0x00004000, 0x00002000, 0x00001000,
				0x00000800, 0x00000400, 0x00000200, 0x00000100,
				0x00000080, 0x00000040, 0x00000020, 0x00000010,
				0x00000008, 0x00000004, 0x00000002, 0x00000001
			};

        private byte[] bits8 = new byte[8] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
        private uint saltbits;
        private uint old_salt;
        private byte[] init_perm = new byte[64];
        private byte[] final_perm = new byte[64];
        private uint[] en_keysl = new uint[16];
        private uint[] en_keysr = new uint[16];
        private uint[] de_keysl = new uint[16];
        private uint[] de_keysr = new uint[16];
        private int des_initialised = 0;
        private byte[,] m_sbox = new byte[4, 4096];
        private uint[,] psbox = new uint[4, 256];
        private uint[,] ip_maskl = new uint[8, 256];
        private uint[,] ip_maskr = new uint[8, 256];
        private uint[,] fp_maskl = new uint[8, 256];
        private uint[,] fp_maskr = new uint[8, 256];
        private uint[,] key_perm_maskl = new uint[8, 128];
        private uint[,] key_perm_maskr = new uint[8, 128];
        private uint[,] comp_maskl = new uint[8, 128];
        private uint[,] comp_maskr = new uint[8, 128];
        private uint old_rawkey0, old_rawkey1;
        private byte[] ascii64 = new byte[] {
			(byte)'.', (byte)'/', (byte)'0', (byte)'1', (byte)'2', (byte)'3',
			(byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9',
			(byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F',
			(byte)'G', (byte)'H', (byte)'I', (byte)'J', (byte)'K', (byte)'L',
			(byte)'M', (byte)'N', (byte)'O', (byte)'P', (byte)'Q', (byte)'R',
			(byte)'S', (byte)'T', (byte)'U', (byte)'V', (byte)'W', (byte)'X',
			(byte)'Y', (byte)'Z', (byte)'a', (byte)'b', (byte)'c', (byte)'d',
			(byte)'e', (byte)'f', (byte)'g', (byte)'h', (byte)'i', (byte)'j',
			(byte)'k', (byte)'l', (byte)'m', (byte)'n', (byte)'o', (byte)'p',
			(byte)'q', (byte)'r', (byte)'s', (byte)'t', (byte)'u', (byte)'v',
			(byte)'w', (byte)'x', (byte)'y', (byte)'z'
		};
        private int ascii_to_bin(char ch)
        {
            if (ch > 'z')
                return 0;
            if (ch >= 'a')
                return ch - 'a' + 38;
            if (ch > 'Z')
                return 0;
            if (ch >= 'A')
                return ch - 'A' + 12;
            if (ch > '9')
                return 0;
            if (ch >= '.')
                return ch - '.';
            return 0;
        }
        private void des_init()
        {
            int b, inbit, obit;
            old_rawkey0 = old_rawkey1 = 0;
            saltbits = 0;
            old_salt = 0;
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 64; j++)
                {
                    b = (j & 0x20) | ((j & 1) << 4) | ((j >> 1) & 0xf);
                    u_sbox[i, j] = sbox[i, b];
                }
            for (b = 0; b < 4; b++)
                for (int i = 0; i < 64; i++)
                    for (int j = 0; j < 64; j++)
                        m_sbox[b, ((i << 6) | j)] =
                            (byte)((u_sbox[(b << 1), i] << 4) |
                            u_sbox[((b << 1) + 1), j]);
            for (int i = 0; i < 64; i++)
            {
                final_perm[i] = (byte)(IP[i] - 1);
                init_perm[final_perm[i]] = (byte)i;
                inv_key_perm[i] = 255;
            }
            for (int i = 0; i < 56; i++)
            {
                inv_key_perm[key_perm[i] - 1] = (byte)i;
                inv_comp_perm[i] = 255;
            }
            for (int i = 0; i < 48; i++)
            {
                inv_comp_perm[comp_perm[i] - 1] = (byte)i;
            }
            for (int k = 0; k < 8; k++)
            {
                for (int i = 0; i < 256; i++)
                {
                    ip_maskl[k, i] = 0;
                    ip_maskr[k, i] = 0;
                    fp_maskl[k, i] = 0;
                    fp_maskr[k, i] = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        inbit = 8 * k + j;
                        if (Convert.ToBoolean(i & bits8[j]))
                        {
                            if ((obit = init_perm[inbit]) < 32)
                                ip_maskl[k, i] |= bits32[obit];
                            else
                                ip_maskr[k, i] |= bits32[obit - 32];
                            if ((obit = final_perm[inbit]) < 32)
                                fp_maskl[k, i] |= bits32[obit];
                            else
                                fp_maskr[k, i] |= bits32[obit - 32];
                        }
                    }
                }
                for (int i = 0; i < 128; i++)
                {
                    key_perm_maskl[k, i] = 0;
                    key_perm_maskr[k, i] = 0;
                    for (int j = 0; j < 7; j++)
                    {
                        inbit = 8 * k + j;
                        if (Convert.ToBoolean(i & bits8[j + 1]))
                        {
                            if ((obit = inv_key_perm[inbit]) == 255)
                                continue;
                            if (obit < 28)
                                key_perm_maskl[k, i] |= bits32[4 + obit];
                            else
                                key_perm_maskr[k, i] |= bits32[4 + obit - 28];
                        }
                    }
                    comp_maskl[k, i] = 0;
                    comp_maskr[k, i] = 0;
                    for (int j = 0; j < 7; j++)
                    {
                        inbit = 7 * k + j;
                        if (Convert.ToBoolean(i & bits8[j + 1]))
                        {
                            if ((obit = inv_comp_perm[inbit]) == 255)
                                continue;
                            if (obit < 24)
                                comp_maskl[k, i] |= bits32[8 + obit];
                            else
                                comp_maskr[k, i] |= bits32[8 + obit - 24];
                        }
                    }
                }
            }
            for (int i = 0; i < 32; i++)
                un_pbox[pbox[i] - 1] = (byte)i;
            for (b = 0; b < 4; b++)
                for (int i = 0; i < 256; i++)
                {
                    psbox[b, i] = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        if (Convert.ToBoolean(i & bits8[j]))
                            psbox[b, i] |= bits32[un_pbox[8 * b + j]];
                    }
                }
            des_initialised = 1;
        }
        private void setup_salt(uint salt)
        {
            uint obit, saltbit;
            if (salt == old_salt)
                return;
            old_salt = salt;
            saltbits = 0;
            saltbit = 1;
            obit = 0x800000;
            for (int i = 0; i < 24; i++)
            {
                if (Convert.ToBoolean(salt & saltbit))
                    saltbits |= obit;
                saltbit <<= 1;
                obit >>= 1;
            }
        }
        private int des_setkey(byte[] k)
        {
            uint k0, k1, rawkey0, rawkey1;
            int shifts;
            if (!Convert.ToBoolean(des_initialised))
                des_init();
            rawkey0 = (uint)(k[0] << 24 | k[1] << 16 | k[2] << 8 | k[3]);
            rawkey1 = (uint)(k[4] << 24 | k[5] << 16 | k[6] << 8 | k[7]);
            if (Convert.ToBoolean(rawkey0 | rawkey1) &&
                rawkey0 == old_rawkey0 && rawkey1 == old_rawkey1)
            {
                return 0;
            }
            old_rawkey0 = rawkey0;
            old_rawkey1 = rawkey1;
            k0 = (uint)(key_perm_maskl[0, rawkey0 >> 25]
                | key_perm_maskl[1, (rawkey0 >> 17) & 0x7f]
                | key_perm_maskl[2, (rawkey0 >> 9) & 0x7f]
                | key_perm_maskl[3, (rawkey0 >> 1) & 0x7f]
                | key_perm_maskl[4, rawkey1 >> 25]
                | key_perm_maskl[5, (rawkey1 >> 17) & 0x7f]
                | key_perm_maskl[6, (rawkey1 >> 9) & 0x7f]
                | key_perm_maskl[7, (rawkey1 >> 1) & 0x7f]);
            k1 = (uint)(key_perm_maskr[0, rawkey0 >> 25]
                | key_perm_maskr[1, (rawkey0 >> 17) & 0x7f]
                | key_perm_maskr[2, (rawkey0 >> 9) & 0x7f]
                | key_perm_maskr[3, (rawkey0 >> 1) & 0x7f]
                | key_perm_maskr[4, rawkey1 >> 25]
                | key_perm_maskr[5, (rawkey1 >> 17) & 0x7f]
                | key_perm_maskr[6, (rawkey1 >> 9) & 0x7f]
                | key_perm_maskr[7, (rawkey1 >> 1) & 0x7f]);
            shifts = 0;
            for (int round = 0; round < 16; round++)
            {
                uint t0, t1;
                shifts += key_shifts[round];
                t0 = (k0 << shifts) | (k0 >> (28 - shifts));
                t1 = (k1 << shifts) | (k1 >> (28 - shifts));
                de_keysl[15 - round] =
                en_keysl[round] = comp_maskl[0, (t0 >> 21) & 0x7f]
                    | comp_maskl[1, (t0 >> 14) & 0x7f]
                    | comp_maskl[2, (t0 >> 7) & 0x7f]
                    | comp_maskl[3, t0 & 0x7f]
                    | comp_maskl[4, (t1 >> 21) & 0x7f]
                    | comp_maskl[5, (t1 >> 14) & 0x7f]
                    | comp_maskl[6, (t1 >> 7) & 0x7f]
                    | comp_maskl[7, t1 & 0x7f];
                de_keysr[15 - round] =
                en_keysr[round] = comp_maskr[0, (t0 >> 21) & 0x7f]
                    | comp_maskr[1, (t0 >> 14) & 0x7f]
                    | comp_maskr[2, (t0 >> 7) & 0x7f]
                    | comp_maskr[3, t0 & 0x7f]
                    | comp_maskr[4, (t1 >> 21) & 0x7f]
                    | comp_maskr[5, (t1 >> 14) & 0x7f]
                    | comp_maskr[6, (t1 >> 7) & 0x7f]
                    | comp_maskr[7, t1 & 0x7f];
            }
            return 0;
        }
        private int do_des(uint l_in, uint r_in, ref uint l_out, ref uint r_out, int count)
        {
            uint l, r, f = 0, r48l, r48r, j = 0;
            uint[] kl, kr;
            uint[] kl1, kr1;
            int round;
            if (count == 0)
                return 1;
            else if (count > 0)
            {
                kl1 = en_keysl;
                kr1 = en_keysr;
            }
            else
            {
                count = -count;
                kl1 = de_keysl;
                kr1 = de_keysr;
            }
            l = (uint)(ip_maskl[0, l_in >> 24]
                | ip_maskl[1, (l_in >> 16) & 0xff]
                | ip_maskl[2, (l_in >> 8) & 0xff]
                | ip_maskl[3, l_in & 0xff]
                | ip_maskl[4, r_in >> 24]
                | ip_maskl[5, (r_in >> 16) & 0xff]
                | ip_maskl[6, (r_in >> 8) & 0xff]
                | ip_maskl[7, r_in & 0xff]);
            r = (uint)(ip_maskr[0, l_in >> 24]
                | ip_maskr[1, (l_in >> 16) & 0xff]
                | ip_maskr[2, (l_in >> 8) & 0xff]
                | ip_maskr[3, l_in & 0xff]
                | ip_maskr[4, r_in >> 24]
                | ip_maskr[5, (r_in >> 16) & 0xff]
                | ip_maskr[6, (r_in >> 8) & 0xff]
                | ip_maskr[7, r_in & 0xff]);
            while (Convert.ToBoolean(count--))
            {
                kl = kl1;
                kr = kr1;
                j = 0;
                round = 16;
                while (Convert.ToBoolean(round--))
                {
                    r48l = (uint)(((r & 0x00000001) << 23)
                            | ((r & 0xf8000000) >> 9)
                            | ((r & 0x1f800000) >> 11)
                            | ((r & 0x01f80000) >> 13)
                            | ((r & 0x001f8000) >> 15));
                    r48r = (uint)(((r & 0x0001f800) << 7)
                            | ((r & 0x00001f80) << 5)
                            | ((r & 0x000001f8) << 3)
                            | ((r & 0x0000001f) << 1)
                            | ((r & 0x80000000) >> 31));
                    f = (r48l ^ r48r) & saltbits;
                    r48l ^= f ^ kl[j];
                    r48r ^= f ^ kr[j++];
                    f = psbox[0, m_sbox[0, r48l >> 12]]
                      | psbox[1, m_sbox[1, r48l & 0xfff]]
                      | psbox[2, m_sbox[2, r48r >> 12]]
                      | psbox[3, m_sbox[3, r48r & 0xfff]];
                    f ^= l;
                    l = r;
                    r = f;
                }
                r = l;
                l = f;
            }
            l_out = fp_maskl[0, l >> 24]
                    | fp_maskl[1, (l >> 16) & 0xff]
                    | fp_maskl[2, (l >> 8) & 0xff]
                    | fp_maskl[3, l & 0xff]
                    | fp_maskl[4, r >> 24]
                    | fp_maskl[5, (r >> 16) & 0xff]
                    | fp_maskl[6, (r >> 8) & 0xff]
                    | fp_maskl[7, r & 0xff];
            r_out = fp_maskr[0, l >> 24]
                    | fp_maskr[1, (l >> 16) & 0xff]
                    | fp_maskr[2, (l >> 8) & 0xff]
                    | fp_maskr[3, l & 0xff]
                    | fp_maskr[4, r >> 24]
                    | fp_maskr[5, (r >> 16) & 0xff]
                    | fp_maskr[6, (r >> 8) & 0xff]
                    | fp_maskr[7, r & 0xff];
            return 0;
        }
        private int des_cipher(byte[] _in, ref byte[] _out, ulong salt, int count)
        {
            uint l_out = 0, r_out = 0, rawl, rawr;
            int retval;
            if (!Convert.ToBoolean(des_initialised))
                des_init();
            setup_salt((uint)salt);
            rawl = (uint)(_in[0] << 24 | _in[1] << 16 | _in[2] << 8 | _in[3]);
            rawr = (uint)(_in[4] << 24 | _in[5] << 16 | _in[6] << 8 | _in[7]);
            retval = do_des(rawl, rawr, ref l_out, ref r_out, count);
            _out[3] = (byte)(l_out >> 24);
            _out[2] = (byte)(l_out >> 16);
            _out[1] = (byte)(l_out >> 8);
            _out[0] = (byte)(l_out);
            _out[7] = (byte)(r_out >> 24);
            _out[6] = (byte)(r_out >> 16);
            _out[5] = (byte)(r_out >> 8);
            _out[4] = (byte)(r_out);
            return retval;
        }

        /// <summary>This method encrypt the given string with the salt indicated, using DES Algorithm</summary> 
        /// <param name="k"> The string to be encrypted.</param>
        /// <param name="s">The salt.</param>
        /// <returns> </returns>
        public string Encrypt(string k, string s)
        {
            string DESHash = "";
            uint count, salt, l, r0 = 0, r1 = 0;
            int it = 0;
            uint[] keybuf = new uint[2];
            byte[] q = new byte[8];
            byte[] output = new byte[21];
            byte[] key = (new ASCIIEncoding()).GetBytes(k);
            byte[] setting = (new ASCIIEncoding()).GetBytes(s);
            if (!Convert.ToBoolean(des_initialised))
                des_init();
            for (int i = 0; i < 8 && i < key.Length; i++)
                q[i] = (byte)(key[it = i] << 1);
            it++;
            for (int i = key.Length; i < 8; i++)
                q[i] = 0;
            if (Convert.ToBoolean(des_setkey(q)))
                return null;
            if (setting[0] == '_')
            {
                count = 0;
                for (int i = 1; i < 5; i++)
                    count |= (uint)(ascii_to_bin((char)setting[i]) << ((i - 1) * 6));
                salt = 0;
                for (int i = 5; i < 9; i++)
                    salt |= (uint)(ascii_to_bin((char)setting[i]) << ((i - 5) * 6));
                for (int i = it; i < key.Length; )
                {
                    if (Convert.ToBoolean(des_cipher(q, ref q, 0, 1)))
                        return null;
                    for (int j = 0; j < 8 && i < key.Length; )
                        q[j++] ^= (byte)(key[i++] << 1);
                    if (Convert.ToBoolean(des_setkey(q)))
                        return null;
                }
                for (int i = 0; i < 9; i++)
                    DESHash += (char)setting[i];
            }
            else
            {
                count = 25;
                salt = (uint)((ascii_to_bin((char)setting[1]) << 6) | ascii_to_bin((char)setting[0]));
                DESHash += (char)setting[0];
                DESHash += (char)((Convert.ToBoolean(setting[1])) ? setting[1] : setting[0]);
            }
            setup_salt(salt);
            if (Convert.ToBoolean(do_des(0, 0, ref r0, ref r1, (int)count)))
                return null;
            l = (r0 >> 8);
            DESHash += (char)ascii64[(l >> 18) & 0x3f];
            DESHash += (char)ascii64[(l >> 12) & 0x3f];
            DESHash += (char)ascii64[(l >> 6) & 0x3f];
            DESHash += (char)ascii64[l & 0x3f];
            l = (r0 << 16) | ((r1 >> 16) & 0xffff);
            DESHash += (char)ascii64[(l >> 18) & 0x3f];
            DESHash += (char)ascii64[(l >> 12) & 0x3f];
            DESHash += (char)ascii64[(l >> 6) & 0x3f];
            DESHash += (char)ascii64[l & 0x3f];
            l = r1 << 2;
            DESHash += (char)ascii64[(l >> 12) & 0x3f];
            DESHash += (char)ascii64[(l >> 6) & 0x3f];
            DESHash += (char)ascii64[l & 0x3f];
            return DESHash;
        }

        public string Encrypt(string s) { return null; }
    }


}
