/*
==============================================================================
Copyright © Jason Drawdy (CloneMerge)

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Reflection;

#endregion
namespace Amaterasu
{
    /// <summary>
    /// Provides access to tools for verifying and registering generated license files using PHP scripts.
    /// </summary>
    public class Licensing
    {
        #region Variables

        private string _verificationAddress { get; set; }
       
        private string _supportAddress { get; set; }

        /// <summary>
        /// The address to a script which will be used for validating and chunking data.
        /// </summary>
        public string VerificationAddress
        {
            get { return _verificationAddress; }
            set { UpdateVerificationAddress(value); }
        }
        /// <summary>
        /// The address to a script which will be used for updating and activating a license.
        /// </summary>
        public string SupportAddress
        {
            get { return _supportAddress; }
            set { UpdateSupportAddress(value); }
        }

        /// <summary>
        /// Collection of script addresses for each method within the type.
        /// </summary>
        Dictionary<string, WebValidationAttribute> scripts = new Dictionary<string, WebValidationAttribute>();

        #endregion
        #region Initialization

        /// <summary>
        /// Allows verification of generated license files using web scripts.
        /// </summary>
        public Licensing()
        {
            EnumerateAttributes();
        }

        /// <summary>
        /// Allows verification of generated license files using web scripts.
        /// </summary>
        /// <param name="verificationURL">Web address pointing to a script used for validating a public key.</param>
        /// <param name="supportURL">Web address pointing to a script used for updating and activating licenses.</param>
        public Licensing(string verificationAddress, string supportAddress)
        {
            UpdateVerificationAddress(verificationAddress);
            UpdateSupportAddress(supportAddress);
            EnumerateAttributes();
        }

        #endregion
        #region Methods

        /// <summary>
        /// Registers a license with an authentication server using the provided license information.
        /// </summary>
        /// <param name="key">The XML public key of a <see cref="RSACryptoServiceProvider"/> to use for authentication.</param>
        /// <param name="username">The account username used for generating the license.</param>
        /// <param name="password">The account password used for generating the license.</param>
        /// <param name="license">The license file </param>
        /// <returns></returns>
        public bool ActivateLicense(string key, string username, string password, string license)
        {   
            // Check the system for blacklisted files and remove them.
            var system = new LocalSystem();
            system.CheckSystem();

            // Check if we have a valid key.
            if (IsKeyValid(key))
            {
                // Setup a new RSA object using our public key.
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(key);
                try
                {
                    // Decrypt our license using our public key.
                    byte[] decrypted = rsa.Decrypt(license.GetBytes(), false);
                    string[] contents = decrypted.GetString().Split('|');

                    // Check how many items we have in the array.
                    if (contents.Length == 7)
                    {
                        // Check if we have a proper email.
                        if (contents[0].Length > 7 && contents[0].Contains("@"))
                        {
                            // Check if we have a proper password length.
                            if (contents[2].Length == 40)
                            {
                                // Check if we have a proper checksum length.
                                if (contents[6].Length == 40)
                                {
                                    // Check if the license has expired yet.
                                    if (!IsLicenseExpired(contents[3]))
                                    {
                                        // Generate a new checksum for the given password and compare to our license.
                                        Cryptography.Hashing h = new Cryptography.Hashing();
                                        string checksum = h.ComputeMessageHash(password, Cryptography.Hashing.HashType.SHA1);
                                        if (checksum.ToUpper() == contents[2])
                                        {
                                            // Generate a new license checksum and compare to the provided digest.
                                            checksum = h.ComputeMessageHash(contents[0] + username + checksum.ToUpper() + contents[3] +
                                                                            contents[4] + contents[5], Cryptography.Hashing.HashType.SHA1);
                                            if (checksum.ToUpper() == contents[6])
                                            {
                                                // Check if the license exists in our database.
                                                if (IsChecksumReal(key, checksum))
                                                {
                                                    // Check if we're allowed to activate the license.
                                                    if (IsChecksumSupported(key, checksum))
                                                    {
                                                        // Activate the application using our license file.
                                                        if (RegisterLicense(key, checksum, contents[3]))
                                                            return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else { }
                }
                catch { }
            }
            return false; // The license is not valid and couldn't be activated.
        }
        
        /// <summary>
        /// Checks if the timestamp of a license is expired.
        /// </summary>
        /// <param name="timestamp">The timestamp of the license file.</param>
        public bool IsLicenseExpired(string timestamp)
        {
            DateTime past = DateTime.Parse(timestamp);
            if (past < DateTime.Now)
                return false;
            return true;
        }

        /// <summary>
        /// Checks if the license exists within our database.
        /// </summary>
        /// <param name="key">The <see cref="RSACryptoServiceProvider"/> public key to use for encryption.</param>
        /// <param name="checksum">The checksum of the license file to check.</param>
        /// <param name="parameter">The parameter to be used when sending the request to the remote PHP script.</param>
        [WebValidation(ScriptType.Support)]
        public bool IsChecksumReal(string key, string checksum, string parameter = "isReal")
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(key);
                string data = rsa.Encrypt(checksum.GetBytes(), false).GetString();
                NetRequest query = new NetRequest(scripts[nameof(IsChecksumReal)].Address, NetRequest.RequestType.POST, (parameter + "=" + data));
                return bool.Parse(query.GetResponse());
            }
            catch (Exception ex) { throw ex; }
        }

        /// <summary>
        /// Checks if the license is allowed to be activated.
        /// </summary>
        /// <param name="key">The <see cref="RSACryptoServiceProvider"/> public key to use for encryption.</param>
        /// <param name="checksum">The checksum of the license file to check.</param>
        /// <param name="parameter">The parameter to be used when sending the request to the remote PHP script.</param>
        [WebValidation(ScriptType.Support)]
        public bool IsChecksumSupported(string key, string checksum, string parameter = "isSupported")
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(key);
                string data = rsa.Encrypt(checksum.GetBytes(), false).GetString();
                NetRequest query = new NetRequest(scripts[nameof(IsChecksumSupported)].Address, NetRequest.RequestType.POST, (parameter + "=" + data));
                return bool.Parse(query.GetResponse());
            }
            catch (Exception ex) { throw ex; }
        }

        /// <summary>
        /// Registers a license through a remote address.
        /// </summary>
        /// <param name="key">The <see cref="RSACryptoServiceProvider"/> public key to use for encryption.</param>
        /// <param name="checksum">The checksum of the license file to register.</param>
        /// <param name="serial">The serial number of the license file to register.</param>
        /// <param name="parameter">The parameter to be used when sending the request to the remote PHP script.</param>
        [WebValidation(ScriptType.Support)]
        private bool RegisterLicense(string key, string checksum, string serial, string parameter = "licenseData")
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(key);
                string data = rsa.Encrypt((checksum + "|" + serial).GetBytes(), false).GetString();
                NetRequest query = new NetRequest(scripts[nameof(RegisterLicense)].Address, NetRequest.RequestType.POST, (parameter + "=" + data));
                return bool.Parse(query.GetResponse());
            }
            catch (Exception ex) { throw ex; }
        }

        /// <summary>
        /// Checks if the provided <see cref="RSACryptoServiceProvider"/> public key is valid by trying to decrypt data from a remote address.
        /// </summary>
        /// <param name="key">The <see cref="RSACryptoServiceProvider"/> public key to validate.</param>
        /// <param name="parameter">The parameter to be used when sending the request to the remote PHP script.</param>
        /// <param name="data">The data to be sent along with the parameter to the remote PHP script.</param>
        [WebValidation(ScriptType.Validation)]
        public bool IsKeyValid(string key, string parameter = "ping", string data = "pong")
        {
            // Play ping-pong to get a score.
            NetRequest game = new NetRequest(scripts[nameof(IsKeyValid)].Address, NetRequest.RequestType.POST, (parameter + "=" + data));
            string score = game.GetResponse();

            // Try to decrypt our score.
            if (DecryptData(key, Encoding.ASCII.GetBytes(score)))
                return true; // The key is good... obviously...
            return false; // The key could not be authenticated.
        }
        
        private static bool DecryptData(string key, byte[] data)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(key);
            try
            {
                // If we don't error while mapping memory then we're valid.
                byte[] decrypted = rsa.Decrypt(data, false);
                return true;
            }
            catch
            {
                rsa.Clear();
                return false; // The key could not be decrypted.
            }
        }

        private void UpdateVerificationAddress(string address)
        {
            _verificationAddress = address;
            foreach (var script in scripts)
            {
                if (script.Value.Type == ScriptType.Validation)
                    script.Value.Address = _verificationAddress;
            }
        }

        private void UpdateSupportAddress(string address)
        {
            _supportAddress = address;
            foreach (var script in scripts)
            {
                if (script.Value.Type == ScriptType.Support)
                    script.Value.Address = _supportAddress;
            }
        }

        private void EnumerateAttributes()
        {
            scripts.Add(nameof(ActivateLicense), (GetAttribute(nameof(ActivateLicense))));
            scripts.Add(nameof(IsLicenseExpired), GetAttribute(nameof(IsLicenseExpired)));
            scripts.Add(nameof(IsChecksumReal), GetAttribute(nameof(IsChecksumReal)));
            scripts.Add(nameof(IsChecksumSupported), GetAttribute(nameof(IsChecksumSupported)));
            scripts.Add(nameof(RegisterLicense), GetAttribute(nameof(RegisterLicense)));
            scripts.Add(nameof(IsKeyValid), GetAttribute(nameof(IsKeyValid)));
        }

        private static WebValidationAttribute GetAttribute(string methodName)
        {
            MethodInfo method = typeof(Licensing).GetMethod((methodName), new Type[] { });
            var attribute = method.GetCustomAttribute<WebValidationAttribute>();
            return attribute;
        }

        #endregion
    }
}
