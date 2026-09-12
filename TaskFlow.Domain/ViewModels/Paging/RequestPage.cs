using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Domain.ViewModels.Paging
{
    public class RequestPage
    {
        public int PageId { get; set; } = 1;

        public int TakeEntity { get; set; } = 10;
    }
}
