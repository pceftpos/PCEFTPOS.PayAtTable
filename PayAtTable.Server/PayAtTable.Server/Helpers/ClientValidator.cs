using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Web;

namespace PayAtTable.Server.Helpers
{
    public interface IClientValidator
    {
        bool Validate(X509Certificate certificate, AuthenticationHeaderValue header, out string message);
    }

    public class ClientValidator : IClientValidator
    {
        bool ValidateCertificate(X509Certificate certificate, out string message)
        {
            message = string.Empty;

            var validate = ConfigurationManager.AppSettings["ValidateClientCert"];
            if (validate?.Equals("1") == false)
                return true;

            var clientCACertFile = ConfigurationManager.AppSettings["ClientCACertFilePath"];
            if (clientCACertFile?.Length == 0)
                return true;

            if (certificate == null)
            {
                message = "No certificates";
                return false;
            }

            try
            {
                if (!File.Exists(clientCACertFile))
                {
                    message = "Certificate file doesn't exist.";
                    return false;
                }

                // validate
                var ca = new X509Certificate(clientCACertFile);
                if (ca.Subject.Equals(certificate.Issuer) && ca.GetCertHashString().Equals(certificate.GetCertHashString()))
                    return true; // valid

                message = "Invalid certificate";
                return false;
            }
            catch (Exception ex)            
            {
                message = ex.Message;
                return false;
            }
        }

        bool ValidateAuthHeader(AuthenticationHeaderValue header, out string message)
        {
            message = string.Empty;
            var validate = ConfigurationManager.AppSettings["ValidateClientAuthHeader"];
            if (validate?.Equals("1") == false)
                return true;

            if (header == null)
            {
                message = "Unknown authorization";
                return false;
            }

            var clientAuthHeader = ConfigurationManager.AppSettings["ClientAuthHeader"];
            if (clientAuthHeader?.Length == 0)
                return true;

            if (clientAuthHeader.Equals(header.ToString()))
                return true;

            message = "Unknown authorization";
            return false;
        }

        public bool Validate(X509Certificate certificate, AuthenticationHeaderValue header, out string message)
        {
            return (ValidateCertificate(certificate, out message) && ValidateAuthHeader(header, out message));
                
        }
    }
}