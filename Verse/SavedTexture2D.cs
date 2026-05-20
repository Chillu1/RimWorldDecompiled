using System;
using UnityEngine;

namespace Verse;

public class SavedTexture2D : IExposable, IDisposable
{
	public Texture2D Texture { get; private set; }

	public SavedTexture2D(Texture2D texture)
	{
		Texture = texture;
	}

	public void ExposeData()
	{
		int value = 0;
		int value2 = 0;
		byte value3 = 0;
		int value4 = -1;
		bool value5 = false;
		string value6 = "";
		int value7 = 0;
		byte value8 = 0;
		byte value9 = 0;
		byte value10 = 0;
		float value11 = 0f;
		int value12 = 0;
		bool value13 = false;
		int value14 = 0;
		byte[] arr = null;
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			value = Texture.width;
			value2 = Texture.height;
			value3 = (byte)Texture.format;
			value4 = Texture.mipmapCount;
			value5 = !Texture.isDataSRGB;
			value6 = Texture.name;
			value7 = Texture.anisoLevel;
			value8 = (byte)Texture.filterMode;
			value9 = (byte)Texture.wrapModeU;
			value10 = (byte)Texture.wrapModeV;
			value11 = Texture.mipMapBias;
			value12 = Texture.requestedMipmapLevel;
			value13 = Texture.ignoreMipmapLimit;
			value14 = Texture.minimumMipmapLevel;
			arr = Texture.GetRawTextureData();
		}
		Scribe_Values.Look(ref value6, "name");
		Scribe_Values.Look(ref value, "width", 0);
		Scribe_Values.Look(ref value2, "height", 0);
		Scribe_Values.Look<byte>(ref value3, "format", 0);
		Scribe_Values.Look(ref value4, "mipCount", 0);
		Scribe_Values.Look(ref value5, "linear", defaultValue: false);
		Scribe_Values.Look(ref value7, "anisoLevel", 0);
		Scribe_Values.Look<byte>(ref value8, "filterMode", 0);
		Scribe_Values.Look<byte>(ref value9, "wrapModeU", 0);
		Scribe_Values.Look<byte>(ref value10, "wrapModeV", 0);
		Scribe_Values.Look(ref value11, "mipmapBias", 0f);
		Scribe_Values.Look(ref value12, "requestedMipmapLevel", 0);
		Scribe_Values.Look(ref value13, "ignoreMipmapLimit", defaultValue: false);
		Scribe_Values.Look(ref value14, "minimumMipmapLevel", 0);
		DataExposeUtility.LookByteArray(ref arr, "textureData");
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			Texture = new Texture2D(value, value2, (TextureFormat)value3, value4, value5);
			Texture.name = value6;
			Texture.anisoLevel = value7;
			Texture.filterMode = (FilterMode)value8;
			Texture.wrapModeU = (TextureWrapMode)value9;
			Texture.wrapModeV = (TextureWrapMode)value10;
			Texture.mipMapBias = value11;
			Texture.requestedMipmapLevel = value12;
			Texture.ignoreMipmapLimit = value13;
			Texture.minimumMipmapLevel = value14;
			Texture.LoadRawTextureData(arr);
			Texture.Apply();
		}
	}

	public static explicit operator SavedTexture2D(Texture2D tex)
	{
		return new SavedTexture2D(tex);
	}

	public static explicit operator Texture2D(SavedTexture2D tex)
	{
		return tex.Texture;
	}

	public override bool Equals(object obj)
	{
		if (obj is SavedTexture2D savedTexture2D)
		{
			return Texture.Equals(savedTexture2D.Texture);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Gen.HashCombine(74839, Texture);
	}

	public override string ToString()
	{
		return $"SavedTexture({Texture})";
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(Texture);
	}
}
