using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ArchivedDialog : IArchivable, IExposable, ILoadReferenceable
	{
		public int ID;

		public string text;

		public string title;

		public Faction relatedFaction;

		public int createdTick;

		Texture IArchivable.ArchivedIcon => null;

		Color IArchivable.ArchivedIconColor => Color.white;

		string IArchivable.ArchivedLabel => text.Flatten();

		string IArchivable.ArchivedTooltip => text;

		int IArchivable.CreatedTicksGame => createdTick;

		bool IArchivable.CanCullArchivedNow => true;

		LookTargets IArchivable.LookTargets => null;

		public ArchivedDialog()
		{
		}

		public ArchivedDialog(string text, string title = null, Faction relatedFaction = null)
		{
			this.text = text;
			this.title = title;
			this.relatedFaction = relatedFaction;
			createdTick = GenTicks.TicksGame;
			if (Find.UniqueIDsManager != null)
			{
				ID = Find.UniqueIDsManager.GetNextArchivedDialogID();
			}
			else
			{
				ID = Rand.Int;
			}
		}

		void IArchivable.OpenArchived()
		{
			DiaNode diaNode = new DiaNode(text);
			DiaOption diaOption = new DiaOption("OK".Translate());
			diaOption.resolveTree = true;
			diaNode.options.Add(diaOption);
			Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(diaNode, relatedFaction, delayInteractivity: false, radioMode: false, title));
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref ID, "ID", 0);
			Scribe_Values.Look(ref text, "text");
			Scribe_Values.Look(ref title, "title");
			Scribe_References.Look(ref relatedFaction, "relatedFaction");
			Scribe_Values.Look(ref createdTick, "createdTick", 0);
		}

		public string GetUniqueLoadID()
		{
			return "ArchivedDialog_" + ID;
		}
	}
}
