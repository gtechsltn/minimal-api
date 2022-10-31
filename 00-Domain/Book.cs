namespace _00_Domain;
public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }= string.Empty;
    public DateTime PublishedDate { get; set; }
    public Author Author {get;set;}
}