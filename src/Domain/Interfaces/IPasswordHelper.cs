namespace Domain.Interfaces;

public interface IPasswordHelper
{
    string GetPasswordHash(string password);

    bool VerifyPasswordHash(string password, string hashedPassword);
}
