using System.Collections.Generic;
using Ext.Net;
using System.ComponentModel;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System.Web.UI;

namespace Smartline.Web.Plugins {
    public class HeaderFilter : Plugin {
        public override string InstanceOf {
            get {
                return "Ext.ux.HeaderFilter";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [XmlIgnore]
        [JsonIgnore]
        public override ConfigOptionsCollection ConfigOptions {
            get {
                ConfigOptionsCollection list = base.ConfigOptions;

                list.Add("activeFilter", new ConfigOption("activeFilter", null, null, this.ActiveFilter));
                list.Add("automaticSave", new ConfigOption("automaticSave", null, true, this.AutomaticSave));
                list.Add("remote", new ConfigOption("remote", null, false, this.UsePaging));
                list.Add("enableFilter", new ConfigOption("enableFilter", null, false, this.EnableFilter));
                list.Add("directEventConfig", new ConfigOption("directEventConfig", new SerializationOptions(JsonMode.Object), null, this.DirectEventConfig));
                list.Add("proxyId", new ConfigOption("proxyId", null, null, this.ClientID));
                list.Add("proxyId", new ConfigOption("headerFilter", null, null, this.HeaderFilters == null ? "" : this.HeaderFilters.ToString()));
                return list;
            }
        }

        [ConfigOption]
        [DefaultValue(true)]
        public bool AutomaticSave {
            get {
                object obj = this.ViewState["AutomaticSave"];
                return obj == null || (bool)obj;
            }
            set {
                this.ViewState["AutomaticSave"] = value;
            }
        }

        [ConfigOption]
        [DefaultValue(null)]
        [DirectEventUpdate(MethodName = "SetActiveFilter")]
        public string ActiveFilter {
            get {
                return this.ViewState["ActiveFilter"] as string;
            }
            set {
                this.ViewState["ActiveFilter"] = value;
            }
        }

        [DefaultValue(null)]
        public Dictionary<string, object> Values {
            get;
            set;
        }


        private BaseDirectEvent directEventConfig;

        [PersistenceMode(PersistenceMode.InnerProperty)]
        [ConfigOption(JsonMode.Object)]
        public BaseDirectEvent DirectEventConfig {
            get {
                if (this.directEventConfig == null) {
                    this.directEventConfig = new BaseDirectEvent();
                }

                return this.directEventConfig;
            }
        }

        [ConfigOption]
        [DefaultValue(false)]
        public bool UsePaging { get; set; }

        [ConfigOption]
        [DefaultValue(null)]
        public JsonObject HeaderFilters { get; set; }

        [ConfigOption]
        [DefaultValue("enable")]
        public string EnableFilter { get; set; }

        protected void SetActiveFilter(string name) {
            Call("applySet", name);
        }

        public void SetActiveFilter(Dictionary<string, object> values) {
            Call("applySet", values);
        }
    }
}