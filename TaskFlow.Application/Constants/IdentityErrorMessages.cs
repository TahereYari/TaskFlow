using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Application.Constants
{
    public static class IdentityErrorMessages
    {
        public static string GetMessage(
            string code,
            string? description = null)
        {
            return code switch
            {
                "DuplicateUserName"
                    => "این نام کاربری قبلاً ثبت شده است.",

                "DuplicateEmail"
                    => "این ایمیل قبلاً ثبت شده است.",

                "InvalidUserName"
                    => "نام کاربری وارد شده معتبر نیست.",

                "InvalidEmail"
                    => "ایمیل وارد شده معتبر نیست.",

                "PasswordTooShort"
                    => "رمز عبور باید حداقل ۶ کاراکتر باشد.",

                "PasswordRequiresDigit"
                    => "رمز عبور باید حداقل یک عدد داشته باشد.",

                "PasswordRequiresUpper"
                    => "رمز عبور باید حداقل یک حرف بزرگ داشته باشد.",

                "PasswordRequiresLower"
                    => "رمز عبور باید حداقل یک حرف کوچک داشته باشد.",

                "PasswordRequiresNonAlphanumeric"
                    => "رمز عبور باید حداقل یک کاراکتر خاص داشته باشد.",

                "PasswordMismatch"
                    => "رمز عبور و تکرار رمز عبور یکسان نیستند.",

                "UserAlreadyHasPassword"
                    => "این کاربر قبلاً رمز عبور دارد.",

                "UserNotInRole"
                    => "کاربر در این نقش قرار ندارد.",

                "UserAlreadyInRole"
                    => "کاربر قبلاً در این نقش قرار گرفته است.",

                "InvalidRoleName"
                    => "نام نقش وارد شده معتبر نیست.",

                "ConcurrencyFailure"
                    => "خطایی در همگام‌سازی اطلاعات کاربر رخ داده است.",

                "LoginAlreadyAssociated"
                    => "این حساب کاربری قبلاً به یک حساب دیگر متصل شده است.",

                _ => description ?? "خطای نامشخصی در اطلاعات کاربر رخ داده است."
            };
        }
    }
}
