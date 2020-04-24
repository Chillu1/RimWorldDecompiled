using UnityEngine;

namespace Verse
{
	public class CompColorable : ThingComp
	{
		private Color color = Color.white;

		private bool active;

		public Color Color
		{
			get
			{
				if (!active)
				{
					return parent.def.graphicData.color;
				}
				return color;
			}
			set
			{
				if (!(value == color))
				{
					active = true;
					color = value;
					parent.Notify_ColorChanged();
				}
			}
		}

		public bool Active => active;

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			if (parent.def.colorGenerator != null && (parent.Stuff == null || parent.Stuff.stuffProps.allowColorGenerators))
			{
				Color = parent.def.colorGenerator.NewRandomizedColor();
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			if (Scribe.mode != LoadSaveMode.Saving || active)
			{
				Scribe_Values.Look(ref color, "color");
				Scribe_Values.Look(ref active, "colorActive", defaultValue: false);
			}
		}

		public override void PostSplitOff(Thing piece)
		{
			base.PostSplitOff(piece);
			if (active)
			{
				piece.SetColor(color);
			}
		}
	}
}
