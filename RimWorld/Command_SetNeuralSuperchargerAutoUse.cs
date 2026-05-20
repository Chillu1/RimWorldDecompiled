using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Command_SetNeuralSuperchargerAutoUse : Command
	{
		private static Texture2D autoUseForEveryone;

		private static Texture2D autoUseWithDesire;

		private static Texture2D noAutoUseTex;

		private readonly CompNeuralSupercharger comp;

		private static Texture2D AutoUseForEveryone
		{
			get
			{
				if (autoUseForEveryone == null)
				{
					autoUseForEveryone = ContentFinder<Texture2D>.Get("UI/Gizmos/NeuralSupercharger_EveryoneAutoUse");
				}
				return autoUseForEveryone;
			}
		}

		private static Texture2D AutoUseWithDesire
		{
			get
			{
				if (autoUseWithDesire == null)
				{
					autoUseWithDesire = ContentFinder<Texture2D>.Get("UI/Gizmos/NeuralSupercharger_AutoUseWithDesire");
				}
				return autoUseWithDesire;
			}
		}

		private static Texture2D NoAutoUseTex
		{
			get
			{
				if (noAutoUseTex == null)
				{
					noAutoUseTex = ContentFinder<Texture2D>.Get("UI/Gizmos/NeuralSupercharger_NoAutoUse");
				}
				return noAutoUseTex;
			}
		}

		public Command_SetNeuralSuperchargerAutoUse(CompNeuralSupercharger comp)
		{
			this.comp = comp;
			switch (comp.autoUseMode)
			{
			case CompNeuralSupercharger.AutoUseMode.NoAutoUse:
				defaultLabel = "CommandNeuralSuperchargerNoAutoUse".Translate();
				defaultDesc = "CommandNeuralSuperchargerNoAutoUseDescription".Translate();
				icon = NoAutoUseTex;
				break;
			case CompNeuralSupercharger.AutoUseMode.AutoUseWithDesire:
				defaultLabel = "CommandNeuralSuperchargerAutoUseWithDesire".Translate();
				defaultDesc = "CommandNeuralSuperchargerAutoUseWithDesireDescription".Translate();
				icon = AutoUseWithDesire;
				break;
			case CompNeuralSupercharger.AutoUseMode.AutoUseForEveryone:
				defaultLabel = "CommandNeuralSuperchargerAutoForEveryone".Translate();
				defaultDesc = "CommandNeuralSuperchargerAutoForEveryoneDescription".Translate();
				icon = AutoUseForEveryone;
				break;
			default:
				Log.Error($"Unknown auto use mode: {comp.autoUseMode}");
				break;
			}
		}

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption("CommandNeuralSuperchargerNoAutoUse".Translate(), delegate
			{
				comp.autoUseMode = CompNeuralSupercharger.AutoUseMode.NoAutoUse;
			}, NoAutoUseTex, Color.white));
			list.Add(new FloatMenuOption("CommandNeuralSuperchargerAutoUseWithDesire".Translate(), delegate
			{
				comp.autoUseMode = CompNeuralSupercharger.AutoUseMode.AutoUseWithDesire;
			}, AutoUseWithDesire, Color.white));
			list.Add(new FloatMenuOption("CommandNeuralSuperchargerAutoForEveryone".Translate(), delegate
			{
				comp.autoUseMode = CompNeuralSupercharger.AutoUseMode.AutoUseForEveryone;
			}, AutoUseForEveryone, Color.white));
			Find.WindowStack.Add(new FloatMenu(list));
		}
	}
}
