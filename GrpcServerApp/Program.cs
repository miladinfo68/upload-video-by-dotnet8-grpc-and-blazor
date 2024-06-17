using GrpcServerApp.Services;
using Shared;

var builder = WebApplication.CreateBuilder(args);

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