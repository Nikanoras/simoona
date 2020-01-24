﻿using AutoMapper;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Books;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Books;
using Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Book;
using Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Book.BookDetails;

namespace Shrooms.Premium.Infrastructure.ModelMappings.Profiles
{
    public class BooksProfile : Profile
    {
        protected override void Configure()
        {
            CreateModelMappings();
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateModelMappings()
        {
            CreateMap<BookMobileDTO, Book>();
            CreateMap<Book, BookMobileDTO>();
            CreateMap<ApplicationUser, MobileUserDTO>();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<BookMobileGetDTO, BookMobileGetViewModel>();
            CreateMap<OfficeBookDTO, OfficeBookViewModel>();
            CreateMap<MobileUserDTO, MobileUserViewModel>();
            CreateMap<BookMobileLogDTO, BookMobileLogViewModel>();
            CreateMap<RetrievedBookInfoDTO, RetrievedMobileBookInfoViewModel>();
            CreateMap<BookReportDTO, BookReportViewModel>();
        }
        private void CreateViewModelToDtoMappings()
        {
            CreateMap<BookMobilePostViewModel, BookMobilePostDTO>();
            CreateMap<BookMobileReturnViewModel, BookMobileReturnDTO>();
            CreateMap<BookMobileTakeViewModel, BookTakeDTO>();
            CreateMap<BookMobileTakeSpecificViewModel, BookMobileTakeSpecificDTO>();
            CreateMap<BookMobileGetViewModel, BookMobileGetDTO>();
            CreateMap<BookReportViewModel, BookReportDTO>();
        }
    }
}
