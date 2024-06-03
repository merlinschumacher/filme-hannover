namespace kinohannover.Models
{
    public class Alias
    {
        public int Id { get; set; }

        public required string Value { get; set; }

        public Movie Movie { get; set; }

        public override string ToString() => Value;

        public override bool Equals(object? obj)
        {
            if (obj is not Alias objAsAlias)
            {
                return false;
            }

            return Value.Equals(objAsAlias.Value, StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
