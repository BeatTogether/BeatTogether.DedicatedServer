
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

using Serilog;

namespace BeatTogether.DedicatedServer.Ignorance.Util;

public static class IgnoranceDebug
{
    public static ILogger? Logger = null;
    
    public static void Log(string message) =>
        Logger?.Verbose(message);
    
    public static void LogWarning(string message) =>
        Logger?.Warning(message);
    
    public static void LogError(string message) =>
        Logger?.Error(message);
}