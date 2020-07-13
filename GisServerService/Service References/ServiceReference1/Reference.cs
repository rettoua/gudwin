﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GisServerService.ServiceReference1 {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://gisserver.org/", ConfigurationName="ServiceReference1.GisServerServiceSoap")]
    public interface GisServerServiceSoap {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://gisserver.org/ConnectToServer", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string ConnectToServer(string login, string pass, string tz);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://gisserver.org/ClientPing", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Xml.XmlNode ClientPing(string a);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://gisserver.org/Disconnect", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Xml.XmlNode Disconnect(string a);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://gisserver.org/GetAlarmTable", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Xml.XmlNode GetAlarmTable(string a);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://gisserver.org/GetRequestedAlarmTable", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Xml.XmlNode GetRequestedAlarmTable(string from_date, string to_date);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://gisserver.org/GetAllObjects", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Xml.XmlNode GetAllObjects(string a);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://gisserver.org/UpdateObjectsInfo", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Xml.XmlNode UpdateObjectsInfo(string a);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://gisserver.org/UpdateAlarms", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Xml.XmlNode UpdateAlarms(string a);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://gisserver.org/GetRoute", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Xml.XmlNode GetRoute(string id, string sensor_id, string from_date, string to_date);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://gisserver.org/GetObjInfo", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Xml.XmlNode GetObjInfo(string id);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://gisserver.org/GetAllObjInfo", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Xml.XmlNode GetAllObjInfo(string id);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://gisserver.org/GetObjParams", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Xml.XmlNode GetObjParams(string id);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://gisserver.org/GetReport", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Xml.XmlNode GetReport(string id, string report_type, string from_date, string to_date);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://gisserver.org/GetLicenseTime", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string GetLicenseTime();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://gisserver.org/SetLicenseTime", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string SetLicenseTime(string hexCode);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface GisServerServiceSoapChannel : GisServerService.ServiceReference1.GisServerServiceSoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GisServerServiceSoapClient : System.ServiceModel.ClientBase<GisServerService.ServiceReference1.GisServerServiceSoap>, GisServerService.ServiceReference1.GisServerServiceSoap {
        
        public GisServerServiceSoapClient() {
        }
        
        public GisServerServiceSoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public GisServerServiceSoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public GisServerServiceSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public GisServerServiceSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string ConnectToServer(string login, string pass, string tz) {
            return base.Channel.ConnectToServer(login, pass, tz);
        }
        
        public System.Xml.XmlNode ClientPing(string a) {
            return base.Channel.ClientPing(a);
        }
        
        public System.Xml.XmlNode Disconnect(string a) {
            return base.Channel.Disconnect(a);
        }
        
        public System.Xml.XmlNode GetAlarmTable(string a) {
            return base.Channel.GetAlarmTable(a);
        }
        
        public System.Xml.XmlNode GetRequestedAlarmTable(string from_date, string to_date) {
            return base.Channel.GetRequestedAlarmTable(from_date, to_date);
        }
        
        public System.Xml.XmlNode GetAllObjects(string a) {
            return base.Channel.GetAllObjects(a);
        }
        
        public System.Xml.XmlNode UpdateObjectsInfo(string a) {
            return base.Channel.UpdateObjectsInfo(a);
        }
        
        public System.Xml.XmlNode UpdateAlarms(string a) {
            return base.Channel.UpdateAlarms(a);
        }
        
        public System.Xml.XmlNode GetRoute(string id, string sensor_id, string from_date, string to_date) {
            return base.Channel.GetRoute(id, sensor_id, from_date, to_date);
        }
        
        public System.Xml.XmlNode GetObjInfo(string id) {
            return base.Channel.GetObjInfo(id);
        }
        
        public System.Xml.XmlNode GetAllObjInfo(string id) {
            return base.Channel.GetAllObjInfo(id);
        }
        
        public System.Xml.XmlNode GetObjParams(string id) {
            return base.Channel.GetObjParams(id);
        }
        
        public System.Xml.XmlNode GetReport(string id, string report_type, string from_date, string to_date) {
            return base.Channel.GetReport(id, report_type, from_date, to_date);
        }
        
        public string GetLicenseTime() {
            return base.Channel.GetLicenseTime();
        }
        
        public string SetLicenseTime(string hexCode) {
            return base.Channel.SetLicenseTime(hexCode);
        }
    }
}
