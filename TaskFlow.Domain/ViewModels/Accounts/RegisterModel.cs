using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskFlow.Domain.ViewModels.Accounts
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "نام کاربری الزامیست.")]
        public  string UserName { get; set; } = null!;

        [Required(ErrorMessage = "ایمیل الزامیست.")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        public  string Email { get; set; } = null!;

        [Required(ErrorMessage = "رمز عبور الزامیست")]
        public  string Password { get; set; } = null!;

        [Required(ErrorMessage = "تکرار رمز عبور الزامیست")]
        [Compare("Password", ErrorMessage = "رمز عبور و تکرار آن مطابقت ندارند")]
        public  string ConfirmPassword { get; set; } = null!;

    }
}
