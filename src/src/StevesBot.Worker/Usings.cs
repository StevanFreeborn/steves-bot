global using System.Diagnostics;
global using System.Net.Http.Json;
global using System.Net.WebSockets;
global using System.Reflection;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;

global using Microsoft.Extensions.Options;

global using OpenTelemetry.Exporter;
global using OpenTelemetry.Logs;
global using OpenTelemetry.Resources;
global using OpenTelemetry.Trace;

global using StevesBot.Worker;
global using StevesBot.Worker.Discord;
global using StevesBot.Worker.Discord.Gateway;
global using StevesBot.Worker.Discord.Gateway.Events;
global using StevesBot.Worker.Discord.Gateway.Events.Data;
global using StevesBot.Worker.Discord.Rest;
global using StevesBot.Worker.Discord.Rest.Requests;
global using StevesBot.Worker.Discord.Rest.Responses;
global using StevesBot.Worker.Discord.Shared;
global using StevesBot.Worker.Handlers;
global using StevesBot.Worker.Telemetry;
global using StevesBot.Worker.Threading;
global using StevesBot.Worker.WebSockets;