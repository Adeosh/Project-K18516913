using Krepim.Identity.Domain.Aggregates;

namespace Krepim.Identity.Application.Interfaces
{
    public interface IJwtProvider
    {
        string Generate(User user);
    }
}
