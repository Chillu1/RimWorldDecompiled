using UnityEngine;
using Verse;

namespace RimWorld
{
	public interface IArchivable : IExposable, ILoadReferenceable
	{
		Texture ArchivedIcon
		{
			get;
		}

		Color ArchivedIconColor
		{
			get;
		}

		string ArchivedLabel
		{
			get;
		}

		string ArchivedTooltip
		{
			get;
		}

		int CreatedTicksGame
		{
			get;
		}

		bool CanCullArchivedNow
		{
			get;
		}

		LookTargets LookTargets
		{
			get;
		}

		void OpenArchived();
	}
}
