using System.IO;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using File = System.IO.File;

namespace uSync.Migration.Pack.Seven.Serializers
{
    public class MemberSerializer
    {
        private readonly IMemberService _memberService;
        private readonly IMemberGroupService _memberGroupService;

        public MemberSerializer()
        {
            _memberService = ApplicationContext.Current.Services.MemberService;
            _memberGroupService = ApplicationContext.Current.Services.MemberGroupService;
        }

        public void SerializeMembersToFile(string folder)
        {
            var members = _memberService.GetAll(0, int.MaxValue, out _);
            
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            
            foreach (var member in members)
            {
                SerializeMemberToFile(member, folder);
            }
        }

        private void SerializeMemberToFile(IMember member, string folder)
        {
            if (member == null) return;

            var memberElement = new XElement("Member",
                new XElement("Key", member.Key),
                new XElement("Name", member.Name),
                new XElement("Username", member.Username),
                new XElement("Email", member.Email),
                new XElement("PasswordHash", member.RawPasswordValue),
                new XElement("PasswordSalt", ""), // You may need to handle password salt if used
                new XElement("IsApproved", member.IsApproved),
                new XElement("IsLockedOut", member.IsLockedOut),
                new XElement("LastLoginDate", member.LastLoginDate.ToString("o")),
                new XElement("LastPasswordChangeDate", member.LastPasswordChangeDate.ToString("o")),
                new XElement("LastLockoutDate", member.LastLockoutDate.ToString("o")),
                new XElement("FailedPasswordAttempts", member.FailedPasswordAttempts));

            var propertiesElement = new XElement("Properties");
            foreach (var property in member.Properties)
            {
                var propertyElement = new XElement("Property",
                    new XAttribute("alias", property.Alias),
                    new XElement("Value", property.Value));
                propertiesElement.Add(propertyElement);
            }

            var groupsElement = new XElement("Groups");
            var memberGroups = _memberService.GetAllRoles(member.Username);
            foreach (var group in memberGroups)
            {
                groupsElement.Add(new XElement("Group", group));
            }

            memberElement.Add(propertiesElement);
            memberElement.Add(groupsElement);
            
            string filename = member.Email.ToSafeFileName();
            string filePath = Path.Combine(folder, filename).EnsureEndsWith(".config");
            
            // Serialize to an XML file
            File.WriteAllText(filePath, memberElement.ToString());
        }

        public void SerializeUserGroupsToFile(string folder)
        {
            var allGroups = _memberGroupService.GetAll();
            foreach (var group in allGroups)
            {
                var groupElement = new XElement("UserGroup",
                    new XElement("Key", group.Key),
                    new XElement("Name", group.Name),
                    new XElement("CreateDate", group.CreateDate.ToString("o")),
                    new XElement("UpdateDate", group.UpdateDate.ToString("o")));
                
                var groupXml = groupElement.ToString();
                string filename = group.Name.ToSafeFileName();
                string filePath = Path.Combine(folder, filename).EnsureEndsWith(".config");
                
                File.WriteAllText(filePath, groupXml);
            }
        }
    }
}

// <?xml version="1.0" encoding="utf-8"?>
// <uSync>
//   <Member>
//     <Key>12345678-1234-1234-1234-1234567890ab</Key>
//     <Name>John Doe</Name>
//     <Username>john.doe@example.com</Username>
//     <Email>john.doe@example.com</Email>
//     <PasswordHash>hashedpasswordvalue</PasswordHash>
//     <PasswordSalt>saltvalue</PasswordSalt>
//     <IsApproved>true</IsApproved>
//     <IsLockedOut>false</IsLockedOut>
//     <LastLoginDate>2024-07-25T12:34:56</LastLoginDate>
//     <LastPasswordChangeDate>2024-06-25T12:34:56</LastPasswordChangeDate>
//     <LastLockoutDate>2024-05-25T12:34:56</LastLockoutDate>
//     <FailedPasswordAttempts>0</FailedPasswordAttempts>
//     <Properties>
//       <Property alias="firstName">
//         <Value>John</Value>
//       </Property>
//       <Property alias="lastName">
//         <Value>Doe</Value>
//       </Property>
//       <Property alias="address">
//         <Value>123 Main Street</Value>
//       </Property>
//       <!-- Add other custom properties here -->
//     </Properties>
//     <Groups>
//       <Group>Group1</Group>
//       <Group>Group2</Group>
//       <!-- Add other groups here -->
//     </Groups>
//   </Member>
// </uSync>