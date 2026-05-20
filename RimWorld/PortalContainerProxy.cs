using Verse;

namespace RimWorld;

public class PortalContainerProxy : ThingOwner
{
	public MapPortal portal;

	public override int Count => 0;

	public override int TryAdd(Thing item, int count, bool canMergeWithExistingStacks = true)
	{
		if (TryAdd(item, canMergeWithExistingStacks))
		{
			return count;
		}
		return 0;
	}

	public override bool TryAdd(Thing item, bool canMergeWithExistingStacks = true)
	{
		Map otherMap = portal.GetOtherMap();
		IntVec3 destinationLocation = portal.GetDestinationLocation();
		portal.Notify_ThingAdded(item);
		GenDrop.TryDropSpawn(item, destinationLocation, otherMap, ThingPlaceMode.Near, out var _);
		return true;
	}

	public override int IndexOf(Thing item)
	{
		return -1;
	}

	public override bool Remove(Thing item)
	{
		return false;
	}

	protected override Thing GetAt(int index)
	{
		return null;
	}
}
