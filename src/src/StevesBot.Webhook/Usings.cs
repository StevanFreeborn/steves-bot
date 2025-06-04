global using System.Collections.Concurrent;
global using System.ComponentModel.DataAnnotations;
global using System.Globalization;
global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;

global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.WebUtilities;
global using Microsoft.Extensions.Options;

global using StevesBot.Library.Discord;
global using StevesBot.Library.Discord.Common;
global using StevesBot.Library.Discord.Rest;
global using StevesBot.Library.Discord.Rest.Requests;
global using StevesBot.Library.Telemetry;
global using StevesBot.Webhook.Telemetry;
global using StevesBot.Webhook.YouTube;
global using StevesBot.Webhook.YouTube.Data;
global using StevesBot.Webhook.YouTube.Handlers;
global using StevesBot.Webhook.YouTube.Tasks;