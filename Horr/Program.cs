namespace Horr
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/User/Login";
                options.AccessDeniedPath = "/User/AccessDenied";
            });

            var app = builder.Build();
            
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}
