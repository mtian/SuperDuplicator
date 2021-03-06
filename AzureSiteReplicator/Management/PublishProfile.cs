﻿using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using AzureSiteReplicator.Utils;

namespace AzureSiteReplicator.Management
{
    public class PublishProfile
    {
        public PublishProfile()
        {
            Current = this;
        }

        public static PublishProfile Current { get; private set; }

        // SchemaVersion="2.0"
        [XmlAttribute]
        public string SchemaVersion { get; set; }

        [XmlAttribute] 
        public string PublishMethod { get; set; }

        [XmlAttribute] 
        public string Url { get; set; }

        [XmlAttribute]
        public string ManagementCertificate { get; set; }
        
        [XmlAttribute]
        public string ManagementCertificatePassword { get; set; }

        [XmlElement(ElementName = "Subscription")]
        public Subscription[] Subscriptions { get; set; }

        internal string GetSubscriptionId()
        {
            return this.Subscriptions[0].Id;
        }

        internal string GetUrl()
        {
            return (this.Url ?? this.Subscriptions[0].ServiceManagementUrl).TrimEnd('/');
        }

        X509Certificate2 _certificate;
        
        internal X509Certificate2 Certificate
        {
            get
            {
                if (_certificate == null)
                {
                    _certificate = new X509Certificate2(
                        Convert.FromBase64String(this.ManagementCertificate ?? this.Subscriptions[0].ManagementCertificate), 
                        this.ManagementCertificatePassword ?? this.Subscriptions[0].ManagementCertificatePassword
                    );
                }

                return _certificate;
            }

            set
            {
                _certificate = value;
            }
        }
        
        public class Subscription
        {
            [XmlAttribute]
            public string Id { get; set; }

            [XmlAttribute]
            public string Name { get; set; }

            [XmlAttribute]
            public string ServiceManagementUrl { get; set; }

            [XmlAttribute]
            public string ManagementCertificate { get; set; }

            [XmlAttribute]
            public string ManagementCertificatePassword { get; set; }
        }
    }
}
