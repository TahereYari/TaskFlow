using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskFlow.Domain.ViewModels.Accounts
{
    public class LoginModel
    {
        [Required(ErrorMessage = "ایمیل الزامی است.")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "رمز عبور الزامی است.")]
        public required string Password { get; set; }
    }
}
