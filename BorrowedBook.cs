namespace Lab5_Elijah_Mckeehan.Shared
{
    public class BorrowedBook
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime BorrowedDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public BorrowedBook(int userId, int bookId)
        {
            UserId = userId;
            BookId = bookId;
            BorrowedDate = DateTime.Now;
        }
    }
}
