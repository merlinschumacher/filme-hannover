namespace kinohannover.Models
{
    public class Alias : IEqualityComparer<Alias>
    {
        public int Id { get; set; }

        public required string Value { get; set; }

        public override string ToString() => Value;

        public bool Equals(Alias? x, Alias? y) => x?.Value.Equals(y?.Value, StringComparison.CurrentCultureIgnoreCase) == true;

        public int GetHashCode(Alias obj) => obj.Value.GetHashCode();
    }
}
