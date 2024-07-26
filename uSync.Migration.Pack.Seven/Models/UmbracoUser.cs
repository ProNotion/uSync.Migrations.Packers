using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace uSync.Migration.Pack.Seven.Models
{
    [XmlRoot(ElementName = "User")]
    public class UmbracoUser
    {
        [XmlAttribute("Key")]
        public string Key { get; set; }

        [XmlAttribute("Alias")]
        public string Alias { get; set; }

        public UmbracoUserInfo Info { get; set; }

        public UmbracoUserGroups Groups { get; set; }
        
        public class UmbracoUserInfo
        {
            public string Comments { get; set; }
            public string Name { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public DateTime? EmailConfirmed { get; set; }
            public int FailedAttempts { get; set; }
            public bool Approved { get; set; }
            public bool LockedOut { get; set; }
            public string Language { get; set; }
            public string RawPassword { get; set; }
        }
    
        public class UmbracoUserGroups
        {
            [XmlElement("Group")]
            public List<UmbracoUserGroup> Group { get; set; } = new List<UmbracoUserGroup>();
        }
    
        public class UmbracoUserGroup
        {
            public string Alias { get; set; }
        }
    }
    
}