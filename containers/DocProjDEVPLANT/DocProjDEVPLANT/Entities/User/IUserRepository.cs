namespace DocProjDEVPLANT.Entities.User;

public interface IUserRepository
{
    public Task<List<UserModel>> GetAllUsersAsync();
}