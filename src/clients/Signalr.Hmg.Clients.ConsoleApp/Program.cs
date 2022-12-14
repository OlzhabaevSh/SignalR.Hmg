using Signalr.Hmg.Core;

namespace Signalr.Hmg.Clients.ConsoleApp
{
    internal class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns>
        /// 1: csproj path
        /// 2. metadata export path
        /// </returns>
        static async Task Main(string[] args)
        {
            var csprojPath = args.Length > 0 ? args[0] : GetTestSignalrCsprojPath();
            var exportPath = args.Length > 1 ? args[1] : GetExportPath();

            var service = SignalrMetadataService
                .CreateMetadataGenerator(csprojPath)
                .ParseAll();

            var result = await service.GenerateMetadataAsync();

            await SaveExporto(result, exportPath);            
        }

        private static string GetTestSignalrCsprojPath() 
        {
            var currentExecutionPath = AppDomain.CurrentDomain.BaseDirectory;

            var path = @"..\..\..\..\..\..\tests\e2es\Signalr.Hmg.Tests.E2es.DefaultSignalrWebservice\Signalr.Hmg.Tests.E2es.DefaultSignalrWebservice.csproj";

            var csprojPath = Path.GetFullPath(Path.Combine(currentExecutionPath, path));

            return csprojPath;
        }

        private static string GetExportPath() 
        {
            return @"./signalr-export.json";
        }

        private static async Task SaveExporto(object data, string exportPath) 
        {
            if (File.Exists(exportPath))
            {
                File.Delete(exportPath);
            }

            using (var fileStream = File.Create(exportPath))
            {
                await System.Text.Json.JsonSerializer.SerializeAsync(fileStream, data);
            }
        }
    }
}