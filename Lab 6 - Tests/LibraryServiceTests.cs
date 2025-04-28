using Moq;
using Lab5_Elijah_Mckeehan.Services;
using Lab5_Elijah_Mckeehan.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace Lab_6___Tests
{
    [TestClass]
    public class LibraryServiceTests
    {
        private Mock<IMessageService> _mockMessageService;
        private LibraryService _service;
        private string tempFilePath;

        // A TestContext is provided to each test method by MSTest
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            // Initialize mock and service for each test to ensure isolation
            _mockMessageService = new Mock<IMessageService>();
            _service = new LibraryService(_mockMessageService.Object);
            _service.LoadBooks();
            _service.LoadUsers(tempFilePath);
            _service.ClearBooks();
            _service.ClearUsers();

            // Set tempFilePath to a temporary file for user persistence tests
            tempFilePath = Path.GetTempFileName();
        }

        private Book CreateBook(int id, string title, string author, string isbn, bool isBorrowed = false, int? borrowedBy = null)
        {
            return new Book
            {
                Id = id,
                Title = title,
                Author = author,
                ISBN = isbn,
                IsBorrowed = isBorrowed,
                BorrowedBy = borrowedBy
            };
        }

        private User CreateUser(int id, string name, string email)
        {
            return new User { Id = id, Name = name, Email = email };
        }

        [TestMethod]
        public void AddBook_ShouldAddBook()
        {
            var book = CreateBook(1, "Clean Code", "Robert C. Martin", "9780132350884");

            _service.AddBook(book);

            var addedBook = _service.GetBooks().FirstOrDefault(b => b.Id == 1);
            Assert.IsNotNull(addedBook);
            Assert.AreEqual("Clean Code", addedBook.Title);
        }

        [TestMethod]
        public void EditBook_ShouldUpdateBook()
        {
            var book = CreateBook(1, "Clean Code", "Robert C. Martin", "9780132350884");
            _service.AddBook(book);
            var updatedBook = CreateBook(1, "Clean Code", "Robert C. Martin", "9780132350884");

            _service.EditBook(updatedBook);

            _mockMessageService.Verify(m => m.AddMessage(It.Is<string>(s => s.Contains("Book saved to CSV"))), Times.AtLeastOnce);
            var editedBook = _service.GetBooks().FirstOrDefault(b => b.Id == 1);
            Assert.IsNotNull(editedBook);
            Assert.AreEqual("Clean Code", editedBook.Title);
        }

        [TestMethod]
        public void DeleteBook_ShouldRemoveBook()
        {
            var book = CreateBook(1, "To Delete", "Author", "123");
            _service.AddBook(book);

            _service.DeleteBook(book.Id);

            var result = _service.GetBooks().FirstOrDefault(b => b.Id == book.Id);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void BorrowBook_ShouldSetBorrowedByUser()
        {
            var user = CreateUser(1, "Alice", "alice@example.com");
            var book = CreateBook(1, "Some Book", "Author", "ISBN");

            _service.AddUser(user);
            _service.AddBook(book);

            _service.BorrowBook(user.Id, book.Id);

            var result = _service.GetBooks().First(b => b.Id == book.Id);
            Assert.IsTrue(result.IsBorrowed);
            Assert.AreEqual(user.Id, result.BorrowedBy);
        }

        [TestMethod]
        public void ReturnBook_ShouldClearBorrowStatus()
        {
            var user = CreateUser(1, "Bob", "bob@example.com");
            var book = CreateBook(1, "Return Me", "Author", "123", true, 1);

            _service.AddUser(user);
            _service.AddBook(book);

            _service.BorrowBook(user.Id, book.Id);
            _service.ReturnBook(user.Id, book.Id);

            var result = _service.GetBooks().First();
            Assert.IsFalse(result.IsBorrowed);
            Assert.IsNull(result.BorrowedBy);
        }

        [TestMethod]
        public void GetBorrowedBooks_ShouldReturnBorrowedByUser()
        {
            var user = CreateUser(1, "User1", "u1@mail.com");
            var borrowedBook = CreateBook(1, "Borrowed", "Author", "123");
            var availableBook = CreateBook(2, "Free Book", "Author", "456");

            _service.AddUser(user);
            _service.AddBook(borrowedBook);
            _service.AddBook(availableBook);

            _service.BorrowBook(user.Id, borrowedBook.Id);

            var results = _service.GetBorrowedBooks(user.Id);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Borrowed", results[0].Title);
        }

        [TestMethod]
        public void GetAvailableBooks_ShouldReturnUnborrowedOnly()
        {
            _service.LoadUsers(tempFilePath);
            _service.ClearBooks();

            var borrowedBook = CreateBook(1, "Borrowed", "Author", "123");
            var availableBook = CreateBook(2, "Available", "Author", "456");

            _service.AddBook(borrowedBook);
            _service.AddBook(availableBook);

            _service.AddUser(CreateUser(1, "Alice", "alice@example.com"));
            _service.BorrowBook(1, 1);

            var results = _service.GetAvailableBooks();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Available", results[0].Title);
        }

        [TestMethod]
        public void AddUser_ShouldAddUser()
        {
            // Arrange
            var user = new User { Name = "NewUser", Email = "new@example.com" };
            // Act
            _service.AddUser(user);
            // Assert
            var addedUser = _service.GetUsers().FirstOrDefault(u => u.Email == "new@example.com");
            Assert.IsNotNull(addedUser);
            Assert.AreEqual("NewUser", addedUser.Name);

            _mockMessageService.Verify(m => m.AddMessage(It.Is<string>(s => s.Contains("❌"))), Times.Never);
        }

        [TestMethod]
        public void EditUser_ShouldModifyUser()
        {
            var user = CreateUser(1, "Original", "original@mail.com");
            _service.AddUser(user);

            var updatedUser = CreateUser(1, "Updated", "updated@mail.com");
            _service.EditUser(updatedUser);

            var editedUser = _service.GetUsers().FirstOrDefault(u => u.Id == 1);
            Assert.IsNotNull(editedUser);
            Assert.AreEqual("Updated", editedUser.Name);
        }

        [TestMethod]
        public void DeleteUser_ShouldRemoveUser()
        {
            var user = CreateUser(1, "To Delete", "delete@mail.com");
            _service.AddUser(user);

            _service.DeleteUser(user.Id);

            var users = _service.GetUsers();
            Assert.AreEqual(0, users.Count);
        }

        [TestMethod]
        public void SaveAndLoadBooks_ShouldPersistData()
        {
            var book = CreateBook(1, "Persistent", "Author", "999");
            _service.AddBook(book);
            _service.SaveBooks();

            var newService = new LibraryService(_mockMessageService.Object);
            newService.LoadBooks();
            var books = newService.GetBooks();

            Assert.IsTrue(books.Any(b => b.Id == 1 && b.Title == "Persistent"));
        }

        [TestMethod]
        public void SaveAndLoadUsers_ShouldPersistData()
        {
            try
            {
                var user = new User { Name = "Persistent User", Email = "user@mail.com" };
                _service.AddUser(user);

                // Save users to the temp file
                _service.SaveUsers(_service.GetUsers(), tempFilePath);

                // Create a new service instance
                var newService = new LibraryService(_mockMessageService.Object);

                newService.LoadUsers(tempFilePath);

                var loadedUsers = newService.GetUsers();

                Assert.IsNotNull(loadedUsers);
                Assert.IsTrue(loadedUsers.Any(u => u.Name == "Persistent User"), "Loaded users did not contain the expected user.");
            }
            finally
            {
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
            }
        }

        [TestMethod]
        public void BorrowBook_ShouldNotAllowDoubleBorrow()
        {
            var user1 = CreateUser(1, "User1", "u1@mail.com");
            var user2 = CreateUser(2, "User2", "u2@mail.com");
            var book = CreateBook(1, "Shared Book", "Author", "123");

            _service.ClearBorrowedBooks();

            _service.AddUser(user1);
            _service.AddUser(user2);
            _service.AddBook(book);

            _service.BorrowBook(user1.Id, book.Id);

            var user1BorrowedBooks = _service.GetBorrowedBooks(user1.Id);
            Assert.AreEqual(1, user1BorrowedBooks.Count);
            Assert.AreEqual(book.Id, user1BorrowedBooks.First().Id);

            // Attempt to borrow again with same user
            _service.BorrowBook(user1.Id, book.Id);
            var user1DoubleBorrowedBooks = _service.GetBorrowedBooks(user1.Id);
            Assert.AreEqual(1, user1DoubleBorrowedBooks.Count);

            // Attempt to borrow with another user
            _service.BorrowBook(user2.Id, book.Id);
            var user2BorrowedBooks = _service.GetBorrowedBooks(user2.Id);
            Assert.AreEqual(0, user2BorrowedBooks.Count); // Should be 0 because already borrowed
        }

        [TestMethod]
        public void ReturnBook_ShouldOnlyWorkIfBorrowedByUser()
        {
            var user1 = CreateUser(1, "User1", "u1@mail.com");
            var user2 = CreateUser(2, "User2", "u2@mail.com");
            var book = CreateBook(1, "Borrowed", "Author", "123", true, 1);
            var bookNotBorrowed = CreateBook(2, "Unborrowed Book", "Author", "999");

            _service.AddUser(user1);
            _service.AddUser(user2);
            _service.AddBook(book);
            _service.AddBook(bookNotBorrowed);

            _service.ReturnBook(user2.Id, bookNotBorrowed.Id);

            var stillBorrowedBook = _service.GetBooks().First(b => b.Id == book.Id);
            Assert.IsTrue(stillBorrowedBook.IsBorrowed);
            Assert.AreEqual(user1.Id, stillBorrowedBook.BorrowedBy);
        }
    }
}
