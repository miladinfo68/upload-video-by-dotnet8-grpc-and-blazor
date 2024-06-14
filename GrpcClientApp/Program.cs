using Grpc.Net.Client;
using GrpcClientApp.Components;
using GrpcClientApp.Services;
using GrpcClientApp.Wasm.Pages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Shared;
using Shared.Protos;

var builder = WebApplication.CreateBuilder(args);


//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.Limits.MaxRequestBodySize = AppConstants.CHUNK_SIZE;
//});

//builder.Services.Configure<FormOptions>(options =>
//{
//    options.ValueLengthLimit = int.MaxValue;
//    options.MultipartBodyLengthLimit = int.MaxValue; // This is the limit for file uploads.
//});

builder.Services.AddSingleton(services =>
{
    return GrpcChannel.ForAddress(new Uri(AppConstants.GRPC_SERVER_URL), new GrpcChannelOptions()
    {
        MaxSendMessageSize = int.MaxValue,
        MaxReceiveMessageSize = int.MaxValue
    });
});

builder.Services.AddScoped<CientFileUploadService>();

builder.Services.AddScoped(sp =>
{
    var channel = sp.GetRequiredService<GrpcChannel>();
    return new UploaderService.UploaderServiceClient(channel);
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