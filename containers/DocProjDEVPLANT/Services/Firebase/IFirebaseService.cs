using FirebaseAdmin.Auth;

namespace DocProjDEVPLANT.Services.Firebase;

public interface IFirebaseService
{
    Task<FirebaseToken> VerifyIdTokenAsync(string idToken);
}