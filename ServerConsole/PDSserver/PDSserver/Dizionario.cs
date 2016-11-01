using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PDSserver
{
    class Dizionario
    {
        private Dictionary<string, Delegate> dict;
        private ServerController serverController; //Class with method to invoke
        public delegate Object InvokeFunc(string code, Object obj);
        public InvokeFunc invokeFunc; //Function to pass to HandlePackets

        public Dizionario(ServerController serverController)
        {
            this.serverController = serverController;
            invokeFunc = invokeFunction;
            dict = new Dictionary<string, Delegate>();
            //dict["000"] = new Action(serverController.closeConnectionWithTheClient);
            dict["017"] = new Func<GenericRequest, GenericRequest>(serverController.helloMessage);
            dict["000"] = new Func<GenericRequest, GenericRequest>(serverController.closeConnectionWithTheClient);
            dict["001"] = new Func<File, WrapFile>(serverController.handleRequestOfFile);
            dict["002"] = new Func<File, WrapFile>(serverController.initializeReceiptOfFile);
            dict["003"] = new Func<WrapFile, WrapFile>(serverController.fileReceived);
            dict["004"] = new Func<Register, Register>(serverController.handleRegistration);
            dict["005"] = new Func<Login, Login>(serverController.handleLogin);
            dict["018"] = new Func<MonitorDir, MonitorDir>(serverController.addMonitorDir);
            dict["015"] = new Func<MonitorDir, MonitorDir>(serverController.getMonitorDir);
            dict["006"] = new Func<CreateVersion, CreateVersion>(serverController.createNewVersion);
            dict["007"] = new Func<UpdateVersion, UpdateVersion>(serverController.updateVersion);
            dict["009"] = new Func<CloseVersion, CloseVersion>(serverController.closeVersion);
            dict["010"] = new Func<GetVersion, GetVersion>(serverController.getVersion);
            dict["016"] = new Func<GetVersion, GetVersion>(serverController.getOpenVersion);
            dict["020"] = new Func<GetVersion, GetVersion>(serverController.getLastVersion);
            dict["019"] = new Func<GenericRequest, GenericRequest>(serverController.deleteUserRepository);
            dict["011"] = new Func<StoredVersions, StoredVersions>(serverController.getIDofAllVersions);
            dict["014"] = new Func<HashRequest, HashRequest>(serverController.sendHashToBeingReceived);
                        
            dict["013"] = new Func<CheckFile, CheckFile>(serverController.checkFile);
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
