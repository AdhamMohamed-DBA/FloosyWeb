using Blazored.LocalStorage;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Database;
using Firebase.Database.Query;
using FloosyWeb.Models;

namespace FloosyWeb;

public class FirebaseService
{
    private readonly string ApiKey = "AIzaSyCXDVWqko25EXCISbg3r6M8auh_t3Juq1U";
    private readonly string DbUrl = "https://mycloudwallet-d787d-default-rtdb.europe-west1.firebasedatabase.app/";
    private readonly string AuthDomain = "mycloudwallet-d787d.firebaseapp.com";

    private readonly FirebaseAuthClient auth;
    private readonly FirebaseClient db;
    private readonly ISyncLocalStorageService _localStorage;

    public string? CurrentUserId { get; private set; }
    public string? UserEmail { get; private set; }

    public FirebaseService(ISyncLocalStorageService localStorage)
    {
        _localStorage = localStorage;
        var config = new FirebaseAuthConfig
        {
            ApiKey = ApiKey,
            AuthDomain = AuthDomain,
            Providers = [new EmailProvider()]
        };
        auth = new FirebaseAuthClient(config);
        db = new FirebaseClient(DbUrl);
    }

    public void InitUser()
    {
        try
        {
            var savedId = _localStorage.GetItem<string>("UserId");
            var savedEmail = _localStorage.GetItem<string>("UserEmail");
            if (!string.IsNullOrEmpty(savedId)) { CurrentUserId = savedId; UserEmail = savedEmail; }
        }
        catch { }
    }

    public async Task<string> Login(string email, string pass)
    {
        try
        {
            var userCred = await auth.SignInWithEmailAndPasswordAsync(email, pass);
            SetUser(userCred.User.Uid, email);

            // 🔥 Save Name on Login if exists
            if (!string.IsNullOrEmpty(userCred.User.Info.DisplayName))
            {
                _localStorage.SetItem("UserName", userCred.User.Info.DisplayName);
            }

            return "Success";
        }
        catch (Exception ex) { return GetFriendlyError(ex); }
    }

    public async Task<string> Register(string name, string email, string pass)
    {
        try
        {
            var userCred = await auth.CreateUserWithEmailAndPasswordAsync(email, pass);
            await userCred.User.ChangeDisplayNameAsync(name);
            SetUser(userCred.User.Uid, email);

            // 🔥 Save Name on Register
            _localStorage.SetItem("UserName", name);

            await Save(new AppData { Accounts = [new Account { Name = "Cash", Balance = 0 }] });
            return "Success";
        }
        catch (Exception ex) { return GetFriendlyError(ex); }
    }

    void SetUser(string uid, string email)
    {
        CurrentUserId = uid; UserEmail = email;
        _localStorage.SetItem("UserId", uid); _localStorage.SetItem("UserEmail", email);
    }

    public void SignOut()
    {
        auth?.SignOut();
        CurrentUserId = null; UserEmail = null;
        _localStorage.RemoveItem("UserId");
        _localStorage.RemoveItem("UserEmail");
        _localStorage.RemoveItem("UserName"); // 🔥 Clear Name
    }

    public async Task<string> ResetPassword(string email)
    {
        try { await auth.ResetEmailPasswordAsync(email); return "Success"; }
        catch (Exception ex) { return GetFriendlyError(ex); }
    }

    public async Task<string> ChangePassword(string newPass)
    {
        if (auth?.User == null) return "Not Logged In";
        try { await auth.User.ChangePasswordAsync(newPass); return "Success"; }
        catch (Exception ex) { return GetFriendlyError(ex); }
    }

    public async Task Save(AppData data)
    {
        if (string.IsNullOrEmpty(CurrentUserId)) return;
        await db.Child("Users").Child(CurrentUserId).PutAsync(data);
    }

    public async Task<AppData> Load()
    {
        if (string.IsNullOrEmpty(CurrentUserId)) return new AppData();
        var data = await db.Child("Users").Child(CurrentUserId).OnceSingleAsync<AppData>();
        if (data == null)
        {
            var newD = new AppData();
            newD.Accounts.Add(new Account { Name = "Cash", Balance = 0 });
            return newD;
        }
        return data;
    }

    // 🔥 FIX: Get Name from LocalStorage first (Faster & Stable)
    public string GetUserName()
    {
        var localName = _localStorage.GetItem<string>("UserName");
        if (!string.IsNullOrEmpty(localName)) return localName;

        return auth?.User?.Info?.DisplayName ?? "User";
    }

    // 🔥 FIX: Save Name to LocalStorage immediately on update
    public async Task UpdateUserName(string newName)
    {
        if (auth?.User != null)
        {
            await auth.User.ChangeDisplayNameAsync(newName);
            _localStorage.SetItem("UserName", newName);
        }
    }

    private static string GetFriendlyError(Exception ex)
    {
        string msg = ex.Message;
        if (msg.Contains("EMAIL_NOT_FOUND") || msg.Contains("INVALID_EMAIL")) return "Invalid Email.";
        if (msg.Contains("INVALID_PASSWORD") || msg.Contains("INVALID_LOGIN_CREDENTIALS")) return "Wrong Password.";
        if (msg.Contains("EMAIL_EXISTS")) return "Email already exists.";
        return "Check credentials or internet.";
    }
}