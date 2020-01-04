#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#endregion
namespace Amaterasu
{
    /// <summary>
    /// The type of script that a <see cref="WebValidationAttribute.Address"/> points to.
    /// </summary>
    internal enum ScriptType { Validation, Support }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal class WebValidationAttribute : Attribute
    {
        #region Variables

        private string _address { get; set; }
        private ScriptType _type { get; set; }
        /// <summary>
        /// The web address that points to a PHP script.
        /// </summary>
        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }
        /// <summary>
        /// The type of script that is associated with the attribute.
        /// </summary>
        public ScriptType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        #endregion
        #region Initialization

        /// <summary>
        /// An attribute allowing the access of a web address pointing to a PHP script.
        /// </summary>
        public WebValidationAttribute()
        {
            //_script = new ScriptAddress();
            _type = ScriptType.Support;
        }
        /// <summary>
        /// An attribute allowing the access of a web address pointing to a PHP script.
        /// </summary>
        /// <param name="type">The type of script the attribute points to.</param>
        public WebValidationAttribute(ScriptType type)
        {
            //_script = new ScriptAddress();
            _type = type;
        }
        /// <summary>
        /// Updates the web address which points to a PHP script.
        /// </summary>
        /// <param name="address">The address pointing to the PHP script.</param>
        public void UpdateAddress(string address)
        {
            _address = address;
        }
        /// <summary>
        /// Updates the web address which points to a PHP script.
        /// </summary>
        /// <param name="address">The address pointing to the PHP script.</param>
        /// <param name="type">The type of PHP script.</param>
        public void UpdateAddress(string address, ScriptType type)
        {
            _address = address;
            _type = type;
        }

        #endregion
    }
}
