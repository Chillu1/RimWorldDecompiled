using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class TransferableImmutable : Transferable
	{
		public List<Thing> things = new List<Thing>();

		public override Thing AnyThing
		{
			get
			{
				if (!HasAnyThing)
				{
					return null;
				}
				return things[0];
			}
		}

		public override ThingDef ThingDef
		{
			get
			{
				if (!HasAnyThing)
				{
					return null;
				}
				return AnyThing.def;
			}
		}

		public override bool HasAnyThing => things.Count != 0;

		public override string Label => AnyThing.LabelNoCount;

		public override bool Interactive => false;

		public override TransferablePositiveCountDirection PositiveCountDirection => TransferablePositiveCountDirection.Destination;

		public override string TipDescription
		{
			get
			{
				if (!HasAnyThing)
				{
					return "";
				}
				return AnyThing.DescriptionDetailed;
			}
		}

		public override int CountToTransfer
		{
			get
			{
				return 0;
			}
			protected set
			{
				if (value != 0)
				{
					throw new InvalidOperationException("immutable transferable");
				}
			}
		}

		public string LabelWithTotalStackCount
		{
			get
			{
				string text = Label;
				int totalStackCount = TotalStackCount;
				if (totalStackCount != 1)
				{
					text = text + " x" + totalStackCount.ToStringCached();
				}
				return text;
			}
		}

		public string LabelCapWithTotalStackCount => LabelWithTotalStackCount.CapitalizeFirst(ThingDef);

		public int TotalStackCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < things.Count; i++)
				{
					num += things[i].stackCount;
				}
				return num;
			}
		}

		public override int GetMinimumToTransfer()
		{
			return 0;
		}

		public override int GetMaximumToTransfer()
		{
			return 0;
		}

		public override AcceptanceReport OverflowReport()
		{
			return false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				things.RemoveAll((Thing x) => x.Destroyed);
			}
			Scribe_Collections.Look(ref things, "things", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && things.RemoveAll((Thing x) => x == null) != 0)
			{
				Log.Warning("Some of the things were null after loading.");
			}
		}
	}
}
