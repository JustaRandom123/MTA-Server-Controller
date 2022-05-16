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

namespace MTA_SDK
{
    /// <summary>
    /// Initializes instance of MTA_LuaArgs class. 
    /// </summary>
    public class MTA_LuaArgs
    {
        private List<string> _Values = new List<string>();
        //private string _JSONStr;

        /// <summary>
        /// Creates instance of the class.
        /// </summary>
        public MTA_LuaArgs()
        {
        }

        /// <summary>
        /// Creates instance of the class. Pass arguments which are going to be used in function call.
        /// </summary>
        /// <param name="args"></param>
        public MTA_LuaArgs(params object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string argJSON = ConvertObjToJSON(args[i]);
                if (!String.IsNullOrEmpty(argJSON))
                    _Values.Add(argJSON);
            }
        }

        /// <summary>
        /// Adds a value to the arguments list which is passed to server function call.
        /// </summary>
        /// <param name="val">value of type: string, bool, int, int[], float, decimal, double, long, uint, ulong, Single and short</param>
        public void AddValue(object val)
        {
            string jsonVal = ConvertObjToJSON(val);
            if( !String.IsNullOrEmpty(jsonVal) )
                _Values.Add( jsonVal );
        }

        private string ConvertObjToJSON(object val)
        {
            Type valType = val.GetType();
            if (valType == typeof(string))
                return "\"" + val + "\"";
            else if (valType == typeof(bool))
                return val.ToString().ToLower();
            else if (
                valType == typeof(int) ||
                valType == typeof(float) ||
                valType == typeof(decimal) ||
                valType == typeof(double) ||
                valType == typeof(long) ||
                valType == typeof(uint) ||
                valType == typeof(ulong) ||
                valType == typeof(Single) ||
                valType == typeof(short)
                )
                return val.ToString().ToLower();
            else if (valType == typeof(int[]))
            {
                int[] arr = (int[])val;
                val = (int[])val;
                string arrStr = "{";
                for (int i = 0; i < arr.Length; i++)
                {
                    if( IsValueSupported( arr[i] ) )
                        arrStr += "\"" +i.ToString() + "\"" + ":" + ConvertObjToJSON( arr[i] ) + ",";
                        // pair of key, value:  luaTable["key"] = value
                }
                arrStr = arrStr.TrimEnd(',');
                arrStr += "}";
                return arrStr;
            }
            /*
                valType == typeof(float[]) ||
                valType == typeof(decimal[]) ||
                valType == typeof(double[]) ||
                valType == typeof(long[]) ||
                valType == typeof(uint[]) ||
                valType == typeof(ulong[]) ||
                valType == typeof(Single[]) ||
                valType == typeof(short[])*/
            return null;
        }

        protected bool IsValueSupported(object val)
        {
            Type valType = val.GetType();
            if (valType == typeof(int))
                return true;
            else if (valType == typeof(string))
                return true;
            else if (valType == typeof(bool))
                return true;
            else if (valType == typeof(int[]))
                return true;
            return false;
        }

        public string ConvertToJSONString()
        {
            string jsonString = "[";
            for (int i = 0; i < _Values.Count; i++)
            {
                jsonString += _Values[i] + ",";
            }
            jsonString = jsonString.TrimEnd(',');
            return jsonString + "]";
        }
    }
}
