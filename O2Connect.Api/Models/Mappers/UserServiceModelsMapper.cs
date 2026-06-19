using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories.Filters;
using O2Connect.Dto.Management.Users;

namespace O2Connect.Api.Models.Mappers;

public static class UserServiceModelsMapper
{
    public static UserFilter ToFilter(this UsersSearchFilterRequest filterRequest)
    {
        if (filterRequest == null)
            return UserFilter.Empty;

        var filter = new UserFilter
        {
            Name = filterRequest.Name,
            Email = filterRequest.Email,
            Role = filterRequest.Role,
            Status = filterRequest.Status?.Select(s => Enum.Parse<EntityStatus>(s, true)).ToHashSet(),
            MinCreatedAt = filterRequest.MinCreatedAt,
            MaxCreatedAt = filterRequest.MaxCreatedAt,
            MinLastModifiedAt = filterRequest.MinLastModifiedAt,
            MaxLastModifiedAt = filterRequest.MaxLastModifiedAt,
            MinRevokedAt = filterRequest.MinRevokedAt,
            MaxRevokedAt = filterRequest.MaxRevokedAt,
        };

        return filter;
    }

    public static UserSummaryResponse ToSummaryDto(this User user)
    {
        return new UserSummaryResponse
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role,
            Status = user.Status.ToString(),
            DisplayName = user.DisplayName,
            ImageUrl = user.ImageUrl,
        };
    }

    public static UserDetailResponse ToDetailDto(this User user)
    {
        return new UserDetailResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            Status = user.Status.ToString(),
            CreatedAt = user.CreatedAt,
            DisplayName = user.DisplayName,
            ImageUrl = user.ImageUrl,
            LastModifiedAt = user.LastModifiedAt,
            RevokedAt = user.RevokedAt
        };
    }
}
