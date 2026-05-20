using System;
using Unity.Jobs;

namespace Gilzoide.ManagedJobs;

public struct DisposeJob<TDisposable> : IJob where TDisposable : struct, IDisposable
{
	public TDisposable Disposable { get; }

	public DisposeJob(TDisposable disposable)
	{
		Disposable = disposable;
	}

	public void Execute()
	{
		Disposable.Dispose();
	}
}
