using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThingOverlaysHandle
	{
		public readonly OverlayDrawer drawer;

		public readonly Thing thing;

		private bool disposed;

		private int handleIdCounter;

		private bool overlayTypesDirty;

		private OverlayTypes overlayTypes;

		private List<OverlayHandle> overlayHandles = new List<OverlayHandle>();

		public OverlayTypes OverlayTypes
		{
			get
			{
				if (overlayTypesDirty)
				{
					overlayTypes = OverlayTypes.None;
					for (int i = 0; i < overlayHandles.Count; i++)
					{
						overlayTypes |= overlayHandles[i].overlayType;
					}
					overlayTypesDirty = false;
				}
				return overlayTypes;
			}
		}

		public ThingOverlaysHandle(OverlayDrawer drawer, Thing thing)
		{
			this.drawer = drawer;
			this.thing = thing;
		}

		public OverlayHandle Enable(OverlayTypes overlayType)
		{
			OverlayHandle overlayHandle = new OverlayHandle(this, overlayType, handleIdCounter++);
			overlayHandles.Add(overlayHandle);
			overlayTypesDirty = true;
			return overlayHandle;
		}

		public void Disable(OverlayHandle handle)
		{
			for (int num = overlayHandles.Count - 1; num >= 0; num--)
			{
				if (overlayHandles[num].handleId == handle.handleId)
				{
					overlayHandles.RemoveAt(num);
				}
			}
			overlayTypesDirty = true;
		}

		public void Disable(ref OverlayHandle? handle)
		{
			if (handle.HasValue)
			{
				Disable(handle.Value);
				handle = null;
			}
		}

		public void Dispose()
		{
			if (disposed)
			{
				Log.Error("Tried disposing already disposed ThingOverlaysHandle!");
			}
			else
			{
				disposed = true;
			}
		}
	}
}
