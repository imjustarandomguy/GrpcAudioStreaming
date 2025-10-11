using GrpcAudioStreaming.Proto.Codecs;
using GrpcAudioStreaming.Server.Services;
using GrpcAudioStreaming.Server.Services.Recorders;
using GrpcAudioStreaming.Server.Settings;
using GrpcAudioStreaming.Server.Streamers.Grpc;
using GrpcAudioStreaming.Server.Streamers.Tcp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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

            services.AddCors(o => o.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
            }));

            var codec = CodecFactory.GetOrDefault(configuration.GetValue<string>($"{AudioSettings.RootPath}:Codec"));

            services.AddGrpc();
            services.AddSingleton(codec);

            services.AddSingleton(provider =>
            {
                var audioSettings = provider.GetRequiredService<IOptions<AudioSettings>>();
                return RecorderEngineFactory.GetOrDefault(configuration.GetValue<string>("Engine"), codec, audioSettings);
            });

            services.AddTransient<IGrpcStreamer, LoopbackGrpcStreamer>();
            //services.AddHostedService<TcpStreamer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseGrpcWeb();
            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GrpcAudioStreamService>().EnableGrpcWeb().RequireCors("AllowAll");
                endpoints.MapGet("/", () => "gRPC server running...");
            });
        }
    }
}