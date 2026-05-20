using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Ionic.Crc;

namespace Ionic.Zlib;

public class ParallelDeflateOutputStream : Stream
{
	[Flags]
	private enum TraceBits : uint
	{
		None = 0u,
		NotUsed1 = 1u,
		EmitLock = 2u,
		EmitEnter = 4u,
		EmitBegin = 8u,
		EmitDone = 0x10u,
		EmitSkip = 0x20u,
		EmitAll = 0x3Au,
		Flush = 0x40u,
		Lifecycle = 0x80u,
		Session = 0x100u,
		Synch = 0x200u,
		Instance = 0x400u,
		Compress = 0x800u,
		Write = 0x1000u,
		WriteEnter = 0x2000u,
		WriteTake = 0x4000u,
		All = uint.MaxValue
	}

	private static readonly int IO_BUFFER_SIZE_DEFAULT = 65536;

	private static readonly int BufferPairsPerCore = 4;

	private List<WorkItem> _pool;

	private bool _leaveOpen;

	private bool emitting;

	private Stream _outStream;

	private int _maxBufferPairs;

	private int _bufferSize = IO_BUFFER_SIZE_DEFAULT;

	private AutoResetEvent _newlyCompressedBlob;

	private object _outputLock = new object();

	private bool _isClosed;

	private bool _firstWriteDone;

	private int _currentlyFilling;

	private int _lastFilled;

	private int _lastWritten;

	private int _latestCompressed;

	private int _Crc32;

	private CRC32 _runningCrc;

	private object _latestLock = new object();

	private Queue<int> _toWrite;

	private Queue<int> _toFill;

	private long _totalBytesProcessed;

	private CompressionLevel _compressLevel;

	private volatile Exception _pendingException;

	private bool _handlingException;

	private object _eLock = new object();

	private TraceBits _DesiredTrace = TraceBits.EmitAll | TraceBits.EmitEnter | TraceBits.Session | TraceBits.Compress | TraceBits.WriteEnter | TraceBits.WriteTake;

	public CompressionStrategy Strategy { get; private set; }

	public int MaxBufferPairs
	{
		get
		{
			return _maxBufferPairs;
		}
		set
		{
			if (value < 4)
			{
				throw new ArgumentException("MaxBufferPairs", "Value must be 4 or greater.");
			}
			_maxBufferPairs = value;
		}
	}

	public int BufferSize
	{
		get
		{
			return _bufferSize;
		}
		set
		{
			if (value < 1024)
			{
				throw new ArgumentOutOfRangeException("BufferSize", "BufferSize must be greater than 1024 bytes");
			}
			_bufferSize = value;
		}
	}

	public int Crc32 => _Crc32;

	public long BytesProcessed => _totalBytesProcessed;

	public override bool CanSeek => false;

	public override bool CanRead => false;

	public override bool CanWrite => _outStream.CanWrite;

	public override long Length
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override long Position
	{
		get
		{
			return _outStream.Position;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public ParallelDeflateOutputStream(Stream stream)
		: this(stream, CompressionLevel.Default, CompressionStrategy.Default, leaveOpen: false)
	{
	}

	public ParallelDeflateOutputStream(Stream stream, CompressionLevel level)
		: this(stream, level, CompressionStrategy.Default, leaveOpen: false)
	{
	}

	public ParallelDeflateOutputStream(Stream stream, bool leaveOpen)
		: this(stream, CompressionLevel.Default, CompressionStrategy.Default, leaveOpen)
	{
	}

	public ParallelDeflateOutputStream(Stream stream, CompressionLevel level, bool leaveOpen)
		: this(stream, CompressionLevel.Default, CompressionStrategy.Default, leaveOpen)
	{
	}

	public ParallelDeflateOutputStream(Stream stream, CompressionLevel level, CompressionStrategy strategy, bool leaveOpen)
	{
		_outStream = stream;
		_compressLevel = level;
		Strategy = strategy;
		_leaveOpen = leaveOpen;
		MaxBufferPairs = 16;
	}

	private void _InitializePoolOfWorkItems()
	{
		_toWrite = new Queue<int>();
		_toFill = new Queue<int>();
		_pool = new List<WorkItem>();
		int val = BufferPairsPerCore * Environment.ProcessorCount;
		val = Math.Min(val, _maxBufferPairs);
		for (int i = 0; i < val; i++)
		{
			_pool.Add(new WorkItem(_bufferSize, _compressLevel, Strategy, i));
			_toFill.Enqueue(i);
		}
		_newlyCompressedBlob = new AutoResetEvent(initialState: false);
		_runningCrc = new CRC32();
		_currentlyFilling = -1;
		_lastFilled = -1;
		_lastWritten = -1;
		_latestCompressed = -1;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		bool mustWait = false;
		if (_isClosed)
		{
			throw new InvalidOperationException();
		}
		if (_pendingException != null)
		{
			_handlingException = true;
			Exception pendingException = _pendingException;
			_pendingException = null;
			throw pendingException;
		}
		if (count == 0)
		{
			return;
		}
		if (!_firstWriteDone)
		{
			_InitializePoolOfWorkItems();
			_firstWriteDone = true;
		}
		do
		{
			EmitPendingBuffers(doAll: false, mustWait);
			mustWait = false;
			int num = -1;
			if (_currentlyFilling >= 0)
			{
				num = _currentlyFilling;
			}
			else
			{
				if (_toFill.Count == 0)
				{
					mustWait = true;
					continue;
				}
				num = _toFill.Dequeue();
				_lastFilled++;
			}
			WorkItem workItem = _pool[num];
			int num2 = ((workItem.buffer.Length - workItem.inputBytesAvailable > count) ? count : (workItem.buffer.Length - workItem.inputBytesAvailable));
			workItem.ordinal = _lastFilled;
			Buffer.BlockCopy(buffer, offset, workItem.buffer, workItem.inputBytesAvailable, num2);
			count -= num2;
			offset += num2;
			workItem.inputBytesAvailable += num2;
			if (workItem.inputBytesAvailable == workItem.buffer.Length)
			{
				if (!ThreadPool.QueueUserWorkItem(_DeflateOne, workItem))
				{
					throw new Exception("Cannot enqueue workitem");
				}
				_currentlyFilling = -1;
			}
			else
			{
				_currentlyFilling = num;
			}
			_ = 0;
		}
		while (count > 0);
	}

	private void _FlushFinish()
	{
		byte[] array = new byte[128];
		ZlibCodec zlibCodec = new ZlibCodec();
		int num = zlibCodec.InitializeDeflate(_compressLevel, wantRfc1950Header: false);
		zlibCodec.InputBuffer = null;
		zlibCodec.NextIn = 0;
		zlibCodec.AvailableBytesIn = 0;
		zlibCodec.OutputBuffer = array;
		zlibCodec.NextOut = 0;
		zlibCodec.AvailableBytesOut = array.Length;
		num = zlibCodec.Deflate(FlushType.Finish);
		if (num != 1 && num != 0)
		{
			throw new Exception("deflating: " + zlibCodec.Message);
		}
		if (array.Length - zlibCodec.AvailableBytesOut > 0)
		{
			_outStream.Write(array, 0, array.Length - zlibCodec.AvailableBytesOut);
		}
		zlibCodec.EndDeflate();
		_Crc32 = _runningCrc.Crc32Result;
	}

	private void _Flush(bool lastInput)
	{
		if (_isClosed)
		{
			throw new InvalidOperationException();
		}
		if (!emitting)
		{
			if (_currentlyFilling >= 0)
			{
				WorkItem wi = _pool[_currentlyFilling];
				_DeflateOne(wi);
				_currentlyFilling = -1;
			}
			if (lastInput)
			{
				EmitPendingBuffers(doAll: true, mustWait: false);
				_FlushFinish();
			}
			else
			{
				EmitPendingBuffers(doAll: false, mustWait: false);
			}
		}
	}

	public override void Flush()
	{
		if (_pendingException != null)
		{
			_handlingException = true;
			Exception pendingException = _pendingException;
			_pendingException = null;
			throw pendingException;
		}
		if (!_handlingException)
		{
			_Flush(lastInput: false);
		}
	}

	public override void Close()
	{
		if (_pendingException != null)
		{
			_handlingException = true;
			Exception pendingException = _pendingException;
			_pendingException = null;
			throw pendingException;
		}
		if (!_handlingException && !_isClosed)
		{
			_Flush(lastInput: true);
			if (!_leaveOpen)
			{
				_outStream.Close();
			}
			_isClosed = true;
		}
	}

	public new void Dispose()
	{
		Close();
		_pool = null;
		Dispose(disposing: true);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public void Reset(Stream stream)
	{
		if (!_firstWriteDone)
		{
			return;
		}
		_toWrite.Clear();
		_toFill.Clear();
		foreach (WorkItem item in _pool)
		{
			_toFill.Enqueue(item.index);
			item.ordinal = -1;
		}
		_firstWriteDone = false;
		_totalBytesProcessed = 0L;
		_runningCrc = new CRC32();
		_isClosed = false;
		_currentlyFilling = -1;
		_lastFilled = -1;
		_lastWritten = -1;
		_latestCompressed = -1;
		_outStream = stream;
	}

	private void EmitPendingBuffers(bool doAll, bool mustWait)
	{
		if (emitting)
		{
			return;
		}
		emitting = true;
		if (doAll || mustWait)
		{
			_newlyCompressedBlob.WaitOne();
		}
		do
		{
			int num = -1;
			int num2 = (doAll ? 200 : (mustWait ? (-1) : 0));
			int num3 = -1;
			do
			{
				if (Monitor.TryEnter(_toWrite, num2))
				{
					num3 = -1;
					try
					{
						if (_toWrite.Count > 0)
						{
							num3 = _toWrite.Dequeue();
						}
					}
					finally
					{
						Monitor.Exit(_toWrite);
					}
					if (num3 < 0)
					{
						continue;
					}
					WorkItem workItem = _pool[num3];
					if (workItem.ordinal != _lastWritten + 1)
					{
						lock (_toWrite)
						{
							_toWrite.Enqueue(num3);
						}
						if (num == num3)
						{
							_newlyCompressedBlob.WaitOne();
							num = -1;
						}
						else if (num == -1)
						{
							num = num3;
						}
						continue;
					}
					num = -1;
					_outStream.Write(workItem.compressed, 0, workItem.compressedBytesAvailable);
					_runningCrc.Combine(workItem.crc, workItem.inputBytesAvailable);
					_totalBytesProcessed += workItem.inputBytesAvailable;
					workItem.inputBytesAvailable = 0;
					_lastWritten = workItem.ordinal;
					_toFill.Enqueue(workItem.index);
					if (num2 == -1)
					{
						num2 = 0;
					}
				}
				else
				{
					num3 = -1;
				}
			}
			while (num3 >= 0);
		}
		while (doAll && _lastWritten != _latestCompressed);
		emitting = false;
	}

	private void _DeflateOne(object wi)
	{
		WorkItem workItem = (WorkItem)wi;
		try
		{
			CRC32 cRC = new CRC32();
			cRC.SlurpBlock(workItem.buffer, 0, workItem.inputBytesAvailable);
			DeflateOneSegment(workItem);
			workItem.crc = cRC.Crc32Result;
			lock (_latestLock)
			{
				if (workItem.ordinal > _latestCompressed)
				{
					_latestCompressed = workItem.ordinal;
				}
			}
			lock (_toWrite)
			{
				_toWrite.Enqueue(workItem.index);
			}
			_newlyCompressedBlob.Set();
		}
		catch (Exception pendingException)
		{
			lock (_eLock)
			{
				if (_pendingException != null)
				{
					_pendingException = pendingException;
				}
			}
		}
	}

	private bool DeflateOneSegment(WorkItem workitem)
	{
		ZlibCodec compressor = workitem.compressor;
		compressor.ResetDeflate();
		compressor.NextIn = 0;
		compressor.AvailableBytesIn = workitem.inputBytesAvailable;
		compressor.NextOut = 0;
		compressor.AvailableBytesOut = workitem.compressed.Length;
		do
		{
			compressor.Deflate(FlushType.None);
		}
		while (compressor.AvailableBytesIn > 0 || compressor.AvailableBytesOut == 0);
		compressor.Deflate(FlushType.Sync);
		workitem.compressedBytesAvailable = (int)compressor.TotalBytesOut;
		return true;
	}

	[Conditional("Trace")]
	private void TraceOutput(TraceBits bits, string format, params object[] varParams)
	{
		if ((bits & _DesiredTrace) != TraceBits.None)
		{
			lock (_outputLock)
			{
				int hashCode = Thread.CurrentThread.GetHashCode();
				Console.ForegroundColor = (ConsoleColor)(hashCode % 8 + 8);
				Console.Write("{0:000} PDOS ", hashCode);
				Console.WriteLine(format, varParams);
				Console.ResetColor();
			}
		}
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}
}
