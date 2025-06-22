namespace backend.Models;

public class MovieTitleAlias
{
	public int Id { get; set; }

	public required string Value { get; set; }

	public Movie Movie { get; set; } = null!;

	public override string ToString() => Value;

	public override bool Equals(object? obj)
	{
		if (obj is not MovieTitleAlias objAsAlias)
		{
			return false;
		}

		return Value.Equals(objAsAlias.Value, StringComparison.OrdinalIgnoreCase);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}
