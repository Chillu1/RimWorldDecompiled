namespace Ionic.Zlib;

internal class WorkItem
{
	public byte[] buffer;

	public byte[] compressed;

	public int crc;

	public int index;

	public int ordinal;

	public int inputBytesAvailable;

	public int compressedBytesAvailable;

	public ZlibCodec compressor;

	public WorkItem(int size, CompressionLevel compressLevel, CompressionStrategy strategy, int ix)
	{
		buffer = new byte[size];
		int num = size + (size / 32768 + 1) * 5 * 2;
		compressed = new byte[num];
		compressor = new ZlibCodec();
		compressor.InitializeDeflate(compressLevel, wantRfc1950Header: false);
		compressor.OutputBuffer = compressed;
		compressor.InputBuffer = buffer;
		index = ix;
	}
}
