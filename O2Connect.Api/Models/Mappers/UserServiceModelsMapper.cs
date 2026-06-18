using O2Connect.Api.Models.Store;
using O2Connect.Dto.Management.Users;

namespace O2Connect.Api.Models.Mappers;

public static class UserServiceModelsMapper
{
    public static UserSummaryResponse ToSummaryDto(this User user)
    {
        return new UserSummaryResponse
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role,
            Status = user.Status.ToString(),
            DisplayName = user.DisplayName,
            ImageUrl = user.PictureUri,
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
            ImageUrl = user.PictureUri,
            LastModifiedAt = user.LastModifiedAt,
            RevokedAt = user.RevokedAt
        };
    }
}
