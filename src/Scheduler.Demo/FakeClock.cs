using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NodaTime;

namespace Scheduler.Demo;
public class FakeClock : IClock
{
    private readonly Instant _now;

    public FakeClock(Instant now)
    {
        _now = now;
    }

    public Instant GetCurrentInstant() => _now;
}
