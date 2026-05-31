namespace O2Connect.Api.Models;

public interface ISmartEnum<TType> where TType : struct
{
    string Value { get; }
}
