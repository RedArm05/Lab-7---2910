namespace Lab5_Elijah_Mckeehan.Shared
{
    public class Book
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        public required string ISBN { get; set; }
        public bool IsBorrowed { get; set; }
        public int? BorrowedBy { get; set; }
    }

}
