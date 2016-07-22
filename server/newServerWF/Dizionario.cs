using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    class Dizionario
    {
        private Dictionary<string, Delegate> dict;
        private HandleClient handleClient; //Class with method to invoke
        public delegate Object InvokeFunc(string code, Object obj);
        public InvokeFunc invokeFunc; //Function to pass to HandlePackets

        public Dizionario(HandleClient handleClient)
        {
            this.handleClient = handleClient;
            invokeFunc = invokeFunction;
            dict = new Dictionary<string, Delegate>();
            dict["000"] = new Action(handleClient.stop);
            dict["001"] = new Func<File, WrapFile>(handleClient.handleRequestOfFile);
            dict["002"] = new Func<File, WrapFile>(handleClient.prepareReceiptOfFile);
            dict["003"] = new Func<Login, Login>(handleClient.handleLogin);
            dict["004"] = new Func<Register, Register>(handleClient.handleRegister);
            dict["006"] = new Func<List<File>, List<File>>(handleClient.handleSynchronizeRequest);
            dict["008"] = new Func<GenericReq, GenericReq>(handleClient.completeSynchronization);
            dict["010"] = new Func<WrapFile, WrapFile>(handleClient.fileReceived);
            //dict["xxx"] = new Action<Object>(handleClient.saveFile);
        }

        public Object invokeFunction(string str, Object obj)
        {
            try
            {
                if( obj.GetType() == typeof(string) )
                {
                    ParameterInfo[] Myarray = dict[str].Method.GetParameters();
                    Type t = Myarray[0].ParameterType;
                    string json = (string)obj;
                    obj = JsonConvert.DeserializeObject(json, t);
                }
                return dict[str].DynamicInvoke(obj);
            }
            catch (TargetParameterCountException e)
            {
                return dict[str].DynamicInvoke();

            }
            catch (KeyNotFoundException e)
            {
                MyConsole.write("Code non valid");
                return null;
            }
        }

        public InvokeFunc getDelegate()
        {
            return invokeFunc;
        }

    }
}
