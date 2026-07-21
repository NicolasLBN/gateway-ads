namespace BlazorApp.Models;

// Mirrors the numeric state codes exposed by GVL_State.nState in the PLC (MAIN.TcPOU).
// This subset of PackML intentionally omits Aborting/Aborted/Held/Holding/Suspending/Suspended.
public enum PackMLState
{
    Clearing = 0,
    Stopped = 1,
    Resetting = 2,
    Idle = 3,
    Starting = 4,
    Execute = 5,
    Completing = 6,
    Complete = 7
}
