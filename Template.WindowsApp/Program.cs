using Microsoft.Extensions.Hosting;

using Template.WindowsApp;

//--------------------------------------------------------------------------------
// Configure builder
//--------------------------------------------------------------------------------

var builder = Host.CreateApplicationBuilder(args);

// System
builder.ConfigureSystemDefaults();

// Logging
builder.ConfigureLoggingDefaults();

// Components
builder.ConfigureComponents();

//--------------------------------------------------------------------------------
// Build host
//--------------------------------------------------------------------------------

var app = builder.Build();

// Startup information
app.LogStartupInformation();

// Run
app.RunApplication();
