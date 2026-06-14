using Identity.Application.Dtos;
using Identity.Domain.Entities;

namespace Identity.Application.Abstractions;

public interface IJwtTokenProvider
{
    AccessTokenResult CreateAccessToken(Account account);
}
