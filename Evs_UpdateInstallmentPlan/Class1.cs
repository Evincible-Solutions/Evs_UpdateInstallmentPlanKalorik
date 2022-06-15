using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Evs_UpdateInstallmentPlan
{
    public class Class1 : IPlugin
    {
        ITracingService tracingService;
        public void Execute(IServiceProvider serviceProvider)
        {
            #region must to have
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            // Create service with context of current user
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            //create tracing service
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            #endregion
            if (context.Depth > 1)
            {
                return;
            }
            try
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity crmccpayment = (Entity)context.InputParameters["Target"];
                    tracingService.Trace(crmccpayment.LogicalName);
                    if (crmccpayment.LogicalName != "new_crmccpayment")
                    {
                        return;
                    }
                    var crmccpaymentId = crmccpayment.Id;
                    tracingService.Trace("crmccpayment Id: " + crmccpaymentId);
                    if (context.MessageName.ToUpper() == "UPDATE")
                    {
                        crmccpayment = service.Retrieve("new_crmccpayment", crmccpaymentId, new ColumnSet(true));
                        if (crmccpayment.Contains("new_relatedinstallmentplan"))
                        {
                            //Get Installment Plan
                            Entity InstallmentPlan = service.Retrieve("evs_installmentplan", ((EntityReference)crmccpayment.Attributes["new_relatedinstallmentplan"]).Id,new ColumnSet(true));
                            if (InstallmentPlan != null)
                            {
                                InstallmentPlan["new_originalorder"] = (EntityReference)crmccpayment["new_relatedorder"];
                                InstallmentPlan["new_customer"] = (EntityReference)crmccpayment["new_relatedcontact"];
                                //InstallmentPlan["transactioncurrencyid"] = (EntityReference)crmccpayment[""];
                                service.Update(InstallmentPlan);
                            }
                            //Get Installment Plan
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace(ex.ToString() + ex.InnerException);
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}
