using System.Runtime.CompilerServices;

namespace Competition.Tests.Infrastructure;

internal static class TestEnvironment
{
    /// <summary>
    /// Integration hosts run as Development, where demo data is on. Tests expect an empty database,
    /// so switch it off for every WebApplicationFactory in this assembly (env vars override appsettings).
    /// </summary>
    [ModuleInitializer]
    internal static void DisableDemoData() => Environment.SetEnvironmentVariable("Seed__DemoData", "false");
}
