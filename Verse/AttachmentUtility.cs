namespace Verse
{
	public static class AttachmentUtility
	{
		public static Thing GetAttachment(this Thing t, ThingDef def)
		{
			return t.TryGetComp<CompAttachBase>()?.GetAttachment(def);
		}

		public static bool HasAttachment(this Thing t, ThingDef def)
		{
			return t.GetAttachment(def) != null;
		}
	}
}
