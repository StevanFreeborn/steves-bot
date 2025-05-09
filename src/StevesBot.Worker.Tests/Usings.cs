global using System.Net.WebSockets;
global using System.Text;
global using System.Text.Json;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Hosting.Server.Features;
global using Microsoft.AspNetCore.Http.Features;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;

global using StevesBot.Worker.Discord;
global using StevesBot.Worker.Discord.Events;
global using StevesBot.Worker.Tests.Integration.Infrastructure;
global using StevesBot.Worker.Threading;
global using StevesBot.Worker.WebSockets;