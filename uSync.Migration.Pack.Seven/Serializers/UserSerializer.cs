using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using uSync.Migration.Pack.Seven.Models;

namespace uSync.Migration.Pack.Seven.Serializers
{
    public class UserSerializer
    {
        private readonly IUserService _userService;

        public UserSerializer(IUserService userService)
        {
            _userService = userService;
        }

        public void SerializeUsersToFile(string folder)
        {
            var users = _userService.GetAll(0, int.MaxValue, out _);
            
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            
            foreach (var user in users)
            {
                SerializeUserToFile(user, folder);
            }
        }

        private void SerializeUserToFile(IUser user, string folder)
        {

            if (user == null) return;
            
            // Map the Umbraco user to the UmbracoUser class
            UmbracoUser userXml = new UmbracoUser
            {
                Key = user.Key.ToString(),
                Alias = user.Username,
                Info = new UmbracoUser.UmbracoUserInfo
                {
                    Name = user.Name,
                    Username = user.Username,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmedDate,
                    Approved = user.IsApproved,
                    Comments = user.Comments,
                    Language = user.Language,
                    FailedAttempts = user.FailedPasswordAttempts,
                    LockedOut = user.IsLockedOut,
                    RawPassword = user.RawPasswordValue
                },
                Groups = new UmbracoUser.UmbracoUserGroups()
            };

            foreach (var group in user.Groups)
            {
                userXml.Groups.Group.Add(new UmbracoUser.UmbracoUserGroup { Alias = group.Name });
            }
            
            string filename = user.Email.ToSafeFileName();
            string filePath = Path.Combine(folder, filename).EnsureEndsWith(".config");
            
            // Serialize to an XML file
            XmlSerializer serializer = new XmlSerializer(typeof(UmbracoUser));
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, userXml);
            }
        }

        public void SerializeUserGroupsToFile(string folder)
        {
            IEnumerable<IUserGroup> userGroups = _userService.GetAllUserGroups();
            
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            
            foreach (var userGroup in userGroups)
            {
                SerializeUserGroupToFile(userGroup.Id, folder);
            }
        }

        /// <summary>
        /// Serializes a user group to file in the specified folder.
        /// </summary>
        /// <param name="userGroupId">The ID of the user group to serialize.</param>
        /// <param name="folder">The folder where the serialized file should be saved.</param>
        private void SerializeUserGroupToFile(int userGroupId, string folder)
        {
            // Fetch user group from Umbraco (assuming you have access to UserService)
            IUserService userService = ApplicationContext.Current.Services.UserService;
            IUserGroup userGroup = userService.GetUserGroupById(userGroupId);

            if (userGroup == null) return;
            
            // Map the Umbraco user group to the UserGroupXml class
            UmbracoUserGroup userGroupXml = new UmbracoUserGroup
            {
                Key = userGroup.Key.ToString(),
                Alias = userGroup.Alias,
                Info = new UmbracoUserGroup.UserGroupInfo
                {
                    Sections = string.Join(",", userGroup.AllowedSections),
                    Icon = userGroup.Icon,
                    Name = userGroup.Name,
                    Permission = string.Join(",", userGroup.Permissions)
                }
            };
                
            string filename = userGroup.Name.ToSafeFileName();
            string filePath = Path.Combine(folder, filename).EnsureEndsWith(".config");

            // Serialize to XML
            XmlSerializer serializer = new XmlSerializer(typeof(UmbracoUserGroup));
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, userGroupXml);
            }
        }
    }
}

// <?xml version="1.0" encoding="utf-8"?>
// <User Key="ffffffff-0000-0000-0000-000000000000" Alias="simon@prolificnotion.co.uk">
//   <Info>
//     <Comments />
//     <Name>Tony Stark</Name>
//     <Username>someone@example.com</Username>
//     <Email>someone@example.com</Email>
//     <EmailConfirmed />
//     <FailedAttempts>0</FailedAttempts>
//     <Approved>true</Approved>
//     <LockedOut>false</LockedOut>
//     <Language>en-GB</Language>
//     <RawPassword>Zp4vfKY68X7R7r8h9m4yXlTQXJY=</RawPassword>
//   </Info>
//   <Groups>
//     <Group>
//       <Alias>admin</Alias>
//     </Group>
//   </Groups>
// </User>