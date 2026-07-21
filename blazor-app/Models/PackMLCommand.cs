namespace BlazorApp.Models;

// Mirrors the momentary command bits in GVL_Command on the PLC.
public enum PackMLCommand
{
    Reset,
    Clear,
    Start,
    Stop,
    Hold
}
