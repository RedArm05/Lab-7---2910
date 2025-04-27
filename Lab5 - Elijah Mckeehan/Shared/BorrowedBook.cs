namespace Lab5_Elijah_Mckeehan.Shared
{
    public class BorrowedBook
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        public DateTime BorrowedDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public BorrowedBook(int userId, int bookId, string title, string author)
        {
            UserId = userId;
            BookId = bookId;
            Title = title;
            Author = author;
            BorrowedDate = DateTime.Now;
        }
    }
}
