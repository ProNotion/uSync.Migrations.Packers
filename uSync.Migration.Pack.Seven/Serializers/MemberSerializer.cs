using System;
using System.IO;
using System.Linq;
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
        private readonly IMemberTypeService _memberTypeService;
        private readonly IDataTypeService _dataTypeService;

        public MemberSerializer()
        {
            _memberService = ApplicationContext.Current.Services.MemberService;
            _memberGroupService = ApplicationContext.Current.Services.MemberGroupService;
            _memberTypeService = ApplicationContext.Current.Services.MemberTypeService;
            _dataTypeService = ApplicationContext.Current.Services.DataTypeService;
        }

        public void SerializeMemberTypes(string folder)
        {
            var memberTypes = _memberTypeService.GetAll();

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            foreach (var memberType in memberTypes)
            {
                SerializeMemberTypeToFile(memberType, folder);
            }
        }

        private void SerializeMemberTypeToFile(IMemberType memberType, string folder)
        {
            var memberTypeElement = new XElement("MemberType",
                new XAttribute("Key", memberType.Key),
                new XAttribute("Alias", memberType.Alias),
                new XAttribute("Level", memberType.Level));

            var infoElement = new XElement("Info",
                new XElement("Name", memberType.Name),
                new XElement("Icon", memberType.Icon),
                new XElement("Thumbnail", memberType.Thumbnail),
                new XElement("Description", memberType.Description),
                new XElement("AllowAtRoot", memberType.AllowedAsRoot),
                new XElement("IsListView", memberType.IsContainer),
                new XElement("Variations", "Nothing"), // Default to "Nothing"
                new XElement("IsElement", false), // Default to false
                new XElement("Compositions"));

            memberTypeElement.Add(infoElement);

            var propertiesElement = new XElement("GenericProperties");
            foreach (var property in memberType.PropertyTypes)
            {
                var propertyGroupAlias = GetPropertyGroupAlias(memberType, property);

                var propertyElement = new XElement("GenericProperty",
                    new XElement("Key", property.Key),
                    new XElement("Name", property.Name),
                    new XElement("Alias", property.Alias),
                    new XElement("Type", GetPropertyTypeAlias(property.PropertyEditorAlias)),
                    new XElement("Mandatory", property.Mandatory),
                    new XElement("Validation", property.ValidationRegExp),
                    new XElement("Description", new XCData(property.Description)),
                    new XElement("SortOrder", property.SortOrder),
                    new XElement("Tab", new XAttribute("Alias", propertyGroupAlias ?? "unknown")),
                    new XElement("CanEdit", false),
                    new XElement("CanView", false),
                    new XElement("IsSensitive", false),
                    new XElement("MandatoryMessage", ""),
                    new XElement("ValidationRegExpMessage", ""),
                    new XElement("LabelOnTop", false));

                var dataType = _dataTypeService.GetDataTypeDefinitionById(property.DataTypeDefinitionId);
                if (dataType != null)
                {
                    propertyElement.Add(new XElement("Definition", dataType.Key));
                }

                propertiesElement.Add(propertyElement);
            }

            memberTypeElement.Add(propertiesElement);

            var tabsElement = new XElement("Tabs");
            foreach (var propertyGroup in memberType.PropertyGroups)
            {
                var tabElement = new XElement("Tab",
                    new XElement("Key", propertyGroup.Key),
                    new XElement("Caption", propertyGroup.Name),
                    new XElement("Alias", propertyGroup.Name.ToLowerInvariant().Replace(" ", "")),
                    new XElement("Type", "Group"),
                    new XElement("SortOrder", propertyGroup.SortOrder));

                tabsElement.Add(tabElement);
            }

            memberTypeElement.Add(tabsElement);

            memberTypeElement.Add(new XElement("Structure"));

            string filename = memberType.Alias.ToSafeFileName();
            string filePath = Path.Combine(folder, filename).EnsureEndsWith(".config");

            // Serialize to an XML file
            File.WriteAllText(filePath, memberTypeElement.ToString());
        }

        private string GetPropertyTypeAlias(string propertyPropertyEditorAlias)
        {
            switch (propertyPropertyEditorAlias)
            {
                // case "WiP.SecureCheckbox":
                //     return Constants.PropertyEditors.TrueFalseAlias;
                // case "WiP.Secure"
            }

            return propertyPropertyEditorAlias;
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

        public void SerializeMemberGroupsToFile(string folder)
        {
            var allGroups = _memberGroupService.GetAll();

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            foreach (var group in allGroups)
            {
                SerializeMemberGroupToFile(group, folder);
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

        public void SerializeMemberGroupToFile(IMemberGroup group, string folder)
        {
            var groupElement = new XElement("MemberGroup",
                new XAttribute("Key", group.Key),
                new XAttribute("Alias", group.Name.ToSafeAlias()),
                new XElement("Name", group.Name),
                new XElement("CreateDate", group.CreateDate.ToString("o")),
                new XElement("UpdateDate", group.UpdateDate.ToString("o"))
            );

            string filename = group.Name.ToSafeFileName();
            string filePath = Path.Combine(folder, filename).EnsureEndsWith(".config");

            File.WriteAllText(filePath, groupElement.ToString());
        }

        private string GetPropertyGroupAlias(IMemberType memberType, PropertyType property)
        {
            var propertyGroup =
                memberType.PropertyGroups.FirstOrDefault(pg => pg.PropertyTypes.Contains(property));
            var propertyGroupAlias = propertyGroup?.Name.ToLowerInvariant().Replace(" ", "");
            return propertyGroupAlias;
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