using Signalr.Hmg.Core;

namespace Signalr.Hmg.Clients.ConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string csProjPath = @"C:\Users\solzhabayev\source\repos\SignalR.Hmg\tests\e2es\Signalr.Hmg.Tests.E2es.DefaultSignalrWebservice\Signalr.Hmg.Tests.E2es.DefaultSignalrWebservice.csproj";

            var service = SignalrMetadataService.CreateMetadataGenerator(csProjPath)
                .ParseAll();

            var result = await service.GenerateMetadataAsync();
        }
    }
}