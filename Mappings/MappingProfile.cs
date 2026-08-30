using AutoMapper;
using BlazorAPIProject.DataAccess.Entities.User;
using BlazorAPIProject.Models.Commands.Users;
using BlazorAPIProject.Models.Responses.Users;

namespace BlazorAPIProject.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<AccountCommand, Account>();
            CreateMap<Account, AccountResponse>();

            CreateMap<RoleCommand, Role>();
            CreateMap<Role, RoleResponse>();

            CreateMap<AccountRoleCommand, AccountRole>();
            CreateMap<AccountRole, AccountRoleResponse>();

            CreateMap<TokenCommand, Token>();
            CreateMap<Token, TokenResponse>();
        }
    }
}
