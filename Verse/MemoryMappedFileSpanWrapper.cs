using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Verse
{
	public class MemoryMappedFileSpanWrapper : IDisposable
	{
		public class SimpleReadStream : Stream
		{
			private readonly MemoryMappedFileSpanWrapper _mmf;

			private long _pos;

			private readonly long _length;

			public override bool CanRead => true;

			public override bool CanSeek => true;

			public override bool CanWrite => false;

			public override long Length => _length;

			public override long Position
			{
				get
				{
					return _pos;
				}
				set
				{
					_pos = value;
				}
			}

			internal SimpleReadStream(MemoryMappedFileSpanWrapper mmf, long start, long length)
			{
				_mmf = mmf;
				_pos = start;
				_length = length;
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				if (_pos + count >= _mmf._length)
				{
					int num = (int)(_mmf._length - _pos);
					if (num <= 0)
					{
						return 0;
					}
					count = num;
				}
				_mmf.GetSpan(_pos, count).CopyTo(buffer.AsSpan(offset, count));
				_pos += count;
				return count;
			}

			public override int Read(Span<byte> buffer)
			{
				if (_pos + buffer.Length >= _mmf._length)
				{
					int num = (int)(_mmf._length - _pos);
					if (num <= 0)
					{
						return 0;
					}
					Span<byte> span = buffer;
					buffer = span.Slice(0, num);
				}
				_mmf.GetReadOnlySpan(_pos, buffer.Length).CopyTo(buffer);
				_pos += buffer.Length;
				return buffer.Length;
			}

			public override int ReadByte()
			{
				if (_pos >= _length)
				{
					return -1;
				}
				byte result = _mmf.Read<byte>(_pos);
				_pos++;
				return result;
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				switch (origin)
				{
				case SeekOrigin.Begin:
					_pos = offset;
					break;
				case SeekOrigin.Current:
					_pos += offset;
					break;
				case SeekOrigin.End:
					_pos = _length + offset;
					break;
				default:
					throw new ArgumentOutOfRangeException("origin", origin, null);
				}
				return _pos;
			}

			public override void Flush()
			{
				throw new NotSupportedException();
			}

			public override void SetLength(long value)
			{
				throw new NotSupportedException();
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				throw new NotSupportedException();
			}
		}

		private readonly MemoryMappedFileAccess _access;

		private readonly MemoryMappedFile _file;

		private MemoryMappedViewAccessor _accessor;

		private unsafe byte* _pointer;

		private readonly long _length;

		public long FileSize => _length;

		private static (MemoryMappedFile file, long length) OpenExistingMmf(string path)
		{
			return OpenExistingMmf(new FileInfo(path), MemoryMappedFileAccess.Read);
		}

		private static (MemoryMappedFile file, long length) OpenExistingMmf(FileInfo info, MemoryMappedFileAccess access, bool suppressExistsCheck = false)
		{
			if (!suppressExistsCheck && !info.Exists)
			{
				throw new FileNotFoundException("File not found", info.FullName);
			}
			long length = info.Length;
			return (file: MemoryMappedFile.CreateFromFile(info.FullName, FileMode.Open, null, length, access), length: length);
		}

		private MemoryMappedFileSpanWrapper((MemoryMappedFile file, long length) data, MemoryMappedFileAccess access)
		{
			_access = access;
			_length = data.length;
			(_file, _) = data;
		}

		public MemoryMappedFileSpanWrapper(string path)
			: this(OpenExistingMmf(path), MemoryMappedFileAccess.Read)
		{
		}

		public MemoryMappedFileSpanWrapper(FileInfo info)
			: this(OpenExistingMmf(info, MemoryMappedFileAccess.Read), MemoryMappedFileAccess.Read)
		{
		}

		public MemoryMappedFileSpanWrapper(FileInfo info, bool suppressExistsCheck)
			: this(OpenExistingMmf(info, MemoryMappedFileAccess.Read, suppressExistsCheck), MemoryMappedFileAccess.Read)
		{
		}

		public MemoryMappedFileSpanWrapper(string path, long newFileSize, bool create = true)
			: this((file: MemoryMappedFile.CreateFromFile(path, create ? FileMode.Create : FileMode.Open, null, newFileSize, MemoryMappedFileAccess.ReadWrite), length: newFileSize), MemoryMappedFileAccess.ReadWrite)
		{
		}

		public void Dispose()
		{
			ReleaseView();
			_file?.Dispose();
		}

		private unsafe void EnsureReadPointer()
		{
			if (_pointer == null)
			{
				_accessor = _file.CreateViewAccessor(0L, _length, _access);
				_accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref _pointer);
				if (_pointer == null)
				{
					_accessor.SafeMemoryMappedViewHandle.ReleasePointer();
					_accessor.Dispose();
					_file.Dispose();
					throw new InvalidOperationException("Failed to acquire pointer to memory mapped file");
				}
			}
		}

		private unsafe Span<byte> GetSpan(long offset, long length)
		{
			EnsureReadPointer();
			if (offset < 0 || offset >= _length)
			{
				throw new ArgumentOutOfRangeException("offset", $"Offset {offset} is out of range [0, {_length})");
			}
			if (length < 0 || offset + length > _length)
			{
				throw new ArgumentOutOfRangeException("length", $"Length {length} is out of range [0, {_length - offset}) given offset {offset} and total file length {_length}");
			}
			if (length > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("length", $"Length must be less than int.MaxValue, was {length}");
			}
			return new Span<byte>(_pointer + offset, (int)length);
		}

		public Span<byte> GetSpan(long offset)
		{
			return GetSpan(offset, _length - offset);
		}

		public ReadOnlySpan<byte> GetReadOnlySpan(long offset, long length)
		{
			return GetSpan(offset, length);
		}

		public unsafe T Read<T>(long offset) where T : unmanaged
		{
			return MemoryMarshal.Read<T>(GetReadOnlySpan(offset, sizeof(T)));
		}

		public unsafe void Write<T>(long offset, T value) where T : unmanaged
		{
			MemoryMarshal.Write(GetSpan(offset, sizeof(T)), ref value);
		}

		public void Write(long offset, ReadOnlySpan<byte> span)
		{
			span.CopyTo(GetSpan(offset, span.Length));
		}

		public void CopyTo(Stream other, long start, long length = -1L)
		{
			if (length == -1)
			{
				length = _length - start;
			}
			long num = length / int.MaxValue;
			long num2 = length % int.MaxValue;
			for (long num3 = 0L; num3 < num; num3++)
			{
				long offset = start + num3 * int.MaxValue;
				other.Write(GetReadOnlySpan(offset, 2147483647L));
			}
			if (num2 > 0)
			{
				other.Write(GetReadOnlySpan(start + num * int.MaxValue, num2));
			}
		}

		private unsafe Span<byte> GetBufferForCopyFrom(long pos, long length)
		{
			if (_pointer != null)
			{
				ReleaseView();
			}
			try
			{
				length = Math.Min(67108864L, length);
				_accessor = _file.CreateViewAccessor(pos, length, _access);
				byte* pointer = null;
				_accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
				return new Span<byte>(pointer, (int)length);
			}
			catch (Exception innerException)
			{
				throw new IOException($"Failed to get buffer to write to MMF of size {_length} at position {pos} with length {length}", innerException);
			}
		}

		private unsafe void ReleaseView()
		{
			_accessor?.SafeMemoryMappedViewHandle.ReleasePointer();
			_accessor?.Dispose();
			_pointer = null;
			_accessor = null;
		}

		public void CopyFrom(Stream other, long initialPos = 0L)
		{
			long num = initialPos;
			long num2 = _length - initialPos;
			Span<byte> bufferForCopyFrom = GetBufferForCopyFrom(num, num2);
			while (true)
			{
				int num3 = other.Read(bufferForCopyFrom);
				if (num3 == 0)
				{
					break;
				}
				num += num3;
				num2 -= num3;
				if (num2 <= 0)
				{
					break;
				}
				bufferForCopyFrom = GetBufferForCopyFrom(num, num2);
			}
			ReleaseView();
		}

		public void CopyFrom(MemoryMappedFileSpanWrapper other, long readPos = 0L, long writePos = 0L)
		{
			long num = writePos;
			long num2 = Math.Min(_length - writePos, other._length - readPos);
			Span<byte> bufferForCopyFrom = GetBufferForCopyFrom(num, num2);
			while (true)
			{
				ReadOnlySpan<byte> readOnlySpan = other.GetReadOnlySpan(readPos, bufferForCopyFrom.Length);
				readOnlySpan.CopyTo(bufferForCopyFrom);
				other.ReleaseView();
				ReleaseView();
				num += readOnlySpan.Length;
				num2 -= readOnlySpan.Length;
				readPos += readOnlySpan.Length;
				if (num2 <= 0)
				{
					break;
				}
				bufferForCopyFrom = GetBufferForCopyFrom(num, num2);
			}
			other.ReleaseView();
			ReleaseView();
		}

		public unsafe byte* GetDirectPointer()
		{
			return _pointer;
		}

		public SimpleReadStream CreateReadStream(long start, long length = -1L)
		{
			if (length == -1)
			{
				length = _length - start;
			}
			return new SimpleReadStream(this, start, length);
		}
	}
}
