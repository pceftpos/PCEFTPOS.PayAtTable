using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace PayAtTable.API.Helpers
{
    public class ApiKeyHandler : DelegatingHandler
    {
        public ApiKeyHandler(string keyParam)
        {
            this.KeyParam = keyParam;
            this.Keys = new List<string>();
            ReloadKeys();
        }

        /// <summary>
        /// Reload our list of valid keys. This could come from a file or a database
        /// </summary>
        protected void ReloadKeys()
        {
            Keys.Clear();
            Keys.AddRange(
                new string[] 
                { 
                    "ececb2bf-3309-4b71-a723-45a582b92af1",
                    "6fd721a7-4377-4645-9386-6faff7e27787",
                    "17f8fb35-1547-4584-851c-6884ef89c1fe",
                    "f642489d-602e-4bb1-a898-baf6a160c999",
                    "7728c5cd-5c69-449b-b4f7-573e0b66ffd9",
                    "2f1cc887-6f3a-44b8-ba00-a67aebb80c50",
                    "517a864d-d202-4b8f-ba4c-ce093dde08a2",
                    "e0eea45d-ae30-4226-8d9f-efc2fbc7f164",
                    "d0b2b8a7-645e-44f8-be05-05ea08c89869",
                    "3c83d890-2cd4-414f-9f72-30ef3fce99f0"
                });
            Keys.Sort();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!ValidateKey(request))
            {
                var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                var tsc = new TaskCompletionSource<HttpResponseMessage>();
                tsc.SetResult(response);
                return tsc.Task;
            }
            return base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Validate the "KeyParam" parameter of a request against our list of valid keys
        /// </summary>
        /// <param name="message">The request message</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateKey(HttpRequestMessage message)
        {
            // Extract our query params
            var query = message.RequestUri.ParseQueryString();
            if (query == null || query.Count == 0)
                return false;
            // Extract our key parameter and validate
            var key = query[KeyParam];
            if (key == null)
                return false;
            return (Keys.BinarySearch(key) >= 0);
        }

        public string KeyParam { get; set; }
        public List<string> Keys { get; set; }
    }
}