using NAudio.Wave;
using System;
using System.IO;

namespace NVorbis.NAudioSupport
{
	internal class VorbisWaveReader : WaveStream, IDisposable, ISampleProvider, IWaveProvider
	{
		private VorbisReader _reader;

		private WaveFormat _waveFormat;

		[ThreadStatic]
		private static float[] _conversionBuffer;

		public override WaveFormat WaveFormat => _waveFormat;

		public override long Length => (long)(_reader.TotalTime.TotalSeconds * (double)_waveFormat.SampleRate * (double)_waveFormat.Channels * 4.0);

		public override long Position
		{
			get
			{
				return (long)(_reader.DecodedTime.TotalMilliseconds * (double)_reader.SampleRate * (double)_reader.Channels * 4.0);
			}
			set
			{
				if (value < 0 || value > Length)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_reader.DecodedTime = TimeSpan.FromSeconds((double)value / (double)_reader.SampleRate / (double)_reader.Channels / 4.0);
			}
		}

		public bool IsParameterChange => _reader.IsParameterChange;

		public int StreamCount => _reader.StreamCount;

		public int? NextStreamIndex
		{
			get;
			set;
		}

		public int CurrentStream
		{
			get
			{
				return _reader.StreamIndex;
			}
			set
			{
				if (!_reader.SwitchStreams(value))
				{
					throw new InvalidDataException("The selected stream is not a valid Vorbis stream!");
				}
				if (NextStreamIndex.HasValue && value == NextStreamIndex.Value)
				{
					NextStreamIndex = null;
				}
			}
		}

		public int UpperBitrate => _reader.UpperBitrate;

		public int NominalBitrate => _reader.NominalBitrate;

		public int LowerBitrate => _reader.LowerBitrate;

		public string Vendor => _reader.Vendor;

		public string[] Comments => _reader.Comments;

		public long ContainerOverheadBits => _reader.ContainerOverheadBits;

		public IVorbisStreamStatus[] Stats => _reader.Stats;

		public VorbisWaveReader(string fileName)
		{
			_reader = new VorbisReader(fileName);
			_waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(_reader.SampleRate, _reader.Channels);
		}

		public VorbisWaveReader(Stream sourceStream)
		{
			_reader = new VorbisReader(sourceStream, closeStreamOnDispose: false);
			_waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(_reader.SampleRate, _reader.Channels);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _reader != null)
			{
				_reader.Dispose();
				_reader = null;
			}
			base.Dispose(disposing);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			count /= 4;
			count -= count % _reader.Channels;
			float[] array = _conversionBuffer ?? (_conversionBuffer = new float[count]);
			if (array.Length < count)
			{
				array = (_conversionBuffer = new float[count]);
			}
			int num = Read(array, 0, count) * 4;
			Buffer.BlockCopy(array, 0, buffer, offset, num);
			return num;
		}

		public int Read(float[] buffer, int offset, int count)
		{
			return _reader.ReadSamples(buffer, offset, count);
		}

		public void ClearParameterChange()
		{
			_reader.ClearParameterChange();
		}

		public bool GetNextStreamIndex()
		{
			if (!NextStreamIndex.HasValue)
			{
				int streamCount = _reader.StreamCount;
				if (_reader.FindNextStream())
				{
					NextStreamIndex = streamCount;
					return true;
				}
			}
			return false;
		}
	}
}
