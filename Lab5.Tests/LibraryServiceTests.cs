using System;
using System.IO;
using System.Linq;
using Xunit;
using Lab5_WebAppDevelopment.Services;
using Lab5_WebAppDevelopment.Models;

namespace Lab5.Tests
{
    public class LibraryServiceTests : IDisposable
    {
        private readonly string _tempFolder;
        private readonly LibraryService _svc;

        public LibraryServiceTests()
        {
            // ARRANGE
            _tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempFolder);

            // Seed Books.csv
            File.WriteAllLines(Path.Combine(_tempFolder, "Books.csv"), new[]
            {
                "1,The Hobbit,J.R.R. Tolkien,123-ABC",
                "2,Dune,Frank Herbert,456-DEF"
            });

            // Seed Users.csv
            File.WriteAllLines(Path.Combine(_tempFolder, "Users.csv"), new[]
            {
                "1,Alice,alice@example.com",
                "2,Bob,bob@example.com"
            });

            // Initialize service, which auto-loads those CSVs
            _svc = new LibraryService(_tempFolder);
        }

        public void Dispose()
        {
            Directory.Delete(_tempFolder, recursive: true);
        }

        [Fact]
        public void Constructor_LoadsBooksAndUsers()
        {

            // ASSERT
            Assert.Equal(2, _svc.Books.Count);
            Assert.Contains(_svc.Books, b => b.Title == "Dune");

            Assert.Equal(2, _svc.Users.Count);
            Assert.Contains(_svc.Users, u => u.Name == "Alice");
        }

        [Fact]
        public void AddBook_AppendsToMemoryAndCsv()
        {
            // ACT
            var newBook = new Book { Title = "Neuromancer", Author = "William Gibson", ISBN = "789-GHI" };
            _svc.AddBook(newBook);

            // ASSERT
            Assert.Equal(3, _svc.Books.Count);
            Assert.Contains(_svc.Books, b => b.Title == "Neuromancer" && b.Id == 3);

            // ASSERT
            var lines = File.ReadAllLines(Path.Combine(_tempFolder, "Books.csv"));
            Assert.Contains(lines, l => l.Contains("Neuromancer") && l.StartsWith("3,"));
        }

        [Fact]
        public void EditBook_UpdatesMemoryAndCsv()
        {
            // ARRANGE
            var updated = new Book { Id = 1, Title = "Hobbit II", Author = "Tolkien", ISBN = "123-ABC" };

            // ACT
            _svc.EditBook(updated);

            // ASSERT
            var b = _svc.Books.First(x => x.Id == 1);
            Assert.Equal("Hobbit II", b.Title);

            // ASSERT
            var lines = File.ReadAllLines(Path.Combine(_tempFolder, "Books.csv"));
            Assert.Contains(lines, l => l.Contains("Hobbit II"));
        }

        [Fact]
        public void DeleteBook_RemovesFromMemoryAndCsv()
        {
            // ACT
            _svc.DeleteBook(2);

            // ASSERT
            Assert.DoesNotContain(_svc.Books, b => b.Id == 2);

            // ASSERT
            var lines = File.ReadAllLines(Path.Combine(_tempFolder, "Books.csv"));
            Assert.DoesNotContain(lines, l => l.Contains("Dune"));
        }

        [Fact]
        public void AddUser_AppendsToMemoryAndCsv()
        {
            // ACT
            var newUser = new User { Name = "Carol", Email = "carol@example.com" };
            _svc.AddUser(newUser);

            // ASSERT
            Assert.Equal(3, _svc.Users.Count);
            Assert.Contains(_svc.Users, u => u.Name == "Carol" && u.Id == 3);

            // ASSERT
            var lines = File.ReadAllLines(Path.Combine(_tempFolder, "Users.csv"));
            Assert.Contains(lines, l => l.Contains("Carol") && l.StartsWith("3,"));
        }

        [Fact]
        public void EditUser_UpdatesMemoryAndCsv()
        {
            // ARRANGE
            var updated = new User { Id = 1, Name = "Alice Smith", Email = "alice@smith.com" };

            // ACT
            _svc.EditUser(updated);

            // ASSERT
            var u = _svc.Users.First(x => x.Id == 1);
            Assert.Equal("Alice Smith", u.Name);

            // ASSERT
            var lines = File.ReadAllLines(Path.Combine(_tempFolder, "Users.csv"));
            Assert.Contains(lines, l => l.Contains("Alice Smith"));
        }

        [Fact]
        public void DeleteUser_RemovesFromMemoryAndCsv()
        {
            // ACT
            _svc.DeleteUser(2);

            // ASSERT
            Assert.DoesNotContain(_svc.Users, u => u.Id == 2);

            // ASSERT
            var lines = File.ReadAllLines(Path.Combine(_tempFolder, "Users.csv"));
            Assert.DoesNotContain(lines, l => l.Contains("Bob"));
        }

        [Fact]
        public void BorrowAndReturnBook_WorksAsExpected()
        {
            // ACT
            bool borrowed = _svc.BorrowBook(1, 2);

            // ASSERT
            Assert.True(borrowed);
            Assert.DoesNotContain(_svc.Books, b => b.Id == 1);
            var userBob = _svc.Users.First(u => u.Id == 2);
            Assert.Contains(_svc.BorrowedBooks[userBob], b => b.Id == 1);

            // ACT
            bool returned = _svc.ReturnBook(2, 0);

            // ASSERT
            Assert.True(returned);
            Assert.Contains(_svc.Books, b => b.Id == 1);
            Assert.Empty(_svc.BorrowedBooks[userBob]);
        }
    }
}
