using System;

namespace stranitza.Utility
{
    public abstract class PagedViewModel
    {        
        public int PageIndex { get; }

        public int PageSize { get; }

        public int TotalRecords { get; }

        public PagedViewModel(int totalRecords, int pageIndex, int pageSize)
        {
            PageSize = pageSize;
            PageIndex = pageIndex;            
            TotalRecords = totalRecords;
        }

        public bool HasPreviousPage
        {
            get { return (PageIndex > 1); }
        }

        public int TotalPages
        {
            get { return (int) Math.Ceiling(d: (decimal) TotalRecords / PageSize); }
        }

        public bool HasNextPage
        {
            get { return (PageIndex < TotalPages); }
        }      
    }
}
