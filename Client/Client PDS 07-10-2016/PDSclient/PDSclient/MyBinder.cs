using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PDSclient
{
    /* Questa classe serve per deserializzare gli oggetti 
     * provenienti da un altro namespace. Infatti tra diversi
     * namespace l'assemblyName è diverso per cui anche se
     * classe risultano identiche bisogna deserializzare gli
     * oggetti con l'assembly del namespace corrente
     * */
    /*class MyBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type typeToDeserialize = null;

            // For each assemblyName/typeName that you want to deserialize to
            // a different type, set typeToDeserialize to the desired type.
            String assemVerCurr = Assembly.GetExecutingAssembly().FullName;
            string typeVerCurr;

            if (assemblyName != assemVerCurr)
            {
                string[] str1 = assemVerCurr.Split(','); //str[0]: nome del namespace corrente
                string[] str2 = typeName.Split('.');     //str[1]: nome della classe da serializzare
                //        (uguale per entrambi i namespace) 
                typeVerCurr = str1[0] + "." + str2[1];
            }
            else
                typeVerCurr = typeName;

            // The following line of code returns the type.
            typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeVerCurr, assemVerCurr));

            return typeToDeserialize;
        }
    }*/
}
