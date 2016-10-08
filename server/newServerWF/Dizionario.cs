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
            //dict["000"] = new Action(handleClient.closeConnectionWithTheClient);
            dict["017"] = new Func<GenericRequest, GenericRequest>(handleClient.helloMessage);
            dict["000"] = new Func<GenericRequest, GenericRequest>(handleClient.closeConnectionWithTheClient);
            dict["001"] = new Func<File, WrapFile>(handleClient.handleRequestOfFile);
            dict["002"] = new Func<File, WrapFile>(handleClient.initializeReceiptOfFile);
            dict["003"] = new Func<WrapFile, WrapFile>(handleClient.fileReceived);
            dict["004"] = new Func<Register, Register>(handleClient.handleRegistration);
            dict["005"] = new Func<Login, Login>(handleClient.handleLogin);
            dict["012"] = new Func<MonitorDir, MonitorDir>(handleClient.changeMonitorDir); 
            dict["015"] = new Func<MonitorDir, MonitorDir>(handleClient.getMonitorDir);
            dict["006"] = new Func<CreateVersion, CreateVersion>(handleClient.createNewVersion);
            dict["007"] = new Func<UpdateVersion, UpdateVersion>(handleClient.updateVersion);
            dict["009"] = new Func<CloseVersion, CloseVersion>(handleClient.closeVersion);
            dict["010"] = new Func<GetVersion, GetVersion>(handleClient.getVersion);
            dict["016"] = new Func<GetVersion, GetVersion>(handleClient.getOpenVersion);
            dict["011"] = new Func<StoredVersions, StoredVersions>(handleClient.getIDofAllVersions);
            dict["014"] = new Func<HashRequest, HashRequest>(handleClient.sendHashToBeingReceived);
            //dict["006"] = new Func<List<File>, List<File>>(handleClient.handleSynchronizeRequest);
            //dict["008"] = new Func<GenericReq, GenericReq>(handleClient.completeSynchronization);
            
            
            dict["013"] = new Func<CheckFile, CheckFile>(handleClient.checkFile);
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
                MyConsole.Write("Code non valid");
                return null;
            }
        }

        public InvokeFunc getDelegate()
        {
            return invokeFunc;
        }

    }
}
