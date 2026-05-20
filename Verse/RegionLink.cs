using System.Linq;

namespace Verse;

public class RegionLink
{
	public Region[] regions = new Region[2];

	public EdgeSpan span;

	public Region RegionA
	{
		get
		{
			return regions[0];
		}
		set
		{
			regions[0] = value;
		}
	}

	public Region RegionB
	{
		get
		{
			return regions[1];
		}
		set
		{
			regions[1] = value;
		}
	}

	public void Register(Region reg)
	{
		if (regions[0] == reg || regions[1] == reg)
		{
			Log.Error($"Tried to double-register region {reg} in {this}");
			return;
		}
		if (RegionA == null || !RegionA.valid)
		{
			RegionA = reg;
			return;
		}
		if (RegionB == null || !RegionB.valid)
		{
			RegionB = reg;
			return;
		}
		Log.Error($"Could not register region {reg} in link {this}: > 2 regions on link!\nRegionA: {RegionA.DebugString}\nRegionB: {RegionB.DebugString}");
	}

	public void Deregister(Region reg)
	{
		if (RegionA == reg)
		{
			RegionA = null;
			if (RegionB == null)
			{
				reg.Map?.regionLinkDatabase.Notify_LinkHasNoRegions(this);
			}
		}
		else if (RegionB == reg)
		{
			RegionB = null;
			if (RegionA == null)
			{
				reg.Map?.regionLinkDatabase.Notify_LinkHasNoRegions(this);
			}
		}
	}

	public Region GetOtherRegion(Region reg)
	{
		if (reg != RegionA)
		{
			return RegionA;
		}
		return RegionB;
	}

	public ulong UniqueHashCode()
	{
		return span.UniqueHashCode();
	}

	public override string ToString()
	{
		string text = (from r in regions
			where r != null
			select r.id.ToString()).ToCommaList();
		string text2 = "span=" + span.ToString() + " hash=" + UniqueHashCode();
		return "(" + text2 + ", regions=" + text + ")";
	}
}
