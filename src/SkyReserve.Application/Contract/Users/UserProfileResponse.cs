using Microsoft.AspNetCore.Http.HttpResults;

namespace Learnova.Business.DTOs.Contract.Users
{
    public record UserProfileResponse
     (
       // string Id,
     string Email,
     string UserName,
     string FirstName,
     string LastName,
     string phonenumber,
     DateTime CreatedAt


     );
}
