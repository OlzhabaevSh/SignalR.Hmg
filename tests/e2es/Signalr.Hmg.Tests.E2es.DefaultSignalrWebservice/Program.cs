namespace Signalr.Hmg.Tests.E2es.DefaultSignalrWebservice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorPages();
            builder.Services.AddSignalR();

            var app = builder.Build();

            if(!app.Environment.IsDevelopment()) 
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.MapRazorPages();
            app.MapHub<Hubs.ChatHub>("/chathub");
            app.MapHub<Hubs.UserNotificationHub>("/userNotificationHub");

            app.Run();
        }
    }
}