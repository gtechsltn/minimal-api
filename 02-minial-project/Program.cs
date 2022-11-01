using System.Text.Json.Serialization;
using _00_Domain;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<MyDbContext>();
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/books", async (MyDbContext dbContext) =>
{
    var books = await dbContext.Books
            .Include(_ => _.Author).ToListAsync();
    return Results.Ok(books);
});

app.MapGet("/books/{id}", async (int id, MyDbContext dbContext) =>
{
    var book = await dbContext.Books
            .Include(_ => _.Author)
            .FirstOrDefaultAsync(_ => _.Id == id);

    if (book is null)
        return Results.NotFound();

    return Results.Ok(book);
});

app.MapPost("/books", async (Book createdBook, MyDbContext dbContext) =>
{
    var foundAuthor = await dbContext.Authors
            .FirstOrDefaultAsync(_ => _.Id == createdBook.Author.Id
                || _.LastName == createdBook.Author.LastName);

    if (foundAuthor is null)
    {
        var createdAuthor = new Author
        {
            FirstName = createdBook.Author.FirstName,
            LastName = createdBook.Author.LastName,
        };

        dbContext.Authors.Add(createdAuthor);
        await dbContext.SaveChangesAsync();

        foundAuthor = createdAuthor;
    }

    createdBook.Author = foundAuthor;
    dbContext.Books.Add(createdBook);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/books/{createdBook.Id}", createdBook);
});

app.MapDelete("/books/{id}", async (int id, MyDbContext dbContext) =>
{
    var book = await dbContext.Books.FindAsync(id);

    if (book is null)
        return Results.NotFound();

    return Results.Ok(book);
});

app.Run();
