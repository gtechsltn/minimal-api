using System.Text.Json.Serialization;
using _00_Domain;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHttpClient();
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

// app.MapGet("/", () => "Hello World");

// app.Urls.Add("");

app.MapGet("/my-supported-platforms", () => new[] { "Windows", "Mac", "Linux", "Unix" })
    .Produces<string>(StatusCodes.Status200OK)
    .WithName("GetAllPlateforms")
    .WithTags("Getters");


app.MapGet("/my-supported-platforms/{id:int}", async (int id, IHttpClientFactory factory) =>
{
    var plateforms = new[] { "Windows", "Mac", "Linux", "Unix" };

    if (id is < 0 or >= 4) return "";

    return plateforms[id];
})
.Produces<string>(StatusCodes.Status200OK)
.WithName("GetSpecificPlateform")
.WithTags("Getters");

app.MapGet("/books", async (MyDbContext dbContext) =>
{
    var books = await dbContext.Books
            .Include(_ => _.Author).ToListAsync();
    return Results.Ok(books);
})
.Produces<Book[]>(StatusCodes.Status200OK)
.WithName("GetAllBooks")
.WithTags("Getters");

app.MapGet("/books/{id}", async (int id, MyDbContext dbContext) =>
{
    var book = await dbContext.Books
            .Include(_ => _.Author)
            .FirstOrDefaultAsync(_ => _.Id == id);

    if (book is null)
        return Results.NotFound();

    return Results.Ok(book);
})
.Produces<Book>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithName("GetBookDetails")
.WithTags("Getters");

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
})
.Produces<Book>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status404NotFound)
.WithName("CreateBook")
.WithTags("Setters");

app.MapPut("/books/{id}", async (int id, Book updatedBook, MyDbContext dbContext) =>
{
    var foundAuthor = await dbContext.Authors
            .FirstOrDefaultAsync(_ => _.Id == updatedBook.Author.Id
                || _.LastName == updatedBook.Author.LastName);

    if (foundAuthor is null)
    {
        var createdAuthor = new Author
        {
            FirstName = updatedBook.Author.FirstName,
            LastName = updatedBook.Author.LastName,
        };

        dbContext.Authors.Add(createdAuthor);
        await dbContext.SaveChangesAsync();

        foundAuthor = createdAuthor;
    }

    var foundBook = dbContext.Books.Find(id);

    if (foundBook is null) return Results.NotFound();

    foundBook.Title = updatedBook.Title;
    foundBook.PublishedDate = updatedBook.PublishedDate;
    foundBook.Author = foundAuthor;

    await dbContext.SaveChangesAsync();

    return Results.NoContent();
})
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound)
.WithName("UpdateBook")
.WithTags("Setters");


app.MapDelete("/books/{id}", async (int id, MyDbContext dbContext) =>
{
    var book = await dbContext.Books.FindAsync(id);

    if (book is null)
        return Results.NotFound();

    return Results.Ok(book);
})
.Produces<Book>(StatusCodes.Status200OK)
.WithName("DeleteBook")
.WithTags("Setters");

app.Run();
