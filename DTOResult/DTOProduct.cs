namespace ShopApi
{
    public class ResponseProduct
    {
        public Guid UserId {get; set;}
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountPrice { get; set; }
        public string Description { get; set; }
        public int StockQuantity { get; set; }
        public string ImageURL { get; set; }

        public int CategoryId { get; set; }

        public string NameEn { get; set; }
        public string DescriptionEn { get; set; }

    }
    public class PaginationRequest
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;

        public int PageNumber {get; set;} = 1;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize :value;
        }

        public string SearchTerm {get; set;}
        public string? SortBy {get; set;}
        public bool SortDescending {get; set;} = false;
        public int CategoryId {get; set;}
        public decimal minPrice {get; set;}
        public decimal maxPrice {get; set;}
    }
    public class PaginationResponse<T>
    {
        public IEnumerable<T> Items {get; set;}
        public int PageNumber {get; set;}
        public int PageSize {get; set;}
        public int TotalCount {get; set;}
        public int TotalPages {get; set;}
        public bool HasPreviosPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PaginationResponse(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }
    }
}