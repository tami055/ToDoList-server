using Microsoft.EntityFrameworkCore;
using TODOAPI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(option =>
 option.AddPolicy("CorsPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Todo API", Description = "Keep track of your tasks", Version = "v1" });
});

builder.Services.AddDbContext<ToDoDbContext>();
var app = builder.Build();

app.UseCors("CorsPolicy");
if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSwagger(options =>
{
    options.SerializeAsV2 = true;
});

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

// app.MapGet("/", () => "Hello World!");

app.MapGet("/todoitems", async (ToDoDbContext db) =>
    await db.Items.ToListAsync());


app.MapPost("/todoitems", async ([FromBody]Item todo, ToDoDbContext db) =>
{
    db.Items.Add(todo);
    await db.SaveChangesAsync();

    return todo;//Results.Created($"/todoitems/{todo.Id}", todo);
});

app.MapPut("/todoitems/{id}", async (int id,[FromBody] Item inputTodo, ToDoDbContext db) =>
{
    var todo = await db.Items.FindAsync(id);
    if (todo is null) return Results.NotFound();
    todo.IsComplete = inputTodo.IsComplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/todoitems/{id}", async (int id, ToDoDbContext db) =>
{
 var existItem=await db.Items.FindAsync(id);
 if(existItem is null) return Results.NotFound();
    db.Items.Remove(existItem);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
