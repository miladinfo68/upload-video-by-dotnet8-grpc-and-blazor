using GrpcServerApp.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Shared;

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

builder.Services.AddGrpc(options =>
{
    // Adjust the maximum message size for gRPC
    options.MaxReceiveMessageSize = AppConstants.CHUNK_SIZE_2MB;

    options.EnableDetailedErrors = true;
    options.MaxReceiveMessageSize = int.MaxValue;
    options.MaxSendMessageSize = int.MaxValue;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<VideoServiceImpl>();

app.MapGet("/hc", () => "healthy");

app.Run();