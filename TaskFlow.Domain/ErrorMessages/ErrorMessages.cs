using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Domain.ErrorMessages
{
    public static class ErrorMessages
    {
        public const string UnknownError = "خطای ناشناخته";
        public const string ValidationError = "خطای اعتبار سنجی";
        public const string ExceptionError = "خطایی رخ داده. دوباره تلاش کنید";
        public const string NotExist = "موجود نیست";
    }
}
