using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Extensions.ArrayExtensions;
using Extensions.IntExtensions;

namespace JetReader.Model.Format.Mobi
{
	public class MobiFile : PalmFile
	{
		protected uint _EncryptionType;
		protected uint HuffOffset;
		protected uint HuffCount;

		public uint EncryptionType => _EncryptionType;

        public string BookText { get; private set; }

		public MobiFile(Stream fs, bool leaveOpen = false):base(fs, leaveOpen)
        {
            if (Type != "BOOK") throw new Exception("Invalid .mobi ebook; must be of type 'BOOK' (is '" + Type + ").");
            if (Creator != "MOBI") throw new Exception("Invalid .mobi ebook; must have creator 'MOBI' (has '" + Creator + ").");

			var header = Records[0].Data;

			_EncryptionType = BytesToUint(new byte[] { 0, 0 }.Concat(header.Skip(12).Take(2)).ToArray());

			if (Compression == 2)
			{
				var sb = new StringBuilder();
                var bytes = new byte[4100];
                var bc = 0;
				foreach(var rec in Records.Skip(1).Take(TextRecordCount))
                {
                    var datatemp = rec.Data;
					var pos = 0;

					while (pos < datatemp.Length && bc < 4096)
					{
						var ab = datatemp[pos++];

						// If this is the end string indicator, we're done.
                        if (ab == 0)
                            break;

                        if (ab > 8 && ab <= 127)
                        {
                            bytes[bc++] = ab;
                        }
                        else if (ab > 0 && ab <= 8)
                        {
                            for (var i = 0; i < ab; i++)
                            {
                                if (pos >= datatemp.Length) break;
                                bytes[bc++] = datatemp[pos++];
                            }
                        }
                        else if (ab > 127 && ab <= 191)
                        {
                            var b = BytesToUint((byte) (ab & 63), pos < datatemp.Length ? datatemp[pos++] : (byte) 0);
                            var dist = b >> 3;
                            var count = (b << 29) >> 29;
                            var uncompressedPosition = bc - (int)dist;
                            for (var i = 0; i < count + 3; i++)
                            {
                                if (uncompressedPosition + i >= bc || uncompressedPosition + i < 0) break;
                                bytes[bc++] = bytes[uncompressedPosition + i];
                            }
                        }
                        else if (ab > 191 && ab <= 255)
                        {
                            bytes[bc++] = 32;
                            bytes[bc++] = (byte)(ab ^ 0x80);
                        }
                    }

					sb.Append(Encoding.UTF8.GetString(bytes.GetRange(0, bc)));
                    bc = 0;
                }
				BookText = sb.ToString();

			}
			else if (Compression == 17480)
            {
				HuffOffset = BytesToUint(header, 112);

				HuffCount = BytesToUint(header, 116);

                uint extraFlags = 0;
				if (header.Length >= 244)
				{
					extraFlags = BytesToUint(header, 240);
				}

				// The high compression version of the MOBI format uses a huffman coding scheme called Huff/cdic.
				// More info: https://github.com/chetpot/frePub/blob/master/docs/mobi.md

				var huffdata = Records[HuffOffset].Data;

				var cdicdata = Records[HuffOffset + 1].Data;
				var entrybits = BytesToUint(cdicdata.GetRange(12, 4));

                var off1 = BytesToUint(huffdata, 16);
                var off2 = BytesToUint(huffdata, 20);
                var huffdict1 = 0.To(256).Select(i => BitConverter.ToUInt32(huffdata, (int) (off1 + (i * 4)))).ToArray();
                var huffdict2 = 0.To(64).Select(i => BitConverter.ToUInt32(huffdata, (int) (off2 + (i * 4)))).ToArray();

                var huffdicts = 1.To(HuffCount).Select(i => Records[HuffOffset + i].Data).ToArray();

				var sb = new StringBuilder();
				for (var i = 0; i < TextRecordCount; i++)
				{
					// Remove Trailing Entries
					var datatemp = new List<byte>(Records[1 + i].Data);
					var size = GetSizeOfTrailingDataEntries(datatemp.ToArray(), datatemp.Count, extraFlags);

					sb.Append(Unpack(new BitReader(datatemp.GetRange(0, datatemp.Count - size).ToArray()), huffdict1, huffdict2, huffdicts, (int)((long)entrybits)));
				}

				BookText = sb.ToString();
			}
			else
			{
				throw new Exception("Unknown compression code: " + Compression);
			}
		}

		protected static string Unpack(BitReader bits, uint[] huffdict1, uint[] huffdict2, byte[][] huffdicts, int entrybits)
		{
			return Unpack(bits, 0, huffdict1, huffdict2, huffdicts, entrybits);
		}

		protected static string Unpack(BitReader bits, int depth, uint[] huffdict1, uint[] huffdict2, byte[][] huffdicts, int entrybits)
		{
			var sb = new StringBuilder();

			if (depth > 32)
			{
				throw new Exception("Too high depth level: " + depth);
			}
			while (bits.Left())
			{
				var dw = bits.Peek(32);
				var v = (huffdict1[dw >> 24]);
				var codelen = v & 0x1F;
				
				var code = dw >> (int)(32 - codelen);
				ulong r = (v >> 8);
				if ((v & 0x80) == 0)
				{
					while (code < ((ulong)huffdict2[(codelen - 1) * 2]))
					{
						codelen += 1;
						code = dw >> (int)(32 - codelen);
					}
					r = huffdict2[(codelen - 1) * 2 + 1];
				}
				r -= code;
				
				if (!bits.Eat(codelen))
				{
					return sb.ToString();
				}

				var dicno = r >> entrybits;
				var off1 = 16 + (r - (dicno << entrybits)) * 2;
				var dic = huffdicts[(int)((long)dicno)];
				var off2 = 16 + (char)(dic[(int)((long)off1)]) * 256 + (char)(dic[(int)((long)off1) + 1]);
				var blen = ((char)(dic[off2]) * 256 + (char)(dic[off2 + 1]));
				var slice = dic.GetRange(off2 + 2, (int)(blen & 0x7fff));

				if ((blen & 0x8000) > 0)
					sb.Append(Encoding.ASCII.GetString(slice));
				else
					sb.Append(Unpack(new BitReader(slice), depth + 1, huffdict1, huffdict2, huffdicts, entrybits));
			}
			return sb.ToString();
		}

		protected static int GetSizeOfTrailingDataEntries(byte[] ptr, int size, uint flags)
		{
			var res = 0;
			flags >>= 1;
			while (flags > 0)
			{
				if ((flags & 1) > 0)
				{
					res += (int)((long)GetSizeOfTrailingDataEntry(ptr, size - res));
				}
				flags >>= 1;
			}
			return res;
		}

		protected static uint GetSizeOfTrailingDataEntry(byte[] ptr, int size)
		{
			uint res = 0;
			var bitpos = 0;
			while (true)
			{
				uint v = (char)(ptr[size - 1]);
				res |= (v & 0x7F) << bitpos;
				bitpos += 7;
				size -= 1;
				if ((v & 0x80) != 0 || (bitpos >= 28) || (size == 0))
				{
					return res;
				}
			}
		}
	}

	public class BitReader
	{
		List<byte> _bytes;
		uint _pos = 0;
		int _bitCount;

		public BitReader(byte[] bytes)
        {
            _bytes = new List<byte>(bytes) {0, 0, 0, 0};
            _bitCount = (_bytes.Count - 4) * 8;
		}

		public ulong Peek(ulong n)
		{
			ulong r = 0;
			ulong g = 0;
			while (g < n)
			{
				r = (r << 8) | (char)(_bytes[(int)((long)(_pos + g >> 3))]);
				g = g + 8 - ((_pos + g) & 7);
			}
			return (ulong)(r >> (int)((long)(g - n))) & (ulong)(((ulong)(1) << (int)n) - 1);
		}

		public bool Eat(uint n)
		{
			_pos += n;
			return _pos <= _bitCount;
		}

		public bool Left()
		{
			return (_bitCount - _pos) > 0;
		}
	}
}
