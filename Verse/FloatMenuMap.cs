using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class FloatMenuMap : FloatMenu
	{
		private Vector3 clickPos;

		public const int RevalidateEveryFrame = 3;

		public FloatMenuMap(List<FloatMenuOption> options, string title, Vector3 clickPos)
			: base(options, title)
		{
			this.clickPos = clickPos;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
			if (pawn == null)
			{
				Find.WindowStack.TryRemove(this);
				return;
			}
			if (Time.frameCount % 3 == 0)
			{
				List<FloatMenuOption> list = FloatMenuMakerMap.ChoicesAtFor(clickPos, pawn);
				List<FloatMenuOption> cachedChoices = list;
				Vector3 cachedChoicesForPos = clickPos;
				for (int i = 0; i < options.Count; i++)
				{
					if (!options[i].Disabled && !StillValid(options[i], list, pawn, ref cachedChoices, ref cachedChoicesForPos))
					{
						options[i].Disabled = true;
					}
				}
			}
			base.DoWindowContents(inRect);
		}

		private static bool StillValid(FloatMenuOption opt, List<FloatMenuOption> curOpts, Pawn forPawn)
		{
			List<FloatMenuOption> cachedChoices = null;
			Vector3 cachedChoicesForPos = new Vector3(-9999f, -9999f, -9999f);
			return StillValid(opt, curOpts, forPawn, ref cachedChoices, ref cachedChoicesForPos);
		}

		private static bool StillValid(FloatMenuOption opt, List<FloatMenuOption> curOpts, Pawn forPawn, ref List<FloatMenuOption> cachedChoices, ref Vector3 cachedChoicesForPos)
		{
			if (opt.revalidateClickTarget == null)
			{
				for (int i = 0; i < curOpts.Count; i++)
				{
					if (OptionsMatch(opt, curOpts[i]))
					{
						return true;
					}
				}
			}
			else
			{
				if (!opt.revalidateClickTarget.Spawned)
				{
					return false;
				}
				Vector3 vector = opt.revalidateClickTarget.Position.ToVector3Shifted();
				List<FloatMenuOption> list;
				if (vector == cachedChoicesForPos)
				{
					list = cachedChoices;
				}
				else
				{
					cachedChoices = FloatMenuMakerMap.ChoicesAtFor(vector, forPawn);
					cachedChoicesForPos = vector;
					list = cachedChoices;
				}
				for (int j = 0; j < list.Count; j++)
				{
					if (OptionsMatch(opt, list[j]))
					{
						return !list[j].Disabled;
					}
				}
			}
			return false;
		}

		public override void PreOptionChosen(FloatMenuOption opt)
		{
			base.PreOptionChosen(opt);
			Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
			if (!opt.Disabled && (pawn == null || !StillValid(opt, FloatMenuMakerMap.ChoicesAtFor(clickPos, pawn), pawn)))
			{
				opt.Disabled = true;
			}
		}

		private static bool OptionsMatch(FloatMenuOption a, FloatMenuOption b)
		{
			if (a.Label == b.Label)
			{
				return true;
			}
			return false;
		}
	}
}
