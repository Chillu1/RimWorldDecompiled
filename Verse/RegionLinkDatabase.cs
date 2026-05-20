using System.Collections.Generic;
using System.Text;

namespace Verse;

public class RegionLinkDatabase
{
	private Dictionary<ulong, RegionLink> links = new Dictionary<ulong, RegionLink>();

	public RegionLink LinkFrom(EdgeSpan span)
	{
		ulong key = span.UniqueHashCode();
		if (!links.TryGetValue(key, out var value))
		{
			value = new RegionLink();
			value.span = span;
			links.Add(key, value);
		}
		return value;
	}

	public void Notify_LinkHasNoRegions(RegionLink link)
	{
		links.Remove(link.UniqueHashCode());
	}

	public void DebugLog()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<ulong, RegionLink> link in links)
		{
			stringBuilder.AppendLine(link.ToString());
		}
		Log.Message(stringBuilder.ToString());
	}
}
