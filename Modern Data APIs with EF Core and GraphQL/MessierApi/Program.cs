using DataAccess;
using MessierApi;
using Microsoft.EntityFrameworkCore;

string rootPath = Directory.GetCurrentDirectory() + "/assets";

var builder = WebApplication.CreateBuilder(args);

var dbPath = System.IO.Path.Combine(rootPath, "messier.db");

builder.Services.AddDbContextFactory<MessierContext>(opt => opt.UseSqlite(
    $"Data Source={dbPath}").LogTo(
    Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }));


builder.Services.AddScoped<MessierContext>(sp =>
    sp.GetRequiredService<IDbContextFactory<MessierContext>>()
    .CreateDbContext());

builder.Services.AddGraphQLServer().AddQueryType<Query>().AddProjections().AddFiltering().AddSorting();

using var htmlStream = typeof(Program).Assembly.GetManifestResourceStream("MessierApi.index.html");
using var htmlReader = new StreamReader(htmlStream);
var html = htmlReader.ReadToEnd();

var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints => endpoints.MapGraphQL("/graphql"));

app.MapGet("/", async (HttpResponse resp) =>
{
    resp.ContentType = "text/html";
    await resp.WriteAsync(html.Replace("{port}", resp.HttpContext.Request.Host.Port.ToString()));
});

app.MapGet("/messier", async (IDbContextFactory<MessierContext> factory) =>
{
    using var ctx = factory.CreateDbContext();
    var result = await ctx.Targets.OrderBy(t => t.Index).ToListAsync();
    return result;
});

app.MapGet("/messier/{id}", async (IDbContextFactory<MessierContext> factory, Guid id) =>
{
    using var ctx = factory.CreateDbContext();
    return await ctx.Targets.FirstOrDefaultAsync(tgt => tgt.Id == id);
});

app.Run();