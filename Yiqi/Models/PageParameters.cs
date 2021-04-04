namespace Yiqi.Models
{
    public record PageParameters
    {
        public string StartDate { get; init; }

        public string EndDate { get; init; }

        public int Current { get; init; }

        public int Size { get; init; }
    }
}