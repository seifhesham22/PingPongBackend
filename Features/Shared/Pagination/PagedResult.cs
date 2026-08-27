namespace PingPong.API.Features.Shared.Pagination
{
    public sealed record PagedResult<T>(List<T> Items, int Total, int Page, int PageSize)
    {
        public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
        public bool HasNext => Page < TotalPages;
        public bool HasPrevious => Page > 1;
    }
}