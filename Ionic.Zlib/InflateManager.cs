namespace Ionic.Zlib
{
	internal sealed class InflateManager
	{
		private enum InflateManagerMode
		{
			METHOD,
			FLAG,
			DICT4,
			DICT3,
			DICT2,
			DICT1,
			DICT0,
			BLOCKS,
			CHECK4,
			CHECK3,
			CHECK2,
			CHECK1,
			DONE,
			BAD
		}

		private const int PRESET_DICT = 32;

		private const int Z_DEFLATED = 8;

		private InflateManagerMode mode;

		internal ZlibCodec _codec;

		internal int method;

		internal uint computedCheck;

		internal uint expectedCheck;

		internal int marker;

		private bool _handleRfc1950HeaderBytes = true;

		internal int wbits;

		internal InflateBlocks blocks;

		private static readonly byte[] mark = new byte[4]
		{
			0,
			0,
			255,
			255
		};

		internal bool HandleRfc1950HeaderBytes
		{
			get
			{
				return _handleRfc1950HeaderBytes;
			}
			set
			{
				_handleRfc1950HeaderBytes = value;
			}
		}

		public InflateManager()
		{
		}

		public InflateManager(bool expectRfc1950HeaderBytes)
		{
			_handleRfc1950HeaderBytes = expectRfc1950HeaderBytes;
		}

		internal int Reset()
		{
			_codec.TotalBytesIn = (_codec.TotalBytesOut = 0L);
			_codec.Message = null;
			mode = ((!HandleRfc1950HeaderBytes) ? InflateManagerMode.BLOCKS : InflateManagerMode.METHOD);
			blocks.Reset();
			return 0;
		}

		internal int End()
		{
			if (blocks != null)
			{
				blocks.Free();
			}
			blocks = null;
			return 0;
		}

		internal int Initialize(ZlibCodec codec, int w)
		{
			_codec = codec;
			_codec.Message = null;
			blocks = null;
			if (w < 8 || w > 15)
			{
				End();
				throw new ZlibException("Bad window size.");
			}
			wbits = w;
			blocks = new InflateBlocks(codec, HandleRfc1950HeaderBytes ? this : null, 1 << w);
			Reset();
			return 0;
		}

		internal int Inflate(FlushType flush)
		{
			if (_codec.InputBuffer == null)
			{
				throw new ZlibException("InputBuffer is null. ");
			}
			int num = 0;
			int num2 = -5;
			while (true)
			{
				switch (mode)
				{
				case InflateManagerMode.METHOD:
					if (_codec.AvailableBytesIn == 0)
					{
						return num2;
					}
					num2 = num;
					_codec.AvailableBytesIn--;
					_codec.TotalBytesIn++;
					if (((method = _codec.InputBuffer[_codec.NextIn++]) & 0xF) != 8)
					{
						mode = InflateManagerMode.BAD;
						_codec.Message = $"unknown compression method (0x{method:X2})";
						marker = 5;
					}
					else if ((method >> 4) + 8 > wbits)
					{
						mode = InflateManagerMode.BAD;
						_codec.Message = $"invalid window size ({(method >> 4) + 8})";
						marker = 5;
					}
					else
					{
						mode = InflateManagerMode.FLAG;
					}
					break;
				case InflateManagerMode.FLAG:
				{
					if (_codec.AvailableBytesIn == 0)
					{
						return num2;
					}
					num2 = num;
					_codec.AvailableBytesIn--;
					_codec.TotalBytesIn++;
					int num3 = _codec.InputBuffer[_codec.NextIn++] & 0xFF;
					if (((method << 8) + num3) % 31 != 0)
					{
						mode = InflateManagerMode.BAD;
						_codec.Message = "incorrect header check";
						marker = 5;
					}
					else
					{
						mode = (((num3 & 0x20) == 0) ? InflateManagerMode.BLOCKS : InflateManagerMode.DICT4);
					}
					break;
				}
				case InflateManagerMode.DICT4:
					if (_codec.AvailableBytesIn == 0)
					{
						return num2;
					}
					num2 = num;
					_codec.AvailableBytesIn--;
					_codec.TotalBytesIn++;
					expectedCheck = (uint)((_codec.InputBuffer[_codec.NextIn++] << 24) & 4278190080u);
					mode = InflateManagerMode.DICT3;
					break;
				case InflateManagerMode.DICT3:
					if (_codec.AvailableBytesIn == 0)
					{
						return num2;
					}
					num2 = num;
					_codec.AvailableBytesIn--;
					_codec.TotalBytesIn++;
					expectedCheck += (uint)((_codec.InputBuffer[_codec.NextIn++] << 16) & 0xFF0000);
					mode = InflateManagerMode.DICT2;
					break;
				case InflateManagerMode.DICT2:
					if (_codec.AvailableBytesIn == 0)
					{
						return num2;
					}
					num2 = num;
					_codec.AvailableBytesIn--;
					_codec.TotalBytesIn++;
					expectedCheck += (uint)((_codec.InputBuffer[_codec.NextIn++] << 8) & 0xFF00);
					mode = InflateManagerMode.DICT1;
					break;
				case InflateManagerMode.DICT1:
					if (_codec.AvailableBytesIn == 0)
					{
						return num2;
					}
					num2 = num;
					_codec.AvailableBytesIn--;
					_codec.TotalBytesIn++;
					expectedCheck += (uint)(_codec.InputBuffer[_codec.NextIn++] & 0xFF);
					_codec._Adler32 = expectedCheck;
					mode = InflateManagerMode.DICT0;
					return 2;
				case InflateManagerMode.DICT0:
					mode = InflateManagerMode.BAD;
					_codec.Message = "need dictionary";
					marker = 0;
					return -2;
				case InflateManagerMode.BLOCKS:
					num2 = blocks.Process(num2);
					switch (num2)
					{
					case -3:
						mode = InflateManagerMode.BAD;
						marker = 0;
						continue;
					case 0:
						num2 = num;
						break;
					}
					if (num2 != 1)
					{
						return num2;
					}
					num2 = num;
					computedCheck = blocks.Reset();
					if (!HandleRfc1950HeaderBytes)
					{
						mode = InflateManagerMode.DONE;
						return 1;
					}
					mode = InflateManagerMode.CHECK4;
					break;
				case InflateManagerMode.CHECK4:
					if (_codec.AvailableBytesIn == 0)
					{
						return num2;
					}
					num2 = num;
					_codec.AvailableBytesIn--;
					_codec.TotalBytesIn++;
					expectedCheck = (uint)((_codec.InputBuffer[_codec.NextIn++] << 24) & 4278190080u);
					mode = InflateManagerMode.CHECK3;
					break;
				case InflateManagerMode.CHECK3:
					if (_codec.AvailableBytesIn == 0)
					{
						return num2;
					}
					num2 = num;
					_codec.AvailableBytesIn--;
					_codec.TotalBytesIn++;
					expectedCheck += (uint)((_codec.InputBuffer[_codec.NextIn++] << 16) & 0xFF0000);
					mode = InflateManagerMode.CHECK2;
					break;
				case InflateManagerMode.CHECK2:
					if (_codec.AvailableBytesIn == 0)
					{
						return num2;
					}
					num2 = num;
					_codec.AvailableBytesIn--;
					_codec.TotalBytesIn++;
					expectedCheck += (uint)((_codec.InputBuffer[_codec.NextIn++] << 8) & 0xFF00);
					mode = InflateManagerMode.CHECK1;
					break;
				case InflateManagerMode.CHECK1:
					if (_codec.AvailableBytesIn == 0)
					{
						return num2;
					}
					num2 = num;
					_codec.AvailableBytesIn--;
					_codec.TotalBytesIn++;
					expectedCheck += (uint)(_codec.InputBuffer[_codec.NextIn++] & 0xFF);
					if (computedCheck != expectedCheck)
					{
						mode = InflateManagerMode.BAD;
						_codec.Message = "incorrect data check";
						marker = 5;
						break;
					}
					mode = InflateManagerMode.DONE;
					return 1;
				case InflateManagerMode.DONE:
					return 1;
				case InflateManagerMode.BAD:
					throw new ZlibException($"Bad state ({_codec.Message})");
				default:
					throw new ZlibException("Stream error.");
				}
			}
		}

		internal int SetDictionary(byte[] dictionary)
		{
			int start = 0;
			int num = dictionary.Length;
			if (mode != InflateManagerMode.DICT0)
			{
				throw new ZlibException("Stream error.");
			}
			if (Adler.Adler32(1u, dictionary, 0, dictionary.Length) != _codec._Adler32)
			{
				return -3;
			}
			_codec._Adler32 = Adler.Adler32(0u, null, 0, 0);
			if (num >= 1 << wbits)
			{
				num = (1 << wbits) - 1;
				start = dictionary.Length - num;
			}
			blocks.SetDictionary(dictionary, start, num);
			mode = InflateManagerMode.BLOCKS;
			return 0;
		}

		internal int Sync()
		{
			if (mode != InflateManagerMode.BAD)
			{
				mode = InflateManagerMode.BAD;
				marker = 0;
			}
			int num;
			if ((num = _codec.AvailableBytesIn) == 0)
			{
				return -5;
			}
			int num2 = _codec.NextIn;
			int num3 = marker;
			while (num != 0 && num3 < 4)
			{
				num3 = ((_codec.InputBuffer[num2] != mark[num3]) ? ((_codec.InputBuffer[num2] == 0) ? (4 - num3) : 0) : (num3 + 1));
				num2++;
				num--;
			}
			_codec.TotalBytesIn += num2 - _codec.NextIn;
			_codec.NextIn = num2;
			_codec.AvailableBytesIn = num;
			marker = num3;
			if (num3 != 4)
			{
				return -3;
			}
			long totalBytesIn = _codec.TotalBytesIn;
			long totalBytesOut = _codec.TotalBytesOut;
			Reset();
			_codec.TotalBytesIn = totalBytesIn;
			_codec.TotalBytesOut = totalBytesOut;
			mode = InflateManagerMode.BLOCKS;
			return 0;
		}

		internal int SyncPoint(ZlibCodec z)
		{
			return blocks.SyncPoint();
		}
	}
}
