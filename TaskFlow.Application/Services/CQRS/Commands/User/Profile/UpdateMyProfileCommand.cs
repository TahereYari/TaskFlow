using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;


namespace TaskFlow.Application.Services.CQRS.Commands.User.Profile
{
    public class UpdateMyProfileCommand 
    {
        [Required(ErrorMessage = "نام الزامیست.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "نام خانوادگی الزامیست.")]
        public required string Family { get; set; }

        public IFormFile? ProfileImage { get; set; }

        [Required(ErrorMessage = "شماره تماس کاربر الزامیست.")]
        public required string PhoneNumber { get; set; }

        public string? BirthDate { get; set; }
    }
}
