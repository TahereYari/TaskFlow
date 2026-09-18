using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;


namespace TaskFlow.Application.Services.CQRS.Commands.User.CreateUser
{
    public class SaveUserCommand
    {
        public Guid? Id { get; set; }
        [Required(ErrorMessage = "نام کاربری الزامیست.")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "ایمیل الزامیست.")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        public string Email { get; set; } = null!;


        public string? Password { get; set; } = null;

        [Required(ErrorMessage = "نقش الزامیست.")]
        public string Role { get; set; } = null!;

        [Required(ErrorMessage = "نام کاربر الزامیست.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "نام خانوادگی  الزامیست.")]
        public string Family { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
