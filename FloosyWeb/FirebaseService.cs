using Blazored.LocalStorage;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Database;
using Firebase.Database.Query;
using FloosyWeb.Models;
using System.Collections.ObjectModel;

namespace FloosyWeb;

public class FirebaseService
{
    string ApiKey = "AIzaSyCXDVWqko25EXCISbg3r6M8auh_t3Juq1U";
    string DbUrl = "https://mycloudwallet-d787d-default-rtdb.europe-west1.firebasedatabase.app/";
    string AuthDomain = "mycloudwallet-d787d.firebaseapp.com";

    FirebaseAuthClient auth;
    FirebaseClient db;
    ISyncLocalStorageService _localStorage;

    public string? CurrentUserId { get; private set; }
    public string? UserEmail { get; private set; }

    public FirebaseService(ISyncLocalStorageService localStorage)
    {
        _localStorage = localStorage;
        var config = new FirebaseAuthConfig
        {
            ApiKey = ApiKey,
            AuthDomain = AuthDomain,
            Providers = new FirebaseAuthProvider[] { new EmailProvider() }
        };
        auth = new FirebaseAuthClient(config);
        db = new FirebaseClient(DbUrl);
        CheckPreviousLogin();
    }

    private void CheckPreviousLogin()
    {
        var savedId = _localStorage.GetItem<string>("UserId");
        var savedEmail = _localStorage.GetItem<string>("UserEmail");
        if (!string.IsNullOrEmpty(savedId)) { CurrentUserId = savedId; UserEmail = savedEmail; }
    }

    public async Task<string> Login(string email, string pass)
    {
        try
        {
            var userCred = await auth.SignInWithEmailAndPasswordAsync(email, pass);
            SetUser(userCred.User.Uid, email);
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
            // Create Default Wallet
            await Save(new AppData { Accounts = new ObservableCollection<Account> { new Account { Name = "Cash", Balance = 0 } } });
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
        _localStorage.RemoveItem("UserId"); _localStorage.RemoveItem("UserEmail");
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

    public async Task<string> DeleteAccount()
    {
        if (auth?.User == null) return "Not Logged In";
        try
        {
            if (CurrentUserId != null) await db.Child("Users").Child(CurrentUserId).DeleteAsync();
            await auth.User.DeleteAsync();
            return "Success";
        }
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
            var newD = new AppData(); newD.Accounts.Add(new Account { Name = "Cash", Balance = 0 }); return newD;
        }
        return data;
    }

    public string GetUserName() => auth?.User?.Info?.DisplayName ?? "User";

    private string GetFriendlyError(Exception ex)
    {
        string msg = ex.Message;
        if (msg.Contains("EMAIL_NOT_FOUND")) return "Email not found.";
        if (msg.Contains("INVALID_PASSWORD")) return "Wrong password.";
        if (msg.Contains("EMAIL_EXISTS")) return "Email already exists.";
        return "Error: " + msg;
    }

    // ... (باقي الكود فوق)

    // ضيف الدالة دي عشان تغيير الاسم يشتغل
    public async Task UpdateUserName(string newName)
    {
        if (auth?.User != null)
        {
            await auth.User.ChangeDisplayNameAsync(newName);
        }
    }
}
