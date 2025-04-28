using CsvHelper.Configuration;
using CsvHelper;
using System.Text.Json;
using System.Globalization;
using System.Text;
using Lab5_Elijah_Mckeehan.Shared;


namespace Lab5_Elijah_Mckeehan.Services
{
    public interface ILibraryService
    {
        List<Book> GetBooks();
        void AddBook(Book book);
        void EditBook(Book updatedBook);
        void DeleteBook(int bookId);
        List<User> GetUsers();
        Task<List<User>> GetUsersAsync();
        void AddUser(User user);
        Task AddUserAsync(User user);
        void SaveUsers(List<User> users);
        void EditUser(User updatedUser);
        void DeleteUser(int userId);
        void UpdateBook(Book updatedBook);
        void BorrowBook(int userId, int bookId);
        void ReturnBook(int userId, int bookId);
        List<Book> GetBorrowedBooks(int userId);
        Task SaveUsersAsync(List<User> users);
        List<Book> GetAvailableBooks();
        void SaveBooks();
        void SaveBorrowedBooks();
        Dictionary<int, List<int>> GetAllBorrowedBooks();
        void ClearBorrowedBooks();
    }

    public class LibraryService : ILibraryService
    {
        private readonly IMessageService _messageService;
        private static readonly object userLock = new object();
        private static string BooksFilePath => Path.Combine("Data", "Books.csv");
        private static string UsersFilePath => Path.Combine("Data", "Users.csv");
        private string borrowedBooksFile = Path.Combine("Data", "BorrowedBooks.json");
        private List<Book> books = new();
        private List<User> users = new();
        private Dictionary<int, List<int>> borrowedBooks = new();
        public void ClearBooks() => books.Clear();
        public void ClearUsers() => users.Clear();

        public LibraryService(IMessageService messageService)
        {
            _messageService = messageService;
            Directory.CreateDirectory("Data");
            LoadBooks();
            LoadUsers();
            LoadBorrowedBooks();
        }

        public void ClearBorrowedBooks()
        {
            borrowedBooks.Clear();  // Clears all borrowed book entries
            SaveBorrowedBooks();    // Save the cleared state
        }

        public void LoadBooks()
        {
            try
            {
                // Create a new CsvConfiguration object to handle missing headers or fields
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,  // Ignore missing headers
                    MissingFieldFound = null // Ignore missing fields
                };

                using (var reader = new StreamReader(BooksFilePath))
                using (var csv = new CsvReader(reader, config))
                {
                    // Read the records from the CSV and map them to a list of Book objects
                    books = csv.GetRecords<Book>().ToList(); // Make sure to assign to the class-level 'books' variable

                    _messageService.AddMessage($"Loaded {books.Count} books from CSV.");
                }
            }
            catch (Exception ex)
            {
                // Handle the error, such as logging the issue or showing a message
                _messageService.AddMessage($"Error loading books: {ex.Message}");
            }
        }

        public void UpdateBook(Book updatedBook)
        {
            var existingBook = books.FirstOrDefault(b => b.Id == updatedBook.Id);
            if (existingBook != null)
            {
                existingBook.Title = updatedBook.Title;
                existingBook.Author = updatedBook.Author;
                existingBook.ISBN = updatedBook.ISBN;
                SaveBooks(); // Save the updated list of books
            }
        }

        public void LoadUsers()
        {
            if (!File.Exists(UsersFilePath)) return;
            
            try
            {
                using var reader = new StreamReader(UsersFilePath);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true
                });
        
                users = csv.GetRecords<User>().ToList();
                _messageService.AddMessage($"Loaded {users.Count} users from CSV.");
            }
            catch (Exception ex)
            {
                _messageService.AddMessage($"Error loading users: {ex.Message}");
            }
        }
        public void LoadBorrowedBooks()
        {
            if (File.Exists(borrowedBooksFile))
            {
                try
                {
                    var json = File.ReadAllText(borrowedBooksFile);
                    var borrowedBookList = JsonSerializer.Deserialize<List<BorrowedBook>>(json) ?? new List<BorrowedBook>();

                    // Transform the list of borrowed books into a dictionary
                    borrowedBooks = borrowedBookList
                        .GroupBy(bb => bb.UserId)  // Group by UserId
                        .ToDictionary(g => g.Key, g => g.Select(bb => bb.BookId).ToList());

                    _messageService.AddMessage("Borrowed books loaded.");
                }
                catch (Exception ex)
                {
                    _messageService.AddMessage($"Error loading borrowed books: {ex.Message}");
                }
            }
            else
            {
                _messageService.AddMessage("Borrowed books file not found.");
            }
        }

        public Dictionary<int, List<int>> GetAllBorrowedBooks()
        {
                return borrowedBooks; // Return the dictionary of borrowed books directly
        }


        public void SaveBooks()
        {
            try
            {
                using var writer = new StreamWriter(BooksFilePath);
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true
                });
                csv.WriteRecords(books); // Synchronous write operation
            }
            catch (Exception ex)
            {
                _messageService.AddMessage($"Error saving books: {ex.Message}");
            }
        }

        public async Task SaveUsersAsync(List<User> users)
        {
            try
            {
                using (var writer = new StreamWriter(UsersFilePath))
                using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    await csv.WriteRecordsAsync(users);  // Asynchronous write operation
                }
            }
            catch (Exception ex)
            {
                _messageService.AddMessage($"Error in SaveUsersAsync: {ex.Message}");
            }
        }

        public void SaveBorrowedBooks()
        {
            try
            {
                // Validate borrowed books to ensure only valid bookIds are present
                borrowedBooks = borrowedBooks
                    .Where(kv => kv.Value.All(bookId => books.Any(b => b.Id == bookId)))
                    .ToDictionary(kv => kv.Key, kv => kv.Value);

                var json = JsonSerializer.Serialize(borrowedBooks);
                File.WriteAllText(borrowedBooksFile, json); // Save back to file
                _messageService.AddMessage("Borrowed books updated.");
            }
            catch (Exception ex)
            {
                _messageService.AddMessage($"Error saving borrowed books: {ex.Message}");
            }
        }


        public List<Book> GetBooks() => books;

        public List<User> GetUsers()
        {
            if (!File.Exists(UsersFilePath))
            {
                return new List<User>();
            }

            try
            {
                using var reader = new StreamReader(UsersFilePath);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true
                });

                return csv.GetRecords<User>().ToList();
            }
            catch (Exception ex)
            {
                _messageService.AddMessage($"Error loading users: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                if (!File.Exists(UsersFilePath))
                {
                    return new List<User>(); // Return an empty list if no file exists
                }

                using (var reader = new StreamReader(UsersFilePath))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    return await Task.Run(() => csv.GetRecords<User>().ToList()); 
                }
            }
            catch (Exception ex)
            {
                // Handle errors such as file access issues
                throw new Exception("Error reading from CSV file", ex);
            }
        }

        public void SaveUsers(List<User> users)
        {
            try
            {
                using var writer = new StreamWriter(UsersFilePath);
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true
                });
                
                csv.WriteRecords(users);
            }
            catch (Exception ex)
            {
                _messageService.AddMessage($"Error saving users: {ex.Message}");
            }
        }


        public void AddUser(User user)
        {
            if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Email))
            {
                _messageService.AddMessage("❌ User name or email is empty.");
                return;
            }

            user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
            users.Add(user);

            SaveUsers(users); // Save after adding

            _messageService.AddMessage($"User {user.Name} added with ID {user.Id}");
        }

        public async Task AddUserAsync(User newUser)
        {
            try
            {
                var users = await ReadUsersAsync(); // Read existing users asynchronously
                users.Add(newUser); // Add new user

                await WriteUsersAsync(users); // Write back to CSV 
            }
            catch (Exception ex)
            {
                _messageService.AddMessage($"Error in AddUserAsync: {ex.Message}"); // Display error in UI
            }
        }

        private async Task WriteUsersAsync(List<User> users)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Name,Email"); // Header

            foreach (var user in users)
            {
                sb.AppendLine($"{user.Name},{user.Email}");
            }

            await File.WriteAllTextAsync(UsersFilePath, sb.ToString());
        }

        // Method to read the users from the CSV file
        private async Task<List<User>> ReadUsersAsync()
        {
            var users = new List<User>();

            if (File.Exists(UsersFilePath))
            {
                var lines = await File.ReadAllLinesAsync(UsersFilePath);
                foreach (var line in lines.Skip(1)) // Skip header if exists
                {
                    var columns = line.Split(',');
                    var user = new User
                    {
                        Name = columns[0],
                        Email = columns[1]
                    };
                    users.Add(user);
                }
            }
            return users;
        }

        public void EditUser(User updatedUser)
        {
            var users = GetUsers(); // Fetch users
            var existingUser = users.FirstOrDefault(u => u.Id == updatedUser.Id);
            if (existingUser != null)
            {
                existingUser.Name = updatedUser.Name;
                existingUser.Email = updatedUser.Email;
                SaveUsers(users); // Save changes after editing
            }
        }

        public void DeleteUser(int userId)
        {
            var userToRemove = users.FirstOrDefault(u => u.Id == userId);
            if (userToRemove != null)
            {
                users.Remove(userToRemove);
                SaveUsers(users); // Sync save
            }
        }

        public void BorrowBook(int userId, int bookId)
        {
            // Ensure the user has a borrowed list
            if (!borrowedBooks.TryGetValue(userId, out var userBorrowedList))
            {
                userBorrowedList = new List<int>();
                borrowedBooks[userId] = userBorrowedList;
            }

            // Ensure the book is available (not borrowed by any user)
            var isBookAlreadyBorrowed = borrowedBooks.Values.Any(b => b.Contains(bookId));
            if (isBookAlreadyBorrowed)
            {
                _messageService.AddMessage($"Error: Book {bookId} is already borrowed.");
                return;
            }

            // If the book is already borrowed by this user, do not allow double borrowing
            if (userBorrowedList.Contains(bookId))
            {
                _messageService.AddMessage($"Error: User {userId} has already borrowed book {bookId}.");
                return;
            }

         
            var book = books.FirstOrDefault(b => b.Id == bookId); // or wherever your books are stored
            if (book != null)
            {
                book.IsBorrowed = true;
                book.BorrowedBy = userId;
            }

            userBorrowedList.Add(bookId);
            SaveBorrowedBooks();

            _messageService.AddMessage($"Success: User {userId} borrowed book {bookId}.");
        }

        public List<Book> GetAvailableBooks() => books.Where(b => !b.IsBorrowed).ToList();

        public void ReturnBook(int userId, int bookId)
        {
            if (borrowedBooks.ContainsKey(userId) && borrowedBooks[userId].Contains(bookId))
            {
                borrowedBooks[userId].Remove(bookId);

                var book = books.FirstOrDefault(b => b.Id == bookId);
                if (book != null)
                {
                    book.IsBorrowed = false;
                    book.BorrowedBy = null;
                }

                SaveBorrowedBooks();
                _messageService.AddMessage($"User {userId} returned book {bookId}.");
            }
            else
            {
                _messageService.AddMessage($"Error: User {userId} has not borrowed book {bookId}.");
            }
        }

        public List<Book> GetBorrowedBooks(int userId)
        {
            if (borrowedBooks.ContainsKey(userId))
            {
                var borrowedBookIds = borrowedBooks[userId];
                return books.Where(b => borrowedBookIds.Contains(b.Id)).ToList();
            }

            return new List<Book>();
        }

        public void AddBook(Book book)
        {
            books.Add(book);
            SaveBooks(); // Sync save
            _messageService.AddMessage("Book saved to CSV");
        }

        public void EditBook(Book updatedBook)
        {
            var book = books.FirstOrDefault(b => b.Id == updatedBook.Id);
            if (book != null)
            {
                book.Title = updatedBook.Title;
                book.Author = updatedBook.Author;
                book.ISBN = updatedBook.ISBN;
                SaveBooks(); // Sync save
                _messageService.AddMessage("Book saved to CSV");
            }
        }

        public void DeleteBook(int bookId)
        {
            var book = books.FirstOrDefault(b => b.Id == bookId);
            if (book != null)
            {
                books.Remove(book);
                SaveBooks(); // Sync save
                _messageService.AddMessage("Book saved to CSV");
            }
        }
    }
}
