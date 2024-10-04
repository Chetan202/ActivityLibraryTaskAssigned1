using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace ActivityLibraryTaskAssigned1
{
    public class CityNameResolver : CodeActivity
    {
        [Input("City Abbreviation")]
        [RequiredArgument]
        public InArgument<string> CityAbbreviation { get; set; }

        [Output("Full City Name")]
        public OutArgument<string> FullCityName { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            // Get the execution context
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();

            // Get the input city abbreviation
            string cityAbbreviation = CityAbbreviation.Get(executionContext);
            tracingService.Trace($"Received city abbreviation: {cityAbbreviation}");
            string fullCity = string.Empty;

            try
            {
                // Query to fetch the city abbreviation from the 'contoso_countryname' entity
                QueryExpression query = new QueryExpression("contoso_countryname")
                {
                    ColumnSet = new ColumnSet("contoso_name", "contoso_abbreviation") // Use your actual field names
                };

                // Add filter to match the input abbreviation
                query.Criteria.AddCondition("contoso_abbreviation", ConditionOperator.Equal, cityAbbreviation.ToUpper());

                // Execute the query
                EntityCollection result = service.RetrieveMultiple(query);

                // Check if any records were found
                if (result.Entities.Count > 0)
                {
                    Entity countryRecord = result.Entities[0]; // Get the first match
                    fullCity = countryRecord.GetAttributeValue<string>("contoso_name");
                    tracingService.Trace($"Mapped {cityAbbreviation} to {fullCity}.");
                }
                else
                {
                    fullCity = "Unknown Country";
                    tracingService.Trace($"No matching record found for city abbreviation '{cityAbbreviation}'. Defaulting to 'Unknown Country name'.");
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Exception: {ex.Message}");
                fullCity = "Error Occurred";
            }

            // Set the output full city name
            FullCityName.Set(executionContext, fullCity);
            tracingService.Trace($"Setting output Full City Name to: {fullCity}");
        }
    }
}