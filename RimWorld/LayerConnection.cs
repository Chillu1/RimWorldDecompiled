using System;
using System.Xml;
using Verse;

namespace RimWorld;

public class LayerConnection : IEquatable<LayerConnection>, IExposable
{
	public enum ZoomMode : byte
	{
		None,
		ZoomIn,
		ZoomOut
	}

	public string tag;

	public float fuelCost;

	public ZoomMode zoomMode;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		XmlHelper.ParseElements(this, xmlRoot, "tag", "fuelCost");
	}

	public bool Equals(LayerConnection other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (tag == other.tag && fuelCost.Equals(other.fuelCost))
		{
			return zoomMode == other.zoomMode;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((LayerConnection)obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(tag, fuelCost, zoomMode);
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref tag, "tag");
		Scribe_Values.Look(ref fuelCost, "fuelCost", 0f);
		Scribe_Values.Look(ref zoomMode, "zoomMode", ZoomMode.None);
	}
}
