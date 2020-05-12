using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Extensions.ArrayExtensions;

namespace JetReader.Model.Format.Mobi
{
	public struct PalmRecord
	{
		public byte[] Data;
	}

	public struct PalmRecordInfo
	{
		public uint DataOffset;
		public uint Attributes;
		public uint UniqueID;
	}

	public class PalmFile
	{
		protected string Name; // Database Name
		internal ushort Attributes; // bit field.
		protected ushort Version; // File Version
		protected DateTime CreationDate; // Creation Date
		protected DateTime ModificationDate; // Modification Date
		protected DateTime LastBackupDate; // Last Backup Date
		protected uint ModificationNumber;
		protected uint AppInfoID;
		protected uint SortInfoID;
		protected string Type;
		protected string Creator;
		protected uint UniqueIDSeed;
		protected uint NextRecordListID;
		protected ushort Compression;
		protected uint TextLength;
		protected ushort TextRecordCount;
		protected ushort TextRecordSize;
		protected ushort CurrentPosition;

		internal PalmRecordInfo[] RecordInfos;
		internal PalmRecord[] Records;

		public PalmFile() { }

		public PalmFile(Stream stream, bool leaveOpen = false)
		{
			EndiannessAwareBinaryReader r = null;
			var baseDate = new DateTime(1904, 1, 1);

			try
			{
				r = new EndiannessAwareBinaryReader(stream, EndiannessAwareBinaryReader.Endianness.Big);

				Name = new string(r.ReadChars(32));
				Attributes = r.ReadUInt16();
				Version = r.ReadUInt16();
				CreationDate = baseDate + new TimeSpan(0, (int)(r.ReadUInt32() / 60), 0);
				ModificationDate = baseDate + new TimeSpan(0, (int)(r.ReadUInt32() / 60), 0);
				LastBackupDate = baseDate + new TimeSpan(0, (int)(r.ReadUInt32() / 60), 0);

				ModificationNumber = r.ReadUInt32();
				AppInfoID = r.ReadUInt32();
				SortInfoID = r.ReadUInt32();

				Type = new string(r.ReadChars(4));
				Creator = new string(r.ReadChars(4));

				UniqueIDSeed = r.ReadUInt32();
				NextRecordListID = r.ReadUInt32();

				var numberOfRecords = r.ReadUInt16();
				RecordInfos = new PalmRecordInfo[numberOfRecords];
				Records = new PalmRecord[numberOfRecords];

				for (var i = 0; i < numberOfRecords; i++)
				{
					RecordInfos[i].DataOffset = r.ReadUInt32();

					var info = r.ReadBytes(4);
					RecordInfos[i].Attributes = info[0];
					info[0] = 0;
					RecordInfos[i].UniqueID = BytesToUint(info);
				}

                r.ReadUInt16(); // Unknown value

				for (var i = 0; i < numberOfRecords; i++)
                {
                    var start = RecordInfos[i].DataOffset;
                    var end = i < numberOfRecords - 1 ? RecordInfos[i + 1].DataOffset : r.BaseStream.Length;
					var length = ((int)(end - start));
					Records[i].Data = r.ReadBytes(length);
				}

                var header = new EndiannessAwareBinaryReader(new MemoryStream(Records[0].Data), EndiannessAwareBinaryReader.Endianness.Big);

				Compression = header.ReadUInt16();
                header.ReadUInt16(); // Unknown value
				TextLength = header.ReadUInt32();
				TextRecordCount = header.ReadUInt16();
				TextRecordSize = header.ReadUInt16();
				CurrentPosition = header.ReadUInt16();
			}
			finally
			{
				if (!leaveOpen)
				{
					r?.Close();
					r?.Dispose();
					stream?.Close();
					stream?.Dispose();
				}
			}
		}

        public static uint BytesToUint(byte[] bytes)
        {
            return (uint)((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]);
		}

        public static uint BytesToUint(byte one, byte two)
        {
            return (uint)((one << 8) | two);
        }

		public static uint BytesToUint(byte[] bytes, int offset)
		{
			return (uint)((bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3]);
		}
	}

	public class EndiannessAwareBinaryReader : BinaryReader
	{
		public enum Endianness
		{
			Little,
			Big,
		}

		private readonly Endianness _endianness = Endianness.Little;

		public EndiannessAwareBinaryReader(Stream input) : base(input)
		{
		}

		public EndiannessAwareBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
		{
		}

		public EndiannessAwareBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
		{
		}

		public EndiannessAwareBinaryReader(Stream input, Endianness endianness) : base(input)
		{
			_endianness = endianness;
		}

		public EndiannessAwareBinaryReader(Stream input, Encoding encoding, Endianness endianness) : base(input, encoding)
		{
			_endianness = endianness;
		}

		public EndiannessAwareBinaryReader(Stream input, Encoding encoding, bool leaveOpen, Endianness endianness) : base(input, encoding, leaveOpen)
		{
			_endianness = endianness;
		}

		public override short ReadInt16() => ReadInt16(_endianness);

		public override int ReadInt32() => ReadInt32(_endianness);

		public override long ReadInt64() => ReadInt64(_endianness);

		public override ushort ReadUInt16() => ReadUInt16(_endianness);

		public override uint ReadUInt32() => ReadUInt32(_endianness);

		public override ulong ReadUInt64() => ReadUInt64(_endianness);

		public short ReadInt16(Endianness endianness) => BitConverter.ToInt16(ReadForEndianness(sizeof(short), endianness), 0);

		public int ReadInt32(Endianness endianness) => BitConverter.ToInt32(ReadForEndianness(sizeof(int), endianness), 0);

		public long ReadInt64(Endianness endianness) => BitConverter.ToInt64(ReadForEndianness(sizeof(long), endianness), 0);

		public ushort ReadUInt16(Endianness endianness) => BitConverter.ToUInt16(ReadForEndianness(sizeof(ushort), endianness), 0);

		public uint ReadUInt32(Endianness endianness) => BitConverter.ToUInt32(ReadForEndianness(sizeof(uint), endianness), 0);

		public ulong ReadUInt64(Endianness endianness) => BitConverter.ToUInt64(ReadForEndianness(sizeof(ulong), endianness), 0);

		private byte[] ReadForEndianness(int bytesToRead, Endianness endianness)
		{
			var bytesRead = ReadBytes(bytesToRead);

			if ((endianness == Endianness.Little && !BitConverter.IsLittleEndian)
				|| (endianness == Endianness.Big && BitConverter.IsLittleEndian))
			{
				Array.Reverse(bytesRead);
			}

			return bytesRead;
		}
	}
}
