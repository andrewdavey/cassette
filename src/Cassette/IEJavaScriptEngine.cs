using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Cassette.Interop;
using ComImports.JavaScriptEngine;
using EXCEPINFO = System.Runtime.InteropServices.ComTypes.EXCEPINFO;

namespace Cassette
{
    class IEJavaScriptEngine : IActiveScriptSite, IDisposable
    {
        ActiveScriptException scriptException;
        IActiveScript engine;
        ActiveScriptParseWrapper parser;
        Type dispatchType;
        object dispatch;
        readonly Dictionary<string, object> globals = new Dictionary<string, object>();

        public IEJavaScriptEngine()
        {
            try
            {
                engine = new ChakraJavaScriptEngine() as IActiveScript;
            }
            catch
            {
                engine = new JavaScriptEngine() as IActiveScript;
            }

            if (engine == null)
            {
                throw new Exception("Could not create IE JavaScript engine.");
            }

            parser = new ActiveScriptParseWrapper(engine);

            parser.InitNew();
            engine.SetScriptSite(this);

            engine.GetScriptDispatch(null, out dispatch);
            dispatchType = dispatch.GetType();
        }

        public void LoadLibrary(string code)
        {
            try
            {
                parser.ParseScriptText(code, null, null, null, IntPtr.Zero, 0, ScriptTextFlags.IsVisible);
            }
            catch
            {
                ThrowAndResetIfScriptException();
                throw;
            }
        }

        void ThrowAndResetIfScriptException()
        {
            if (scriptException == null) return;
            var ex = scriptException;
            scriptException = null;
            throw ex;
        }

        public void AddGlobalValue(string name, object globalData)
        {
            globals[name] = globalData;
            engine.AddNamedItem(name, ScriptItemFlags.IsVisible);
        }

        public T CallFunction<T>(string functionName, params object[] arguments)
        {
            try
            {
                return (T)dispatchType.InvokeMember(functionName, BindingFlags.InvokeMethod, null, dispatch, arguments);
            }
            catch
            {
                ThrowAndResetIfScriptException();
                throw;
            }
        }

        public void GetLCID(out int lcid)
        {
            lcid = CultureInfo.CurrentCulture.LCID;
        }

        public void GetItemInfo(string name, ScriptInfoFlags returnMask, out object item, IntPtr typeInfo)
        {
            if (globals.TryGetValue(name, out item))
            {
                if (typeInfo != IntPtr.Zero)
                {
                    Marshal.WriteIntPtr(typeInfo, Marshal.GetITypeInfoForType(item.GetType()));
                }
            }
            else
            {
                const int TYPE_E_ELEMENTNOTFOUND = unchecked((int)0x8002802BL);
                throw new COMException(name + " is unknown", TYPE_E_ELEMENTNOTFOUND);
            }
        }

        public void GetDocVersionString(out string versionString)
        {
            versionString = "";
        }

        public void OnScriptTerminate(object result, EXCEPINFO exceptionInfo)
        {
        }

        public void OnStateChange(ScriptState scriptState)
        {
        }

        public void OnScriptError(IActiveScriptError scriptError)
        {
            scriptException = ActiveScriptException.Create(scriptError);
        }

        public void OnEnterScript()
        {
        }

        public void OnLeaveScript()
        {
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~IEJavaScriptEngine()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            ComRelease(ref dispatch, !disposing);
            ComRelease(ref engine, !disposing);
        }

        static void ComRelease<T>(ref T o, bool final = false)
            where T : class
        {
            if (o != null && Marshal.IsComObject(o))
            {
                if (final)
                {
                    Marshal.FinalReleaseComObject(o);
                }
                else
                {
                    Marshal.ReleaseComObject(o);
                }
            }
            o = null;
        }
    }
}
