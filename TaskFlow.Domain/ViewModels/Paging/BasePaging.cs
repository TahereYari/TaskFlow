using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Domain.ViewModels.Paging
{
    using Microsoft.EntityFrameworkCore;

    public class BasePaging<T>
    {
        public BasePaging()
        {
            PageId = 1;
            TakeEntity = 10;
            HowManyPageAfterAndBefore = 1;
            Entities = new List<T>();
        }

        public int PageId { get; set; }

        public int PageCount { get; set; }

        public int AllEntitiesCount { get; set; }

        public int StartPage { get; set; }

        public int EndPage { get; set; }

        public int TakeEntity { get; set; }

        public int SkipEntity { get; set; }

        public int HowManyPageAfterAndBefore { get; set; }

        public List<T> Entities { get; set; }


        public async Task<BasePaging<T>> Paging(
            IQueryable<T> query,
            CancellationToken cancellationToken = default)
        {
            // تعداد کل رکوردها
            var allEntitiesCount =
                await query.CountAsync(cancellationToken);

            // تعداد کل صفحات
            var pageCount = (int)Math.Ceiling(
                allEntitiesCount / (double)TakeEntity
            );

            // اگر PageId از تعداد صفحات بیشتر بود
            // آخرین صفحه را نمایش بده
            if (pageCount > 0 && PageId > pageCount)
            {
                PageId = pageCount;
            }

            // اگر PageId کمتر از 1 بود
            if (PageId < 1)
            {
                PageId = 1;
            }

            AllEntitiesCount = allEntitiesCount;

            // تعداد رکوردهایی که باید Skip شوند
            SkipEntity = (PageId - 1) * TakeEntity;

            PageCount = pageCount;

            // اولین شماره صفحه‌ای که برای UI نمایش داده می‌شود
            StartPage =
                PageId - HowManyPageAfterAndBefore <= 0
                    ? 1
                    : PageId - HowManyPageAfterAndBefore;

            // آخرین شماره صفحه‌ای که برای UI نمایش داده می‌شود
            EndPage =
                PageId + HowManyPageAfterAndBefore > PageCount
                    ? PageCount
                    : PageId + HowManyPageAfterAndBefore;

            // گرفتن فقط رکوردهای صفحه فعلی
            Entities = await query
                .Skip(SkipEntity)
                .Take(TakeEntity)
                .ToListAsync(cancellationToken);

            return this;
        }


        public PagingViewModel GetCurrentPaging()
        {
            return new PagingViewModel
            {
                PageId = PageId,
                StartPage = StartPage,
                EndPage = EndPage,
                PageCount = PageCount,
                AllEntitiesCount = AllEntitiesCount
            };
        }
    }


    public class PagingViewModel
    {
        public int PageId { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }
        public int PageCount { get; set; }
        public int AllEntitiesCount { get; set; }
    }
}
