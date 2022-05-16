/*
 *      C# MTA SDK
 * 
 *      Author:     50p
 *      Version:    1.0
 *      Purpose:    Extending MTA:SE functionality
 *      Description:
 *                  This SDK lets you call server exported functions.
 * 
 *      Date:       15/10/2009
 */


using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Net;
using System.IO;

namespace MTA_SDK
{
    /// <summary>
    /// Connect to MTA server and make calls to exported functions.
    /// </summary>
    public class MTA
    {

        private string _Host = "localhost"; 
        private int _Port = 22055;
        private string _Username = "";
        private string _Password = "";

        private bool _ReconnectOnFailure = true;

        #region Constructors
        /// <summary>
        /// Creates an instance of MTA class. With default host of 127.0.0.1 and port of 22005.
        /// </summary>
        public MTA()
        {
        }

        /// <summary>
        /// Creates an instance of MTA class. With default port of 22005.
        /// </summary>
        /// <param name="host">Host to which to connect to.</param>
        public MTA(string host)
        {
            _Host = host;
        }

        /// <summary>
        /// Creates an instance of MTA class.
        /// </summary>
        /// <param name="host">Server address of which you want to call functions.</param>
        /// <param name="port">Port of the server.</param>
        public MTA(string host, int port)
        {
            _Host = host;
            _Port = port;
        }

        /// <summary>
        /// Creates an instance of MTA class.
        /// </summary>
        /// <param name="host">Server address of which you want to call functions.</param>
        /// <param name="port">Port of the server.</param>
        /// <param name="username">Username to login if required.</param>
        /// <param name="password">Password of the account to login.</param>
        public MTA(string host, int port, string username, string password)
        {
            _Host = host;
            _Port = port;
            _Username = username;
            _Password = password;
        }
        #endregion


        /// <summary>
        /// Calls an exported Lua function.
        /// </summary>
        /// <param name="resourceName">Resource name of which the function will be called.</param>
        /// <param name="functionName">Name of the function to call.</param>
        /// <param name="args">List of arguments which will be forwarded to the Lua function.</param>
        /// <returns></returns>
        public string CallFunction(string resourceName, string functionName, MTA_LuaArgs args )
        {
            string requestContent = args.ConvertToJSONString();
            try
            {
                string requrl = "http://"+_Host + ":" + _Port.ToString() + "/" + resourceName + "/call/" + functionName;
                string result = this.DoPOSTRequest(requrl, requestContent);
                return result;
            }
            catch (WebException ex)
            {
                string str = "The remote server returned an error: (401) Unauthorized.";
                if (ex.Message == str)
                {
                    if (_ReconnectOnFailure)
                    {
                        _ReconnectOnFailure = false;
                        return this.DoPOSTRequest(ex.Response.ResponseUri.AbsoluteUri, requestContent);
                    }
                    else
                    {
                        _ReconnectOnFailure = true;
                        throw new MTARequestException(ex.Message, ex.InnerException, WebExceptionStatus.ConnectFailure, ex.Response);
                    }
                }
                else
                    throw new MTARequestException(ex.Message, ex.InnerException, WebExceptionStatus.ConnectFailure, ex.Response);
            }
        }


        private string DoPOSTRequest(string url, string json)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";
                req.AllowAutoRedirect = true;
                if (!String.IsNullOrEmpty(_Username) && !String.IsNullOrEmpty(_Password))
                {
                    byte[] credentialsAuth = new UTF8Encoding().GetBytes(_Username + ":" + _Password);
                    req.Headers["Authorization"] = "Basic " + Convert.ToBase64String(credentialsAuth);
                }
                req.ContentType = "application/x-www-form-urlencoded";
                req.Timeout = 3500;

                StreamWriter writer = new StreamWriter(req.GetRequestStream());
                writer.Write(json);
                writer.Close();

                WebResponse resp = req.GetResponse();
                StreamReader reader = new StreamReader(resp.GetResponseStream());
                string result = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
                writer.Dispose();
                if (result.StartsWith("error"))
                    throw new MTARequestException(result);
                return result;
            }
            catch (WebException ex)
            {
                if (_ReconnectOnFailure)
                    throw new WebException( ex.Message, ex.InnerException, WebExceptionStatus.ConnectFailure, ex.Response);
                else
                {
                    string msg = "";
                    bool usernameEmptyOrNull = String.IsNullOrEmpty(_Username);
                    bool passwordEmptyOrNull = String.IsNullOrEmpty(_Password);
                    if (!usernameEmptyOrNull && !passwordEmptyOrNull)
                        msg = "Login or Password are incorrect.";
                    else if (!usernameEmptyOrNull && passwordEmptyOrNull)
                        msg = "Password is required.";
                    else if (usernameEmptyOrNull && !passwordEmptyOrNull)
                        msg = "Login is required.";
                    else
                        msg = "Login and Password are required.";
                    throw new MTARequestException( "Access denied. " + msg, ex.InnerException, WebExceptionStatus.ConnectFailure, ex.Response); 
                }
            }
        }
    }

    [Serializable]
    public class MTARequestException : ApplicationException
    {
        private WebExceptionStatus _ExceptionStatus;
        private WebResponse _Response;
        public MTARequestException() : base() {}
        public MTARequestException(string message) : base(message) {}
        public MTARequestException(string message, Exception innerException) : base(message, innerException) { }
        public MTARequestException(string message, Exception innerException, WebExceptionStatus status, WebResponse response) 
            : base(message, innerException)
        {
            _Response = response;
            _ExceptionStatus = status;
        }
    }
}
