using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Verse;

public class LoadableXmlAsset
{
	public readonly string name;

	public readonly string fullFolderPath;

	public readonly XmlDocument xmlDoc;

	public readonly ModContentPack mod;

	private const int BufferSize = 16000;

	private static readonly XmlReaderSettings Settings = new XmlReaderSettings
	{
		IgnoreComments = true,
		IgnoreWhitespace = true,
		CheckCharacters = false,
		Async = false
	};

	public string FullFilePath
	{
		get
		{
			string text = fullFolderPath;
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			return text + directorySeparatorChar + name;
		}
	}

	public LoadableXmlAsset(string name, string text)
	{
		this.name = name;
		fullFolderPath = string.Empty;
		try
		{
			using StringReader input = new StringReader(text);
			using XmlReader reader = XmlReader.Create(input, Settings);
			xmlDoc = new XmlDocument();
			xmlDoc.Load(reader);
		}
		catch (Exception arg)
		{
			Log.Warning($"Exception reading {name} as XML: {arg}");
			xmlDoc = null;
		}
	}

	public LoadableXmlAsset(FileInfo file, ModContentPack mod)
	{
		this.mod = mod;
		name = file.Name;
		fullFolderPath = file.Directory.FullName;
		try
		{
			using MemoryMappedFileSpanWrapper memoryMappedFileSpanWrapper = new MemoryMappedFileSpanWrapper(file);
			ReadOnlySpan<byte> readOnlySpan = memoryMappedFileSpanWrapper.GetReadOnlySpan(0L, memoryMappedFileSpanWrapper.FileSize);
			if (readOnlySpan.Length >= 3 && readOnlySpan[0] == 239 && readOnlySpan[1] == 187 && readOnlySpan[2] == 191)
			{
				ReadOnlySpan<byte> readOnlySpan2 = readOnlySpan;
				readOnlySpan = readOnlySpan2.Slice(3, readOnlySpan2.Length - 3);
			}
			using StringReader input = new StringReader(Encoding.UTF8.GetString(readOnlySpan));
			using XmlReader reader = XmlReader.Create(input, Settings);
			xmlDoc = new XmlDocument();
			xmlDoc.Load(reader);
		}
		catch (Exception arg)
		{
			Log.Warning($"Exception reading {name} as XML: {arg}");
			xmlDoc = null;
		}
	}

	public override string ToString()
	{
		return name;
	}
}
