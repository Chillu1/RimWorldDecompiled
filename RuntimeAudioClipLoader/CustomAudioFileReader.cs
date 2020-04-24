using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NVorbis.NAudioSupport;
using System.IO;
using UnityEngine;

namespace RuntimeAudioClipLoader
{
	internal class CustomAudioFileReader : WaveStream, ISampleProvider
	{
		private WaveStream readerStream;

		private readonly SampleChannel sampleChannel;

		private readonly int destBytesPerSample;

		private readonly int sourceBytesPerSample;

		private readonly long length;

		private readonly object lockObject;

		public override WaveFormat WaveFormat => sampleChannel.WaveFormat;

		public override long Length => length;

		public override long Position
		{
			get
			{
				return SourceToDest(readerStream.Position);
			}
			set
			{
				lock (lockObject)
				{
					readerStream.Position = DestToSource(value);
				}
			}
		}

		public float Volume
		{
			get
			{
				return sampleChannel.Volume;
			}
			set
			{
				sampleChannel.Volume = value;
			}
		}

		public CustomAudioFileReader(Stream stream, AudioFormat format)
		{
			lockObject = new object();
			CreateReaderStream(stream, format);
			sourceBytesPerSample = readerStream.WaveFormat.BitsPerSample / 8 * readerStream.WaveFormat.Channels;
			sampleChannel = new SampleChannel(readerStream, forceStereo: false);
			destBytesPerSample = 4 * sampleChannel.WaveFormat.Channels;
			length = SourceToDest(readerStream.Length);
		}

		private void CreateReaderStream(Stream stream, AudioFormat format)
		{
			switch (format)
			{
			case AudioFormat.wav:
				readerStream = new WaveFileReader(stream);
				if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm && readerStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
				{
					readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
					readerStream = new BlockAlignReductionStream(readerStream);
				}
				break;
			case AudioFormat.mp3:
				readerStream = new Mp3FileReader(stream);
				break;
			case AudioFormat.aiff:
				readerStream = new AiffFileReader(stream);
				break;
			case AudioFormat.ogg:
				readerStream = new VorbisWaveReader(stream);
				break;
			default:
				Debug.LogWarning("Audio format " + format + " is not supported");
				break;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			WaveBuffer waveBuffer = new WaveBuffer(buffer);
			int count2 = count / 4;
			return Read(waveBuffer.FloatBuffer, offset / 4, count2) * 4;
		}

		public int Read(float[] buffer, int offset, int count)
		{
			lock (lockObject)
			{
				return sampleChannel.Read(buffer, offset, count);
			}
		}

		private long SourceToDest(long sourceBytes)
		{
			return destBytesPerSample * (sourceBytes / sourceBytesPerSample);
		}

		private long DestToSource(long destBytes)
		{
			return sourceBytesPerSample * (destBytes / destBytesPerSample);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && readerStream != null)
			{
				readerStream.Dispose();
				readerStream = null;
			}
			base.Dispose(disposing);
		}
	}
}
