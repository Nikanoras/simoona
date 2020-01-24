﻿using Shrooms.DataTransferObjects.Models;

namespace Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Books.BooksByOffice
{
    public class BooksByOfficeOptionsDTO : UserAndOrganizationDTO
    {
        public int OfficeId { get; set; }
        public string SearchString { get; set; }
        public int Page { get; set; }
    }
}
