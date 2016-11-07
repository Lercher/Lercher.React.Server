using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Singhammer.Pos.Interfaces;

namespace Lercher.ReactJS.Host
{
    class LsPosConnector
    {
        public const string StdUrlFmt = "http://{0}:62334/Singhammer/pos/2006/12/WorkflowServer";
        private IWorkflowServer MyChannel;
        private readonly TaskFactory tf = new TaskFactory();

        public void CreateChannel()
        {
            CreateChannel("localhost");
        }

        public void CreateChannel(string Servername)
        {
            var url = String.Format(StdUrlFmt, Servername);

            var WFServerBinding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);
            var quotas = new System.Xml.XmlDictionaryReaderQuotas()
            {
                MaxArrayLength = int.MaxValue,
                MaxBytesPerRead = int.MaxValue,
                MaxDepth = 512,
                MaxNameTableCharCount = int.MaxValue,
                MaxStringContentLength = int.MaxValue
            };
            WFServerBinding.ReaderQuotas = quotas;
            WFServerBinding.MaxReceivedMessageSize = int.MaxValue;

            // siehe auch C:\daten\LeasySOFT.pos\Kernel\Singhammer.Pos.Website\WFServerAdapter.vb
            var cf = new System.ServiceModel.ChannelFactory<IWorkflowServerSync>(WFServerBinding, url);
            MyChannel = cf.CreateChannel();
        }

        public async Task<string>JsonAction(JsonRequest Request)
        {
            return await tf.FromAsync(MyChannel.BeginJsonAction, MyChannel.EndJsonAction, Request, (object)null);
        }

        public async Task<string>View(string viewname)
        {
            var req = new JsonRequest{ WebApplicationName = "/ScaniaPOS", Login = "SCAIntern", Typ ="view", Key = viewname, IsActive = true, P1 = "" }; // P1 = quickfind.
            return await JsonAction(req);
        }

        public async Task<string> Data(string guid)
        {
            var req = new JsonRequest { WebApplicationName = "/ScaniaPOS", Login = "SCAIntern", Typ = "data", Key = guid };
            return await JsonAction(req);
        }

    }
}
