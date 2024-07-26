using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace uSync.Migration.Pack.Seven.Models
{
    [XmlRoot(ElementName = "UserGroup")]
    public class UmbracoUserGroup
    {
        [XmlAttribute("Key")]
        public string Key { get; set; }

        [XmlAttribute("Alias")]
        public string Alias { get; set; }

        public UserGroupInfo Info { get; set; }

        public string AssignedPermissions { get; set; }
        
        public class UserGroupInfo
        {
            public string Sections { get; set; }
            public string Icon { get; set; }
            public string Name { get; set; }
            public string StartContentId { get; set; } = Guid.Empty.ToString(); // Assuming default value
            public string StartMediaId { get; set; } = Guid.Empty.ToString(); // Assuming default value
            public string Permission { get; set; }
        }
    }
}