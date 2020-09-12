using System.ComponentModel.DataAnnotations;

namespace stranitza.Utility
{
    public static class StranitzaConstants
    {
        public const int DefaultCoverPageNumber = 1;
        public const int DefaultIndexPageNumber = 3;

        public const string CyrillicNamePattern = "^[\\u0400-\\u04FF][\\u0400-\\u04FF-]+[\\u0400-\\u04FF]$";

        public const string AllowedUserNameLatinCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string AllowedUserNameCyrillicCharacters = "абвгдежзийклмнопрстуфхцчшщъьюяАБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЬЮЯ";
        public const string AllowedUserNameNumberCharacters = "0123456789";
        public const string AllowedUserNameSpecialCharacters = "-._@ ";

        public const string AllowedUserNameCharacters = AllowedUserNameCyrillicCharacters +
                                                        AllowedUserNameLatinCharacters +
                                                        AllowedUserNameNumberCharacters +
                                                        AllowedUserNameSpecialCharacters;

        public const string AdministratorEmail = "panayot_zhi@abv.bg";       // TODO: admin@stranitza.eu
        public const string AdministratorUsername = "p.ivanov";
        public const string AdministratorFirstName = "Панайот";
        public const string AdministratorLastName = "Иванов";

        public const string HeadEditorEmail = "m_vlashki@abv.bg";      // TODO: editor@stranitza.eu
        public const string HeadEditorUsername = "m.vlashki";
        public const string HeadEditorFirstName = "Младен";
        public const string HeadEditorLastName = "Влашки";

        public const string ForbiddenPagePdfFileName = "forbidden-page.pdf";
        public const string IndexJsonFileName = "stranitza-index.json";
        public const string ThumbnailsFolderName = "thumb";        
        public const string IssuesFolderName = "issues";
        public const string UploadsFolderName = "uploads";

        public const int AvatarMaxSize = 163840;

        public const string PdfAuthor = "издава „Сдружение „Литературна къща“ — Пловдив";
        public const string PdfSubject = "Списание Страница";
        public const string PdfKeywords = "ISSN 1310—9081";

    }

    public static class CountQueryType
    {
        public const int Sources = 1;
        public const int EPages = 2;
        public const int Issues = 3;
        public const int AvailableIssues = 4;
    }

    public static class StranitzaCacheProfile
    {
        public const string NoCache = "NoCache";
        public const string Hourly = "Hourly";
        public const string Weekly = "Weekly";
        public const string Monthly = "Monthly";
        public const string Yearly = "Yearly";
    }

    /// <summary>
    /// From System.Security.Claims
    /// </summary>
    public class StranitzaClaimTypes
    {
        //
        // From System.IdentityModel.Claims
        //
        private const string ClaimType2005Namespace = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims";

        public const string Picture = ClaimType2005Namespace + "/picture";
        public const string VerifiedEmail = ClaimType2005Namespace + "/verified_email";
    }

    public enum StranitzaRoles
    {
        Administrator = StranitzaRolesHelper.AdministratorWeight,
        HeadEditor = StranitzaRolesHelper.HeadEditorWeight,
        Editor = StranitzaRolesHelper.EditorWeight,
        UserPlus = StranitzaRolesHelper.UserPlusWeight,
        //User = StranitzaRolesHelper.UserWeight
    }

    public enum StranitzaPageType
    {
        Regular,

        Index,
        Cover,

        Missing,
        Forbidden
    }

    public enum StranitzaAvatarType
    {
        Default,    // Anonymous

        Gravatar,

        Facebook,
        Twitter,
        Google,

        Internal
    }

    public enum StranitzaDisplayNameType
    {
        [Display(Name = "Псевдоним")]
        UserName,

        [Display(Name = "Имена")]
        Names,

        [Display(Name = "Имена и псевдоним")]
        NamesAndUserName,

        [Display(Name = "Да се показва само 'Анонимен'")]
        Anonymous
    }

    public enum ModalMessageType
    {
        Info,

        Warning,
        Danger,
        Success
    }

    public enum SortOrder
    {
        Unknown,

        Asc,
        Desc,

    }
}
