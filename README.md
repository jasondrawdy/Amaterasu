<p align="center">
    <img width="256" height="256" src="https://user-images.githubusercontent.com/40871836/43242728-544787a4-9068-11e8-9796-631a65b4ef6a.png">
</p>

# Amaterasu
<p align="left">
    <!-- Version -->
    <img src="https://img.shields.io/badge/version-1.0.0-brightgreen.svg">
    <!-- <img src="https://img.shields.io/appveyor/ci/gruntjs/grunt.svg"> -->
    <!-- Docs -->
    <img src="https://img.shields.io/badge/docs-not%20found-lightgrey.svg">
    <!-- License -->
    <img src="https://img.shields.io/badge/license-MIT-blue.svg">
</p>

Amaterasu is a lightweight licensing library for .NET applications which allows the managing of licenses via web based scripts and on-the-fly code compilation. The library also includes common encryption and hashing algorithms such as AES and SHA to ensure that data is protected when communicating with the licensing server; however, since runtime code compilation is available, all methods currently used for licensing can be replaced with custom written modules instead of utilizing the prebuilt licensing structure.

### Requirements
- .NET Framework 4.6.1

# Features
- Activate and manage license files using web scripts
- Dynamically generate and explore C# code *in-memory*
- Call managed assembly modules, methods, and types
- Secure communication layer between licensing server and client
- Create blacklists from arrays or from a *`NetRequest`*
- Manage the local filesystem using both managed and native methods
- Built-in anti-debugging checking
- User privilege enumeration
- System search (finds and removes blacklisted files from the system)
- Encryption, hashing, and cryptographically strong data generation
    ###### Encryption
    - AES

    ###### Hashing
    - MD5
    - RIPEMD160
    - SHA-1
    - SHA-256
    - SHA-384
    - SHA-512

# Examples
### License Activation

In order to utilize the license activation methods you must first write two scripts for both validating data as well as one for actually handling the updating and activation of license files; the library uses **POST** requests, usually sent to PHP scripts, which handle all of the server side calculations and managing. The first script is known as the *`Verification Script`* and the second is recognized as the *`Support Script`*. Here's how they are meant to be used:

```c#
using System;
using Amaterasu;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            // Read a RSA public key to be used for decrypting the license and validating data.
            string publicKey = File.ReadAllText("PathToPublicKey");

            // Read the license file to activate.
            string licenseFile = File.ReadAllText("PathToLicense");

            // Set the username and password that were used at the time of generating the license.
            string username = "someusername";
            string password = "somepassword";

            // Set our script urls to use for authentication and activation.
            string verificationScript = "https://www.yourwebsite.com/verification.php"; // Change this to your website.
            string supportScript = "https://www.yourwebsite.com/support.php"; // Change this to your website.

            // Create a new licensing object with our script urls and try activating a license.
            Licensing manager = new Licensing(verificationScript, supportScript);
            if (manager.ActivateLicense(publicKey, username, password, licenseFile))
                Console.WriteLine("The license has successfully been activated!");
            else
                Console.WriteLine("The license could not be activated.");

            // Wait for user response.
            Console.Read();
        }
    }
}
```
### Dynamic Code Compilation

Since Amaterasu has the ability to compile C# on-the-fly the code above can be modified to behave differently without actually modifying the library source. Using this feature in conjunction with the availible features in the library facilitates the possibility of creating a unique licensing schema by writing multiple modules and compiling them as need within a method. An example of this would be to have code check certain aspects of a license file which in turn references another runtime compiled module to actually do any activation or managing of said license. Shown below is a example of compiling an *`AMC`* or *`Authentication Module Chain`* that decodes a license, reroutes data validation scripts, and activates the license:


The main namespace which will encapsulate the following code will be `namespace DynamicCode`. Each module should be placed either in separate text documents or all in the same document, however, each module **MUST** have the same namespace in order to be utilized properly when exploring the compiled assembly.

###### Encoding Module

This module is responsible for taking care of all data encoding and or encryption transformations.

```c#
using System;
using System.Security.Cryptography;
using Amaterasu;

namespace DynamicCode
{
    public static class Encoding
    {
        /// <summary>
        /// Decrypts a license file for further processing.
        /// </summary>
        /// <param name="publicKey">The <see cref="RSACryptoServiceProvider"/> XML formatted public key.</param>
        /// <param name="licenseFile">The license file to decrypt.</param>
        public static string DecryptLicense(string publicKey, string licenseFile)
        {
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            provider.FromXmlString(publicKey);
            var encryptedBytes = licenseFile.GetBytes();
            var decryptedBytes = provider.Decrypt(encryptedBytes, false);
            return decryptedBytes.GetString();
        }

        /// <summary>
        /// Converts and decodes a license file into a readable format.
        /// </summary>
        /// <param name="licenseFile">The license file to decode.</param>
        public static string DecodeLicense(string licenseFile)
        {
            // Some parsing could be done here too, or separately.
            var encoded = licenseFile;
            var decoded = Convert.FromBase64String(encoded).GetString();
            return decoded;
        }
    }
}
```

###### Scripting Module

This module is responsible for all authentication and activation of license files and public keys. It resembles the structure of the current scripting schema presently found in the library and can be edited and compiled on-the-fly either at compile time or even runtime; for instance, you can write a scripting generator which in turn would compile and run selected methods with a command or a button press depending on the selected projects interface.

```c#
using System;
using System.Security.Cryptography;
using Amaterasu;

namespace DynamicCode
{
    public static class Scripting
    {
        private static string verificationScript { get; set; }
        private static string supportScript { get; set; }

        /// <summary>
        /// The address of the script which will handle all license and key authentication.
        /// </summary>
        public static string VerificationScript
        {
            get { return verificationScript; }
            set { verificationScript = value; }
        }

        /// <summary>
        /// The address of the script which will handle the management and activation of licenses.
        /// </summary>
        public static string SupportScript
        {
            get { return supportScript; }
            set { supportScript = value; }
        }

        /// <summary>
        /// Activates a license file with an authentication server.
        /// </summary>
        /// <param name="publicKey">The public key created when the license was generated.</param>
        /// <param name="licenseFile">The license file to activate.</param>
        public static bool ActivateLicense(string publicKey, string licenseFile)
        {
            // Check if our key and license is valid and or expired.
            if (IsKeyValid(publicKey))
            {
                if (IsLicenseValid(publicKey, licenseFile))
                {
                    if (!IsLicenseExpired(publicKey, licenseFile))
                    {
                        // From here you could explore the array of parsed license items or continue directly with activation.
                        var parsedLicense = ParseLicense(licenseFile);

                        // Initialize a new RSA provider in order to encrypt the license.
                        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                        rsa.FromXmlString(publicKey);

                        // Send our license to the server for activation.
                        var encryptedLicense = rsa.Encrypt(licenseFile.GetBytes(), false).GetString();
                        var requestParameter = "activation";
                        var requestData = requestParameter + "=" + encryptedLicense; // Include an equals sign because we're posting to a PHP script.
                        NetRequest request = new NetRequest(verificationScript, NetRequest.RequestType.POST, requestData);

                        // Return whether or not the activation was successful.
                        return bool.Parse(request.GetResponse());
                    }
                }   
            }
            return false;
        }

        /// <summary>
        /// Validates an <see cref="RSACryptoServiceProvider"/> public key with an authentication server.
        /// </summary>
        /// <param name="publicKey">The public key to validate with the server.</param>
        public static bool IsKeyValid(string publicKey)
        {
            // Request a chunk of encrypted data from the server.
            var requestParameter = "ping";
            var requestData = requestParameter + "=" + "pong"; // Include an equals sign because we're posting to a PHP script.
            NetRequest request = new NetRequest(VerificationScript, NetRequest.RequestType.POST, requestData);
            string response = request.GetResponse();

            // Try to decrypt our response.
            if (DecryptData(publicKey, response.GetBytes()))
                return true; // The key is good.
            return false; // The key could not be authenticated.
        }

        /// <summary>
        /// Validates a license file with an authentication server.
        /// </summary>
        /// <param name="publicKey">An <see cref="RSACryptoServiceProvider"/> public key created when the license was generated.</param>
        /// <param name="licenseFile">The license file to validate.</param>
        public static bool IsLicenseValid(string publicKey, string licenseFile)
        {
            // Create a new RSA provider in order to encrypt the license.
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKey);

            // Send our license to the server to check validity.
            var encryptedLicense = rsa.Encrypt(licenseFile.GetBytes(), false).GetString();
            var requestParam = "validation";
            var requestData = requestParam + "=" + encryptedLicense; // Include an equals sign because we're posting to a PHP script.
            NetRequest request = new NetRequest(verificationScript, NetRequest.RequestType.POST, requestData);

            // Return our server's response.
            return bool.Parse(request.GetResponse());
        }

        /// <summary>
        /// Checks if a license if expired or not.
        /// </summary>
        /// <param name="publicKey">An <see cref="RSACryptoServiceProvider"/> public key created when the license was generated.</param>
        /// <param name="licenseFile">The license file to check.</param>
        public static bool IsLicenseExpired(string publicKey, string licenseFile)
        {
            // Decrypt the license to obtain its info.
            string decryptedLicense = Encoding.DecryptLicense(publicKey, licenseFile);

            // Parse the decrypted license.
            var parsed = ParseLicense(decryptedLicense);

            // Check the timestamp of the license to determine if the license is expired.
            DateTime past = DateTime.Parse(parsed[0]); // The timestamp could be at any index in the array.
            if (past < DateTime.Now)
                return false;
            return true;
        }

        /// <summary>
        /// Parses all parts of a license file.
        /// </summary>
        /// <param name="licenseFile">The license file to parse.</param>
        private static string[] ParseLicense(string licenseFile)
        {
            // Split the license using an uncommon character.
            var parsed = licenseFile.Split('|');

            // Return our array if its length is greater than zero.
            return (parsed.Length > 0) ? parsed : null;
        }

        /// <summary>
        /// Decrypts data using an <see cref="RSACryptoServiceProvider"/> public key.
        /// </summary>
        /// <param name="key">The public key used to decrypt the date.</param>
        /// <param name="data">The data to decrypt.</param>
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
                return false; // The data could not be decrypted.
            }
        }
    }
}
```

---

#### Module Compilation & Use

This is a simple example showcasing how to compile the above modules from a single code document using built-in .NET modules and the *Amaterasu* library itself by defining the aforementioned module imports in a list.

```c#
using System;
using System.IO;
using System.Collections.Generic;
using Amaterasu;

namespace Example
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            /* We will read our code from a single document, however, you can have as many as
            you would like and compile them at different points during the application. */
            string code = File.ReadAllText("PathToCodeFile");

            // Create a list of our imports (optional).
            List<string> imports = new List<string>();
            imports.Add("System.dll");
            imports.Add("System.Security.dll");
            imports.Add("Amaterasu.dll");

            // Create a new assembly compiler and compile our code.
            AssemblyCompiler compiler = new AssemblyCompiler();
            var assembly = compiler.Compile(code, imports.ToArray());

            // Display the assembly's name.
            Console.WriteLine(assembly.FullName);

            // Wait for user input.
            Console.Read();
        }
    }
}
```
#### Assembly Exploration

Exploring a compiled assembly, calling methods, and obtaining types is rather simple given the `AssemblyInfo` type provided within the *Amaterasu* library. Using the same method as above we'll compile an assembly from a single document and obtain all of its methods and types. After we have obtained an object containing all of the assembly's info we can explore it or call our activation method in order to register a license.

```c#
using System;
using System.IO;
using System.Reflection;
using Amaterasu;

namespace Example
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Read in our code from a document and create an imports array (optional).
            string code = File.ReadAllText("PathToCode");
            string[] imports = new string[] { "System.dll", "System.Security.dll", "Amaterasu.dll" };

            // Create a new assembly compiler and compile our code.
            AssemblyCompiler compiler = new AssemblyCompiler();
            var assembly = compiler.Compile(code, imports);

            // Get the information about the assembly.
            AssemblyInfo info = compiler.GetAssemblyInfo(assembly);

            // Print all modules within the assembly.
            foreach (var module in info.Modules)
                Console.WriteLine("Module: {0}", module.Name);

            // Print each type within the assembly.
            foreach (var type in info.Types)
                Console.WriteLine("Type: {0}", type.Name);

            // Print each method within the assembly.
            foreach (var method in info.Methods)
                Console.WriteLine("Method: {0}", method.Name);

            // Try activating a license using the compiled assembly.
            var publicKey = File.ReadAllText("PathToPublicKey");
            var licenseFile = File.ReadAllText("PathToLicense");
            Activator activator = new Activator(assembly);
            var result = activator.ActivateLicense(publicKey, licenseFile);

            // Print our result (which should be a bool) to the console.
            Console.WriteLine("License Activated: {0}", result);

            // Wait for user input.
            Console.Read();
        }
    }

    class Activator
    {
        private Assembly _compiledAssembly { get; set; }

        /// <summary>
        /// The assembly that was compiled in-memory.
        /// </summary>
        public Assembly CompiledAssembly
        {
            get { return _compiledAssembly; }
            private set { _compiledAssembly = value; }
        }

        /// <summary>
        /// Activates a license file using an in-memory compiled assembly.
        /// </summary>
        /// <param name="compiled">The assembly that was compiled in-memory.</param>
        public Activator(Assembly compiledAssembly)
        {
            CompiledAssembly = compiledAssembly;
        }

        /// <summary>
        /// Activates a license file using an authentication server.
        /// </summary>
        /// <param name="publicKey">The public key created when the license was generated.</param>
        /// <param name="licenseFile">The license file to be activated.</param>
        public object ActivateLicense(string publicKey, string licenseFile)
        {
            // Set what method we'll be running and its parameters.
            string rootNamespace = "DynamicCode";
            string rootClass = "Scripting";
            string methodName = "ActivateLicense";
            object[] methodParameters = new object[] { publicKey, licenseFile };

            // Create a new assembly compiler and try running the method.
            AssemblyCompiler compiler = new AssemblyCompiler();
            var result = compiler.Run(_compiledAssembly, rootNamespace, rootClass, methodName, parameters);

            // Return any objects of the method or null if it's a void.
            return result;
        }
    }
}
```

# License

Copyright © ∞ Jason Drawdy (CloneMerge)

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
