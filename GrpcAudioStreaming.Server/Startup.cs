using GrpcAudioStreaming.Proto.Codecs;
using GrpcAudioStreaming.Server.Services;
using GrpcAudioStreaming.Server.Settings;
using GrpcAudioStreaming.Server.Sources;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace GrpcAudioStreaming.Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            services.Configure<AudioSettings>(configuration.GetSection(AudioSettings.RootPath));
             
            services.AddGrpc();
            services.AddSingleton(CodecFactory.GetOrDefault(configuration.GetValue<string>($"{AudioSettings.RootPath}:Codec")));
            services.AddSingleton<LoopbackAudioStreamerService>();
            services.AddTransient<IAudioSampleSource, LoopbackAudioSampleSource>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GrpcAudioStreamService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}