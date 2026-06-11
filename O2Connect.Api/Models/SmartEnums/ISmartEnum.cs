namespace O2Connect.Api.Models.SmartEnums;

public interface ISmartEnum<TType> where TType : struct
{
    string Value { get; }
}
