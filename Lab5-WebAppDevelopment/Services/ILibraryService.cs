using System.Collections.Generic;
using Lab5_WebAppDevelopment.Models;

namespace Lab5_WebAppDevelopment.Services
{
    public interface ILibraryService
    {
        /// <summary>
        /// The complete list of available books.
        /// </summary>
        List<Book> Books { get; }

        /// <summary>
        /// The complete list of users.
        /// </summary>
        List<User> Users { get; }

        /// <summary>
        /// Which books each user currently has borrowed.
        /// </summary>
        Dictionary<User, List<Book>> BorrowedBooks { get; }

        // --- Book CRUD ---
        void AddBook(Book book);
        void EditBook(Book updatedBook);
        void DeleteBook(int bookId);

        // --- User CRUD ---
        void AddUser(User user);
        void EditUser(User updatedUser);
        void DeleteUser(int userId);

        // --- Borrow & Return ---
        /// <summary>
        /// Moves the specified book into the specified user's borrowed list.
        /// Returns true on success, false if bookId or userId invalid.
        /// </summary>
        bool BorrowBook(int bookId, int userId);

        /// <summary>
        /// Returns the borrowed book at the given index from that user's list.
        /// Returns true on success, false if invalid userId or index.
        /// </summary>
        bool ReturnBook(int userId, int borrowedBookIndex);
    }
}
