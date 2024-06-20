using Grpc.Net.Client;
using GrpcClientApp.Components;
using GrpcClientApp.Services;
using GrpcClientApp.Wasm.Pages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Shared;
using Shared.Protos;

var builder = WebApplication.CreateBuilder(args);


//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.Limits.MaxRequestBodySize = AppConstants.CHUNK_SIZE;
//});

builder.Services.Configure<KestrelServerOptions>(options =>
{

    options.Limits.MaxRequestBodySize = AppConstants.CHUNK_SIZE_2GB;
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = AppConstants.CHUNK_SIZE_2GB;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = AppConstants.CHUNK_SIZE_2GB;
});

builder.Services.AddSingleton(services =>
{
    return GrpcChannel.ForAddress(new Uri(AppConstants.GRPC_SERVER_URL), new GrpcChannelOptions()
    {
        MaxSendMessageSize = AppConstants.CHUNK_SIZE_2GB,
        MaxReceiveMessageSize = AppConstants.CHUNK_SIZE_2GB
    });
});

builder.Services.AddScoped<ClientVideoService>();

builder.Services.AddScoped(sp =>
{
    var channel = sp.GetRequiredService<GrpcChannel>();
    return new VideoService.VideoServiceClient(channel);
});




builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();


app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CSR).Assembly);

app.Run();