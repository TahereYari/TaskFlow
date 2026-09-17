using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskFlow.Domain.ViewModels.RefreshToken
{
    public class RefreshTokenModel
    {
        [Required(ErrorMessage ="توکن Refresh الزامیست.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
