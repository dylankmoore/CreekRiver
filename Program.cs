using CreekRiver.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core
builder.Services.AddNpgsql<CreekRiverDbContext>(builder.Configuration["CreekRiverDbConnectionString"]);

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//GET CAMPSITES
app.MapGet("/api/campsites", (CreekRiverDbContext db) =>
{
    return db.Campsites.ToList();
});

//GET CAMPSITES BY ID
app.MapGet("/api/campsites/{id}", (CreekRiverDbContext db, int id) =>
{
    //the Single method throws an exception - by using SingleOrDefault we can access the if statement to return the error
    var campsite = db.Campsites.Include(c => c.CampsiteType).SingleOrDefault(c => c.Id == id);

    if (campsite == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(campsite);
});

//CREATE CAMPSITES
app.MapPost("/api/campsites", (CreekRiverDbContext db, Campsite campsite) =>
{
    db.Campsites.Add(campsite);
    db.SaveChanges();
    return Results.Created($"/api/campsites/{campsite.Id}", campsite);
});

//DELETE CAMPSITES
app.MapDelete("/api/campsites/{id}", (CreekRiverDbContext db, int id) =>
{
    Campsite campsite = db.Campsites.SingleOrDefault(campsite => campsite.Id == id);
    if (campsite == null)
    {
        return Results.NotFound();
    }
    db.Campsites.Remove(campsite);
    db.SaveChanges();
    return Results.Ok("Campsite deleted successfully!");

});

app.Run();

