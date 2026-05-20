namespace RimWorld
{
	public struct OverlayHandle
	{
		public readonly ThingOverlaysHandle thingOverlayHandle;

		public readonly OverlayTypes overlayType;

		public int handleId;

		public OverlayHandle(ThingOverlaysHandle thingOverlayHandle, OverlayTypes overlayType, int handleId)
		{
			this.thingOverlayHandle = thingOverlayHandle;
			this.overlayType = overlayType;
			this.handleId = handleId;
		}
	}
}
