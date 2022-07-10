using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using VaultChanger.Repositories;
using VaultChanger.Data;
using VaultChanger.Extensions;
using VaultChanger.Models.Configuration;

namespace VaultChanger
{
    public class Startup
    {
        private readonly AppSettingsConfig _appsettingsConfig = new();
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            configuration.Bind(_appsettingsConfig);
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<VaultContext>(opt => opt.UseSqlServer
                (Configuration.GetConnectionString("SqlConnection")));

            services.Configure<VaultConfig>(Configuration.GetSection(VaultConfig.Vault));
            
            services.AddVaultClient(_appsettingsConfig.Vault);
            
            services.AddScoped<IVaultRepository, VaultRepository>();
            services.AddScoped<IRequestsRepository, RequestsRepository>();
            
            services.AddControllers().AddNewtonsoftJson();
            services.AddSwaggerGen(c  =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "VaultChanger", Version = "v1"});
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VaultChanger v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}