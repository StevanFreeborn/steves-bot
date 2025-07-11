global using System.Collections.Concurrent;
global using System.Diagnostics;
global using System.Globalization;
global using System.Net;
global using System.Text;
global using System.Text.Json;

global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Http.HttpResults;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using Moq;

global using RichardSzalay.MockHttp;

global using StevesBot.Library.Discord.Rest;
global using StevesBot.Library.Discord.Rest.Requests;
global using StevesBot.Webhook.Telemetry;
global using StevesBot.Webhook.YouTube;
global using StevesBot.Webhook.YouTube.Data;
global using StevesBot.Webhook.YouTube.Handlers;
global using StevesBot.Webhook.YouTube.Tasks;