using FirebaseAdmin.Auth;

namespace DocProjDEVPLANT.Services.Firebase;

public class FirebaseService : IFirebaseService
{
    public FirebaseService()
    {
        
    }

    public async Task<FirebaseToken> VerifyIdTokenAsync(string idToken)
    {
        FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
        return decodedToken;
    }
}