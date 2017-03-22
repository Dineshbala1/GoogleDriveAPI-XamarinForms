using Google.Apis.Drive.v2;

namespace XamarinFormsGoogleDriveAPI.Services
{
    public static class Constants
    {
        public static string ClientId = "Your Client Id";
        public static string ClientSecret = "Your Client secret";
        public static string AuthorizationUri = "https://accounts.google.com/o/oauth2/auth";
        public static string AccessTokenUri = "https://www.googleapis.com/oauth2/v4/token";
        public static string UserInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
        public static string RedirectUri = "https://www.googleapis.com/plus/v1/people/me";

        public static string Scopes = DriveService.Scope.DriveAppdata + " " + DriveService.Scope.DriveFile + " " +
                                      DriveService.Scope.Drive + " " + "https://www.googleapis.com/auth/userinfo.email";

        public static string ApplicationName = "Your Applicationname";
    }
}
