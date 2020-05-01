/*
==============================================================================
Copyright © Jason Drawdy 

All rights reserved.

The MIT License (MIT)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

Except as contained in this notice, the name of the above copyright holder
shall not be used in advertising or otherwise to promote the sale, use or
other dealings in this Software without prior written authorization.
==============================================================================
*/

#region Imports

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

#endregion
namespace Amaterasu
{
    public class NetRequest
    {
        #region Variables

        /// <summary>
        /// The type of request that will be made to a given address.
        /// </summary>
        public enum RequestType { GET, POST }
        private WebRequest request;
        private Stream dataStream;
        private string status;
        public string Status
        {
            get { return status; }
            set { status = value; }
        }

        #endregion
        #region Initialization

        /// <summary>
        /// Creates a web request using a URL that can proces a get and post requests.
        /// </summary>
        /// <param name="url">The address that a request will be made to.</param>
        public NetRequest(string url)
        {
            request = WebRequest.Create(url);
        }
        /// <summary>
        /// Creates a web request using a URL that can proces a get and post requests.
        /// </summary>
        /// <param name="url">The address that a request will be mdade to.</param>
        /// <param name="method">The tyoe of request to be made.</param>
        public NetRequest(string url, RequestType method)
            : this(url)
        {
            // Set our web request type.
            switch(method)
            {
                case RequestType.GET:
                    request.Method = "GET";
                    break;
                case RequestType.POST:
                    request.Method = "POST";
                    break;
            }
        }

        /// <summary>
        /// Creates a web request using a URL that can proces a get and post requests.
        /// </summary>
        /// <param name="url">The address that a request will be mdade to.</param>
        /// <param name="method">The tyoe of request to be made.</param>
        /// <param name="data">The data to be sent in the request.</param>
        public NetRequest(string url, RequestType method, string data)
            : this(url, method)
        {
            // Create POST data and convert it to a byte array.
            string postData = data;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/x-www-form-urlencoded";

            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;

            // Get the request stream.
            dataStream = request.GetRequestStream();

            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);

            // Close the Stream object.
            dataStream.Close();
        }

        #endregion
        #region Methods

        /// <summary>
        /// Obtains a response from the inital web request.
        /// </summary>
        public string GetResponse()
        {
            // Get the original response.
            WebResponse response = request.GetResponse();

            Status = ((HttpWebResponse)response).StatusDescription;

            // Get the stream containing all content returned by the requested server.
            dataStream = response.GetResponseStream();

            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);

            // Read the content fully up to the end.
            string responseFromServer = reader.ReadToEnd();

            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();

            return responseFromServer;
        }

        #endregion
    }
}
