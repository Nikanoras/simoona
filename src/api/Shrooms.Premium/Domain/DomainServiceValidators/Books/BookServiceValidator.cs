using System;
using Shrooms.Contracts.Constants;
using Shrooms.Premium.Domain.DomainExceptions.Book;

namespace Shrooms.Premium.Domain.DomainServiceValidators.Books
{
    public class BookServiceValidator : IBookServiceValidator
    {
        public void CheckIfBookAlreadyExists(bool alreadyExists)
        {
            if (!alreadyExists)
            {
                return;
            }

            throw new BookException(ErrorCodes.BookAlreadyExistsCode);
        }

        public void CheckIfBookAllQuantitiesAreNotZero(bool allQuantitiesNotZero)
        {
            if (allQuantitiesNotZero != false)
            {
                return;
            }

            throw new BookException(ErrorCodes.BoolAllQuantitiesAreZeroCode);
        }

        public void CheckIfRequestedOfficesExist(bool officeIsValid)
        {
            if (officeIsValid)
            {
                return;
            }

            throw new ArgumentException("Incorrect offices provided while creating new book");
        }

        public void CheckIfBookOfficesFoundWhileDeleting(bool booksWereFound)
        {
            if (booksWereFound)
            {
                return;
            }

            throw new ArgumentException("Book was not found while deleting");
        }

        public void CheckIfBookWasFoundByIsbnFromExternalProvider(object book)
        {
            if (book != null)
            {
                return;
            }

            throw new BookException(ErrorCodes.BookNotFoundByExternalProviderCode);
        }

        public void ThrowIfBookCannotBeReturned(bool bookExists)
        {
            if (bookExists)
            {
                return;
            }

            throw new BookException(Resources.Models.Books.Books.BookCannotBeReturned);
        }
    }
}
