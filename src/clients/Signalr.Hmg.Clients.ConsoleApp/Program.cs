using Signalr.Hmg.Core;

namespace Signalr.Hmg.Clients.ConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var currentExecutionPath = AppDomain.CurrentDomain.BaseDirectory;

            var path = @"..\..\..\..\..\..\tests\e2es\Signalr.Hmg.Tests.E2es.DefaultSignalrWebservice\Signalr.Hmg.Tests.E2es.DefaultSignalrWebservice.csproj";

            var csprojPath = Path.GetFullPath(Path.Combine(currentExecutionPath, path));

            var service = SignalrMetadataService.CreateMetadataGenerator(csprojPath)
                .ParseAll();

            var result = await service.GenerateMetadataAsync();
        }
    }
}