using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using Ext.Net;
using Smartline.Mapping;

namespace Smartline.Web {
    /// <summary>
    /// Summary description for operators
    /// </summary>
    public class operators : IHttpHandler, IReadOnlySessionState {
        public void ProcessRequest(HttpContext context) {
            string action = context.Request.QueryString["action"];
            var user = (User)context.Session["user"];
            if (action == "read") {
                user = CouchbaseManager.GetUser(user.Id + "");
                string operators = JSON.Serialize(user.Operators);
                context.Response.ContentType = "text/json";
                context.Response.Write(operators);
            } else {
                var dataHandler = new StoreDataHandler(context);
                List<InternalUser> operators = dataHandler.ObjectData<InternalUser>();
                switch (action) {
                    case "update": {
                            List<InternalUser> operatorsToRemove = user.Operators.Where(o => operators.Select(z => z.Id).Contains(o.Id)).ToList();
                            operatorsToRemove.ForEach(o => user.Operators.Remove(o));
                            user.Operators.AddRange(operators);
                            foreach (InternalUser internalUser in operators) {
                                internalUser.Secret = User.ComputeSecretNew(internalUser.NormalSecret);
                            }
                            user.Update();
                        }
                        break;
                    case "create": {
                            foreach (InternalUser internalUser in operators) {
                                internalUser.Id = Increments.GenerateUserId();
                                internalUser.ParentUserId = user.Id;
                                internalUser.Secret = User.ComputeSecretNew(internalUser.NormalSecret);
                            }
                            user.Operators.AddRange(operators);
                            user.Update();
                        }
                        break;
                    case "destroy": {
                            List<InternalUser> operatorsToRemove = user.Operators.Where(o => operators.Select(z => z.Id).Contains(o.Id)).ToList();
                            operatorsToRemove.ForEach(o => user.Operators.Remove(o));
                            user.Update();
                        }
                        break;
                }
                var response = new StoreResponseData {
                    Data = JSON.Serialize(user.Operators),
                    Total = user.Operators.Count
                };
                response.Return();
            }
        }

        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}