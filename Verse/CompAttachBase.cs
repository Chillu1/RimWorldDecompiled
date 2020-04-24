using System.Collections.Generic;
using System.Text;

namespace Verse
{
	public class CompAttachBase : ThingComp
	{
		public List<AttachableThing> attachments;

		public override void CompTick()
		{
			if (attachments != null)
			{
				for (int i = 0; i < attachments.Count; i++)
				{
					attachments[i].Position = parent.Position;
				}
			}
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			base.PostDestroy(mode, previousMap);
			if (attachments != null)
			{
				for (int num = attachments.Count - 1; num >= 0; num--)
				{
					attachments[num].Destroy();
				}
			}
		}

		public override string CompInspectStringExtra()
		{
			if (attachments != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < attachments.Count; i++)
				{
					stringBuilder.AppendLine(attachments[i].InspectStringAddon);
				}
				return stringBuilder.ToString().TrimEndNewlines();
			}
			return null;
		}

		public Thing GetAttachment(ThingDef def)
		{
			if (attachments != null)
			{
				for (int i = 0; i < attachments.Count; i++)
				{
					if (attachments[i].def == def)
					{
						return attachments[i];
					}
				}
			}
			return null;
		}

		public bool HasAttachment(ThingDef def)
		{
			return GetAttachment(def) != null;
		}

		public void AddAttachment(AttachableThing t)
		{
			if (attachments == null)
			{
				attachments = new List<AttachableThing>();
			}
			attachments.Add(t);
		}

		public void RemoveAttachment(AttachableThing t)
		{
			attachments.Remove(t);
		}
	}
}
