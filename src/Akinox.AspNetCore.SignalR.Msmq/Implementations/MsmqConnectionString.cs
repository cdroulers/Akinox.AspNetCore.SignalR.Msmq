using System;
using System.Collections.Generic;
using System.Text;

namespace Akinox.AspNetCore.SignalR.Msmq.Implementations
{
    public class MsmqConnectionString
    {
        private const string FormatName = "FormatName:DIRECT=";

        public string Host { get; set; }

        public string Modifier { get; set; }

        public string QueueName { get; set; }

        public string DirectType { get; set; }

        public MsmqConnectionString(string host, string modifier, string queueName, string directType)
        {
            this.Host = host;
            this.Modifier = modifier;
            this.QueueName = queueName;
            this.DirectType = directType;
        }

        public static MsmqConnectionString Parse(string value)
        {
            string host = null, modifier = null, queueName = null, directType = null;
            if (value.StartsWith(FormatName))
            {
                value = value.Replace(FormatName, string.Empty);
                directType = value.Substring(0, value.IndexOf(":"));
                value = value.Substring(value.IndexOf(":") + 1);
            }

            var firstSlashIndex = value.IndexOf("\\");
            host = value.Substring(0, firstSlashIndex >= 0 ? firstSlashIndex : value.Length);
            if (firstSlashIndex >= 0)
            {
                value = value.Substring(firstSlashIndex + 1);
            }

            if (value.Length > 0)
            {
                firstSlashIndex = value.IndexOf("\\");
                var nextValue = value.Substring(0, firstSlashIndex >= 0 ? firstSlashIndex : value.Length);
                if (nextValue.Contains("$"))
                {
                    modifier = nextValue;
                }
                else
                {
                    queueName = nextValue;
                }

                if (firstSlashIndex >= 0)
                {
                    value = value.Substring(firstSlashIndex + 1);
                }
                else
                {
                    value = string.Empty;
                }
            }

            if (value.Length > 0)
            {
                firstSlashIndex = value.IndexOf("\\");
                queueName = value.Substring(0, firstSlashIndex >= 0 ? firstSlashIndex : value.Length);
            }

            return new MsmqConnectionString(host, modifier, queueName, directType);
        }

        public string Build()
        {
            var modifier = string.IsNullOrWhiteSpace(this.Modifier) ? string.Empty : "\\" + this.Modifier;
            var queueName = string.IsNullOrWhiteSpace(this.QueueName) ? string.Empty : "\\" + this.QueueName;
            var directType = string.IsNullOrWhiteSpace(this.DirectType) ? string.Empty : FormatName + this.DirectType + ":";
            return $"{directType}{this.Host}{modifier}{queueName}";
        }

        public MsmqConnectionString Clone()
        {
            return new MsmqConnectionString(this.Host, this.Modifier, this.QueueName, this.DirectType);
        }
    }
}
