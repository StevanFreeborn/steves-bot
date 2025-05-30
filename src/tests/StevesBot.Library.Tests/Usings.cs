global using System.Diagnostics;
global using System.Net;
global using System.Text;
global using System.Text.Json;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

global using Moq;

global using OpenTelemetry.Logs;
global using OpenTelemetry.Trace;

global using RichardSzalay.MockHttp;

global using StevesBot.Library.Discord;
global using StevesBot.Library.Discord.Common;
global using StevesBot.Library.Discord.Rest;
global using StevesBot.Library.Discord.Rest.Requests;
global using StevesBot.Library.Discord.Rest.Responses;
global using StevesBot.Library.Telemetry;
