using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class BodyPartRecord
	{
		public BodyDef body;

		[TranslationHandle]
		public BodyPartDef def;

		[MustTranslate]
		public string customLabel;

		[Unsaved(false)]
		[TranslationHandle(Priority = 100)]
		public string untranslatedCustomLabel;

		public List<BodyPartRecord> parts = new List<BodyPartRecord>();

		public BodyPartHeight height;

		public BodyPartDepth depth;

		public float coverage = 1f;

		public List<BodyPartGroupDef> groups = new List<BodyPartGroupDef>();

		[Unsaved(false)]
		public BodyPartRecord parent;

		[Unsaved(false)]
		public float coverageAbsWithChildren;

		[Unsaved(false)]
		public float coverageAbs;

		[Unsaved(false)]
		private string cachedCustomLabelCap;

		public bool IsCorePart => parent == null;

		public string Label
		{
			get
			{
				if (!customLabel.NullOrEmpty())
				{
					return customLabel;
				}
				return def.label;
			}
		}

		public string LabelCap
		{
			get
			{
				if (customLabel.NullOrEmpty())
				{
					return def.LabelCap;
				}
				if (cachedCustomLabelCap == null)
				{
					cachedCustomLabelCap = customLabel.CapitalizeFirst();
				}
				return cachedCustomLabelCap;
			}
		}

		public string LabelShort => def.LabelShort;

		public string LabelShortCap => def.LabelShortCap;

		public int Index => body.GetIndexOfPart(this);

		public override string ToString()
		{
			return "BodyPartRecord(" + ((def != null) ? def.defName : "NULL_DEF") + " parts.Count=" + parts.Count + ")";
		}

		public void PostLoad()
		{
			untranslatedCustomLabel = customLabel;
		}

		public bool IsInGroup(BodyPartGroupDef group)
		{
			for (int i = 0; i < groups.Count; i++)
			{
				if (groups[i] == group)
				{
					return true;
				}
			}
			return false;
		}

		public IEnumerable<BodyPartRecord> GetChildParts(BodyPartTagDef tag)
		{
			if (def.tags.Contains(tag))
			{
				yield return this;
			}
			int i = 0;
			while (i < parts.Count)
			{
				foreach (BodyPartRecord childPart in parts[i].GetChildParts(tag))
				{
					yield return childPart;
				}
				int num = i + 1;
				i = num;
			}
		}

		public IEnumerable<BodyPartRecord> GetDirectChildParts()
		{
			int i = 0;
			while (i < parts.Count)
			{
				yield return parts[i];
				int num = i + 1;
				i = num;
			}
		}

		public bool HasChildParts(BodyPartTagDef tag)
		{
			return GetChildParts(tag).Any();
		}

		public IEnumerable<BodyPartRecord> GetConnectedParts(BodyPartTagDef tag)
		{
			BodyPartRecord bodyPartRecord = this;
			while (bodyPartRecord.parent != null && bodyPartRecord.parent.def.tags.Contains(tag))
			{
				bodyPartRecord = bodyPartRecord.parent;
			}
			foreach (BodyPartRecord childPart in bodyPartRecord.GetChildParts(tag))
			{
				yield return childPart;
			}
		}
	}
}
