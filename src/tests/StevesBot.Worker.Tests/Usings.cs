global using System.Diagnostics;
global using System.Net.WebSockets;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Hosting.Server.Features;
global using Microsoft.AspNetCore.Http.Features;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;

global using Moq;

global using StevesBot.Library.Discord.Common;
global using StevesBot.Library.Discord.Rest;
global using StevesBot.Library.Discord.Rest.Requests;
global using StevesBot.Worker.Discord;
global using StevesBot.Worker.Discord.Gateway;
global using StevesBot.Worker.Discord.Gateway.Events;
global using StevesBot.Worker.Discord.Gateway.Events.Data;
global using StevesBot.Worker.Telemetry;
global using StevesBot.Worker.Tests.Integration.Infrastructure;
global using StevesBot.Worker.Threading;
global using StevesBot.Worker.WebSockets;