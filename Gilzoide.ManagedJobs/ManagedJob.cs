using System;
using System.Runtime.InteropServices;
using Unity.Jobs;

namespace Gilzoide.ManagedJobs;

public struct ManagedJob : IJob, IDisposable
{
	private GCHandle _managedJobGcHandle;

	public IJob Job
	{
		get
		{
			if (!_managedJobGcHandle.IsAllocated)
			{
				return null;
			}
			return (IJob)_managedJobGcHandle.Target;
		}
	}

	public bool HasJob => Job != null;

	public ManagedJob(IJob managedJob)
	{
		_managedJobGcHandle = ((managedJob != null) ? GCHandle.Alloc(managedJob) : default(GCHandle));
	}

	public void Execute()
	{
		Job?.Execute();
	}

	public void Dispose()
	{
		if (_managedJobGcHandle.IsAllocated)
		{
			_managedJobGcHandle.Free();
		}
	}

	public JobHandle Schedule(JobHandle dependsOn = default(JobHandle))
	{
		JobHandle jobHandle = IJobExtensions.Schedule(this, dependsOn);
		new DisposeJob<ManagedJob>(this).Schedule(jobHandle);
		return jobHandle;
	}

	public void Run()
	{
		try
		{
			IJobExtensions.Run(this);
		}
		finally
		{
			Dispose();
		}
	}
}
