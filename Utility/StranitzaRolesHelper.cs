using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace stranitza.Utility
{
    public static class StranitzaRolesHelper
    {
        public const string AdministratorRoleName = "ADMINISTRATOR";
        public const string HeadEditorRoleName = "HEAD_EDITOR";
        public const string EditorRoleName = "EDITOR";
        public const string UserPlusRoleName = "USER_PLUS";
        //public const string UserRoleName = "USER";

        public const int AdministratorWeight = 1;        
        public const int HeadEditorWeight = 10;
        public const int EditorWeight = 15;        
        public const int UserPlusWeight = 25;        
        //public const int UserWeight = 50;
        
        /// <summary>
        /// Assigns weights to roles, the smaller the weight the broader the access rights.
        /// </summary>
        public static readonly Dictionary<int, string> KnownRoles = new Dictionary<int, string>()
        {
            {AdministratorWeight, AdministratorRoleName},
            {HeadEditorWeight, HeadEditorRoleName},
            {EditorWeight, EditorRoleName},
            {UserPlusWeight, UserPlusRoleName},
            //{UserWeight, UserRoleName}
        };

        public static string GetRoleName(StranitzaRoles role)
        {
            var roleWeight = (int) role;
            if (!KnownRoles.ContainsKey(roleWeight))
            {
                throw new StranitzaException(
                    $"Role '{role}' ({roleWeight}) is not part of the known roles for the application.");
            }

            return KnownRoles[roleWeight];
        }

        public static IEnumerable<string> GetRoleNamesAbove(StranitzaRoles role)
        {
            var roleWeight = (int) role;
            if (!KnownRoles.ContainsKey(roleWeight))
            {
                throw new StranitzaException(
                    $"Role '{role}' ({roleWeight}) is not part of the known roles for the application.");
            }

            return KnownRoles.Where(x => x.Key <= roleWeight).Select(x => x.Value);
        }

        public static bool Is(this ClaimsPrincipal user, StranitzaRoles role)
        {
            if (!user.Identity.IsAuthenticated)
            {
                // user is not authenticated
                // challenge him to log in
                return false;
            }

            var roleWeight = (int)role;
            if (!KnownRoles.ContainsKey(roleWeight))
            {
                throw new StranitzaException(
                    $"Role '{role}' ({roleWeight}) is not part of the known roles for the application.");
            }

            return user.IsInRole(KnownRoles[roleWeight]);
        }

        public static bool IsAtLeast(this ClaimsPrincipal user, StranitzaRoles role)
        {
            if (!user.Identity.IsAuthenticated)
            {
                // user is not authenticated
                // challenge him to log in
                return false;
            }

            var roleWeight = (int) role;
            if (!KnownRoles.ContainsKey(roleWeight))
            {
                throw new StranitzaException(
                    $"Role '{role}' ({roleWeight}) is not part of the known roles for the application.");
            }

            foreach (var knownRole in KnownRoles)
            {
                if (user.IsInRole(knownRole.Value))
                {
                    return knownRole.Key <= roleWeight;
                }
            }

            return false;
        }
    }
}