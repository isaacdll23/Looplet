namespace Looplet.Hub.Features.Scheduler;

public class SchedulerState
{
    private volatile bool _enabled = true;
    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }
}
