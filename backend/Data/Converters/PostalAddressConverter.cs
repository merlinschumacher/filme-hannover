using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Schema.NET;

namespace backend.Data.Converters
{
    public class PostalAddressConverter : ValueConverter<PostalAddress, string>
    {
        public PostalAddressConverter()
            : base(
                address => SchemaSerializer.SerializeObject(address),
                json => SchemaSerializer.DeserializeObject<PostalAddress>(json) ?? new PostalAddress()
            )
        {
        }
    }
}