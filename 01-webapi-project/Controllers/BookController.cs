using _00_Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _01_mvc_project.Controllers;

[ApiController]
[Route("[controller]")]
public class BookController : ControllerBase
{
    private readonly MyDbContext _dbContext;
    private readonly ILogger<BookController> _logger;

    public BookController(
        MyDbContext dbContext,
        ILogger<BookController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpPost(Name = "CreateBook")]
    public async Task<IActionResult> Post(Book createdBook)
    {
        var foundAuthor = await _dbContext.Authors
            .FirstOrDefaultAsync(_ => _.Id == createdBook.Author.Id
                || _.LastName == createdBook.Author.LastName);

        if (foundAuthor is null)
        {
            var createdAuthor = new Author
            {
                FirstName = createdBook.Author.FirstName,
                LastName = createdBook.Author.LastName,
            };

            _dbContext.Authors.Add(createdAuthor);
            await _dbContext.SaveChangesAsync();

            foundAuthor = createdAuthor;
        }

        createdBook.Author = foundAuthor;
        _dbContext.Books.Add(createdBook);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = createdBook.Id }, createdBook);
    }

    [HttpGet(Name = "GetBooks")]
    public async Task<IActionResult> GetAll()
    {
        var books = await _dbContext.Books
            .Include(_ => _.Author).ToListAsync();
        return new OkObjectResult(books);
    }

    [HttpGet("{id:int}", Name = "GetBook")]
    public async Task<IActionResult> Get(int id)
    {
        var book = await _dbContext.Books
            .Include(_ => _.Author)
            .FirstOrDefaultAsync(_ => _.Id == id);

        if (book is null)
            return NotFound();

        return new OkObjectResult(book);
    }

    [HttpPut("{id:int}", Name = "UpdateBook")]
    public async Task<IActionResult> Put(int id, Book updatedBook)
    {
        if (await _dbContext.Books.FindAsync(id) is Book foundBook)
        {
            foundBook.Title = updatedBook.Title;
            foundBook.PublishedDate = updatedBook.PublishedDate;

            await _dbContext.SaveChangesAsync();
            return new OkObjectResult(await _dbContext.Books.FindAsync(id));
        }

        return NotFound();
    }

    [HttpDelete("{id:int}", Name = "DeleteBook")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await _dbContext.Books.FindAsync(id) is Book foundBook)
        {
            _dbContext.Remove(foundBook);
            await _dbContext.SaveChangesAsync();

            return new OkObjectResult(await _dbContext.Books.FindAsync(id));
        }

        return NotFound();
    }
}
