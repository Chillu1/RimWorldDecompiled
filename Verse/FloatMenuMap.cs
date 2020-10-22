using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public class FloatMenuMap : FloatMenu
	{
		private Vector3 clickPos;

		private static Dictionary<Vector3, List<FloatMenuOption>> cachedChoices = new Dictionary<Vector3, List<FloatMenuOption>>();

		private List<FloatMenuOption> lastOptionsForRevalidation;

		private int nextOptionToRevalidate;

		public const int RevalidateEveryFrame = 4;

		public FloatMenuMap(List<FloatMenuOption> options, string title, Vector3 clickPos)
			: base(options, title)
		{
			this.clickPos = clickPos;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Pawn selPawn = Find.Selector.SingleSelectedThing as Pawn;
			if (selPawn == null)
			{
				Find.WindowStack.TryRemove(this);
				return;
			}
			bool flag = options.Count >= 3;
			if (Time.frameCount % 4 == 0 || lastOptionsForRevalidation == null)
			{
				lastOptionsForRevalidation = FloatMenuMakerMap.ChoicesAtFor(clickPos, selPawn);
				cachedChoices.Clear();
				cachedChoices.Add(clickPos, lastOptionsForRevalidation);
				if (!flag)
				{
					for (int i = 0; i < options.Count; i++)
					{
						RevalidateOption(options[i]);
					}
				}
			}
			else if (flag)
			{
				if (nextOptionToRevalidate >= options.Count)
				{
					nextOptionToRevalidate = 0;
				}
				int num = Mathf.CeilToInt((float)options.Count / 3f);
				int num2 = nextOptionToRevalidate;
				int num3 = 0;
				while (num2 < options.Count && num3 < num)
				{
					RevalidateOption(options[num2]);
					nextOptionToRevalidate++;
					num2++;
					num3++;
				}
			}
			base.DoWindowContents(inRect);
			void RevalidateOption(FloatMenuOption option)
			{
				if (!option.Disabled && !StillValid(option, lastOptionsForRevalidation, selPawn))
				{
					option.Disabled = true;
				}
			}
		}

		[Obsolete("Only need this overload to not break mod compatibility.")]
		private static bool StillValid(FloatMenuOption opt, List<FloatMenuOption> curOpts, Pawn forPawn, ref List<FloatMenuOption> cachedChoices, ref Vector3 cachedChoicesForPos)
		{
			return StillValid(opt, curOpts, forPawn);
		}

		private static bool StillValid(FloatMenuOption opt, List<FloatMenuOption> curOpts, Pawn forPawn)
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
				Vector3 key = opt.revalidateClickTarget.Position.ToVector3Shifted();
				if (!cachedChoices.TryGetValue(key, out var value))
				{
					List<FloatMenuOption> list = FloatMenuMakerMap.ChoicesAtFor(key, forPawn);
					cachedChoices.Add(key, list);
					value = list;
				}
				for (int j = 0; j < value.Count; j++)
				{
					if (OptionsMatch(opt, value[j]))
					{
						return !value[j].Disabled;
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
