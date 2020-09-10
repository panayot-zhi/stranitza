using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.WebUtilities;

namespace stranitza.Utility
{
    [HtmlTargetElement("div", Attributes = ValidationForAttributeName + "," + ValidationErrorClassName)]
    public class ValidationClassTagHelper : TagHelper
    {
        private const string ValidationForAttributeName = "str-validation-for";
        private const string ValidationErrorClassName = "str-error-class";

        [HtmlAttributeName(ValidationForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName(ValidationErrorClassName)]
        public string ValidationErrorClass { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var modelState = ViewContext.ViewData.ModelState;
            modelState.TryGetValue(For.Name, out var entry);
            if (entry == null || !entry.Errors.Any())
            {
                return;
            }

            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass(ValidationErrorClass);
            output.MergeAttributes(tagBuilder);
        }
    }

    [HtmlTargetElement("th", Attributes = TableColumnForAttributeName)]
    public class SortableTableColumnTagHelper : TagHelper
    {
        private const string TableColumnForAttributeName = "str-sortable-for";
        private const string OrderQueryStringParameterName = "order";
        private const string DescendingOrderQueryStringParameterValue = "desc";
        private const string SortQueryStringParameterName = "sort";

        private IUrlHelperFactory urlHelperFactory;

        public SortableTableColumnTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            this.urlHelperFactory = urlHelperFactory;
        }

        [HtmlAttributeName(TableColumnForAttributeName)]
        public string PropertyName { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }
        
        private SortOrder ResolveSortOrder(string name)
        {
            var query = ViewContext.HttpContext.Request.Query;
            var sort = query[SortQueryStringParameterName].FirstOrDefault();
            if (sort == null || sort != name)
            {                
                return SortOrder.Unknown;
            }

            var order = query[OrderQueryStringParameterName].FirstOrDefault();

            return order == DescendingOrderQueryStringParameterValue ?
                SortOrder.Desc : SortOrder.Asc; // the default order is ascending
        }

        private string ResolveUrl(string name, SortOrder sortOrder)
        {            
            var request = ViewContext.HttpContext.Request;
            var currentQueryString = request.Query;
            var dictionary = new Dictionary<string, string>();

            switch (sortOrder)
            {                                                       
                case SortOrder.Unknown:
                    // do not add keys when no sort
                    break;

                default:
                    dictionary.Add(
                        key: SortQueryStringParameterName,
                        value: name
                    );

                    dictionary.Add(
                        key: OrderQueryStringParameterName,
                        value: sortOrder.ToString().ToLowerInvariant()
                    );
                    break;                    
            }

            // gather existing query key:values
            foreach (var item in currentQueryString)
            {
                // add these only once
                if (item.Key == SortQueryStringParameterName || 
                    item.Key == OrderQueryStringParameterName)
                {
                    continue;
                }
                
                dictionary.TryAdd(item.Key, item.Value);
            }

            // construct query string
            return QueryHelpers.AddQueryString($"{request.PathBase}{request.Path}", dictionary);
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var aBuilder = new TagBuilder("a");            
            var sortOrder = ResolveSortOrder(PropertyName);
            output.AddClass("sortable", HtmlEncoder.Default);            

            switch (sortOrder)
            {
                case SortOrder.Unknown:
                    aBuilder.Attributes.Add("href", ResolveUrl(PropertyName, SortOrder.Asc));
                    break;
                case SortOrder.Asc:
                    aBuilder.Attributes.Add("href", ResolveUrl(PropertyName, SortOrder.Desc));                    
                    output.AddClass("desc", HtmlEncoder.Default);
                    break;
                case SortOrder.Desc:
                    aBuilder.Attributes.Add("href", ResolveUrl(PropertyName, SortOrder.Unknown));
                    output.AddClass("asc", HtmlEncoder.Default);
                    break;
                default:
                    aBuilder.Attributes.Add("href", "#");
                    break;
            }

            output.PreContent.SetHtmlContent(aBuilder.RenderStartTag());
            output.PostContent.SetHtmlContent(aBuilder.RenderEndTag());            
        }
    }
}