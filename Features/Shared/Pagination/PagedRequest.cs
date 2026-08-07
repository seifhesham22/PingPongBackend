namespace PingPong.API.Features.Shared.Pagination
{
    public record PagedRequest
    {
        private const int MaxPageSize = 100;
        private const int MaxPage = 100_000;
        private readonly int _page = 1;
        private readonly int _pageSize = 20;

        public int Page
        {
            get => _page;
            init => _page = value < 1 ? 1 : Math.Min(value, MaxPage);
        }

        public int PageSize
        {
            get => _pageSize;
            init => _pageSize = value < 1 ? 20 : Math.Min(value, MaxPageSize);
        }

        public int Skip => (Page - 1) * PageSize;
    }
}